using DirectoryScanner.Synchronous;
using FatturaElettronica;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using Walkways.Extensions.Attributes;
using Walkways.Extensions.Strings;
using XPDF.Model.Enums;
using XPDF.Model.Event;
using XPDF.Model.Event.Enums;
using XPDF.Model.Event.Interface;
using XPDF.Model.Interface;
using XPDF.Properties;

namespace XPDF.Model.FatturaElettronica12
{
    internal class FatturaElecttronicaConversionManager : IXPDFConversionManager
    {
        #region Private Variables

        Boolean                 _Aborting         = false;
        Scanner                 _DirectoryScanner = null;
        IFileConverter          _FEFileConverter  = new FatturaElecttronicaFileConverter( );
        private readonly Object _ThreadLockObject = new Object( );
        EXPDFConverterState     _State            = EXPDFConverterState.Unavailable;
        FileConversionUpdate    _UpdateContainer  = new FileConversionUpdate( null );

        #endregion

        public FatturaElecttronicaConversionManager( )
        {
            SetState( EXPDFConverterState.Available );
        }

        private void _Abort( Exception Exception = null )
        {
            SetState( EXPDFConverterState.Available, Exception );
        }

        public void Abort( )
        {
            _Abort( );
        }
        
        private string[] GetExtensionStrings( )
        {
            List<String> resutlt = new List<String>( );

            foreach ( EFileExtension FileExtension in SupportedFileExtensions )
            {
                if ( FileExtension == EFileExtension.P7M && !Settings.Default.EnableP7M )
                    continue;
                
                resutlt.Add( FileExtension.GetDescription( ) );
            }

            return resutlt.ToArray( );
        }

        public void ConvertAll( String PathToSourceDirectory, String PathToDestinationDirectory )
        {
            if ( State != EXPDFConverterState.Available )
                throw new InvalidOperationException( "Converter State Is Incompatable With Chosen Action" );

            SetState( EXPDFConverterState.Working );

            if ( String.IsNullOrEmpty( PathToDestinationDirectory ) )
                PathToDestinationDirectory = PathToSourceDirectory;

            _UpdateContainer.Reset( );
            _UpdateContainer.Items.Clear( );

            _DirectoryScanner = new Scanner( PathToSourceDirectory )
            {
                ScanMode     = DirectoryScanner.Common.ScanMode.MatchExtension,
                SearchFor    = GetExtensionStrings( ),
                IgnoreCase   = true,
                IgnoreLocale = true
            };

            _DirectoryScanner.FileFoundEvent += ( s, e ) => 
            {
                if ( State == EXPDFConverterState.Working && !_Aborting )
                {
                    _UpdateContainer.Items.Add( new FileInformation( new Uri( e.FullName ) ) );
                }
            };

            new Thread( new ThreadStart( () => 
            {
                _DirectoryScanner.Scan( );

                ProgressUpdateEvent?.Invoke( this, new StateChangeEventArgs<IProgressUpdate<IFileInformation>>( _UpdateContainer ) );

                if ( !_UpdateContainer.Completed )
                    ProcessFiles( PathToDestinationDirectory );

                SetState( EXPDFConverterState.Available );

            } ) ).Start( );
        }

        private void ProcessFiles( String Destination )
        {
            //Parallel.ForEach<IFileInformation>( _UpdateContainer.Items.AsEnumerable( ),
            //new Action<IFileInformation, ParallelLoopState>( ( IFileInformation Element, ParallelLoopState state ) =>
            //{
            //    if ( State != EXPDFConverterState.Working || _Aborting )
            //    {
            //        state.Break( );
            //    }
            //
            //    _UpdateContainer.IncrementProgress( );
            //
            //    try
            //    {
            //        Convert( _UpdateContainer.LastItem );
            //    }
            //    catch ( Exception Ex )
            //    {
            //        ProgressUpdateEvent?.Invoke( this, new StateChangeEventArgs<IProgressUpdate<IFileInformation>>( _UpdateContainer, null, Ex ) );
            //    }
            //
            //    ProgressUpdateEvent?.Invoke( this, new StateChangeEventArgs<IProgressUpdate<IFileInformation>>( _UpdateContainer ) );
            //
            //    if ( _UpdateContainer.Completed )
            //        SetState( EXPDFConverterState.Available );
            //
            //} ) );

            while ( State == EXPDFConverterState.Working && !_Aborting )
            {
                try
                {
                    _UpdateContainer.IncrementProgress( );

                    IFileInformation ConvertedFileInfo = null;

                    if ( _UpdateContainer.LastItem.FormatInformation.FileExtension != EFileExtension.XML )
                    {
                        IFileInformation _AutoXML = TryGenerateXMLFile( _UpdateContainer.LastItem );

                        ViewModel.SettingsViewModel.Singleton.IncrementConvertedFilesCount( 1 );

                        try
                        {
                            if ( Settings.Default.EnablePDF )
                            {
                                ConvertedFileInfo = _FEFileConverter.Convert( _AutoXML );
                                ViewModel.SettingsViewModel.Singleton.IncrementConvertedFilesCount( 1 );
                            }
                        }
                        finally
                        {
                            if ( !Settings.Default.EnableXML )
                            {
                                File.Delete( _AutoXML.Path.LocalPath );
                                ViewModel.SettingsViewModel.Singleton.IncrementConvertedFilesCount( -1 );
                            }
                        }
                    }
                    else
                    {
                        if ( Settings.Default.EnablePDF )
                        {
                            ConvertedFileInfo = Convert( _UpdateContainer.LastItem );
                            ViewModel.SettingsViewModel.Singleton.IncrementConvertedFilesCount( 1 );
                        }
                    }

                    if ( !Settings.Default.InheritFileName && ConvertedFileInfo != null )
                    {
                        if ( File.Exists( Destination + "\\" + ConvertedFileInfo.FallbackPath ) )
                            File.Delete( Destination + "\\" + ConvertedFileInfo.FallbackPath );

                        File.Move( ConvertedFileInfo.Path.LocalPath, Destination + "\\" + ConvertedFileInfo.FallbackPath );
                    }
                }
                catch ( Exception Ex )
                {
                    ProgressUpdateEvent?.Invoke( this, new StateChangeEventArgs<IProgressUpdate<IFileInformation>>( _UpdateContainer, null, Ex ) );
                }


                ProgressUpdateEvent?.Invoke( this, new StateChangeEventArgs<IProgressUpdate<IFileInformation>>( _UpdateContainer ) );


                if ( _UpdateContainer.Completed )
                    SetState( EXPDFConverterState.Available );
            }
        }
        
        private IFileInformation TryGenerateXMLFile( IFileInformation _FileInformation )
        {
            String FileText = File.ReadAllText( _FileInformation.Path.LocalPath );

            if ( !FileText.Contains( "FatturaElettronicaHeader" ) && !FileText.Contains( "FatturaElettronicaBody" ) )
                throw new InvalidOperationException( "Unsupported Format" + _FileInformation.Path );


            int    _XMLStartIndex   = FileText.IndexOf( Conventions.HeaderGaurd ) - 1;
            int    _XMLEndIndex     = FileText.LastIndexOf( Conventions.BodyGaurd ) - _XMLStartIndex + Conventions.BodyGaurd.Length + 1;
            String _RawBoundedData  = FileText.Substring( _XMLStartIndex, _XMLEndIndex );
            String _PossibleXMLData = XML.Conventions.Header + Conventions.Header + CleanInvalidXmlChars( _RawBoundedData ) + Conventions.Footer;

            FileInformation _ResultFileInformation = new FileInformation( new FileFormat( EFileExtension.XML, EFormat.Uknown, "" ), _FileInformation.Path.LocalPath + ".xml" );

            using ( XmlReader _XmlReader = XmlReader.Create( new StringReader( _PossibleXMLData ), new XmlReaderSettings { IgnoreWhitespace = true, IgnoreComments = true } ) )
            {
                Fattura _Fattura = new Fattura( );
            
                try
                {
                    _Fattura.ReadXml( _XmlReader );

                    if ( _Fattura.Validate( ).IsValid )
                    {
                        using ( XmlWriter _XmlWriter = XmlWriter.Create( _ResultFileInformation.Path.LocalPath, new XmlWriterSettings { Indent = true } ) )
                        {
                            _Fattura.WriteXml( _XmlWriter );
                        }
                    }
                    else
                    {
                        throw new ArgumentException( "Invalid XMLPA FileContent " );
                    }
                }
                catch
                {
                    File.WriteAllText( _ResultFileInformation.Path.LocalPath, _PossibleXMLData );
                }
            }
            

            return File.Exists( _ResultFileInformation.Path.LocalPath ) ? _ResultFileInformation : null;
        }

        public static string CleanInvalidXmlChars( String _XML )
        {
            // From xml spec valid chars: 
            // #x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]     
            // any Unicode character, excluding the surrogate blocks, FFFE, and FFFF. 
            string re = @"[^\x09\x0A\x0D\x20-\xD7FF\xE000-\xFFFD\x10000-x10FFFF]";
            return Regex.Replace( _XML, re, "" );
        }

        public IEnumerable<EFormat> InputFormats
        {
            get
            {
                return _FEFileConverter.InputFormats;
            }
        }

        public IEnumerable<EFormat> OutputFormats
        {
            get
            {
                return _FEFileConverter.OutputFormats;
            }
        }

        public event EventHandler<StateChangeEventArgs<IProgressUpdate<IFileInformation>>> ProgressUpdateEvent;
        
        private void SetState( EXPDFConverterState _EXPDFConverterState, Exception _Exception = null )
        {
            State = _EXPDFConverterState;
            StateChangedEvent?.Invoke( this, new StateChangeEventArgs<EXPDFConverterState>( _EXPDFConverterState, State, _Exception ) );
        }

        public IFileInformation Convert( IFileInformation Input )
        {
            if ( IsValidXML( File.ReadAllText( Input.Path.LocalPath ) ) )
                return _FEFileConverter.Convert( Input );
                    
            return null;
        }

        public bool IsValidXML( string InputXML )
        {
            return ( _FEFileConverter as IXMLConverter ).IsValidXML( InputXML );
        }

        public EXPDFConverterState State
        {
            get
            {
                lock ( _ThreadLockObject )
                {
                    return _State;
                }
            }

            private set
            {
                lock ( _ThreadLockObject )
                {
                    _State = value;
                }
            }
        }

        public IEnumerable<EFileExtension> SupportedFileExtensions
        {
            get
            {
                List<EFileExtension> ConverterEFileExtension = new List<EFileExtension>( _FEFileConverter.SupportedFileExtensions );

                ConverterEFileExtension.Add( EFileExtension.P7M );

                return ConverterEFileExtension;
            }
        }

        public IFormatInformation[] SupportedFormats
        {
            get
            {
                return ( _FEFileConverter as IXMLConverter ).SupportedFormats;
            }
        }

        public event EventHandler<StateChangeEventArgs<EXPDFConverterState>> StateChangedEvent;
    }
}
