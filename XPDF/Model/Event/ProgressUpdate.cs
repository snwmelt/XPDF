using System;
using System.Collections.Generic;
using XPDF.Model.Event.Interface;
using XPDF.Model.Interface;

namespace XPDF.Model.Event
{
    internal class ProgressUpdate : IProgressUpdate<IFormatInformation>
    {
        public ProgressUpdate( Boolean Completed )
        {
            this.Completed = Completed;
        }

        public IList<IFormatInformation> Items => throw new System.NotImplementedException( );

        public IFormatInformation LastItem => throw new System.NotImplementedException( );

        public IFormatInformation NextItem => throw new System.NotImplementedException( );

        public float PercentComplete => throw new System.NotImplementedException( );

        public bool Completed
        {
            get;
        }
    }
}
