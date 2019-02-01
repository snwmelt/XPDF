using Ookii.Dialogs.Wpf;
using System;
using System.ComponentModel;
using System.Diagnostics;
using Walkways.MVVM.ViewModel;
using XPDF.Properties;
using XPDF.View.Localization;

namespace XPDF.ViewModel
{
    internal sealed class SettingsViewModel : INotifyPropertyChanged
    {
        #region Private Variables

        private readonly INPCInvoker _INPCInvoker;

        #endregion


        private void _InvokeSelectDefaultSourceDirectory( object obj )
        {
            DefaultSourceDirectory = _GetFolderBrowserDialogPath( true ) ?? DefaultSourceDirectory;
        }

        private void _InvokeSelectDefaultDestinationDirectory( object obj )
        {
            DefaultDestinationDirectory = _GetFolderBrowserDialogPath( true ) ?? DefaultDestinationDirectory;
        }

        private String _GetFolderBrowserDialogPath( Boolean ShowNewFolderButton = false )
        {
            VistaFolderBrowserDialog FolderBrowser = new VistaFolderBrowserDialog
            {
                Description            = LocalisedUI.FolderBrowserDescription,
                UseDescriptionForTitle = true,
                ShowNewFolderButton    = ShowNewFolderButton
            };


            if ( ( Boolean )FolderBrowser.ShowDialog( ) )
                return FolderBrowser.SelectedPath;

            return null;
        }

        private void _SettingsViewModelPropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            Settings.Default.Save( );
        }


        public String DefaultDestinationDirectory
        {
            get
            {
                return Settings.Default.DefaultDestinationDirectory;
            }

            set
            {
                Settings.Default.DefaultDestinationDirectory = value;
                _INPCInvoker.NotifyPropertyChanged( ref PropertyChanged );
            }
        }

        public String DefaultSourceDirectory
        {
            get
            {
                return Settings.Default.DefaultSourceDirectory;
            }

            set
            {
                Settings.Default.DefaultSourceDirectory = value;
                _INPCInvoker.NotifyPropertyChanged( ref PropertyChanged );
            }
        }

        public String DestinationDirectory
        {
            get
            {
                return Settings.Default.DestinationDirectory;
            }

            set
            {
                Settings.Default.DestinationDirectory = value;
                _INPCInvoker.NotifyPropertyChanged( ref PropertyChanged );
            }
        }

        public Boolean? EnableAutoPrint
        {
            get
            {
                return Settings.Default.EnableAutoPrint;
            }

            set
            {
                Settings.Default.EnableAutoPrint = value.Value;
                _INPCInvoker.NotifyPropertyChanged( ref PropertyChanged );
            }
        }

        public Boolean? EnableP7M
        {
            get
            {
                return Settings.Default.EnableP7M;
            }

            set
            {
                Settings.Default.EnableP7M = value.Value;
                _INPCInvoker.NotifyPropertyChanged( ref PropertyChanged );
            }
        }
        
        public Boolean? EnablePDF
        {
            get
            {
                return Settings.Default.EnablePDF;
            }

            set
            {
                Settings.Default.EnablePDF = value.Value;
                _INPCInvoker.NotifyPropertyChanged( ref PropertyChanged );
            }
        }

        public Boolean? EnableXML
        {
            get
            {
                return Settings.Default.EnableXML;
            }

            set
            {
                Settings.Default.EnableXML = value.Value;
                _INPCInvoker.NotifyPropertyChanged( ref PropertyChanged );
            }
        }

        public String GetDestinationDirectory( )
        {
            if ( UseDefaultDirectories.Value || RememberDirectories.Value )
                return UseDefaultDirectories.Value ? DefaultDestinationDirectory : DestinationDirectory;

            return Settings.Default.TempDestinationDirectory;
        }

        public String GetSourceDirectory( )
        {
            if ( UseDefaultDirectories.Value || RememberDirectories.Value )
                return UseDefaultDirectories.Value ? DefaultSourceDirectory : SourceDirectory;

            return Settings.Default.TempSourceDirectory;
        }

        public void IncrementConvertedFilesCount( int Value )
        {
            Settings.Default.ConvertedFilesCount += Value;
            _INPCInvoker.NotifyPropertyChanged( ref PropertyChanged, "ConvertedFilesCount" );
        }

        public Boolean? InheritFileName
        {
            get
            {
                return Settings.Default.InheritFileName;
            }

            set
            {
                Settings.Default.InheritFileName = value.Value;
                _INPCInvoker.NotifyPropertyChanged( ref PropertyChanged );
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Boolean? RememberDirectories
        {
            get
            {
                return Settings.Default.RememberDirectories;
            }

            set
            {
                if ( value.Value )
                    UseDefaultDirectories = false;

                Settings.Default.RememberDirectories = value.Value;
                _INPCInvoker.NotifyPropertyChanged( ref PropertyChanged );
            }
        }

        public CommandRelay<object> SelectDefaultDestinationCommand
        {
            get;
        }

        public CommandRelay<object> SelectDefaultSourceCommand
        {
            get;
        }

        private SettingsViewModel( )
        {
            _INPCInvoker = new INPCInvoker( this );
            this.PropertyChanged += _SettingsViewModelPropertyChanged;

            SelectDefaultDestinationCommand = new CommandRelay<object>( _InvokeSelectDefaultDestinationDirectory );
            SelectDefaultSourceCommand      = new CommandRelay<object>( _InvokeSelectDefaultSourceDirectory );

            Settings.Default.TempDestinationDirectory = null;
            Settings.Default.TempSourceDirectory      = null;

            if ( Debugger.IsAttached )
                Settings.Default.Reset( );
        }

        public static readonly SettingsViewModel Singleton = new SettingsViewModel( );

        public String SourceDirectory
        {
            get
            {
                return Settings.Default.SourceDirectory;
            }

            set
            {
                Settings.Default.SourceDirectory = value;
                _INPCInvoker.NotifyPropertyChanged( ref PropertyChanged );
            }
        }

        internal void UpdateDestinationDirectory( String value )
        {
            if ( !RememberDirectories.HasValue || !RememberDirectories.Value )
            {
                Settings.Default.TempDestinationDirectory = value;
            }
            else
            {
                DestinationDirectory = value;
            }
        }
        internal void UpdateSourceDirectory( String value )
        {
            if ( !RememberDirectories.HasValue || !RememberDirectories.Value )
            {
                Settings.Default.TempSourceDirectory = value;
            }
            else
            {
                SourceDirectory = value;
            }
        }

        public Boolean? UseDefaultDirectories
        {
            get
            {
                return Settings.Default.UseDefaultDirectories;
            }

            set
            {
                if ( value.Value )
                    RememberDirectories = false;

                Settings.Default.UseDefaultDirectories = value.Value;
                _INPCInvoker.NotifyPropertyChanged( ref PropertyChanged );
            }
        }
    }
}
