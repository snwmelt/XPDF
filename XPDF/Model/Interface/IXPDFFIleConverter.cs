using System;

namespace XPDF.Model.Interface
{
    internal interface IXPDFFIleConverter : IFileConverter, IDisposable
    {
        void Abort( );

        void Convert( string InputXMLFilePath, string OutputPDFPath );

        Boolean IsValidXML( string InputXMLFilePath );

    }
}
