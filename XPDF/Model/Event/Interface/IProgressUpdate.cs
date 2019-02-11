using System;

namespace XPDF.Model.Event.Interface
{
    internal interface IProgressUpdate
    {
        Boolean Completed { get; }

        float PercentComplete { get; }
    }
}