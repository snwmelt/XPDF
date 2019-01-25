using System;

namespace XPDF.Model.Interface
{
    internal interface IXPDFFIleConverter : IFileConverter, IDisposable, IXMLConverter
    {
        void Abort( );
    }
}
