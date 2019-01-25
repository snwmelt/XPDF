using System.Collections.Generic;
using XPDF.Model.Enums;

namespace XPDF.Model.Interface
{
    internal interface IFormatConverter<I, O>
    {
        IEnumerable<EFormat> InputFormats { get; }

        IEnumerable<EFormat> OutputFormats { get; }

        O Convert( I Input );
    }
}
