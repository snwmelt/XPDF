using System;

namespace XPDF.Model.Event
{
    /// <summary>
    /// An EventArgs class for a change in object state.
    /// </summary>
    /// <typeparam name="T">Type specifier.</typeparam>
    internal class StateChangeEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Current object state.
        /// </summary>
        public T CurrentState
        {
            private set;
            get;
        }

        /// <summary>
        /// Previous object state.
        /// </summary>
        public T PreviousState
        {
            private set;
            get;
        }

        /// <summary>
        /// An exception, will be null if no exceptions were raised.
        /// </summary>
        public Exception Exception
        {
            private set;
            get;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="PreviousState">Object's previous state.</param>
        /// <param name="CurrentState">Object's current state.</param>
        /// <param name="Exception">Raised exception.</param>
        public StateChangeEventArgs( T CurrentState, T PreviousState = default( T ), Exception Exception = null )
        {
            this.CurrentState = CurrentState;
            this.Exception = Exception;
            this.PreviousState = PreviousState;
        }
    }
}
