using System.Collections.Generic;
using XPDF.Model.Event.Interface;
using XPDF.Model.Interface;

namespace XPDF.Model.Event
{
    internal class FileConversionUpdate : IProgressUpdate<IFileInformation>
    {
        private int             _CurrentIndex;
        private readonly object _ThreadLock = new object( );

        public FileConversionUpdate ( IList<IFileInformation> Items )
        {
            this.Items    = Items ?? new List<IFileInformation>( );
            _CurrentIndex = 0;
        }

        public void IncrementProgress( )
        {
            if ( !Completed )
            {
                lock ( _ThreadLock )
                {
                    LastItem = Items[ _CurrentIndex ];

                    if ( _CurrentIndex + 1 < Items.Count )
                    {
                        _CurrentIndex++;
                        NextItem = Items[ _CurrentIndex ];
                    }
                }
            }
        }

        public void Reset( )
        {
            lock ( _ThreadLock )
            {
                _CurrentIndex = 0;
            }
        }

        public IList<IFileInformation> Items
        {
            get;
            set;
        }

        public IFileInformation LastItem
        {
            get;
            private set;
        }

        public IFileInformation NextItem
        {
            get;
            private set;
        }

        public float PercentComplete
        {
            get
            {
                lock ( _ThreadLock )
                {
                    return ( Items == null ) ? -1 : ( Items.Count > 0 ) ? _CurrentIndex / Items.Count : 1;
                }
            }
        }

        public bool Completed
        {
            get
            {
                lock ( _ThreadLock )
                {
                    return Items == null || Items.Count == 0 || Items[ Items.Count - 1 ] == LastItem;
                }
            }
        }
    }
}
