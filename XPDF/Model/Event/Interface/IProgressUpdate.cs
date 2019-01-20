using System;

namespace XPDF.Model.Event.Interface
{
    internal interface IProgressUpdate
    {
        float PercentComplete { get; }
        Boolean Completed { get; }
    }
}