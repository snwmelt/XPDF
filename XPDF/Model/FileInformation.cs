using System;
using Walkways.Extensions.Enum;
using XPDF.Model.Enums;
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

        public FileInformation( FileFormat FormatInformation, String Path, String FallbackPath = null ) : this( FormatInformation, new Uri( Path ), FallbackPath )
        {
        }

        public FileInformation( Uri Path )
        {
            this.Path = Path;

            FormatInformation = new FileFormat( EnumConversionExtensions.FromString<EFileExtension>( System.IO.Path.GetExtension( Path.LocalPath ).Replace( ".", "" ), true ), EFormat.Uknown, "" );
            FallbackPath      = null;
        }


        public String Directory
        {
            get
            {
                return System.IO.Path.GetDirectoryName( Path.LocalPath );
            }
        }

        public String ExtensionlessFileName
        {
            get
            {
                return System.IO.Path.GetFileNameWithoutExtension( Path.LocalPath );
            }
        }

        public String FileName
        {
            get
            {
                return System.IO.Path.GetFileName( Path.LocalPath );
            }
        }

        public FileInformation( String Path ) : this( new Uri( Path ) )
        {
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
