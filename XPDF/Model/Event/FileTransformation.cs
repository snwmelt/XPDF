using XPDF.Model.Event.Enums;
using XPDF.Model.Interface;

namespace XPDF.Model.Event
{
    internal sealed class FileTransformation
    {
        internal StateEventArgs EventData
        {
            get;
        }

        public FileTransformation( EFileTransformation Transformation, IFileInformation Source, IFileInformation Result, StateEventArgs EventData = null )
        {
            this.EventData      = EventData;
            this.Result         = Result;
            this.Source         = Source;
            this.Transformation = Transformation;
        }

        internal IFileInformation Result
        {
            get;
        }

        internal IFileInformation Source
        {
            get;
        }

        internal EFileTransformation Transformation
        {
            get;
        }
    }
}
