using System.Collections.Generic;
using XPDF.Model.Interface;

namespace XPDF.Model.Event.Interface
{
    internal interface IFileConversionUpdate
    {
        bool AddTransformation( FileTransformation Transformation );

        bool Complete { get; }

        IFileInformation Original { get; }

        IEnumerable<FileTransformation> Transformations { get; }
    }
}