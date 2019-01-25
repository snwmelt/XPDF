using System;

namespace XPDF.Model.Interface
{
    internal interface IXMLConverter
    {
        IFormatInformation[] SupportedFormats { get; }

        Boolean IsValidXML( String InputXML );
    }
}
