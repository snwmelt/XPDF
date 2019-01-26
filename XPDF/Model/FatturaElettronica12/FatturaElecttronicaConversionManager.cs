using DirectoryScanner.Synchronous;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Walkways.Extensions.Attributes;
using XPDF.Model.Enums;
using XPDF.Model.Event;
using XPDF.Model.Event.Enums;
using XPDF.Model.Event.Interface;
using XPDF.Model.Interface;

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
                resutlt.Add( FileExtension.GetDescription( ).ToLowerInvariant( ) );

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
                ScanMode  = DirectoryScanner.Common.ScanMode.MatchExtension,
                SearchFor = GetExtensionStrings( )
            };

            _DirectoryScanner.FileFoundEvent += ( s, e ) => 
            {
                if ( State == EXPDFConverterState.Working && !_Aborting )
                {
                    _UpdateContainer.Items.Add( new FileInformation( new FileFormat( EFileExtension.XML, EFormat.Uknown, null ), new Uri( e.FullName ) ) );
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
                _UpdateContainer.IncrementProgress( );

                try
                {
                    IFileInformation ConvertedFileInfo = Convert( _UpdateContainer.LastItem );

                    File.Move( ConvertedFileInfo.Path.LocalPath, Destination + "\\" + ConvertedFileInfo.FallbackPath );
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
                return _FEFileConverter.SupportedFileExtensions;
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
