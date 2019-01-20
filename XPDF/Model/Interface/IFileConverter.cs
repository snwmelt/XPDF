using System.Collections.Generic;
using XPDF.Model.Enums;

namespace XPDF.Model.Interface
{
    internal interface IFileConverter : IFormatConverter
    {
        IEnumerable<EFileExtension> SupportedFileExtensions { get; }
    }
}