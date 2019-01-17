using System;
using System.ComponentModel;
using Walkways.MVVM.ViewModel;

namespace XPDF.ViewModel
{
    internal class DocumentConverter : INotifyPropertyChanged
    {
        #region Private Properties

        private INPCInvoker _INPCInvoke;
        private String      _SelectDestinationText;
        private String      _SelectSourceText;
        private string      _SelectedDestinationText = "";
        private string      _SelectedSourceText      = "";

        #endregion

        public DocumentConverter()
        {
            _INPCInvoke = new INPCInvoker( this );

            InvokeITALocalizationCommand = new CommandRelay<Object>( InvokeITALocalization );
            InvokeENGLocalizationCommand = new CommandRelay<Object>( InvokeENGLocalization );
            SelectDestinationCommand = new CommandRelay<Object>( SelectDestination );
            SelectSourceCommand = new CommandRelay<Object>( SelectSource );
        }
        
        private void InvokeITALocalization( object obj )
        {
            throw new NotImplementedException( );
        }

        public CommandRelay<object> InvokeITALocalizationCommand
        {
            get;
        }

        private void InvokeENGLocalization( object obj )
        {
            throw new NotImplementedException( );
        }

        public CommandRelay<object> InvokeENGLocalizationCommand { get; }
        
        public event PropertyChangedEventHandler PropertyChanged;

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
    }
}
