using System;

namespace XPDF.Model.Enums
{
    internal static class FiscalCodeID
    {
        public static String ToString( String code )
        {
            switch ( code )
            {
                case "03516130154":
                    return "ACQUANOVA";

                case "08591950962":
                    return "BICTOWER";

                case "07238040963":
                    return "CC_032HOLDING";

                case "06810510963":
                    return "FENICE_0322018";

                case "06080630962":
                    return "CIAI";

                case "09669720964":
                    return "CINDRE";

                case "06440260963":
                    return "CLADDING";

                case "10301770961":
                    return "CR2_CIERREDUE";

                case "13439340152":
                    return "CMS";

                case "12965710150":
                    return "COIBENTAZIONI";

                case "05691440969":
                    return "CENTRO_032CONTRACT";

                case "05485740962":
                    return "SIGN";

                case "12965720159":
                    return "CONTRACT";

                case "00628430159":
                    return "FERLEGNO";

                case "09647740969":
                    return "FLAGS";

                case "05352660962":
                    return "FLENG";

                case "10162560964":
                    return "LUX";

                case "03593150125 ":
                    return "S5_032RAPALLO";

                case "05455920966":
                    return "HOLDING";

                case "09456390963":
                    return "I18";

                case "01982090761":
                    return "LUCANIA_032S.I.";

                case "06747950969":
                    return "MAC10_032HOTEL";

                case "13439320154":
                    return "PRAESIDIA";

                case "09697700962":
                    return "SVILUPPO_03210_032RE";

                default:
                    return code;
            }
        }
    }
}
