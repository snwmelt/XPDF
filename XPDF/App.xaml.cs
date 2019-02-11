using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using XPDF.Properties;

namespace XPDF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static readonly String AdobeExecutable    = @"Adobe\Acrobat 5.0\Acrobat\acrobat.exe";
        internal static readonly String AdobeLicense       = @"Adobe\Acrobat 5.0\Legal\license.txt";
        internal static readonly String ExecutingDirectory = Path.GetDirectoryName( Assembly.GetExecutingAssembly( ).Location );

        public App( ) : base( )
        {
            if ( Debugger.IsAttached )
                Settings.Default.Reset( );
        }
    }
}
