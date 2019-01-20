using System.Collections.Generic;
using XPDF.Model.Enums;

namespace XPDF.Model.Interface
{
    internal interface IFormatConverter
    {
        IEnumerable<EFormat> InputFormats { get; }

        IEnumerable<EFormat> OutputFormats { get; }
    }
}
