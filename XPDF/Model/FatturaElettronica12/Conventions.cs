using System;

namespace XPDF.Model.FatturaElettronica12
{
    internal static class Conventions
    {
        public static String Header
        {
            get
            {
                return "<p:FatturaElettronica versione=\"FPR12\" xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://ivaservizi.agenziaentrate.gov.it/docs/xsd/fatture/v1.2 fatturaordinaria_v1.2.xsd\" xmlns:p=\"http://ivaservizi.agenziaentrate.gov.it/docs/xsd/fatture/v1.2\">";
            }
        }

        public static String Footer
        {
            get
            {
                return @"</p:FatturaElettronica>";
            }
        }
    }
}
