using System;
using System.Collections.Generic;
using XPDF.Model.Interface;

namespace XPDF.Model.Event
{
    internal class FileConversionQueue
    {
        private readonly Queue<IFileInformation> _FileQueue       = new Queue<IFileInformation>( );
        private readonly Object                  _QueueLockObject = new Object( );

        public int Added
        {
            get;
            private set;
        }

        public int Count
        {
            get
            {
                lock ( _QueueLockObject )
                    return _FileQueue.Count;
            }
        }

        public IFileInformation Current
        {
            get;
            private set;
        }

        internal bool EnQueueFile( IFileInformation FileInformation )
        {
            lock ( _QueueLockObject )
            {
                if ( FileInformation is null )
                    return false;

                Added++;

                _FileQueue.Enqueue( FileInformation );

                return true;
            }
        }

        public FileConversionQueue( )
        {
            Added         = 0;
            ItteratedOver = 0;
        }

        internal IFileInformation GetNext( )
        {
            lock( _QueueLockObject )
            {
                if ( _FileQueue.Count < 1 )
                    return null;

                ItteratedOver++;

                Previous = Current;
                Current  = _FileQueue.Dequeue( );

                return Current;
            }
        }

        public int ItteratedOver
        {
            get;
            private set;

        }

        public float PercentItterated
        {
            get
            {
                return ( Added > 0 ) ? ( float )ItteratedOver / ( float )Added : 1;
            }
        }

        public IFileInformation Previous
        {
            get;
            private set;
        }

        internal void Reset( )
        {
            lock ( _QueueLockObject )
            {
                _FileQueue.Clear( );

                Added         = 0;
                ItteratedOver = 0;
            }
        }
    }
}
