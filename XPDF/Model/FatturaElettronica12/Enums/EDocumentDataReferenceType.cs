using System.ComponentModel;

namespace XPDF.Model.FatturaElettronica12.Enums
{
    internal enum EDocumentDataReferenceType
    {
        [Description( "Contratto" )]
        DatiContratto,
        [Description( "Convenzione" )]
        DatiConvenzione,
        [Description( "Fattura Connessa" )]
        DatiFattureCollegate,
        [Description( "Ordine/Acquisto" )]
        DatiOrdineAcquisto,
        [Description( "Ricezione" )]
        DatiRicezione
    }
}
