using System;
using System.IO;
using XPDF.Model.Event;
using XPDF.Model.Event.Enums;
using XPDF.Properties;

namespace XPDF.Model
{
    internal static class Log
    {
        private static StreamWriter _CreateOrOpenFile( )
        {
            if ( !File.Exists( _LogFilePath ) )
                return new StreamWriter( File.Create( _LogFilePath ) );

            return File.AppendText( _LogFilePath );
        }

        private static String _DateTimeStamp
        {
            get
            {
                return DateTime.Now.ToShortDateString( ) + " " + DateTime.Now.ToLongTimeString( );
            }
        }

        private static String _LogFilePath
        {
            get
            {
                return Settings.Default.DestinationDirectory + "\\Log.txt";
            }
        }

        private static readonly Object _ThreadLock = new Object( );


        internal static void AmmendBreak( )
        {
            lock ( _ThreadLock )
            {
                using ( StreamWriter _StreamWriter = _CreateOrOpenFile( ) )
                {
                    _StreamWriter.WriteLine( "END : " + _DateTimeStamp );
                    _StreamWriter.WriteLine( );
                    _StreamWriter.WriteLine( );
                    _StreamWriter.WriteLine( );
                    _StreamWriter.WriteLine( );
                }
            }
        }

        internal static void Commit( FileConversionUpdate _ConversionData )
        {
            lock ( _ThreadLock )
            {
                if ( _ConversionData is null || _ConversionData.Original is null )
                    return;

                using ( StreamWriter _StreamWriter = _CreateOrOpenFile( ) )
                {
                    _StreamWriter.Write( _DateTimeStamp + " | ");
                    _StreamWriter.WriteLine( _ConversionData.Original.Path.LocalPath + ": " );
                    _StreamWriter.WriteLine( "\t Transformations: " );


                    foreach ( FileTransformation _FileTransformation in _ConversionData.Transformations )
                    {
                        if ( _FileTransformation.Transformation == EFileTransformation.ConvertedToCopied )
                            Settings.Default.ConvertedFilesCount += 1;

                        if ( _FileTransformation.Source != null )
                        {
                            _StreamWriter.Write( "\t\t" + _FileTransformation.Source.Path.LocalPath );
                            _StreamWriter.WriteLine( " -> " + ( ( _FileTransformation.Result is null ) ? " " : _FileTransformation.Result.Path.LocalPath ) );


                            if ( _FileTransformation.EventData != null )
                            {
                                _StreamWriter.WriteLine( "\t\t\t State: " + _FileTransformation.EventData.SubjectState.ToString( ) );

                                if ( _FileTransformation.EventData.Exception != null )
                                    _StreamWriter.WriteLine( "\t\t\t Error: " + _FileTransformation.EventData.Exception.Message );
                            }
                        }
                    }

                    _StreamWriter.WriteLine( );
                    _StreamWriter.WriteLine( );
                }
            }
        }

        internal static void Commit( String String = null )
        {
            lock ( _ThreadLock )
            {
                using ( StreamWriter _StreamWriter = _CreateOrOpenFile( ) )
                {
                    _StreamWriter.WriteLine( String );
                }
            }
        }
    }
}
