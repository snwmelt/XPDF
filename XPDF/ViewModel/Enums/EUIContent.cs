using System.ComponentModel;

namespace XPDF.ViewModel.Enums
{
    internal enum EUIContent
    {
        [Description( "pack://application:,,,/View/AboutPageView.xaml" )]
        AboutPage = 3,
        [Description( "pack://application:,,,/View/DocumentConverterPageView.xaml" )]
        ConverterPage = 0,
        [ Description( "pack://application:,,,/View/SettingsPageView.xaml" )]
        SettingsPage = 1
    }
}
