using System;
using XPDF.Model.Enums;

namespace XPDF.Model.Interface
{
    interface IFormatInformation
    {
        EFormat Format { get; }

        String FormatName { get; }

        String Version { get; }
    }
}
