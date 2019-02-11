using System;
using XPDF.Model.Event;
using XPDF.Model.Event.Enums;

namespace XPDF.Model.Interface
{
    internal interface IXPDFConversionManager : IFileConverter, IXMLConverter
    {
        void Abort( );

        void ConvertAll( string PathToSourceDirectory, string PathToDestinationDirectory );

        event EventHandler<FileConversionUpdate> FileConversionUpdateEvent;

        long NumberProcessed { get; }

        long NumberToProcess { get; }
        
        float PercentCompleted { get; }

        EXPDFConverterState State { get; }
        bool Aborting { get; }

        event EventHandler<StateEventArgs<EXPDFConverterState>> StateChangedEvent;
    }
}
