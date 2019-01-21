using FatturaElettronica;
using FatturaElettronica.FatturaElettronicaBody;
using FatturaElettronica.FatturaElettronicaBody.DatiGenerali;
using FatturaElettronica.FatturaElettronicaHeader;
using FatturaElettronica.FatturaElettronicaHeader.CedentePrestatore;
using FatturaElettronica.FatturaElettronicaHeader.CessionarioCommittente;
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

        Fattura           _FatturaDocument   = null;
        FatturaValidator  _Validator         = new FatturaValidator( );
        XmlReaderSettings _XmlReaderSettings = new XmlReaderSettings { IgnoreWhitespace = true, IgnoreComments = true };
        Color             _HeaderColour      = Color.LIGHT_GRAY;
        Color             _BorderColour      = Color.LIGHT_GRAY;

        #endregion


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

            Document _PDFDocument = new Document( PageSize.LETTER, 0.0f, 0.0f, 36.0f, 36.0f );


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
        }

        private IElement AddGeneralDocumentData( DatiGeneraliDocumento GeneralDocumentData )
        {
            PdfPTable _GeneralDocumentDataTable  = new PdfPTable( 8 );
        
            // creating header
            
            _GeneralDocumentDataTable.AddCell( new PdfPCell( new Phrase( LocalisedString.DocumentType ) ) { BackgroundColor = _HeaderColour } );
            _GeneralDocumentDataTable.AddCell( new PdfPCell( new Phrase( LocalisedString.Date ) )         { BackgroundColor = _HeaderColour } );
            _GeneralDocumentDataTable.AddCell( new PdfPCell( new Phrase( LocalisedString.Number ) )       { BackgroundColor = _HeaderColour } );
            _GeneralDocumentDataTable.AddCell( new PdfPCell( new Phrase( LocalisedString.Currency ) )     { BackgroundColor = _HeaderColour } );
            _GeneralDocumentDataTable.AddCell( new PdfPCell( new Phrase( LocalisedString.TotalAmount ) )  { BackgroundColor = _HeaderColour } );
            _GeneralDocumentDataTable.AddCell( new PdfPCell( new Phrase( LocalisedString.Rounding ) )     { BackgroundColor = _HeaderColour } );
            _GeneralDocumentDataTable.AddCell( new PdfPCell( new Phrase( LocalisedString.VirtualStamp ) ) { BackgroundColor = _HeaderColour } );
            _GeneralDocumentDataTable.AddCell( new PdfPCell( new Phrase( LocalisedString.StampAmount ) )  { BackgroundColor = _HeaderColour } );

            // filling rows

            _GeneralDocumentDataTable.AddCell( LocalisedString.Invoice );
            _GeneralDocumentDataTable.AddCell( GeneralDocumentData.Data.ToShortDateString( ) );
            _GeneralDocumentDataTable.AddCell( GeneralDocumentData.Numero );
            _GeneralDocumentDataTable.AddCell( GeneralDocumentData.Divisa );

            if ( GeneralDocumentData.ImportoTotaleDocumento.HasValue )
            {
                _GeneralDocumentDataTable.AddCell( GeneralDocumentData.ImportoTotaleDocumento.Value.ToString( ) );
            }
            else
            {
                _GeneralDocumentDataTable.AddCell( "" );
            }

            if ( GeneralDocumentData.Arrotondamento.HasValue )
            {
                _GeneralDocumentDataTable.AddCell( GeneralDocumentData.Arrotondamento.Value.ToString( ) );
            }
            else
            {
                _GeneralDocumentDataTable.AddCell( "" );
            }
            
            _GeneralDocumentDataTable.AddCell( GeneralDocumentData.DatiBollo.BolloVirtuale );


            if ( GeneralDocumentData.DatiBollo.ImportoBollo.HasValue )
            {
                _GeneralDocumentDataTable.AddCell( GeneralDocumentData.DatiBollo.ImportoBollo.Value.ToString( ) );
            }
            else
            {
                _GeneralDocumentDataTable.AddCell( "" );
            }

            PdfPTable _GeneralDocumentCauseTable = new PdfPTable( 1 );

            _GeneralDocumentCauseTable.AddCell( LocalisedString.Cause ); //header

            foreach( String Cause in GeneralDocumentData.Causale )
            {
                _GeneralDocumentCauseTable.AddCell( Cause );
            }


            PdfPTable _MainTable = new PdfPTable( 1 );

            _MainTable.DefaultCell.Border = Rectangle.NO_BORDER;
            _MainTable.WidthPercentage = 100;

            _MainTable.AddCell( new Phrase( " " ) ); // Padding
            _MainTable.AddCell( LocalisedString.GeneralData ); // header
            _MainTable.AddCell( new PdfPCell( _GeneralDocumentDataTable ) );
            _MainTable.AddCell( new PdfPCell( _GeneralDocumentCauseTable ) );

            return _MainTable;
        }

        private PdfPTable GeneratePDFHeader( FatturaElettronicaHeader _FatturaElettronicaHeader )
        {
            PdfPTable _PDFSenderTable = new PdfPTable( 1 );
            PdfPTable _PDFRecieverTable = new PdfPTable( 1 );



            PdfPCell _Sender = new PdfPCell( new Phrase( LocalisedString.Sender ) )
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_CENTER,
                BackgroundColor = _HeaderColour,
                Border = Rectangle.NO_BORDER,
                FixedHeight = 30.0f
            };

            PdfPCell _Reciever = new PdfPCell( new Phrase( LocalisedString.Reciever ) )
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_CENTER,
                BackgroundColor = _HeaderColour,
                Border = Rectangle.NO_BORDER,
                FixedHeight = 30.0f
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
                BorderWidth = 1.2f,
                BorderColor = _BorderColour
            };

            PdfPCell _PDFHeaderReciverCell = new PdfPCell( _PDFRecieverTable )
            {
                BorderWidth = 1.2f,
                BorderColor = _BorderColour
            };

            PdfPTable _PDFHeaderContainer = new PdfPTable( 3 );
            _PDFHeaderContainer.DefaultCell.Border = Rectangle.NO_BORDER;


            _PDFHeaderContainer.SetWidths( new float[] { 12f, 1f, 12f } );

            _PDFHeaderContainer.AddCell( _PDFHeaderSenderCell );
            _PDFHeaderContainer.AddCell( new PdfPCell( ) { Border = Rectangle.NO_BORDER } );
            _PDFHeaderContainer.AddCell( _PDFHeaderReciverCell );


            return _PDFHeaderContainer;
        }

        private void InsertRecieverHeaderData( PdfPTable _PDFRecieverTable, CessionarioCommittente _FatturaRecieverHeader, String RecipientCode, String PECRecipient )
        {
            _PDFRecieverTable.AddCell( new Phrase( _FatturaRecieverHeader.DatiAnagrafici.Anagrafica.Denominazione ) ); // First address line
            _PDFRecieverTable.AddCell( new Phrase( _FatturaRecieverHeader.Sede.Indirizzo + " " + _FatturaRecieverHeader.Sede?.NumeroCivico )  ); // Second address line
            _PDFRecieverTable.AddCell( new Phrase( _FatturaRecieverHeader.Sede.CAP + " " + _FatturaRecieverHeader.Sede.Comune + " (" + _FatturaRecieverHeader.Sede.Provincia + ") - " + _FatturaRecieverHeader.Sede.Nazione ) ); // Third address line
            _PDFRecieverTable.AddCell( new Phrase( "P.IVA: " + _FatturaRecieverHeader.DatiAnagrafici.IdFiscaleIVA.IdPaese + " " + _FatturaRecieverHeader.DatiAnagrafici.IdFiscaleIVA.IdCodice ) ); // Fiscal ID Code + Fiscal ID
            _PDFRecieverTable.AddCell( new Phrase( "C.F.: " + _FatturaRecieverHeader.DatiAnagrafici.CodiceFiscale ) ); // Fiscal Code
            _PDFRecieverTable.AddCell( new Phrase( " " ) );

            if ( !String.IsNullOrEmpty( RecipientCode ) )
                _PDFRecieverTable.AddCell( new Phrase( "Codice IPA: " + RecipientCode ) ); // IPA Recipient Code

            if ( !String.IsNullOrEmpty( PECRecipient ) )
                _PDFRecieverTable.AddCell( new Phrase( "PEC: " + PECRecipient ) ); // PEC Recipient Code
        }

        private void InsertSenderHeaderData( PdfPTable _PDFSenderTable, CedentePrestatore _FatturaSenderHeader )
        {
            _PDFSenderTable.AddCell( new Phrase( _FatturaSenderHeader.DatiAnagrafici.Anagrafica?.Denominazione ) ); // First address line
            _PDFSenderTable.AddCell( new Phrase( _FatturaSenderHeader.Sede.Indirizzo + " " + _FatturaSenderHeader.Sede.NumeroCivico ) ); // Second address line
            _PDFSenderTable.AddCell( new Phrase( _FatturaSenderHeader.Sede.CAP + " " + _FatturaSenderHeader.Sede.Comune + " (" + _FatturaSenderHeader.Sede.Provincia + ") - " + _FatturaSenderHeader.Sede.Nazione ) ); // Third address line
            _PDFSenderTable.AddCell( new Phrase( "P.IVA: " + _FatturaSenderHeader.DatiAnagrafici.IdFiscaleIVA.IdPaese + " " + _FatturaSenderHeader.DatiAnagrafici.IdFiscaleIVA.IdCodice ) ); // Fiscal ID Code + Fiscal ID
            _PDFSenderTable.AddCell( new Phrase( "C.F.: " + _FatturaSenderHeader.DatiAnagrafici.CodiceFiscale ) ); // Fiscal Code
            _PDFSenderTable.AddCell( new Phrase( " " ) ); 

            Boolean HasContactHeader = false;

            if ( !String.IsNullOrEmpty( _FatturaSenderHeader.Contatti.Telefono ) )
            {
                if ( !HasContactHeader )
                {
                    _PDFSenderTable.AddCell( new Phrase( LocalisedString.Contacts ) );
                    HasContactHeader = true;
                }

                _PDFSenderTable.AddCell( new Phrase( LocalisedString.Telephone + " " + _FatturaSenderHeader.Contatti.Telefono ) );
            }

            if ( !String.IsNullOrEmpty( _FatturaSenderHeader.Contatti.Fax ) )
            {
                if ( !HasContactHeader )
                {
                    _PDFSenderTable.AddCell( new Phrase( LocalisedString.Contacts ) );
                    HasContactHeader = true;
                }

                _PDFSenderTable.AddCell( new Phrase( LocalisedString.Fax + " " + _FatturaSenderHeader.Contatti.Fax ) );
            }

            if ( !String.IsNullOrEmpty( _FatturaSenderHeader.Contatti.Email ) )
            {
                if ( !HasContactHeader )
                {
                    _PDFSenderTable.AddCell( new Phrase( LocalisedString.Contacts ) );
                    HasContactHeader = true;
                }

                _PDFSenderTable.AddCell( new Phrase( LocalisedString.Email + " " + _FatturaSenderHeader.Contatti.Email ) );
            } 
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
