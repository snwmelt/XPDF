using System;
using XPDF.Model.Event.Enums;

namespace XPDF.Model.Event
{
    internal class StateEventArgs : EventArgs 
    {
        internal Exception Exception
        {
            get;
        }

        internal ESourceState SubjectState
        {
            get;
        }

        internal StateEventArgs( ESourceState SubjectState = ESourceState.Stable, Exception Exception = null )
        {
            this.Exception    = Exception;
            this.SubjectState = SubjectState;
        }
    }
}
