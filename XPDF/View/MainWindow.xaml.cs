using System.Windows;
using System.Windows.Navigation;
using Walkways.MVVM.View.Interfaces;

namespace XPDF.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INavigationServiceProvider
    {
        public MainWindow( )
        {
            InitializeComponent( );
        }

        public NavigationService NavigationService
        {
            get
            {
                return MainWindowContentFrame.NavigationService;
            }
        }
    }
}
