using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;

namespace XPDF.View.Controls
{
    /// <summary>
    /// Interaction logic for LabeledCheckboxUserControl.xaml
    /// </summary>
    public partial class LabeledCheckboxUserControl : UserControl
    {
        #region Depedency Properties

        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register( "IsChecked",
                                                                                              typeof( Boolean? ),
                                                                                              typeof( LabeledCheckboxUserControl ),
                                                                                              new PropertyMetadata( false ) );

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register( "Label",
                                                                                              typeof( String ),
                                                                                              typeof( LabeledCheckboxUserControl ),
                                                                                              new PropertyMetadata( "" ) );

        public static readonly DependencyProperty LabelFontProperty = DependencyProperty.Register( "LabelFont",
                                                                                              typeof( Font ),
                                                                                              typeof( LabeledCheckboxUserControl ) );
        #endregion


        public Boolean? IsChecked
        {
            get
            {
                return ( Boolean? )GetValue( IsCheckedProperty );
            }

            set
            {
                SetValue( IsCheckedProperty, value );
            }
        }

        public Font LabelFont
        {
            get
            {
                return ( Font )GetValue( LabelFontProperty );
            }

            set
            {
                SetValue( LabelFontProperty, value );
            }
        }

        public String Label
        {
            get
            {
                return ( String )GetValue( LabelProperty );
            }

            set
            {
                SetValue( LabelProperty, value );
            }
        }

        public LabeledCheckboxUserControl( )
        {
            InitializeComponent( );
        }
    }
}
