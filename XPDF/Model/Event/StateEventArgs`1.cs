using System;
using XPDF.Model.Event.Enums;

namespace XPDF.Model.Event
{
    internal class StateEventArgs<T> : StateEventArgs
    {
        internal T Subject
        {
            get;
        }

        internal StateEventArgs( T Subject, ESourceState SubjectState = ESourceState.Stable, Exception Exception = null ) : base( SubjectState, Exception )
        {
            this.Subject = Subject;
        }
    }
}
