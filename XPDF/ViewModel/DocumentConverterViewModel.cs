using Ookii.Dialogs.Wpf;
using System;
using System.ComponentModel;
using Walkways.MVVM.ViewModel;
using XPDF.Model.FatturaElettronica12;
using XPDF.Model.Event;
using XPDF.Model.Event.Interface;
using XPDF.Model.Interface;
using XPDF.View.Localization;
using System.IO;

namespace XPDF.ViewModel
{
    internal sealed class DocumentConverterViewModel : INotifyPropertyChanged
    {
        #region Private Properties

        private Boolean                 _ConversionInProgress;
        private INPCInvoker             _INPCInvoke;
        private IXPDFConversionManager  _XPDFConverter;

        #endregion

        public DocumentConverterViewModel()
        {
            _INPCInvoke    = new INPCInvoker( this );
            _XPDFConverter = new FatturaElecttronicaConversionManager( );

            SelectDestinationCommand = new CommandRelay<Object>( SelectDestination, ConversionInProgressPredicate );
            SelectSourceCommand      = new CommandRelay<Object>( SelectSource, ConversionInProgressPredicate );
            XPDFConvertCommand       = new CommandRelay<Object>( XPDFConvert, ConversionInteractionAllowed );
            ConversionInProgress     = false;

            _XPDFConverter.ProgressUpdateEvent += UpdateProgress;
        }
        
        private void UpdateProgress( object sender, StateChangeEventArgs<IProgressUpdate<IFileInformation>> e )
        {
            if ( e.CurrentState.Completed )
            {
                if ( ConversionInProgress )
                {
                    ToggleConversionState( );
                }
            }
        }

        public bool ConversionInProgress
        {
            get
            {
                return _ConversionInProgress;
            }

            set
            {
                _INPCInvoke.AssignPropertyValue<Boolean>( ref PropertyChanged, ref _ConversionInProgress, value );
            }
        }


        private bool ConversionInteractionAllowed( object obj )
        {
            Boolean SrcTest = !String.IsNullOrEmpty( SelectedSourceText ) && Directory.Exists( SelectedSourceText );
            Boolean DesTest = String.IsNullOrEmpty( SelectedDestinationText ) || Directory.Exists( SelectedDestinationText ) ;

            return SrcTest && DesTest;
        }

        private bool ConversionInProgressPredicate( object obj )
        {
            return !ConversionInProgress;
        }

        public Boolean EnablePathwaySelectors
        {
            get
            {
                return ( !SettingsViewModel.Singleton.UseDefaultDirectories.Value );
            }
        }


        private String GetFolderBrowserDialogPath( Boolean ShowNewFolderButton = false )
        {
            VistaFolderBrowserDialog FolderBrowser = new VistaFolderBrowserDialog
            {
                Description = LocalisedUI.FolderBrowserDescription,
                UseDescriptionForTitle = true,
                ShowNewFolderButton = ShowNewFolderButton
            };


            if ( ( Boolean )FolderBrowser.ShowDialog( ) )
                return FolderBrowser.SelectedPath;

            return null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        private void SelectDestination( object obj )
        {
            SelectedDestinationText = GetFolderBrowserDialogPath( true ) ?? SelectedDestinationText;
        }
        
        public CommandRelay<object> SelectDestinationCommand { get; }
        
        private void SelectSource( object obj )
        {
            SelectedSourceText = GetFolderBrowserDialogPath( ) ?? SelectedSourceText;
        }

        public CommandRelay<object> SelectSourceCommand { get; }
        
        public String SelectedDestinationText
        {
            get
            {
                return SettingsViewModel.Singleton.GetDestinationDirectory( );
            }
        
            set
            {
                SettingsViewModel.Singleton.UpdateDestinationDirectory( value );
                _INPCInvoke.NotifyPropertyChanged( ref PropertyChanged );
            }
        }
        
        public String SelectedSourceText
        {
            get
            {
                return SettingsViewModel.Singleton.GetSourceDirectory( );
            }
        
            set
            {
                SettingsViewModel.Singleton.UpdateSourceDirectory( value );
                _INPCInvoke.NotifyPropertyChanged( ref PropertyChanged );
            }
        }

        private void ToggleConversionState()
        {
            ConversionInProgress = !ConversionInProgress;
        }
        
        private void XPDFConvert( Object obj )
        {
            if ( ConversionInProgress )
            {
                _XPDFConverter.Abort( );

                ToggleConversionState( );
            }
            else
            {
                ToggleConversionState( );

                _XPDFConverter.ConvertAll( SelectedSourceText, SelectedDestinationText );
            }
        }

        public CommandRelay<object> XPDFConvertCommand { get; }
    }
}
