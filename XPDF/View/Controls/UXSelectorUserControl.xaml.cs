using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace XPDF.View.Controls
{
    /// <summary>
    /// Interaction logic for UXSelector.xaml
    /// </summary>
    public partial class UXSelector : UserControl
    {
        #region Dependency Properties

        public static readonly DependencyProperty NavAboutPageCommandProperty = DependencyProperty.Register( "NavAboutPageCommand",
                                                                                                       typeof( ICommand ),
                                                                                                       typeof( UXSelector ) );

        public static readonly DependencyProperty NavHomePageCommandProperty = DependencyProperty.Register( "NavHomePageCommand",
                                                                                                       typeof( ICommand ),
                                                                                                       typeof( UXSelector ) );

        public static readonly DependencyProperty NavSettingsPageCommandProperty = DependencyProperty.Register( "NavSettingsCommand",
                                                                                                       typeof( ICommand ),
                                                                                                       typeof( UXSelector ) );

        public static readonly DependencyProperty SelectEngCommandProperty = DependencyProperty.Register( "SelectEngCommand",
                                                                                                       typeof( ICommand ),
                                                                                                       typeof( UXSelector ) );

        public static readonly DependencyProperty SelectItaCommandProperty = DependencyProperty.Register( "SelectItaCommand",
                                                                                                       typeof( ICommand ),
                                                                                                       typeof( UXSelector ) );

        #endregion

        public UXSelector( )
        {
            InitializeComponent( );
        }

        public ICommand NavAboutPageCommand
        {
            get
            {
                return ( ICommand )GetValue( NavAboutPageCommandProperty );
            }

            set
            {
                SetValue( NavAboutPageCommandProperty, value );
            }
        }
        public ICommand NavHomePageCommand
        {
            get
            {
                return ( ICommand )GetValue( NavHomePageCommandProperty );
            }

            set
            {
                SetValue( NavHomePageCommandProperty, value );
            }
        }
        public ICommand NavSettingsCommand
        {
            get
            {
                return ( ICommand )GetValue( NavSettingsPageCommandProperty );
            }

            set
            {
                SetValue( NavSettingsPageCommandProperty, value );
            }
        }
        public ICommand SelectEngCommand
        {
            get
            {
                return ( ICommand )GetValue( SelectEngCommandProperty );
            }

            set
            {
                SetValue( SelectEngCommandProperty, value );
            }
        }
        public ICommand SelectItaCommand
        {
            get
            {
                return ( ICommand )GetValue( SelectItaCommandProperty );
            }

            set
            {
                SetValue( SelectItaCommandProperty, value );
            }
        }
    }
}
