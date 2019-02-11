using FatturaElettronica;
using FatturaElettronica.Common;
using FatturaElettronica.FatturaElettronicaBody;
using FatturaElettronica.FatturaElettronicaBody.DatiBeniServizi;
using FatturaElettronica.FatturaElettronicaBody.DatiGenerali;
using FatturaElettronica.FatturaElettronicaBody.DatiPagamento;
using FatturaElettronica.FatturaElettronicaHeader;
using FatturaElettronica.FatturaElettronicaHeader.CedentePrestatore;
using FatturaElettronica.FatturaElettronicaHeader.CessionarioCommittente;
using FatturaElettronica.Tabelle;
using FatturaElettronica.Validators;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using Walkways.Extensions.Attributes;
using XPDF.Model.Enums;
using XPDF.Model.FatturaElettronica12.Enums;
using XPDF.Model.Interface;
using XPDF.Model.Localization;

namespace XPDF.Model.FatturaElettronica12
{
    internal class FatturaElecttronicaFileConverter : IFileConverter, IXMLConverter
    {
        #region Private Variables

        Fattura                    _FatturaDocument     = null;
        readonly BaseFont          _BaseFontHelvetica   = BaseFont.CreateFont( BaseFont.HELVETICA, BaseFont.CP1252, true );
        readonly Font              _BodyHelvetica       = null;
        Color                      _BorderColour        = Color.LIGHT_GRAY;
        readonly float             _BorderWidth         = 1.4f;
        readonly float             _CellPaddingTop      = 4f;
        readonly Boolean           _CellUseAscender     = true;
        readonly XmlReaderSettings _XmlReaderSettings   = new XmlReaderSettings { IgnoreWhitespace = true, IgnoreComments = true };
        Color                      _HeaderColour        = Color.LIGHT_GRAY;
        readonly Font              _HeaderHelvetica     = null;
        readonly Font              _HeaderHelveticaDark = null;
        readonly Font              _TitleHelvetica      = null;
        readonly FatturaValidator  _Validator           = new FatturaValidator( );

        #endregion

        public FatturaElecttronicaFileConverter( )
        {
            _HeaderHelvetica     = new Font( _BaseFontHelvetica, 8,  Font.NORMAL, Color.LIGHT_GRAY );
            _HeaderHelveticaDark = new Font( _BaseFontHelvetica, 8,  Font.NORMAL, Color.DARK_GRAY );
            _BodyHelvetica       = new Font( _BaseFontHelvetica, 8,  Font.NORMAL, Color.BLACK );
            _TitleHelvetica      = new Font( _BaseFontHelvetica, 10, Font.NORMAL, Color.BLACK );
        }

        public IFileInformation Convert( IFileInformation Input )
        {
            using ( XmlReader _XmlReader = XmlReader.Create( new StringReader( File.ReadAllText( Input.Path.LocalPath ) ), _XmlReaderSettings ) )
            {
                _FatturaDocument = new Fattura( );

                _FatturaDocument.ReadXml( _XmlReader );
            }


            Document _PDFDocument = OpenPDFDoc( 25.0f, 25.0f, Input ); 
            

            _PDFDocument.Add( GeneratePDFHeader( _FatturaDocument.FatturaElettronicaHeader ) );

            for ( int i = 0; i < _FatturaDocument.FatturaElettronicaBody.Count; i++ )
            {
                AddInvoiceBodyToPDFPage( _FatturaDocument.FatturaElettronicaBody[ i ], _PDFDocument, i );
            }

            String _FileName = _XMLPAPDFFileName( );

            _PDFDocument.Add( new Paragraph( "\n" + _FileName, _HeaderHelvetica ) );


            _PDFDocument.Close( );

            return new FileInformation( new FileFormat( EFileExtension.PDF, EFormat.PDF, "1.4" ), 
                                        new Uri( Input.Path.LocalPath + ".pdf" ),
                                        Input.Directory + "\\" + _FileName.Replace( @"/", "" ) + ".pdf" );
        }

        private string _XMLPAPDFFileName( )
        {
            int Year  = _FatturaDocument.FatturaElettronicaBody[ 0 ].DatiGenerali.DatiGeneraliDocumento.Data.Year;
            int Month = _FatturaDocument.FatturaElettronicaBody[ 0 ].DatiGenerali.DatiGeneraliDocumento.Data.Month;
            int Day   = _FatturaDocument.FatturaElettronicaBody[ 0 ].DatiGenerali.DatiGeneraliDocumento.Data.Day;
            
            String FiscalCodeIDString = FiscalCodeID.ToString( _FatturaDocument.FatturaElettronicaHeader.CessionarioCommittente.DatiAnagrafici.IdFiscaleIVA.IdCodice );
            String RecipientName      = TryGetParticipantName( _FatturaDocument.FatturaElettronicaHeader.CedentePrestatore.DatiAnagrafici.Anagrafica );

            return @"FATT;" +
                   _FatturaDocument.FatturaElettronicaBody[ 0 ].DatiGenerali.DatiGeneraliDocumento.Numero + // Number
                   ";AZIENDA;" +
                   FiscalCodeIDString +                                                          // sender fiscal id code ! Convert
                   ";DEL;" +
                   Year +                                                                        // Year
                   ( ( Month < 10 ) ? "0" + Month.ToString( ) : Month.ToString( ) ) +            // Month
                   ( ( Day < 10 )   ? "0" + Day.ToString( ) : Day.ToString( ) ) +                // Day
                   ";CLIENTE;" +
                   RecipientName?.Replace( " ", "_032" ) +                                          // AddressName ! _O32 for spaces
                   ";PIVA;" +
                   _FatturaDocument.FatturaElettronicaHeader.CedentePrestatore.DatiAnagrafici.IdFiscaleIVA.IdCodice; // client fiscal id code
        }

        private Document OpenPDFDoc( float BottomTopMargin, float LeftRightMargin, IFileInformation Input )
        {
            Document _PDFDocument = new Document( PageSize.LETTER, LeftRightMargin, LeftRightMargin, BottomTopMargin, BottomTopMargin );

            PdfWriter.GetInstance( _PDFDocument, new FileStream( Input.Path.LocalPath + ".pdf", FileMode.OpenOrCreate ) );

            _PDFDocument.Open( );

            return _PDFDocument;
        }

        private void AddInvoiceBodyToPDFPage( FatturaElettronicaBody _FatturaElettronicaBody, Document _Document, int _PageNumber )
        {
            AddGeneralDocumentData( _FatturaElettronicaBody.DatiGenerali.DatiGeneraliDocumento, _Document );

            AddExternalDocumentReferences( _FatturaElettronicaBody.DatiGenerali, _Document );
            
            List<DettaglioLinee> _InvoiceDetails         = _FatturaElettronicaBody.DatiBeniServizi.DettaglioLinee;
            List<DatiRiepilogo>  _Summeries              = _FatturaElettronicaBody.DatiBeniServizi.DatiRiepilogo;
            List<DatiPagamento>  _PaymentInformationList = _FatturaElettronicaBody.DatiPagamento;

            TryAddInvoiceDetails( _InvoiceDetails, _Document );
            TryAddGeneralSummery( _Summeries, _Document );
            TryAddPaymentInformation( _PaymentInformationList, _Document );
            TryAddCostSummery( _Summeries, _FatturaElettronicaBody.DatiGenerali.DatiGeneraliDocumento.Divisa, _Document );
        }

        private void TryAddCostSummery( List<DatiRiepilogo> _Summeries, String _Divisa, Document _Document )
        {
            if ( _Summeries == null || _Summeries.Count < 1 )
                return;


            PdfPTable _Container              = new PdfPTable( 1 );
            PdfPTable _CostSummeryTable       = new PdfPTable( 2 );
            PdfPTable _CostSummeryHeaderTable = new PdfPTable( 1 );
            PdfPTable _CostSummeryValuesTable = new PdfPTable( 1 );


            _Container.DefaultCell.Border  = Rectangle.NO_BORDER;
            _Container.WidthPercentage     = 33;
            _Container.HorizontalAlignment = Element.ALIGN_RIGHT;


            _CostSummeryTable.DefaultCell.Border = Rectangle.NO_BORDER;
            _CostSummeryTable.DefaultCell.BorderWidth = 0;

            _CostSummeryHeaderTable.DefaultCell.BorderColor         = _BorderColour;
            _CostSummeryHeaderTable.DefaultCell.BorderWidth         = _BorderWidth;
            _CostSummeryHeaderTable.DefaultCell.BackgroundColor     = _BorderColour;
            _CostSummeryHeaderTable.DefaultCell.HorizontalAlignment = Element.ALIGN_RIGHT;

            _CostSummeryValuesTable.DefaultCell.BorderColor         = _BorderColour;
            _CostSummeryValuesTable.DefaultCell.BorderWidth         = _BorderWidth;
            _CostSummeryValuesTable.DefaultCell.HorizontalAlignment = Element.ALIGN_RIGHT;


            _CostSummeryTable.AddCell( _CostSummeryHeaderTable );
            _CostSummeryTable.AddCell( _CostSummeryValuesTable );

            _Container.AddCell( new Phrase( ) );
            _Container.AddCell( new Phrase( ) );
            _Container.AddCell( new Phrase( ) );
            _Container.AddCell( _CostSummeryTable );


            String _CurrencySymbol = TryGetCurrencySymbol( _Divisa );

            _CostSummeryHeaderTable.AddCell( new Phrase( LocalisedString.Total + " " + LocalisedString.Taxable, _BodyHelvetica ) );
            _CostSummeryHeaderTable.AddCell( new Phrase( LocalisedString.Total + " " + LocalisedString.Tax, _BodyHelvetica ) );
            _CostSummeryHeaderTable.AddCell( new Phrase( LocalisedString.Total + " " + LocalisedString.Exempt, _BodyHelvetica ) );
            _CostSummeryHeaderTable.AddCell( new Phrase( LocalisedString.Total + " " + LocalisedString.AmountDue, _BodyHelvetica ) );
            
            Decimal _Tax       = 0.0M;
            Decimal _Taxable   = 0.0M;
            Decimal _TaxExempt = 0.0M;

            for ( int i = 0; i < _Summeries.Count; i++ )
            {
                if ( _Summeries[i].AliquotaIVA.Equals(0M) )
                {
                    _TaxExempt += _Summeries[ i ].ImponibileImporto;
                }
                else
                {
                    _Taxable += _Summeries[ i ].ImponibileImporto;
                    _Tax     += _Summeries[ i ].Imposta;
                }
            }

            Decimal _TotalDue = _Taxable + _TaxExempt + _Tax;

            _CostSummeryValuesTable.AddCell( new Phrase( _CurrencySymbol + GetNullableString( _Taxable ), _BodyHelvetica ) );
            _CostSummeryValuesTable.AddCell( new Phrase( _CurrencySymbol + GetNullableString( _Tax ), _BodyHelvetica ) );
            _CostSummeryValuesTable.AddCell( new Phrase( _CurrencySymbol + GetNullableString( _TaxExempt ), _BodyHelvetica ) );
            _CostSummeryValuesTable.AddCell( new Phrase( _CurrencySymbol + GetNullableString( _TotalDue ), _BodyHelvetica ) );


            _Document.Add( _Container );
        }

        private String TryGetCurrencySymbol( String _Divisa )
        {
            String Symbol = null;
            Symbol = CultureInfo
                     .GetCultures( CultureTypes.AllCultures )
                     .Where( c => !c.IsNeutralCulture )
                     .Select( culture => {
                         try
                         {
                             return new RegionInfo( culture.LCID );
                         }
                         catch
                         {
                             return null;
                         }
                     } )
                     .Where( ri => ri != null && ri.ISOCurrencySymbol.ToLowerInvariant( ) == _Divisa.ToLowerInvariant( ) )
                     .Select( ri => ri.CurrencySymbol )
                     .FirstOrDefault( );

            if ( Symbol != null )
                return Symbol + " ";

            return "";
        }

        private void TryAddPaymentInformation( List<DatiPagamento> PaymentInformationList, Document _Document )
        {
            if ( PaymentInformationList == null || PaymentInformationList.Count < 1 )
                return;

            PdfPTable _PaymentInformationTable = CreateBodyPdfPTable( new String[]
            {
                LocalisedString.Beneficiary,
                LocalisedString.Type,
                LocalisedString.Mode,
                LocalisedString.EndDate,
                LocalisedString.PaymentDates,
                LocalisedString.ExpiryDate,
                LocalisedString.Amount
            } );


            for ( int i = 0; i < PaymentInformationList.Count; i++ )
            {
                List<DettaglioPagamento> PaymentDetails = PaymentInformationList[ i ].DettaglioPagamento;

                if ( PaymentDetails != null && PaymentDetails.Count > 0 )
                {
                    for ( int j = 0; j < PaymentDetails.Count; j++ )
                    {
                        _PaymentInformationTable.AddCell( new Paragraph( GetNullableString( PaymentDetails[ j ].Beneficiario ), _BodyHelvetica ) );
                        _PaymentInformationTable.AddCell( new Paragraph( PaymentConditionsCodeToString( PaymentInformationList[ i ].CondizioniPagamento ), _BodyHelvetica ) );
                        _PaymentInformationTable.AddCell( new Paragraph( PaymentModeCodeToString( PaymentDetails[ j ].ModalitaPagamento ), _BodyHelvetica ) );
                        _PaymentInformationTable.AddCell( new Paragraph( " ", _BodyHelvetica ) );
                        _PaymentInformationTable.AddCell( new Paragraph( " ", _BodyHelvetica ) );
                        _PaymentInformationTable.AddCell( new Paragraph( GetNullableString( PaymentDetails[ j ].DataScadenzaPagamento ), _BodyHelvetica ) );
                        _PaymentInformationTable.AddCell( new Paragraph( GetNullableString( PaymentDetails[ j ].ImportoPagamento ), _BodyHelvetica ) );

                        if ( !String.IsNullOrEmpty( PaymentDetails[ j ].IstitutoFinanziario ) )
                        {
                            _PaymentInformationTable.AddCell( GetInstitutionSpanCell( PaymentDetails[ j ] ) );
                        }
                    }
                }
                else
                {
                    _PaymentInformationTable.AddCell( new Paragraph( " ", _BodyHelvetica ) );
                    _PaymentInformationTable.AddCell( new Paragraph( PaymentConditionsCodeToString( PaymentInformationList[ i ].CondizioniPagamento ), _BodyHelvetica ) );
                    _PaymentInformationTable.AddCell( new Paragraph( " ", _BodyHelvetica ) );
                    _PaymentInformationTable.AddCell( new Paragraph( " ", _BodyHelvetica ) );
                    _PaymentInformationTable.AddCell( new Paragraph( " ", _BodyHelvetica ) );
                    _PaymentInformationTable.AddCell( new Paragraph( " ", _BodyHelvetica ) );
                    _PaymentInformationTable.AddCell( new Paragraph( " ", _BodyHelvetica ) );
                }
            }

            _Document.Add( GenerateBodyTable( new Paragraph( LocalisedString.PaymentInformation, _HeaderHelvetica ),
                                              new PdfPCell( _PaymentInformationTable ) ) );
        }

        private PdfPCell GetInstitutionSpanCell( DettaglioPagamento _PaymentDetails )
        {
            PdfPCell InstitutionSpanCell = new PdfPCell
            {
                UseAscender         = _CellUseAscender,
                PaddingTop          = _CellPaddingTop,
                Colspan             = 7,
                BorderColor         = _BorderColour,
                BorderWidth         = _BorderWidth,
                VerticalAlignment   = Element.ALIGN_TOP,
                HorizontalAlignment = Element.ALIGN_LEFT
            };

            Paragraph Content = new Paragraph( );

            String _Spacer = "\t";

            Content.Add( new Phrase( LocalisedString.Institution + ": " + _PaymentDetails.IstitutoFinanziario, _BodyHelvetica ) );
            Content.Add( _Spacer );

            if ( _PaymentDetails.IBAN != null )
            {
                Content.Add( new Phrase( LocalisedString.IBAN + ": " + _PaymentDetails.IBAN, _BodyHelvetica ) );
                Content.Add( _Spacer );
            }


            if ( _PaymentDetails.ABI != null )
            {
                Content.Add( new Phrase( LocalisedString.ABI + ": " + _PaymentDetails.ABI, _BodyHelvetica ) );
                Content.Add( _Spacer );
            }


            if ( _PaymentDetails.CAB != null )
            {
                Content.Add( new Phrase( LocalisedString.CAB + ": " +  _PaymentDetails.CAB, _BodyHelvetica ) );
                Content.Add( _Spacer );
            }

            InstitutionSpanCell.AddElement( Content );

            return InstitutionSpanCell;
        }

        private String PaymentModeCodeToString( String Code )
        {
            if ( String.IsNullOrEmpty( Code ) )
                return " ";

            return CodiceToNome( new ModalitaPagamento( ), Code );
        }

        private String PaymentConditionsCodeToString( String Code )
        {
            if ( String.IsNullOrEmpty( Code ) )
                return " ";

            return CodiceToNome( new CondizioniPagamento( ), Code );
        }

        private String DocumentTypeCodeToString( String Code )
        {
            if ( String.IsNullOrEmpty( Code ) )
                return " ";

            return CodiceToNome( new TipoDocumento( ), Code );
        }

        private void TryAddGeneralSummery( List<DatiRiepilogo> GeneralSummeriesList, Document _Document )
        {
            if ( GeneralSummeriesList == null || GeneralSummeriesList.Count < 1 )
                return;

            PdfPTable _GeneralSummeryTable = CreateBodyPdfPTable( new String[]
            {
                LocalisedString.Taxable,
                LocalisedString.VAT + "%",
                LocalisedString.Tax,
                LocalisedString.Nature,
                LocalisedString.NormativeReference,
                LocalisedString.Collectable
            } );

            _GeneralSummeryTable.SetWidths( new float[]
            {
                1f, // Taxable
                1f, // VAT %
                1f, // Tax
                1f, // Nature
                2f, // NormativeReference
                1f  // Collectable
            } );


            for ( int i = 0; i < GeneralSummeriesList.Count; i++ )
            {
                _GeneralSummeryTable.AddCell( new Paragraph( GetNullableString( GeneralSummeriesList[ i ].ImponibileImporto ), _BodyHelvetica ) );
                _GeneralSummeryTable.AddCell( new Paragraph( GetNullableString( GeneralSummeriesList[ i ].AliquotaIVA ), _BodyHelvetica ) );
                _GeneralSummeryTable.AddCell( new Paragraph( GetNullableString( GeneralSummeriesList[ i ].Imposta ), _BodyHelvetica ) );
                _GeneralSummeryTable.AddCell( new Paragraph( NaturaCodeToString( GeneralSummeriesList[ i ].Natura ), _BodyHelvetica ) );
                
                if ( !String.IsNullOrEmpty( GeneralSummeriesList[ i ].RiferimentoNormativo ) )
                {
                    _GeneralSummeryTable.AddCell( new Paragraph( GeneralSummeriesList[ i ].RiferimentoNormativo, _BodyHelvetica ) );
                }
                else
                {
                    _GeneralSummeryTable.AddCell( new Paragraph( " ", _BodyHelvetica ) );
                }

                _GeneralSummeryTable.AddCell( new Paragraph( VatExCodeToString( GeneralSummeriesList[ i ].EsigibilitaIVA ), _BodyHelvetica ) );
            }

            _Document.Add( GenerateBodyTable( new Paragraph( LocalisedString.GeneralSummery, _HeaderHelvetica ),
                                              new PdfPCell( _GeneralSummeryTable ) ) );
        }

        private String VatExCodeToString( String Code )
        {
            if ( String.IsNullOrEmpty( Code ) )
                return " ";
            
            return CodiceToNome( new EsigibilitaIVA( ), Code );
        }

        private String CodiceToNome( Tabella Table, String Code )
        {
            for ( int i = 0; i < Table.List.Length; i++ )
            {
                if ( Code == Table.List[ i ].Codice )
                    return Table.List[ i ].Nome;
            }

            return " ";
        }
        private String TipoCassaCodeToString( String Code )
        {
            if ( String.IsNullOrEmpty( Code ) )
                return " ";

            return CodiceToNome( new TipoCassa( ), Code );
        }


        private String NaturaCodeToString( String Code )
        {
            if ( String.IsNullOrEmpty( Code ) )
                return " ";

            return CodiceToNome( new Natura( ), Code );
        }

        private void TryAddInvoiceDetails( List<DettaglioLinee> InvoiceDetails, Document _Document )
        {
            if ( InvoiceDetails == null || InvoiceDetails.Count < 1 )
                return;

            PdfPTable _LineDetailsTable = CreateBodyPdfPTable( new String[]
            {
                LocalisedString.Line,
                LocalisedString.Cod,
                LocalisedString.Value,
                LocalisedString.Description,
                LocalisedString.QTA,
                LocalisedString.UM,
                LocalisedString.Price,
                LocalisedString.scmg,
                "%",
                LocalisedString.Val,
                LocalisedString.Amount,
                LocalisedString.VAT + "%"
            } );

            _LineDetailsTable.SetWidths( new float[] 
            {
                0.4f,  // Line
                1f,    // Cod
                1f,    // Value
                4f,    // Description
                0.75f, // QTA
                0.4f,  // UM
                1f,    // Price
                0.4f,  // scmg
                0.55f, // "%"
                0.55f, // Val
                1f,    // Amount
                0.55f  // VAT + "%"
            } );

            for ( int i = 0; i < InvoiceDetails.Count; i++ )
            {
                _LineDetailsTable.AddCell( new Paragraph( InvoiceDetails[ i ].NumeroLinea.ToString( ), _BodyHelvetica ) );

                if ( InvoiceDetails[i].CodiceArticolo != null && InvoiceDetails[i].CodiceArticolo.Count > 0 )
                {
                    _LineDetailsTable.AddCell( new Paragraph( InvoiceDetails[ i ].CodiceArticolo[ 0 ].CodiceTipo, _BodyHelvetica ) );
                    _LineDetailsTable.AddCell( new Paragraph( InvoiceDetails[ i ].CodiceArticolo[ 0 ].CodiceValore, _BodyHelvetica ) );
                }
                else
                {
                    _LineDetailsTable.AddCell( new Paragraph( " ", _BodyHelvetica ) );
                    _LineDetailsTable.AddCell( new Paragraph( " ", _BodyHelvetica ) );
                }

                _LineDetailsTable.AddCell( GenerateLineItemDescription( InvoiceDetails[ i ] ) );
                _LineDetailsTable.AddCell( new Paragraph( GetNullableString( InvoiceDetails[ i ].Quantita ), _BodyHelvetica ) );
                _LineDetailsTable.AddCell( new Paragraph( InvoiceDetails[ i ].UnitaMisura, _BodyHelvetica ) );
                _LineDetailsTable.AddCell( new Paragraph( GetNullableString( InvoiceDetails[ i ].PrezzoUnitario ), _BodyHelvetica ) );

                TryInsertBonusDiscountsValues( InvoiceDetails[ i ].ScontoMaggiorazione, _LineDetailsTable );
                
                _LineDetailsTable.AddCell( new Paragraph( GetNullableString( InvoiceDetails[ i ].PrezzoTotale ), _BodyHelvetica ) );
                _LineDetailsTable.AddCell( new Paragraph( GetNullableString( InvoiceDetails[ i ].AliquotaIVA ), _BodyHelvetica ) );
            }

            _Document.Add( GenerateBodyTable( new Paragraph( LocalisedString.InvoiceDetails, _HeaderHelvetica ),
                                              new PdfPCell( _LineDetailsTable ) ) );
        }

        private void TryInsertBonusDiscountsValues( List<FatturaElettronica.Common.ScontoMaggiorazione> _BonusDiscounsList, PdfPTable _LineDetailsTable )
        {
            Paragraph Scmg    = new Paragraph( );
            Paragraph Percent = new Paragraph( );
            Paragraph Val     = new Paragraph( );

            if ( _BonusDiscounsList != null && _BonusDiscounsList.Count > 0 )
            {
                for ( int i = 0; i < _BonusDiscounsList.Count; i++ )
                {
                    Scmg.Add( new Phrase( GetNullableString( _BonusDiscounsList[ i ].Tipo ) + " ", _BodyHelvetica ) );
                    Percent.Add( new Phrase( GetNullableString( _BonusDiscounsList[ i ].Percentuale ) + " ", _BodyHelvetica ) );
                    Val.Add( new Phrase( GetNullableString( _BonusDiscounsList[ i ].Importo ) + " ", _BodyHelvetica ) );
                }
            }

            _LineDetailsTable.AddCell( Scmg );
            _LineDetailsTable.AddCell( Percent );
            _LineDetailsTable.AddCell( Val );
        }

        private PdfPCell GenerateLineItemDescription( DettaglioLinee _LineItem )
        {
            PdfPCell DescriptionCell = new PdfPCell( );

            DescriptionCell.UseAscender         = _CellUseAscender;
            DescriptionCell.PaddingTop          = _CellPaddingTop;
            DescriptionCell.BorderColor         = _BorderColour;
            DescriptionCell.BorderWidth         = _BorderWidth;
            DescriptionCell.VerticalAlignment   = Element.ALIGN_TOP;
            DescriptionCell.HorizontalAlignment = Element.ALIGN_LEFT;

            Paragraph Content = new Paragraph( );
            String _Spacer = "\n";

            Content.Add( new Phrase( GetNullableString( _LineItem.Descrizione ), _BodyHelvetica ) );
            Content.Add( _Spacer );
            
            if ( _LineItem.DataInizioPeriodo != null )
            {
                Content.Add( new Phrase( LocalisedString.StartOfService + ": ", _HeaderHelvetica ) );
                Content.Add( new Phrase( GetNullableString( _LineItem.DataInizioPeriodo ), _BodyHelvetica ) );
                Content.Add( _Spacer );
            }

            if ( _LineItem.DataFinePeriodo != null )
            {
                Content.Add( new Phrase( LocalisedString.EndDate + ": ", _HeaderHelvetica ) );
                Content.Add( new Phrase( GetNullableString( _LineItem.DataFinePeriodo ), _BodyHelvetica ) );
                Content.Add( _Spacer );
            }

            if ( _LineItem.Natura != null )
            {
                Content.Add( new Phrase( NaturaCodeToString( _LineItem.Natura ), _HeaderHelvetica ) );
            }

            DescriptionCell.AddElement( Content );

            return DescriptionCell;
        }

        private IEnumerable<DocumentDataContainer> CollectionToContainerCollection<T>( List<T> _DocumentDataList, EDocumentDataReferenceType _ReferenceType )
        {
            foreach ( T _Document in _DocumentDataList )
                yield return new DocumentDataContainer( ( _Document as DatiDocumento ), _ReferenceType );
        }

        private void AddExternalDocumentReferences( DatiGenerali GeneralData, Document _Document )
        {
            List<DocumentDataContainer> ExDocs = new List<DocumentDataContainer>( );

            if ( GeneralData.DatiContratto != null )
                ExDocs.AddRange( CollectionToContainerCollection( GeneralData.DatiContratto, EDocumentDataReferenceType.DatiContratto ) );

            if ( GeneralData.DatiConvenzione != null )
                ExDocs.AddRange( CollectionToContainerCollection( GeneralData.DatiConvenzione, EDocumentDataReferenceType.DatiConvenzione ) );

            if ( GeneralData.DatiFattureCollegate != null )
                ExDocs.AddRange( CollectionToContainerCollection( GeneralData.DatiFattureCollegate, EDocumentDataReferenceType.DatiFattureCollegate ) );

            if ( GeneralData.DatiOrdineAcquisto != null )
                ExDocs.AddRange( CollectionToContainerCollection( GeneralData.DatiOrdineAcquisto, EDocumentDataReferenceType.DatiOrdineAcquisto ) );

            if ( GeneralData.DatiRicezione != null )
                ExDocs.AddRange( CollectionToContainerCollection( GeneralData.DatiRicezione, EDocumentDataReferenceType.DatiRicezione ) );


            if ( ExDocs.Count > 0 || ( GeneralData.DatiDDT != null && GeneralData.DatiDDT.Count > 0 ) )
            {
                PdfPTable _ExternalDocumentReferenceTable = CreateBodyPdfPTable( new String[]
                {
                    LocalisedString.ReferenceType,
                    LocalisedString.Row,
                    LocalisedString.ReferenceNumber,
                    LocalisedString.DateOfReference,
                    LocalisedString.CUPCode,
                    LocalisedString.CIGCode,
                } );

                for ( int i = 0; i < ExDocs.Count; i++ )
                {
                    _ExternalDocumentReferenceTable.AddCell( new Paragraph( GetNullableString( ExDocs[ i ].ReferenceType.GetDescription( ) ), _BodyHelvetica ) );
                    _ExternalDocumentReferenceTable.AddCell( new Paragraph( IntListToString( ExDocs[ i ].Document.RiferimentoNumeroLinea ), _BodyHelvetica ) );
                    _ExternalDocumentReferenceTable.AddCell( new Paragraph( GetNullableString( ExDocs[ i ].Document.IdDocumento ), _BodyHelvetica ) );
                    _ExternalDocumentReferenceTable.AddCell( new Paragraph( GetNullableString( ExDocs[ i ].Document.Data ), _BodyHelvetica ) );
                    _ExternalDocumentReferenceTable.AddCell( new Paragraph( GetNullableString( ExDocs[ i ].Document.CodiceCUP ), _BodyHelvetica ) );
                    _ExternalDocumentReferenceTable.AddCell( new Paragraph( GetNullableString( ExDocs[ i ].Document.CodiceCIG ), _BodyHelvetica ) );
                }

                if ( GeneralData.DatiDDT != null && GeneralData.DatiDDT.Count > 0 )
                {
                    for ( int i = 0; i < GeneralData.DatiDDT.Count; i++ )
                    {
                        _ExternalDocumentReferenceTable.AddCell( new Phrase( "DDT", _BodyHelvetica ) );
                        _ExternalDocumentReferenceTable.AddCell( new Paragraph( IntListToString( GeneralData.DatiDDT[ i ].RiferimentoNumeroLinea ), _BodyHelvetica ) );
                        _ExternalDocumentReferenceTable.AddCell( new Paragraph( GetNullableString( GeneralData.DatiDDT[ i ].NumeroDDT ), _BodyHelvetica ) );
                        _ExternalDocumentReferenceTable.AddCell( new Paragraph( GetNullableString( GeneralData.DatiDDT[ i ].DataDDT ), _BodyHelvetica ) );
                        _ExternalDocumentReferenceTable.AddCell( "" );
                        _ExternalDocumentReferenceTable.AddCell( "" );
                    }
                }

                _Document.Add( GenerateBodyTable( new Paragraph( LocalisedString.ExternalDocumentReferences, _HeaderHelvetica ), new PdfPCell( _ExternalDocumentReferenceTable ) ) );
            }
        }

        private String IntListToString( List<int> IntList )
        {
            String result = "";

            for ( int i = 0; i < IntList.Count; i++ )
            {
                result = IntList[ i ].ToString( ) + " ";
            }

            return result;
        }

        private PdfPTable CreateBodyPdfPTable( String[] _Headers, int _HorizontalAlignment = Element.ALIGN_RIGHT, int _VerticalAlignment = Element.ALIGN_CENTER )
        {
            PdfPTable _PdfPTable = new PdfPTable( _Headers.Length );

            _PdfPTable.DefaultCell.BorderColor         = _BorderColour;
            _PdfPTable.DefaultCell.BorderWidth         = _BorderWidth;
            _PdfPTable.DefaultCell.VerticalAlignment   = _VerticalAlignment;
            _PdfPTable.DefaultCell.HorizontalAlignment = _HorizontalAlignment;

            foreach ( String _Header in _Headers )
            {
                _PdfPTable.AddCell( new PdfPCell( new Phrase( _Header, _HeaderHelveticaDark ) ) { BackgroundColor = _HeaderColour, BorderColor = _BorderColour, BorderWidth = _BorderWidth } );
            }

            return _PdfPTable;
        }

        private String GetNullableString ( String _String )
        {
            if ( !String.IsNullOrEmpty( _String ) )
            {
                return _String;
            }

            return "";
        }
        private String GetNullableString ( Nullable<Decimal> _NullableDecimal )
        {
            if ( _NullableDecimal.HasValue )
            {
                return _NullableDecimal.Value.ToString( );
            }

            return "";
        }

        private String GetNullableString( Nullable<DateTime> _NullableDateTime )
        {
            if ( _NullableDateTime.HasValue )
            {
                return _NullableDateTime.Value.ToShortDateString( );
            }

            return "";
        }

        private void AddGeneralDocumentData( DatiGeneraliDocumento GeneralDocumentData, Document _Document )
        {
            PdfPTable _GeneralDocumentDataTable = CreateBodyPdfPTable( new String[]
            {
                LocalisedString.DocumentType,
                LocalisedString.Date,
                LocalisedString.Number,
                LocalisedString.Currency,
                LocalisedString.TotalAmount,
                LocalisedString.Rounding,
                LocalisedString.VirtualStamp,
                LocalisedString.StampAmount
            } );

            _GeneralDocumentDataTable.DefaultCell.HorizontalAlignment = Element.ALIGN_RIGHT;

            // filling rows 

            _GeneralDocumentDataTable.AddCell( new Paragraph ( DocumentTypeCodeToString( GeneralDocumentData.TipoDocumento ), _BodyHelvetica ) ); // Document Type
            _GeneralDocumentDataTable.AddCell( new Paragraph ( GeneralDocumentData.Data.ToShortDateString( ), _BodyHelvetica ) ); // Date
            _GeneralDocumentDataTable.AddCell( new Paragraph ( GeneralDocumentData.Numero, _BodyHelvetica ) ); // Number
            _GeneralDocumentDataTable.AddCell( new Paragraph ( GeneralDocumentData.Divisa, _BodyHelvetica ) ); // Document Currency
            _GeneralDocumentDataTable.AddCell( new Paragraph ( GetNullableString( GeneralDocumentData.ImportoTotaleDocumento ), _BodyHelvetica ) ); // Total Amount
            _GeneralDocumentDataTable.AddCell( new Paragraph ( GetNullableString( GeneralDocumentData.Arrotondamento ), _BodyHelvetica ) ); // Rounding
            _GeneralDocumentDataTable.AddCell( new Paragraph ( GeneralDocumentData.DatiBollo.BolloVirtuale, _BodyHelvetica ) ); // Virtual Stamp 
            _GeneralDocumentDataTable.AddCell( new Paragraph ( GetNullableString( GeneralDocumentData.DatiBollo.ImportoBollo ), _BodyHelvetica ) ); // Stamp Amount


            PdfPTable _GeneralDocumentCauseTable = CreateBodyPdfPTable( new String[] 
            {
                LocalisedString.Cause
            }, Element.ALIGN_LEFT );

            foreach ( String Cause in GeneralDocumentData.Causale )
            {
                _GeneralDocumentCauseTable.AddCell( new Paragraph( Cause, _BodyHelvetica ) );
            }

            if ( !String.IsNullOrEmpty( GeneralDocumentData.Art73 ) )
            {
                _GeneralDocumentCauseTable.AddCell( new Paragraph( Conventions.Art73IssueNotice, _BodyHelvetica ) );
            }
            

            // Add General Document Data tables

            _Document.Add( GenerateBodyTable( new Paragraph( LocalisedString.GeneralData, _HeaderHelvetica ),
                                              new PdfPCell( _GeneralDocumentDataTable ),
                                              new PdfPCell( _GeneralDocumentCauseTable ) ) );

            // Handle Pension Data Subsection

            if ( GeneralDocumentData.DatiCassaPrevidenziale != null && GeneralDocumentData.DatiCassaPrevidenziale.Count > 0 )
                AddPensionFundDataTable( GeneralDocumentData.DatiCassaPrevidenziale, _Document );
        }

        private void AddPensionFundDataTable( List<DatiCassaPrevidenziale> PensionFundDataList, Document _Document )
        {
            PdfPTable _PensionFundDataTableHeader = CreateBodyPdfPTable( new String[]
            {
                LocalisedString.PensionFund
            }, Element.ALIGN_LEFT );

            PdfPTable _PensionFundDataTable = CreateBodyPdfPTable( new String[] 
            {
                LocalisedString.Rate,
                LocalisedString.Amount + " " + LocalisedString.Contributed,
                LocalisedString.Taxable + " " + LocalisedString.Amount,
                LocalisedString.TaxRate,
                LocalisedString.Retained,
                LocalisedString.Nature,
                LocalisedString.Ref + " " + LocalisedString.Administrater
            } );

            for ( int i = 0; i < PensionFundDataList.Count; i++ )
            {
                _PensionFundDataTableHeader.AddCell( new Paragraph( TipoCassaCodeToString( PensionFundDataList[ i ].TipoCassa ), _BodyHelvetica ) );

                _PensionFundDataTable.AddCell( new Paragraph( GetNullableString( PensionFundDataList[ i ].AlCassa ), _BodyHelvetica ) ); // Rate
                _PensionFundDataTable.AddCell( new Paragraph( GetNullableString( PensionFundDataList[ i ].ImportoContributoCassa ), _BodyHelvetica ) ); // Amount Contributed
                _PensionFundDataTable.AddCell( new Paragraph( GetNullableString( " " ), _BodyHelvetica ) ); // Taxable Amount -?> PensionFundDataList[ i ].ImponibileCassa
                _PensionFundDataTable.AddCell( new Paragraph( GetNullableString( PensionFundDataList[ i ].AliquotaIVA ), _BodyHelvetica ) ); // Tax Rate 
                _PensionFundDataTable.AddCell( new Paragraph( GetNullableString( PensionFundDataList[ i ].Ritenuta ), _BodyHelvetica ) ); // Retained
                _PensionFundDataTable.AddCell( new Paragraph( NaturaCodeToString( PensionFundDataList[ i ].Natura ), _BodyHelvetica ) ); // Nature
                _PensionFundDataTable.AddCell( new Paragraph( GetNullableString( PensionFundDataList[ i ].RiferimentoAmministrazione ), _BodyHelvetica ) ); // Ref Administrater
            }

            _Document.Add( GenerateBodyTable ( new Paragraph( LocalisedString.PensionFundData, _HeaderHelvetica ),
                                               new PdfPCell( _PensionFundDataTableHeader ),
                                               new PdfPCell( _PensionFundDataTable ) ) );
        }

        private PdfPTable GenerateBodyTable( Phrase _Header, params PdfPCell[] _PDFPCells )
        {
            PdfPTable _Container = new PdfPTable( 1 );

            _Container.DefaultCell.Border = Rectangle.NO_BORDER;
            _Container.WidthPercentage    = 100;

            _Container.AddCell( new Phrase( ) );
            _Container.AddCell( _Header );
            
            for ( int i = 0; i < _PDFPCells.Length; i++ )
            {
                if ( _PDFPCells[ i ] != null )
                    _Container.AddCell( _PDFPCells[ i ] );
            }

            return _Container;
        }

        private PdfPTable GeneratePDFHeader( FatturaElettronicaHeader _FatturaElettronicaHeader )
        {
            PdfPTable _PDFSenderTable = new PdfPTable( 1 );
            PdfPTable _PDFRecieverTable = new PdfPTable( 1 );
            
            PdfPCell _Sender = new PdfPCell( new Phrase( LocalisedString.Sender, _TitleHelvetica ) )
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_CENTER,
                BackgroundColor = _HeaderColour,
                Border = Rectangle.NO_BORDER,
                FixedHeight = 20.0f
            };

            PdfPCell _Reciever = new PdfPCell( new Paragraph( LocalisedString.Reciever, _TitleHelvetica ) )
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_CENTER,
                BackgroundColor = _HeaderColour,
                Border = Rectangle.NO_BORDER,
                FixedHeight = 20.0f
            };


            _PDFSenderTable.DefaultCell.Border   = Rectangle.NO_BORDER;
            _PDFRecieverTable.DefaultCell.Border = Rectangle.NO_BORDER;

            _PDFSenderTable.AddCell( _Sender );
            _PDFRecieverTable.AddCell( _Reciever );

            InsertSenderHeaderData( _PDFSenderTable, _FatturaElettronicaHeader.CedentePrestatore );

            InsertRecieverHeaderData( _PDFRecieverTable, 
                                      _FatturaElettronicaHeader.CessionarioCommittente, 
                                      _FatturaElettronicaHeader.DatiTrasmissione.CodiceDestinatario, 
                                      _FatturaElettronicaHeader.DatiTrasmissione.PECDestinatario );
            

            PdfPCell _PDFHeaderSenderCell = new PdfPCell( _PDFSenderTable )
            {
                BorderWidth = 1.5f,
                BorderColor = _BorderColour
            };

            PdfPCell _PDFHeaderReciverCell = new PdfPCell( _PDFRecieverTable )
            {
                BorderWidth = 1.5f,
                BorderColor = _BorderColour
            };

            PdfPTable _PDFHeaderContainer = new PdfPTable( 3 );

            _PDFHeaderContainer.WidthPercentage = 100;
            _PDFHeaderContainer.HorizontalAlignment = Element.ALIGN_LEFT;
            _PDFHeaderContainer.DefaultCell.Border = Rectangle.NO_BORDER;


            _PDFHeaderContainer.SetWidths( new float[] { 12f, 1f, 12f } );

            _PDFHeaderContainer.AddCell( _PDFHeaderSenderCell );
            _PDFHeaderContainer.AddCell( new PdfPCell( ) { Border = Rectangle.NO_BORDER } );
            _PDFHeaderContainer.AddCell( _PDFHeaderReciverCell );


            return _PDFHeaderContainer;
        }

        private void InsertRecieverHeaderData( PdfPTable _PDFRecieverTable, CessionarioCommittente _FatturaRecieverHeader, String RecipientCode, String PECRecipient )
        {
            _PDFRecieverTable.AddCell( new Phrase( TryGetParticipantName( _FatturaRecieverHeader.DatiAnagrafici.Anagrafica ), _TitleHelvetica ) ); // First address line, Reciever Name
            _PDFRecieverTable.AddCell( new Phrase( _FatturaRecieverHeader.Sede.Indirizzo + " " + _FatturaRecieverHeader.Sede?.NumeroCivico, _BodyHelvetica )  ); // Second address line
            _PDFRecieverTable.AddCell( new Phrase( _FatturaRecieverHeader.Sede.CAP + " " + _FatturaRecieverHeader.Sede.Comune + " (" + _FatturaRecieverHeader.Sede.Provincia + ") - " + _FatturaRecieverHeader.Sede.Nazione, _BodyHelvetica ) ); // Third address line
            _PDFRecieverTable.AddCell( new Phrase( "P.IVA: " + _FatturaRecieverHeader.DatiAnagrafici.IdFiscaleIVA.IdPaese + " " + _FatturaRecieverHeader.DatiAnagrafici.IdFiscaleIVA.IdCodice, _BodyHelvetica ) ); // Fiscal ID Code + Fiscal ID
            _PDFRecieverTable.AddCell( new Phrase( "C.F.: " + _FatturaRecieverHeader.DatiAnagrafici.CodiceFiscale, _BodyHelvetica ) ); // Fiscal Code
            _PDFRecieverTable.AddCell( new Phrase( " ", _BodyHelvetica ) );

            if ( !String.IsNullOrEmpty( RecipientCode ) )
                _PDFRecieverTable.AddCell( new Phrase( "Codice IPA: " + RecipientCode, _BodyHelvetica ) ); // IPA Recipient Code

            if ( !String.IsNullOrEmpty( PECRecipient ) )
                _PDFRecieverTable.AddCell( new Phrase( "PEC: " + PECRecipient, _BodyHelvetica ) ); // PEC Recipient Code
        }

        private void InsertSenderHeaderData( PdfPTable _PDFSenderTable, CedentePrestatore _FatturaSenderHeader )
        {
            _PDFSenderTable.AddCell( new Phrase( TryGetParticipantName( _FatturaSenderHeader.DatiAnagrafici.Anagrafica ), _TitleHelvetica ) ); // First address line, Sender Name
            _PDFSenderTable.AddCell( new Phrase( _FatturaSenderHeader.Sede.Indirizzo + " " + _FatturaSenderHeader.Sede.NumeroCivico, _BodyHelvetica ) ); // Second address line
            _PDFSenderTable.AddCell( new Phrase( _FatturaSenderHeader.Sede.CAP + " " + _FatturaSenderHeader.Sede.Comune + " (" + _FatturaSenderHeader.Sede.Provincia + ") - " + _FatturaSenderHeader.Sede.Nazione, _BodyHelvetica ) ); // Third address line
            _PDFSenderTable.AddCell( new Phrase( "P.IVA: " + _FatturaSenderHeader.DatiAnagrafici.IdFiscaleIVA.IdPaese + " " + _FatturaSenderHeader.DatiAnagrafici.IdFiscaleIVA.IdCodice, _BodyHelvetica ) ); // Fiscal ID Code + Fiscal ID
            _PDFSenderTable.AddCell( new Phrase( "C.F.: " + _FatturaSenderHeader.DatiAnagrafici.CodiceFiscale, _BodyHelvetica ) ); // Fiscal Code
            _PDFSenderTable.AddCell( new Paragraph( " ", _BodyHelvetica ) );
            
            Boolean HasContactHeader = false;

            TryInsertSenderContactData( _PDFSenderTable, LocalisedString.Telephone, _FatturaSenderHeader.Contatti.Telefono, ref HasContactHeader );
            TryInsertSenderContactData( _PDFSenderTable, LocalisedString.Fax,       _FatturaSenderHeader.Contatti.Fax,      ref HasContactHeader );
            TryInsertSenderContactData( _PDFSenderTable, LocalisedString.Email,     _FatturaSenderHeader.Contatti.Email,    ref HasContactHeader );
        }

        private String TryGetParticipantName( Anagrafica _PersonalData )
        {
            if ( !String.IsNullOrEmpty( _PersonalData.Denominazione ) )
                return _PersonalData.Denominazione;

            if ( !String.IsNullOrEmpty( _PersonalData.Cognome ) && !String.IsNullOrEmpty( _PersonalData.Nome ) )
                return _PersonalData.Cognome + " " + _PersonalData.Nome;

            if ( !String.IsNullOrEmpty( _PersonalData.CognomeNome ) )
                return _PersonalData.CognomeNome;

            return "";
        }

        private void TryInsertSenderContactData( PdfPTable _PDFSenderTable, String _Prefix, String _Content, ref Boolean _HasContactHeader )
        {
            if ( !String.IsNullOrEmpty( _Content ) )
            {
                if ( !_HasContactHeader )
                {
                    _PDFSenderTable.AddCell( new Phrase( LocalisedString.Contacts, _BodyHelvetica ) );
                    _HasContactHeader = true;
                }

                _PDFSenderTable.AddCell( new Phrase( _Prefix + " " + _Content, _BodyHelvetica ) );
            }
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

        public bool IsValidXML( String InputXML )
        {
            using ( var r = XmlReader.Create( new StringReader( InputXML ), _XmlReaderSettings ) )
            {
                _FatturaDocument = new Fattura( );

                _FatturaDocument.ReadXml( r );

                return _FatturaDocument.Validate( ).IsValid;
            }
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

        public IFormatInformation[] SupportedFormats
        {
            get
            {
                return new IFormatInformation[]
                {
                    new FileFormat( EFileExtension.XML, EFormat.XMLPA, "1.2" ),
                    new FileFormat( EFileExtension.PDF, EFormat.PDF, "1.4" )
                };
            }
        }
    }
}