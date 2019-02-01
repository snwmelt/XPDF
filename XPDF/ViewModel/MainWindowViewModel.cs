using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows.Controls;
using Walkways.MVVM.ViewModel;
using XPDF.ViewModel.Enums;

namespace XPDF.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Private Variables

        private Page        _CurrentView    = null;
        private INPCInvoker _INPCInvoke     = null;

        #endregion

        private bool _CanDisplayAboutPage( object obj )
        {
            return true;
        }

        private bool _CanDisplayConvertPage( object obj )
        {
            return true;
        }

        private bool _CanDisplaySettingsPage( object obj )
        {
            return true;
        }

        private bool _CanLocaliseToENG( object obj )
        {
            return CultureInfo.CurrentUICulture.ThreeLetterISOLanguageName != "eng";
        }

        private bool _CanLocaliseToITA( object obj )
        {
            return CultureInfo.CurrentUICulture.ThreeLetterISOLanguageName != "ita";
        }

        private void _InitialiseUI( Object obj )
        {
            Nav.NavigateTo( EUIContent.ConverterPage );
        }

        private void _InvokeDisplayAbout( object obj )
        {
            Nav.NavigateTo( EUIContent.AboutPage );
        }

        private void _InvokeDisplayConvert( object obj )
        {
            Nav.NavigateTo( EUIContent.ConverterPage );
        }

        private void _InvokeDisplaySettings( object obj )
        {
            Nav.NavigateTo( EUIContent.SettingsPage );
        }

        private void _InvokeENGLocalisation( object obj )
        {
            LocalizationViewModel.Instance.SetCurrentCulture( CultureInfo.CreateSpecificCulture( "En" ) );
        }

        private void _InvokeITALocalisation( object obj )
        {
            LocalizationViewModel.Instance.SetCurrentCulture( CultureInfo.CreateSpecificCulture( "It" ) );
        }

        public Page CurrentView
        {
            get
            {
                return _CurrentView;
            }

            set
            {
                _INPCInvoke.AssignPropertyValue<Page>( ref PropertyChanged, ref _CurrentView, value );
            }
        }

        public CommandRelay<object> DisplayAboutCommand
        {
            get;
        }

        public CommandRelay<object> DisplayConvertCommand
        {
            get;
        }

        public CommandRelay<object> DisplaySettingsCommand
        {
            get;
        }

        public CommandRelay<Object> InitializeUICommand
        {
            get;
        }

        public MainWindowViewModel( )
        {
            _INPCInvoke            = new INPCInvoker( this );
            InitializeUICommand    = new CommandRelay<Object>( _InitialiseUI );
            SelectEngCommand       = new CommandRelay<Object>( _InvokeENGLocalisation, _CanLocaliseToENG );
            SelectItaCommand       = new CommandRelay<Object>( _InvokeITALocalisation, _CanLocaliseToITA );
            DisplayAboutCommand    = new CommandRelay<Object>( _InvokeDisplayAbout,    _CanDisplayAboutPage   );
            DisplayConvertCommand  = new CommandRelay<Object>( _InvokeDisplayConvert,  _CanDisplayConvertPage );
            DisplaySettingsCommand = new CommandRelay<Object>( _InvokeDisplaySettings, _CanDisplaySettingsPage );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public CommandRelay<object> SelectEngCommand
        {
            get;
        }

        public CommandRelay<object> SelectItaCommand
        {
            get;
        }
    }
}
