using System;
using System.Collections.Generic;
using XPDF.Model.Event.Enums;
using XPDF.Model.Event.Interface;
using XPDF.Model.Interface;

namespace XPDF.Model.Event
{
    internal class FileConversionUpdate : EventArgs, IFileConversionUpdate
    {
        private HashSet<FileTransformation> _Transformations;

        
        public Boolean AddTransformation( FileTransformation Transformation )
        {
            return _Transformations.Add( Transformation );
        }

        public Boolean AddTransformation( EFileTransformation Transformation, IFileInformation Result, StateEventArgs EventData = default)
        {
            return AddTransformation ( Transformation, Original, Result, EventData );
        }
        
        public Boolean AddTransformation( EFileTransformation Transformation, IFileInformation Source, IFileInformation Result, StateEventArgs EventData = default )
        {
            return _Transformations.Add( new FileTransformation( Transformation, Source, Result, EventData ) );
        }

        public bool Complete
        {
            get;
            set;
        }

        public FileConversionUpdate( IFileInformation Original )
        {
            _Transformations = new HashSet<FileTransformation>( );

            this.Original = Original;
        }

        public IFileInformation Original
        {
            get;
        }

        public IEnumerable<FileTransformation> Transformations
        {
            get
            {
                return _Transformations;
            }
        }
    }
}
