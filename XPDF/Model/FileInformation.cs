using System;
using XPDF.Model.Interface;

namespace XPDF.Model
{
    internal class FileInformation : IFileInformation
    {
        public FileInformation( FileFormat FormatInformation, Uri Path, String FallbackPath = null )
        {
            this.FormatInformation = FormatInformation;
            this.Path              = Path;
            this.FallbackPath      = FallbackPath;
        }

        public String FallbackPath
        {
            get;
        }

        public FileFormat FormatInformation
        {
            get;
        }

        public Uri Path
        {
            get;
        }
    }
}
