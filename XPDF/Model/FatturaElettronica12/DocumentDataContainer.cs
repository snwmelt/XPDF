using FatturaElettronica.Common;
using XPDF.Model.FatturaElettronica12.Enums;

namespace XPDF.Model.FatturaElettronica12
{
    internal class DocumentDataContainer
    {
        public DocumentDataContainer( DatiDocumento Document, EDocumentDataReferenceType ReferenceType )
        {
            this.Document = Document;
            this.ReferenceType = ReferenceType;
        }

        public EDocumentDataReferenceType ReferenceType
        {
            get;
        }

        public DatiDocumento Document
        {
            get;
        }
    }
}
