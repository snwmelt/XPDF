using System;
using System.ComponentModel;
using System.Diagnostics;
using Walkways.MVVM.ViewModel;

namespace XPDF.ViewModel
{
    internal sealed class AboutViewModel : INotifyPropertyChanged
    {
        #region Private Variables

        private readonly INPCInvoker _INPCInvoker;
        private int                  _ConversionTotal;

        #endregion

        private void _InvokeOpenDonationLink( object obj )
        {
            Process.Start( @"https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=dibartolo%40gmail.com&currency_code=EUR&source=url" );
        }

        public AboutViewModel( )
        {
            _INPCInvoker     = new INPCInvoker( this );
            ConversionTotal  = 0;
            OpenDonationLink = new CommandRelay<object>( _InvokeOpenDonationLink );
        }

        public int ConversionTotal
        {
            get
            {
                return _ConversionTotal;
            }

            set
            {
                _INPCInvoker.AssignPropertyValue( ref PropertyChanged, ref _ConversionTotal, value );
            }
        }


        public CommandRelay<object> OpenDonationLink
        {
            get;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
