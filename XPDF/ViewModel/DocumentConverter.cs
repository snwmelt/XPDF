using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using Walkways.MVVM.ViewModel;
using XPDF.View.Localization;

namespace XPDF.ViewModel
{
    internal class DocumentConverter : INotifyPropertyChanged
    {
        #region Private Properties

        private INPCInvoker _INPCInvoke;
        private String      _SearchLocationText;
        private String      _SelectDestinationText;
        private String      _SelectSourceText;
        private string      _SelectedDestinationText = "";
        private string      _SelectedSourceText      = "";
        private String      _XPDFConvertText;

        #endregion

        public DocumentConverter()
        {
            _INPCInvoke = new INPCInvoker( this );

            InvokeITALocalizationCommand = new CommandRelay<Object>( InvokeITALocalization, CanLocalizeToITA );
            InvokeENGLocalizationCommand = new CommandRelay<Object>( InvokeENGLocalization, CanLocaliseToENG );
            SelectDestinationCommand     = new CommandRelay<Object>( SelectDestination );
            SelectSourceCommand          = new CommandRelay<Object>( SelectSource );
            XPDFConvertCommand           = new CommandRelay<Object>( XPDFConvert );

            UpdateUILables( );
        }

        private bool CanLocalizeToITA( object obj )
        {
            return CultureInfo.CurrentUICulture.ThreeLetterISOLanguageName != "ita";
        }

        private bool CanLocaliseToENG( object obj )
        {
            return CultureInfo.CurrentUICulture.ThreeLetterISOLanguageName != "eng";
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
            throw new NotImplementedException( );
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
            throw new NotImplementedException( );
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

        private void UpdateUILables( )
        {
            SelectDestinationText = LocalisedUI.Destination;
            SelectSourceText      = LocalisedUI.Source;
            XPDFConvertText       = LocalisedUI.Convert;
            SearchLocationText    = LocalisedUI.Search;
        }

        private void XPDFConvert( Object obj )
        {
            throw new System.NotImplementedException( );
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
