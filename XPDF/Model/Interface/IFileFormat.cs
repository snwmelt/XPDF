using XPDF.Model.Enums;

namespace XPDF.Model.Interface
{
    internal interface IFileFormat : IFormatInformation
    {
        EFileExtension FileExtension { get; }
    }
}
