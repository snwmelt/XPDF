﻿using FatturaElettronica;
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
using XPDF.Model.Enums;
using XPDF.Model.Interface;
using XPDF.Model.Localization;

namespace XPDF.Model
{
    internal class FatturaElecttronicaFileConverter : IXPDFFIleConverter
    {
        #region Private Variables

        Fattura                    _FatturaDocument   = null;
        readonly FatturaValidator  _Validator         = new FatturaValidator( );
        readonly XmlReaderSettings _XmlReaderSettings = new XmlReaderSettings { IgnoreWhitespace = true, IgnoreComments = true };
        Color                      _HeaderColour      = Color.LIGHT_GRAY;
        Color                      _BorderColour      = Color.LIGHT_GRAY;
        readonly float             _BorderWidth       = 1.4f;
        BaseFont                   _BaseFontHelvetica = BaseFont.CreateFont( BaseFont.HELVETICA, BaseFont.CP1252, true );
        Font                       _HeaderHelvetica;
        Font                       _HeaderHelveticaDark;
        Font                       _BodyHelvetica;
        Font                       _TitleHelvetica;
        #endregion

        public FatturaElecttronicaFileConverter( )
        {
            _HeaderHelvetica = new Font( _BaseFontHelvetica, 8, Font.NORMAL, Color.LIGHT_GRAY );
            _HeaderHelveticaDark = new Font( _BaseFontHelvetica, 8, Font.NORMAL, Color.DARK_GRAY );
            _BodyHelvetica = new Font( _BaseFontHelvetica, 8, Font.NORMAL, Color.BLACK );
            _TitleHelvetica = new Font( _BaseFontHelvetica, 10, Font.NORMAL, Color.BLACK );
        }

        public void Abort( )
        {
            //throw new System.NotImplementedException( );
        }

        public void Convert( string InputXMLFilePath, string OutputPDFPath )
        {
            using ( var r = XmlReader.Create( new StringReader( File.ReadAllText( InputXMLFilePath ) ), _XmlReaderSettings ) )
            {
                _FatturaDocument = new Fattura( );

                _FatturaDocument.ReadXml( r );
            }

            Document _PDFDocument = new Document( PageSize.LETTER, 25.0f, 25.0f, 25.0f, 25.0f );


            PdfWriter.GetInstance( _PDFDocument, new FileStream( OutputPDFPath, FileMode.OpenOrCreate ) );

            _PDFDocument.Open( );

            _PDFDocument.Add( GeneratePDFHeader( _FatturaDocument.FatturaElettronicaHeader ) );

            for ( int i = 0; i < _FatturaDocument.FatturaElettronicaBody.Count; i++ )
            {
                AddInvoiceBodyToPDFPage( _FatturaDocument.FatturaElettronicaBody[i], _PDFDocument, i );
            }

            _PDFDocument.Close( );
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
                    }
                }
                else
                {
                    _PaymentInformationTable.AddCell( new Paragraph( " ", _BodyHelvetica ) );
                    _PaymentInformationTable.AddCell( PaymentConditionsCodeToString( PaymentInformationList[ i ].CondizioniPagamento ) );
                }
            }


            PdfPTable _MainTable = new PdfPTable( 1 );

            _MainTable.DefaultCell.Border = Rectangle.NO_BORDER;
            _MainTable.WidthPercentage = 100;

            _MainTable.AddCell( new Paragraph( " ", _BodyHelvetica ) ); // Padding
            _MainTable.AddCell( new Paragraph( LocalisedString.PaymentInformation, _HeaderHelvetica ) ); // header
            _MainTable.AddCell( new PdfPCell( _PaymentInformationTable ) );

            return _MainTable;
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


            PdfPTable _MainTable = new PdfPTable( 1 );

            _MainTable.DefaultCell.Border = Rectangle.NO_BORDER;
            _MainTable.WidthPercentage = 100;

            _MainTable.AddCell( new Paragraph( " ", _BodyHelvetica ) ); // Padding
            _MainTable.AddCell( new Paragraph( LocalisedString.GeneralSummery, _HeaderHelvetica ) ); // header
            _MainTable.AddCell( new PdfPCell( _GeneralSummeryTable ) );

            return _MainTable;
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

                _LineDetailsTable.AddCell(new Paragraph( new Paragraph( LineDetails[ 0 ].Descrizione, _BodyHelvetica) ) );
                _LineDetailsTable.AddCell(new Paragraph( GetNullableString( LineDetails[ i ].Quantita ), _BodyHelvetica) );
                _LineDetailsTable.AddCell(new Paragraph( LineDetails[ i ].UnitaMisura, _BodyHelvetica) );
                _LineDetailsTable.AddCell(new Paragraph( GetNullableString( LineDetails[ i ].PrezzoUnitario ), _BodyHelvetica) );
                _LineDetailsTable.AddCell(new Paragraph( " ", _BodyHelvetica) );
                _LineDetailsTable.AddCell(new Paragraph( " ", _BodyHelvetica) );
                _LineDetailsTable.AddCell(new Paragraph( " ", _BodyHelvetica) );
                _LineDetailsTable.AddCell(new Paragraph( GetNullableString( LineDetails[ i ].PrezzoTotale ), _BodyHelvetica) );
                _LineDetailsTable.AddCell( new Paragraph( GetNullableString( LineDetails[ i ].AliquotaIVA ), _BodyHelvetica ) );
            }

            PdfPTable _MainTable = new PdfPTable( 1 );

            _MainTable.DefaultCell.Border = Rectangle.NO_BORDER;
            _MainTable.WidthPercentage = 100;
            
            _MainTable.AddCell( new Paragraph( " ", _BodyHelvetica ) ); // Padding
            _MainTable.AddCell(  new Paragraph( LocalisedString.InvoiceDetails, _HeaderHelvetica ) ); // header
            _MainTable.AddCell( new PdfPCell( _LineDetailsTable ) );

            return _MainTable;
        }

        private void AddExternalDocumentReferences( DatiGenerali GeneralData, Document _Document )
        {
            List<FatturaElettronica.Common.DatiDocumento> ExDocs = new List<FatturaElettronica.Common.DatiDocumento>( );

            if ( GeneralData.DatiContratto != null  )
                ExDocs.AddRange( GeneralData.DatiContratto );

            if ( GeneralData.DatiConvenzione != null )
                ExDocs.AddRange( GeneralData.DatiConvenzione );

            if ( GeneralData.DatiFattureCollegate != null )
                ExDocs.AddRange( GeneralData.DatiFattureCollegate );

            if ( GeneralData.DatiOrdineAcquisto != null )
                ExDocs.AddRange( GeneralData.DatiOrdineAcquisto );

            if ( GeneralData.DatiRicezione != null )
                ExDocs.AddRange( GeneralData.DatiRicezione );

            
            if ( ExDocs.Count > 0 || GeneralData.DatiDDT != null )
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
                    _ExternalDocumentReferenceTable.AddCell( "" );
                    _ExternalDocumentReferenceTable.AddCell( new Paragraph( GetNullableString( ExDocs[ i ].CodiceCommessaConvenzione ), _BodyHelvetica ) );
                    _ExternalDocumentReferenceTable.AddCell( new Paragraph( GetNullableString( ExDocs[ i ].IdDocumento ), _BodyHelvetica ) );
                    _ExternalDocumentReferenceTable.AddCell( new Paragraph( GetNullableString( ExDocs[ i ].Data ), _BodyHelvetica ) );
                    _ExternalDocumentReferenceTable.AddCell( new Paragraph( GetNullableString( ExDocs[ i ].CodiceCUP ), _BodyHelvetica ) );
                    _ExternalDocumentReferenceTable.AddCell( new Paragraph( GetNullableString( ExDocs[ i ].CodiceCIG ), _BodyHelvetica ) );
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

                PdfPTable _MainTable = new PdfPTable( 1 );

                _MainTable.DefaultCell.Border = Rectangle.NO_BORDER;
                _MainTable.WidthPercentage = 100;

                _MainTable.AddCell( new Paragraph( " ", _BodyHelvetica ) ); // Padding
                _MainTable.AddCell( new Paragraph( LocalisedString.ExternalDocumentReferences, _HeaderHelvetica ) ); // header
                _MainTable.AddCell( new PdfPCell( _ExternalDocumentReferenceTable ) );

                _Document.Add( _MainTable );
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

            _PdfPTable.DefaultCell.BorderColor = _BorderColour;
            _PdfPTable.DefaultCell.BorderWidth = _BorderWidth;
            _PdfPTable.DefaultCell.VerticalAlignment = Element.ALIGN_CENTER;
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


            PdfPTable _MainTable = new PdfPTable( 1 );

            _MainTable.DefaultCell.Border = Rectangle.NO_BORDER;
            _MainTable.WidthPercentage = 100;
            
            _MainTable.AddCell( new Paragraph( " ", _BodyHelvetica ) );
            _MainTable.AddCell( new Paragraph( LocalisedString.GeneralData, _HeaderHelvetica ) ); // header
            _MainTable.AddCell( new PdfPCell( _GeneralDocumentDataTable ) );
            _MainTable.AddCell( new PdfPCell( _GeneralDocumentCauseTable ) );

            return _MainTable;
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



            _PDFSenderTable.DefaultCell.Border = Rectangle.NO_BORDER;
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

            if ( !String.IsNullOrEmpty( _FatturaSenderHeader.Contatti.Telefono ) )
            {
                if ( !HasContactHeader )
                {
                    _PDFSenderTable.AddCell( new Phrase( LocalisedString.Contacts, _BodyHelvetica ) );
                    HasContactHeader = true;
                }

                _PDFSenderTable.AddCell( new Phrase( LocalisedString.Telephone + " " + _FatturaSenderHeader.Contatti.Telefono, _BodyHelvetica ) );
            }

            if ( !String.IsNullOrEmpty( _FatturaSenderHeader.Contatti.Fax ) )
            {
                if ( !HasContactHeader )
                {
                    _PDFSenderTable.AddCell( new Phrase( LocalisedString.Contacts, _BodyHelvetica ) );
                    HasContactHeader = true;
                }

                _PDFSenderTable.AddCell( new Phrase( LocalisedString.Fax + " " + _FatturaSenderHeader.Contatti.Fax, _BodyHelvetica ) );
            }

            if ( !String.IsNullOrEmpty( _FatturaSenderHeader.Contatti.Email ) )
            {
                if ( !HasContactHeader )
                {
                    _PDFSenderTable.AddCell( new Phrase( LocalisedString.Contacts, _BodyHelvetica ) );
                    HasContactHeader = true;
                }

                _PDFSenderTable.AddCell( new Phrase( LocalisedString.Email + " " + _FatturaSenderHeader.Contatti.Email, _BodyHelvetica ) );
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

        public bool IsValidXML( string InputXMLFilePath )
        {
            using ( var r = XmlReader.Create( new StringReader( File.ReadAllText( InputXMLFilePath ) ), _XmlReaderSettings ) )
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
    }
}
