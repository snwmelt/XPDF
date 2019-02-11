using DirectoryScanner.Synchronous;
using FatturaElettronica;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Walkways.Extensions.Attributes;
using XPDF.Model.Enums;
using XPDF.Model.Event;
using XPDF.Model.Event.Enums;
using XPDF.Model.Interface;
using XPDF.Properties;

namespace XPDF.Model.FatturaElettronica12
{
    internal class FatturaElecttronicaConversionManager : IXPDFConversionManager
    {
        #region Private Variables

        Boolean                 _Aborting            = false;
        Scanner                 _DirectoryScanner    = null;
        IFileConverter          _FEFileConverter     = new FatturaElecttronicaFileConverter( );
        EXPDFConverterState     _State               = EXPDFConverterState.Unavailable;
        private readonly Object _ThreadLockObject    = new Object( );
        FileConversionQueue     _FileConversionQueue = new FileConversionQueue( );
        HashSet<String>         _DocumentsToPrint    = new HashSet<String>( );

        #endregion

        public FatturaElecttronicaConversionManager( )
        {
            SetState( EXPDFConverterState.Available );
        }

        private void _Abort( Exception Exception = null )
        {
            SetState( EXPDFConverterState.Available, Exception );
            Thread.Sleep( 500 );
        }

        public void Abort( )
        {
            _Aborting = true;
            _Abort( );
        }

        public Boolean Aborting
        {
            get
            {
                return _Aborting;
            }
        }


        public long NumberToProcess
        {
            get
            {
                return _FileConversionQueue.Count;
            }
        }

        public float PercentCompleted
        {
            get
            {
                return _FileConversionQueue.PercentItterated;
            }
        }

        public long NumberProcessed
        {
            get
            {
                return _FileConversionQueue.ItteratedOver;
            }
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
            _Aborting = false;

            if ( String.IsNullOrEmpty( PathToDestinationDirectory ) )
                PathToDestinationDirectory = PathToSourceDirectory;

            _FileConversionQueue.Reset( );
            _DocumentsToPrint.Clear( );

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
                    _FileConversionQueue.EnQueueFile( new FileInformation( new Uri( e.FullName ) ) );
                }
            };

            new Thread( new ThreadStart( () => 
            {
                _DirectoryScanner.Scan( );

                if ( _FileConversionQueue.Count > 0 )
                    ProcessFiles( PathToDestinationDirectory );
            } ) ).Start( );
        }

        private void _InvokeFileProgressUpdateEvent( FileConversionUpdate _FileConversionUpdate )
        {
            FileConversionUpdateEvent?.Invoke( this, _FileConversionUpdate );
        }

        private void ProcessFiles( String Destination )
        {
            while ( State == EXPDFConverterState.Working && !_Aborting )
            {
                FileConversionUpdate _FileConversionUpdate = null;

                try
                {
                    _FileConversionUpdate = new FileConversionUpdate( _FileConversionQueue.GetNext( ) );

                    _DocumentsToPrint.Add( _ConvertDocument( _FileConversionUpdate, Destination ) );

                    _InvokeFileProgressUpdateEvent( _FileConversionUpdate );
                }
                catch ( Exception Ex )
                {
                    if ( _FileConversionUpdate is null )
                    {
                        _FileConversionUpdate = new FileConversionUpdate( null );

                        _FileConversionUpdate.AddTransformation( new FileTransformation( EFileTransformation.Failed,
                                                                                         _FileConversionUpdate.Original,
                                                                                         null,
                                                                                         new StateEventArgs( ESourceState.Unstable, Ex ) ) );
                    }
                    
                    _InvokeFileProgressUpdateEvent( _FileConversionUpdate );
                }


                if ( _FileConversionQueue.PercentItterated == 1 )
                {
                    if ( PrintDocuments )
                    {
                        Parallel.ForEach<String>( _DocumentsToPrint,
                        new Action<String, ParallelLoopState>( ( String _Document, ParallelLoopState state ) =>
                        {
                            if ( _Aborting )
                            {
                                state.Break( );
                            }
                            else
                            {
                                if ( !String.IsNullOrEmpty( _Document ) )
                                {
                                    PDFPrinter.Print( _Document, Settings.Default.SelectedPrinter );
                                    Log.Commit( "Sent To Printer:\t" + _Document );
                                }
                            }
                        } ) );

                        Log.Commit( );
                        Log.Commit( );
                    }

                    SetState( EXPDFConverterState.Available );
                }
            }
        }

        private String _ConvertDocument( FileConversionUpdate _FileConversionUpdate, String Destination )
        {
            IFileInformation ConvertedFileInfo = _FileConversionUpdate.Original;

            if ( ConvertedFileInfo.FormatInformation.FileExtension != EFileExtension.XML )
            {
                IFileInformation _AutoXML = TryGenerateXMLFile( ConvertedFileInfo, Destination, _FileConversionUpdate );

                if ( _AutoXML == null )
                    return null;


                try
                {
                    if ( !Settings.Default.EnablePDF )
                        return null;

                    ConvertedFileInfo = _FEFileConverter.Convert( _AutoXML );

                    _FileConversionUpdate.AddTransformation( EFileTransformation.ConvertedToCopied, _AutoXML, ConvertedFileInfo );

                    if ( ConvertedFileInfo == null )
                        return null;
                }
                catch ( Exception Ex )
                {
                    _FileConversionUpdate.AddTransformation( EFileTransformation.Failed,
                                                             _AutoXML,
                                                             null,
                                                             new StateEventArgs( ESourceState.Failed, Ex ) );

                    throw Ex;
                }
                finally
                {
                    if ( !Settings.Default.EnableXML )
                    {
                        File.Delete( _AutoXML.Path.LocalPath );

                        _FileConversionUpdate.AddTransformation( EFileTransformation.Deleted, null );
                    }
                }
            }
            else
            {
                if ( !Settings.Default.EnablePDF )
                    return null;

                try
                {
                    ConvertedFileInfo = Convert( ConvertedFileInfo );

                    _FileConversionUpdate.AddTransformation( EFileTransformation.ConvertedToCopied, ConvertedFileInfo );
                }
                catch ( Exception Ex )
                {
                    _FileConversionUpdate.AddTransformation( EFileTransformation.Failed, 
                                                             ConvertedFileInfo, 
                                                             new StateEventArgs( ESourceState.Failed, Ex ) );

                    throw Ex;
                }

                

                if ( ConvertedFileInfo == null )
                    return null;
            }

            if ( ConvertedFileInfo != null )
            {
                if ( !Settings.Default.InheritFileName )
                {
                    if ( File.Exists( ConvertedFileInfo.FallbackPath ) )
                        File.Delete( ConvertedFileInfo.FallbackPath );

                    File.Move( ConvertedFileInfo.Path.LocalPath, ConvertedFileInfo.FallbackPath );

                    ConvertedFileInfo = new FileInformation( ConvertedFileInfo.FormatInformation, ConvertedFileInfo.FallbackPath );

                    _FileConversionUpdate.AddTransformation( EFileTransformation.RenamedTo, ConvertedFileInfo );
                }

                if ( Destination != ConvertedFileInfo.Directory )
                {
                    if ( File.Exists( Destination + "\\" + ConvertedFileInfo.FileName ) )
                        File.Delete( Destination + "\\" + ConvertedFileInfo.FileName );

                    File.Move( ConvertedFileInfo.Path.LocalPath, Destination + "\\" + ConvertedFileInfo.FileName );

                    _FileConversionUpdate.AddTransformation( EFileTransformation.RenamedTo, new FileInformation( ConvertedFileInfo.FormatInformation, 
                                                                                                                 Destination + "\\" + ConvertedFileInfo.FileName ) );

                    return Destination + "\\" + ConvertedFileInfo.FileName;
                }

                return ConvertedFileInfo.Path.LocalPath;
            }

            return null;
        }

        private Boolean PrintDocuments
        {
            get
            {
                return Settings.Default.PrintingEnabled && 
                       Settings.Default.AutoPrintAuthorised && 
                       Settings.Default.EnablePDF &&
                       !String.IsNullOrEmpty( Settings.Default.SelectedPrinter ) &&
                       ( _DocumentsToPrint.Count > 0 );
            }
        }
        
        private IFileInformation TryGenerateXMLFile( IFileInformation _FileInformation, String Destination, FileConversionUpdate _FileConversionUpdate )
        {
            String FileText = File.ReadAllText( _FileInformation.Path.LocalPath );

            if ( !FileText.Contains( "FatturaElettronicaHeader" ) && !FileText.Contains( "FatturaElettronicaBody" ) )
                throw new InvalidOperationException( "Unsupported Format" + _FileInformation.Path );


            int    _XMLStartIndex   = FileText.IndexOf( Conventions.HeaderGaurd ) - 1;
            int    _XMLEndIndex     = FileText.LastIndexOf( Conventions.BodyGaurd ) - _XMLStartIndex + Conventions.BodyGaurd.Length + 1;
            String _RawBoundedData  = FileText.Substring( _XMLStartIndex, _XMLEndIndex );
            String _PossibleXMLData = XML.Conventions.Header + Conventions.Header + CleanInvalidXmlChars( _RawBoundedData ) + Conventions.Footer;

            FileInformation _ResultFileInformation = new FileInformation( new FileFormat( EFileExtension.XML, EFormat.Uknown, "" ), Destination + "\\" + _FileInformation.FileName + ".xml" );

            using (  XmlReader _XmlReader = XmlReader.Create( new StringReader( _PossibleXMLData ), new XmlReaderSettings { IgnoreWhitespace = true, IgnoreComments = true } ) )
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

                        _FileConversionUpdate.AddTransformation( EFileTransformation.ConvertedToCopied, _FileInformation, _ResultFileInformation );
                    }
                    else
                    {
                        throw new ArgumentException( "Invalid XMLPA FileContent " );
                    }
                }
                catch ( Exception Ex )
                {
                    File.WriteAllText( _ResultFileInformation.Path.LocalPath, _PossibleXMLData );

                    _FileConversionUpdate.AddTransformation( EFileTransformation.ConvertedToCopied, 
                                                             _FileInformation, 
                                                             _ResultFileInformation,
                                                             new StateEventArgs( ESourceState.Unstable, Ex ) );
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

        public event EventHandler<FileConversionUpdate> FileConversionUpdateEvent;

        private void SetState( EXPDFConverterState _EXPDFConverterState, Exception _Exception = null )
        {
            State = _EXPDFConverterState;

            StateChangedEvent?.Invoke( this, new StateEventArgs<EXPDFConverterState>( State, 
                                                                                     ( _Exception is null ) ? ESourceState.Stable : ESourceState.Unstable, 
                                                                                     _Exception ) );
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

        public event EventHandler<StateEventArgs<EXPDFConverterState>> StateChangedEvent;
    }
}
