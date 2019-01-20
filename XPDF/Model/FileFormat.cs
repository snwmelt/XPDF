using System;
using Walkways.Extensions.Attributes;
using XPDF.Model.Enums;
using XPDF.Model.Interface;

namespace XPDF.Model
{
    internal class FileFormat : IFileFormat
    {
        public FileFormat( EFileExtension FileExtension, EFormat Format, String Version )
        {
            this.FileExtension = FileExtension;
            this.Format        = Format;
            this.Version       = Version;
        }

        public EFileExtension FileExtension
        {
            get;
        }
        
        public EFormat Format
        {
            get;
        }

        public String FormatName
        {
            get
            {
                return Format.GetDescription( );
            }
        }

        public String Version
        {
            get;
        }
    }
}
