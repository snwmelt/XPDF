using Ookii.Dialogs.Wpf;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using Walkways.MVVM.ViewModel;
using XPDF.Model.FatturaElettronica12;
using XPDF.Model.Event;
using XPDF.Model.Event.Interface;
using XPDF.Model.Interface;
using XPDF.View.Localization;

namespace XPDF.ViewModel
{
    internal class DocumentConverter : INotifyPropertyChanged
    {
        #region Private Properties

        private Boolean                 _ConversionInProgress;
        private INPCInvoker             _INPCInvoke;
        private String                  _PathwaySelectorPathPrompt;
        private String                  _SearchLocationText;
        private String                  _SelectDestinationText;
        private String                  _SelectSourceText;
        private string                  _SelectedDestinationText   = "";
        private string                  _SelectedSourceText        = "";
        private String                  _XPDFConvertText;
        private IXPDFConversionManager  _XPDFConverter             = new FatturaElecttronicaConversionManager( );

        #endregion

        public DocumentConverter()
        {
            _INPCInvoke = new INPCInvoker( this );

            InvokeITALocalizationCommand = new CommandRelay<Object>( InvokeITALocalization, CanLocalizeToITA );
            InvokeENGLocalizationCommand = new CommandRelay<Object>( InvokeENGLocalization, CanLocaliseToENG );
            SelectDestinationCommand     = new CommandRelay<Object>( SelectDestination, ConversionInProgressPredicate );
            SelectSourceCommand          = new CommandRelay<Object>( SelectSource, ConversionInProgressPredicate );
            XPDFConvertCommand           = new CommandRelay<Object>( XPDFConvert, ConversionInteractionAllowed );
            ConversionInProgress         = false;

            UpdateUILables( );
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
            return !String.IsNullOrEmpty( SelectedSourceText );
        }

        private bool ConversionInProgressPredicate( object obj )
        {
            return !ConversionInProgress;
        }

        private bool CanLocalizeToITA( object obj )
        {
            return CultureInfo.CurrentUICulture.ThreeLetterISOLanguageName != "ita";
        }

        private bool CanLocaliseToENG( object obj )
        {
            return CultureInfo.CurrentUICulture.ThreeLetterISOLanguageName != "eng";
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

        private void InvokeITALocalization( object obj )
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture( "It" );

            UpdateUILables( );
        }

        public CommandRelay<object> InvokeITALocalizationCommand
        {
            get;
        }

        private void InvokeENGLocalization( object obj )
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture( "En" );

            UpdateUILables( );
        }

        public CommandRelay<object> InvokeENGLocalizationCommand { get; }

        public String PathwaySelectorPathPrompt
        {
            get
            {
                return _PathwaySelectorPathPrompt;
            }

            set
            {
                _INPCInvoke.AssignPropertyValue<String>( ref PropertyChanged, ref _PathwaySelectorPathPrompt, value );
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public String SearchLocationText
        {
            get
            {
                return _SearchLocationText;
            }

            set
            {
                _INPCInvoke.AssignPropertyValue<String>( ref PropertyChanged, ref _SearchLocationText, value );
            }
        }

        private void SelectDestination( object obj )
        {
            SelectedDestinationText = GetFolderBrowserDialogPath( true );
        }

        public CommandRelay<object> SelectDestinationCommand { get; }

        public String SelectDestinationText
        {
            get
            {
                return _SelectDestinationText;
            }

            set
            {
                _INPCInvoke.AssignPropertyValue<String>( ref PropertyChanged, ref _SelectDestinationText, value );
            }
        }

        private void SelectSource( object obj )
        {
            SelectedSourceText = GetFolderBrowserDialogPath( );
        }

        public CommandRelay<object> SelectSourceCommand { get; }

        public String SelectSourceText
        {
            get
            {
                return _SelectSourceText;
            }

            set
            {
                _INPCInvoke.AssignPropertyValue<String>( ref PropertyChanged, ref _SelectSourceText, value );
            }
        }

        public String SelectedDestinationText
        {
            get
            {
                return _SelectedDestinationText;
            }

            set
            {
                _INPCInvoke.AssignPropertyValue<String>( ref PropertyChanged, ref _SelectedDestinationText, value );
            }
        }

        public String SelectedSourceText
        {
            get
            {
                return _SelectedSourceText;
            }

            set
            {
                _INPCInvoke.AssignPropertyValue<String>( ref PropertyChanged, ref _SelectedSourceText, value );
            }
        }

        private void ToggleConversionState()
        {
            ConversionInProgress = !ConversionInProgress;
            XPDFConvertText       = ConversionInProgress ? LocalisedUI.Cancel : LocalisedUI.Convert;
        }

        private void UpdateUILables( )
        {
            SelectDestinationText     = LocalisedUI.Destination;
            SelectSourceText          = LocalisedUI.Source;
            XPDFConvertText           = ConversionInProgress ? LocalisedUI.Cancel : LocalisedUI.Convert;
            SearchLocationText        = LocalisedUI.Search;
            PathwaySelectorPathPrompt = LocalisedUI.TypePathHere;
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

        public String XPDFConvertText
        {
            get
            {
                return _XPDFConvertText;
            }

            set
            {
                _INPCInvoke.AssignPropertyValue<String>( ref PropertyChanged, ref _XPDFConvertText, value );
            }
        }
    }
}
