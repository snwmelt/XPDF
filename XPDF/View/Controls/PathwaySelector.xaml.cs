using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace XPDF.View.Controls
{
    /// <summary>
    /// Interaction logic for PathwaySelector.xaml
    /// </summary>
    public partial class PathwaySelector : UserControl
    {
        #region Dependency Properties

        public static readonly DependencyProperty ButtonCommandProperty = DependencyProperty.Register( "ButtonCommand",
                                                                                              typeof( ICommand ),
                                                                                              typeof( PathwaySelector ) );

        public static readonly DependencyProperty ButtonContentProperty = DependencyProperty.Register( "ButtonContent",
                                                                                              typeof( Object ),
                                                                                              typeof( PathwaySelector ),
                                                                                              new PropertyMetadata( null ) );

        public static readonly DependencyProperty TextBoxTextProperty = DependencyProperty.Register( "TextBoxText",
                                                                                              typeof( String ),
                                                                                              typeof( PathwaySelector ),
                                                                                              new PropertyMetadata( "" ) );

        public static readonly DependencyProperty TextBoxPlaceholderProperty = DependencyProperty.Register( "TextBoxPlaceholder",
                                                                                              typeof( String ),
                                                                                              typeof( PathwaySelector ),
                                                                                              new PropertyMetadata( "" ) );

        #endregion

        public PathwaySelector( )
        {
            InitializeComponent( );
        }

        public ICommand ButtonCommand
        {
            get
            {
                return ( ICommand )GetValue( ButtonCommandProperty );
            }

            set
            {
                SetValue( ButtonCommandProperty, value );
            }
        }

        public Object ButtonContent
        {
            get
            {
                return ( Object )GetValue( ButtonContentProperty );
            }

            set
            {
                SetValue( ButtonContentProperty, value );
            }
        }

        public String TextBoxPlaceholder
        {
            get
            {
                return ( String )GetValue( TextBoxPlaceholderProperty );
            }

            set
            {
                SetValue( TextBoxPlaceholderProperty, value );
            }
        }

        public String TextBoxText
        {
            get
            {
                return ( String )GetValue( TextBoxTextProperty );
            }

            set
            {
                SetValue( TextBoxTextProperty, value );
            }
        }
    }
}
