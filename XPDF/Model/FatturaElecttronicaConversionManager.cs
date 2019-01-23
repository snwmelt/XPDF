using DirectoryScanner.Synchronous;
using System;
using System.Collections.Generic;
using Walkways.Extensions.Attributes;
using XPDF.Model.Enums;
using XPDF.Model.Event;
using XPDF.Model.Event.Enums;
using XPDF.Model.Event.Interface;
using XPDF.Model.Interface;

namespace XPDF.Model
{
    internal class FatturaElecttronicaConversionManager : IXPDFConversionManager
    {
        #region Private Variables

        Scanner                 _DirectoryScanner = null;
        private readonly Object _ThreadLockObject = new Object( );
        EXPDFConverterState     _State            = EXPDFConverterState.Unavailable;
        IXPDFFIleConverter      _FEFileConverter  = new FatturaElecttronicaFileConverter( );

        public FatturaElecttronicaConversionManager( )
        {
            SetState( EXPDFConverterState.Available );
        }
        #endregion

        private void _Abort( Exception Exception = null )
        {
            SetState( EXPDFConverterState.Available, Exception );
        }

        public void Abort( )
        {
            _FEFileConverter.Abort( );

            _Abort( );
        }

        private void Convert( String InputXMLFilePath, String OutputPDFDirectory )
        {
            try
            {
                if (_FEFileConverter.IsValidXML( InputXMLFilePath ))
                {
                    _FEFileConverter.Convert( InputXMLFilePath, OutputPDFDirectory );

                    ProgressUpdateEvent?.Invoke( this, new StateChangeEventArgs<IProgressUpdate>( new ProgressUpdate( false ) ) );
                }
            }
            catch ( Exception Ex )
            {
                ProgressUpdateEvent?.Invoke( this, new StateChangeEventArgs<IProgressUpdate>( new ProgressUpdate( false ), null, Ex ) );
            }
        }

        public void ConvertAll( String PathToSourceDirectory, String PathToDestinationDirectory )
        {
            if ( State != EXPDFConverterState.Available )
                throw new InvalidOperationException( "Converter State Is Incompatable With Chosen Action" );

            SetState( EXPDFConverterState.Working );

            _DirectoryScanner = new Scanner( PathToSourceDirectory )
            {
                ScanMode  = DirectoryScanner.Common.ScanMode.MatchExtension,
                SearchFor = new String[] { EFileExtension.XML.GetDescription( ).ToLowerInvariant( ) }
            };

            _DirectoryScanner.FileFoundEvent += ( s, e ) => 
            {
                if ( State == EXPDFConverterState.Working )
                {
                    String OutputPDFDirectory = PathToDestinationDirectory;

                    if ( String.IsNullOrEmpty( OutputPDFDirectory ) )
                        OutputPDFDirectory = e.DirectoryName;

                    Convert( e.FullName, OutputPDFDirectory );
                }
            };

            _DirectoryScanner.Scan( );

            ProgressUpdateEvent?.Invoke( this, new StateChangeEventArgs<IProgressUpdate>( new ProgressUpdate( true ) ) );
            SetState( EXPDFConverterState.Available );
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

        public event EventHandler<StateChangeEventArgs<IProgressUpdate>> ProgressUpdateEvent;

        private void SetState( EXPDFConverterState _EXPDFConverterState, Exception _Exception = null )
        {
            State = _EXPDFConverterState;
            StateChangedEvent?.Invoke( this, new StateChangeEventArgs<EXPDFConverterState>( _EXPDFConverterState, State, _Exception ) );
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
        
        public event EventHandler<StateChangeEventArgs<EXPDFConverterState>> StateChangedEvent;
    }
}
