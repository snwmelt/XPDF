using System;
using System.Windows.Navigation;
using Walkways.Extensions.Attributes;
using Walkways.MVVM.View.Interfaces;
using Walkways.MVVM.ViewModel.Interfaces;
using XPDF.ViewModel.Enums;

namespace XPDF.ViewModel
{
    internal sealed class Nav
    {
        #region Private Variables

        private static readonly Nav     _Instance = new Nav( );
        private Lazy<NavigationService> _LazyNavigationService;

        #endregion

        private Nav()
        {
            _LazyNavigationService = new Lazy<NavigationService>( ( ) => ( ( INavigationServiceProvider )App.Current.MainWindow ).NavigationService );
        }

        public static NavigationService NavigationService
        {
            get
            {
                return _Instance._LazyNavigationService.Value;
            }
        }

        public static bool NavigationServiceAvailable
        {
            get
            {
                return _Instance._LazyNavigationService.IsValueCreated && NavigationService != null;
            }
        }

        public static bool NavigateBackward( )
        {
            if ( NavigationService.CanGoBack )
            {
                NavigationService.GoBack( );

                return true;
            }

            return false;
        }

        public static bool NavigateForward( )
        {
            if ( NavigationService.CanGoForward )
            {
                NavigationService.GoForward( );

                return true;
            }

            return false;
        }

        public static bool NavigateTo( EUIContent UIContent )
        {
            return NavigationService.Navigate( new Uri( UIContent.GetDescription( ) ) );
        }
    }
}
