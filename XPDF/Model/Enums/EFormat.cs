using System.ComponentModel;

namespace XPDF.Model.Enums
{
    internal enum EFormat
    {
        [Description( "PKCS #7 MIME" )]
        P7M,
        [Description( "PDF" )]
        PDF,
        [Description( "XMLPA" )]
        XMLPA,
        [Description( "UNKNOWN" )]
        Uknown
    }
}
