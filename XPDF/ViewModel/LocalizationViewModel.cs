using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using Walkways.MVVM.ViewModel;
using XPDF.View.Localization;

namespace XPDF.ViewModel
{
    public  class LocalizationViewModel : INotifyPropertyChanged
    {
        #region Private Variables
        
        private INPCInvoker _INPCInvoke;


        #endregion

        public static readonly LocalizationViewModel Instance = new LocalizationViewModel( );

        private LocalizationViewModel()
        {
            _INPCInvoke = new INPCInvoker( this );
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        public String About
        {
            get
            {
                return LocalisedUI.About;
            }
        }

        public String Cancel
        {
            get
            {
                return LocalisedUI.Cancel;
            }
        }

        public String PConversionTotal
        {
            get
            {
                return LocalisedUI.PConversionTotal;
            }
        }

        public String PEnableP7M
        {
            get
            {
                return LocalisedUI.Enable + " P7M -> XML ?-> PDF";
            }
        }

        public String PEnableXML
        {
            get
            {
                return LocalisedUI.Enable + " P7M -> XML";
            }
        }

        public String PEnablePDF
        {
            get
            {
                return LocalisedUI.Enable + " XML -> PDF";
            }
        }

        public String PEnableAutoPrint
        {
            get
            {
                return LocalisedUI.Enable + " " + LocalisedUI.Printing;
            }
        }

        public String PInheritFileName
        {
            get
            {
                return LocalisedUI.PInheritFileName;
            }
        }

        public String PRememberDirectories
        {
            get
            {
                return LocalisedUI.PRememberDirectories;
            }
        }

        public String PUseDefaultDirectories
        {
            get
            {
                return LocalisedUI.PUseDefaultDirectories;
            }
        }

        public String Convert
        {
            get
            {
                return LocalisedUI.Convert;
            }
        }

        public String Destination
        {
            get
            {
                return LocalisedUI.Destination;
            }
        }
        
        public String Donate
        {
            get
            {
                return LocalisedUI.Donate;
            }
        }

        public String Search
        {
            get
            {
                return LocalisedUI.Search;
            }
        }

        public void SetCurrentCulture( CultureInfo Target )
        {
            Thread.CurrentThread.CurrentCulture   = CultureInfo.DefaultThreadCurrentCulture   = Target;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.DefaultThreadCurrentUICulture = Target;

            _INPCInvoke.NotifyPropertyChanged( ref PropertyChanged, String.Empty );
        }

        public String Settings
        {
            get
            {
                return LocalisedUI.Settings;
            }
        }

        public String Source
        {
            get
            {
                return LocalisedUI.Source;
            }
        }

        public String TypePathHere
        {
            get
            {
                return LocalisedUI.TypePathHere;
            }
        }
    }
}
