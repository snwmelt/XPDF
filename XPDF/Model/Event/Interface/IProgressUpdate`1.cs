using System.Collections.Generic;

namespace XPDF.Model.Event.Interface
{
    interface IProgressUpdate<T> : IProgressUpdate
    {
        IList<T> Items { get; }

        T LastItem { get; }

        T NextItem { get; }
    }
}
