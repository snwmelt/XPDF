using System;
using System.Diagnostics;

namespace XPDF.Model
{
    internal sealed class PDFPrinter
    {
        private static readonly Object _PrintQueueLock = new Object( );
        private static readonly Object _ThreadLock     = new Object( );

        internal static void Print( String _FilePath, String _PrinterName )
        {
            ProcessStartInfo _PDFPrintProcessStartInfo = new ProcessStartInfo( )
            {
                FileName        = App.ExecutingDirectory + "\\" + App.AdobeExecutable,
                Arguments       = String.Format( "/n /s /o /h /t \"{0}\" \"{1}\"", _FilePath, _PrinterName ),
                CreateNoWindow  = true,
                UseShellExecute = false,
                WindowStyle     = ProcessWindowStyle.Hidden
            };

            Process _PDFPrintProcess = new Process( )
            {
                StartInfo = _PDFPrintProcessStartInfo
            };

            lock ( _PrintQueueLock )
            {
                _PDFPrintProcess.Start( );

                _PDFPrintProcess.WaitForInputIdle( );
            }

            System.Threading.Thread.Sleep( 2400 );

            lock ( _PrintQueueLock )
            {
                _PDFPrintProcess.Kill( );
                _PDFPrintProcess.WaitForExit( );
                _PDFPrintProcess.Close( );
            }
        }

        internal static void VerifyAdobeEULA( )
        {
            ProcessStartInfo _PDFPrintProcessStartInfo = new ProcessStartInfo( )
            {
                FileName        = App.ExecutingDirectory + "\\" + App.AdobeExecutable,
                Arguments       = String.Format( App.ExecutingDirectory + "\\" + App.AdobeLicense ),
                CreateNoWindow  = true,
                UseShellExecute = false,
                WindowStyle     = ProcessWindowStyle.Hidden
            };

            Process _PDFPrintProcess = new Process( )
            {
                StartInfo = _PDFPrintProcessStartInfo
            };

            lock ( _ThreadLock )
            {
                _PDFPrintProcess.Start( );

                _PDFPrintProcess.WaitForInputIdle( );
            }
        }
    }
}
