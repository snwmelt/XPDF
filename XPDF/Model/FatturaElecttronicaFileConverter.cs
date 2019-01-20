using System.Collections.Generic;
using XPDF.Model.Enums;
using XPDF.Model.Interface;

namespace XPDF.Model
{
    internal class FatturaElecttronicaFileConverter : IXPDFFIleConverter
    {
        #region Private Variables



        #endregion


        public void Abort( )
        {
            //throw new System.NotImplementedException( );
        }

        public void Convert( string InputXMLFilePath, string OutputPDFPath )
        {
            throw new System.NotImplementedException( );
        }

        public void Dispose( )
        {
            throw new System.NotImplementedException( );
        }

        public IEnumerable<EFormat> InputFormats
        {
            get
            {
                return new EFormat[]
                {
                    EFormat.XMLPA
                };
            }
        }

        public bool IsValidXML( string InputXMLFilePath )
        {
            throw new System.NotImplementedException( );
        }

        public IEnumerable<EFormat> OutputFormats
        {
            get
            {
                return new EFormat[]
                {
                    EFormat.PDF
                };
            }
        }

        public IEnumerable<EFileExtension> SupportedFileExtensions
        {
            get
            {
                return new EFileExtension[]
                {
                    EFileExtension.XML
                };
            }
        }
    }
}
