using System;
using System.IO;

namespace XPDF.Model.Interface
{
    internal interface IFileInformation
    {
        String Directory { get; }

        String ExtensionlessFileName { get; }

        String FallbackPath { get; }

        String FileName { get; }

        FileFormat FormatInformation { get; }

        Uri Path { get; }
    }
}
