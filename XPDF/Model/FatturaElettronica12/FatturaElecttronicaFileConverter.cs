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
using System.IO;
using System.Xml;
using Walkways.Extensions.Attributes;
using XPDF.Model.Enums;
using XPDF.Model.FatturaElettronica12;
using XPDF.Model.FatturaElettronica12.Enums;
using XPDF.Model.Interface;
using XPDF.Model.Localization;

namespace XPDF.Model.FatturaElettronica12
{
    internal class FatturaElecttronicaFileConverter : IXPDFFIleConverter
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

        public void Abort( )
        {
            //throw new System.NotImplementedException( );
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

            return new FileInformation( new FileFormat( EFileExtension.PDF, EFormat.PDF, "1.4" ), new Uri( Input.Path.LocalPath + ".pdf" ), _FileName + ".pdf" );
        }

        private string _XMLPAPDFFileName( )
        {
            return @"FATT;" +
                   _FatturaDocument.FatturaElettronicaBody[ 0 ].DatiGenerali.DatiGeneraliDocumento.Numero + // Number
                   ";AZIENDA;" +
                   FiscalCodeID.ToString( _FatturaDocument.FatturaElettronicaHeader.CedentePrestatore.DatiAnagrafici.IdFiscaleIVA.IdCodice ) + // fiscal id code ! Convert
                   ";DEL;" +
                   _FatturaDocument.FatturaElettronicaBody[ 0 ].DatiGenerali.DatiGeneraliDocumento.Data.Year + // date
                   _FatturaDocument.FatturaElettronicaBody[ 0 ].DatiGenerali.DatiGeneraliDocumento.Data.Month + // date
                   _FatturaDocument.FatturaElettronicaBody[ 0 ].DatiGenerali.DatiGeneraliDocumento.Data.Day + // date
                   ";CLIENTE;" +
                   _FatturaDocument.FatturaElettronicaHeader.CessionarioCommittente.DatiAnagrafici.Anagrafica.Denominazione.Replace( " ", "_032" ) + // AddressName ! _O32 for spaces
                   ";PIVA;" +
                   _FatturaDocument.FatturaElettronicaHeader.CessionarioCommittente.DatiAnagrafici.IdFiscaleIVA.IdCodice; // sender fiscal id code
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
            _Document.Add( AddGeneralDocumentData( _FatturaElettronicaBody.DatiGenerali.DatiGeneraliDocumento ) );

            AddExternalDocumentReferences( _FatturaElettronicaBody.DatiGenerali, _Document );


            List<DettaglioLinee> LineDetails = _FatturaElettronicaBody.DatiBeniServizi.DettaglioLinee;

            if ( LineDetails != null && LineDetails.Count > 0 )
            {
                _Document.Add( AddLineDetails( LineDetails ) );
            }

            List<DatiRiepilogo> Summeries = _FatturaElettronicaBody.DatiBeniServizi.DatiRiepilogo;

            if ( LineDetails != null && LineDetails.Count > 0 )
            {
                _Document.Add( AddGeneralSummery( Summeries ) );
            }

            List<DatiPagamento> PaymentInformationList = _FatturaElettronicaBody.DatiPagamento;

            if ( LineDetails != null && LineDetails.Count > 0 )
            {
                _Document.Add( AddPaymentInformation( PaymentInformationList ) );
            }
        }

        private PdfPTable AddPaymentInformation( List<DatiPagamento> PaymentInformationList )
        {
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
                        _PaymentInformationTable.AddCell( new Paragraph( " ", _BodyHelvetica ) );
                        _PaymentInformationTable.AddCell( new Paragraph( PaymentConditionsCodeToString( PaymentInformationList[ i ].CondizioniPagamento ), _BodyHelvetica ) );
                        _PaymentInformationTable.AddCell( new Paragraph( PaymentModeCodeToString( PaymentDetails[ j ].ModalitaPagamento ), _BodyHelvetica ) );
                        _PaymentInformationTable.AddCell( new Paragraph( " ", _BodyHelvetica ) );
                        _PaymentInformationTable.AddCell( new Paragraph( " ", _BodyHelvetica ) );
                        _PaymentInformationTable.AddCell( new Paragraph( GetNullableString( PaymentDetails[ j ].DataScadenzaPagamento ), _BodyHelvetica ) );
                        _PaymentInformationTable.AddCell( new Paragraph( GetNullableString( PaymentDetails[ j ].ImportoPagamento ), _BodyHelvetica ) );

                        if ( !String.IsNullOrEmpty( PaymentDetails[ i ].IstitutoFinanziario ) )
                        {
                            _PaymentInformationTable.AddCell( GetInstitutionSpanCell( PaymentDetails[ i ] ) );
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

            return GenerateBodyTable( new Paragraph( LocalisedString.PaymentInformation, _HeaderHelvetica ),
                                      new PdfPCell( _PaymentInformationTable ) );
        }

        private PdfPCell GetInstitutionSpanCell( DettaglioPagamento _PaymentDetails )
        {
            PdfPCell InstitutionSpanCell = new PdfPCell();

            InstitutionSpanCell.UseAscender         = _CellUseAscender;
            InstitutionSpanCell.PaddingTop          = _CellPaddingTop;
            InstitutionSpanCell.Colspan             = 7;
            InstitutionSpanCell.BorderColor         = _BorderColour;
            InstitutionSpanCell.BorderWidth         = _BorderWidth;
            InstitutionSpanCell.VerticalAlignment   = Element.ALIGN_TOP;
            InstitutionSpanCell.HorizontalAlignment = Element.ALIGN_LEFT;

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

        private PdfPTable AddGeneralSummery( List<DatiRiepilogo> GeneralSummeriesList )
        {
            PdfPTable _GeneralSummeryTable = CreateBodyPdfPTable( new String[]
            {
                LocalisedString.Taxable,
                LocalisedString.VAT + "%",
                LocalisedString.Tax,
                LocalisedString.Natural,
                LocalisedString.NormativeReference,
                LocalisedString.Collectable
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

            return GenerateBodyTable( new Paragraph( LocalisedString.GeneralSummery, _HeaderHelvetica ),
                                      new PdfPCell( _GeneralSummeryTable ) );
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

        private String NaturaCodeToString( String Code )
        {
            if ( String.IsNullOrEmpty( Code ) )
                return " ";

            return CodiceToNome( new Natura( ), Code );
        }

        private PdfPTable AddLineDetails( List<DettaglioLinee> LineDetails )
        {
            PdfPTable _LineDetailsTable = CreateBodyPdfPTable( new String[]
            {
                LocalisedString.Line,
                LocalisedString.Cod,
                LocalisedString.Value,
                LocalisedString.Description,
                LocalisedString.QTA,
                LocalisedString.UM,
                LocalisedString.Price,
                "%",
                LocalisedString.scmg,
                LocalisedString.Val,
                LocalisedString.Amount,
                LocalisedString.VAT + "%"
            } );

            _LineDetailsTable.SetWidths( new float[] 
            {
                0.5f, // Line
                1f, // Cod
                1f, // Value
                4f, // Description
                0.75f, // QTA
                0.5f, // UM
                1f, // Price
                0.5f, // "%"
                0.5f, // scmg
                1f, // Val
                1f, // Amount
                0.5f  // VAT + "%"
            } );

            for ( int i = 0; i < LineDetails.Count; i++ )
            {
                _LineDetailsTable.AddCell( new Paragraph( LineDetails[ i ].NumeroLinea.ToString( ), _BodyHelvetica ) );

                if ( LineDetails[i].CodiceArticolo != null && LineDetails[i].CodiceArticolo.Count > 0 )
                {
                    _LineDetailsTable.AddCell( new Paragraph( LineDetails[ i ].CodiceArticolo[ 0 ].CodiceTipo, _BodyHelvetica ) );
                    _LineDetailsTable.AddCell( new Paragraph( LineDetails[ i ].CodiceArticolo[ 0 ].CodiceValore, _BodyHelvetica ) );
                }
                else
                {
                    _LineDetailsTable.AddCell( new Paragraph( " ", _BodyHelvetica ) );
                    _LineDetailsTable.AddCell( new Paragraph( " ", _BodyHelvetica ) );
                }

                _LineDetailsTable.AddCell( GenerateLineItemDescription( LineDetails[ i ] ) );
                _LineDetailsTable.AddCell( new Paragraph( GetNullableString( LineDetails[ i ].Quantita ), _BodyHelvetica ) );
                _LineDetailsTable.AddCell( new Paragraph( LineDetails[ i ].UnitaMisura, _BodyHelvetica ) );
                _LineDetailsTable.AddCell( new Paragraph( GetNullableString( LineDetails[ i ].PrezzoUnitario ), _BodyHelvetica ) );
                _LineDetailsTable.AddCell( new Paragraph( " ", _BodyHelvetica ) );
                _LineDetailsTable.AddCell( new Paragraph( " ", _BodyHelvetica ) );
                _LineDetailsTable.AddCell( new Paragraph( " ", _BodyHelvetica ) );
                _LineDetailsTable.AddCell( new Paragraph( GetNullableString( LineDetails[ i ].PrezzoTotale ), _BodyHelvetica ) );
                _LineDetailsTable.AddCell( new Paragraph( GetNullableString( LineDetails[ i ].AliquotaIVA ), _BodyHelvetica ) );
            }

            return GenerateBodyTable( new Paragraph( LocalisedString.InvoiceDetails, _HeaderHelvetica ),
                                      new PdfPCell( _LineDetailsTable ) );
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

        private PdfPTable CreateBodyPdfPTable( String[] _Headers )
        {
            PdfPTable _PdfPTable = new PdfPTable( _Headers.Length );

            _PdfPTable.DefaultCell.BorderColor         = _BorderColour;
            _PdfPTable.DefaultCell.BorderWidth         = _BorderWidth;
            _PdfPTable.DefaultCell.VerticalAlignment   = Element.ALIGN_CENTER;
            _PdfPTable.DefaultCell.HorizontalAlignment = Element.ALIGN_RIGHT;

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

        private IElement AddGeneralDocumentData( DatiGeneraliDocumento GeneralDocumentData )
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

            _GeneralDocumentDataTable.AddCell( new Paragraph ( LocalisedString.Invoice, _BodyHelvetica ) ); // Document Type
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
            } );

            foreach ( String Cause in GeneralDocumentData.Causale )
            {
                _GeneralDocumentCauseTable.AddCell( new Paragraph( Cause, _BodyHelvetica ) );
            }

            return GenerateBodyTable( new Paragraph( LocalisedString.GeneralData, _HeaderHelvetica ),
                                      new PdfPCell( _GeneralDocumentDataTable ),
                                      new PdfPCell( _GeneralDocumentCauseTable ) );
        }

        private PdfPTable GenerateBodyTable( Phrase _Header, params PdfPCell[] _PDFPCells )
        {
            PdfPTable _Container = new PdfPTable( 1 );

            _Container.DefaultCell.Border = Rectangle.NO_BORDER;
            _Container.WidthPercentage    = 100;

            _Container.AddCell( _Header );
            
            for ( int i = 0; i < _PDFPCells.Length; i++ )
            {
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
            _PDFRecieverTable.AddCell( new Phrase( _FatturaRecieverHeader.DatiAnagrafici.Anagrafica.Denominazione, _TitleHelvetica ) ); // First address line
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
            _PDFSenderTable.AddCell( new Phrase( _FatturaSenderHeader.DatiAnagrafici.Anagrafica?.Denominazione, _TitleHelvetica ) ); // First address line
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

        public void Dispose( )
        {
            Abort( );
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

        public IFormatInformation[] SupportedFormats => throw new NotImplementedException( );
    }
}
