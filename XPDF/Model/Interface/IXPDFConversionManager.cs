using System;
using XPDF.Model.Event;
using XPDF.Model.Event.Enums;
using XPDF.Model.Event.Interface;

namespace XPDF.Model.Interface
{
    internal interface IXPDFConversionManager : IFormatConverter
    {
        void Abort( );

        void ConvertAll( string PathToSourceDirectory, string PathToDestinationDirectory );

        event EventHandler<StateChangeEventArgs<IProgressUpdate>>     ProgressUpdateEvent;

        EXPDFConverterState State { get; }

        event EventHandler<StateChangeEventArgs<EXPDFConverterState>> StateChangedEvent;
    }
}
