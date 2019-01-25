using System;
using System.IO;

namespace XPDF.Model.Interface
{
    internal interface IFileInformation
    {
        String FallbackPath { get; }

        FileFormat FormatInformation { get; }

        Uri Path { get; }
    }
}
