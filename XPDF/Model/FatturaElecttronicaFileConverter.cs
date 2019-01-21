using FatturaElettronica;
using FatturaElettronica.FatturaElettronicaHeader;
using FatturaElettronica.FatturaElettronicaHeader.CedentePrestatore;
using FatturaElettronica.FatturaElettronicaHeader.CessionarioCommittente;
using FatturaElettronica.Validators;
using FluentValidation.Results;
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

            Document _PDFDocument = new Document( );

            PdfWriter.GetInstance( _PDFDocument, new FileStream( OutputPDFPath, FileMode.OpenOrCreate ) );

            _PDFDocument.Open( );


            FatturaElettronicaHeader header = _FatturaDocument.FatturaElettronicaHeader;

            //using ( var r = XmlWriter.Create( InputXMLFilePath + "_header", new XmlWriterSettings { Indent = true } ) )
            //{
            //    header.WriteXml( r );
            //}

            _PDFDocument.Add( GeneratePDFHeader( header ) );

            foreach ( var x in _FatturaDocument.FatturaElettronicaBody )
            {

            }

            _PDFDocument.Close( );
        }

        private PdfPTable GeneratePDFHeader( FatturaElettronicaHeader fatturaElettronicaHeader )
        {
            PdfPTable _PDFSenderTable = new PdfPTable( 1 );
            PdfPTable _PDFRecieverTable = new PdfPTable( 1 );



            PdfPCell _Sender = new PdfPCell( new Phrase( LocalisedString.Sender ) )
            {
                HorizontalAlignment = 1,
                VerticalAlignment = 1,
                BackgroundColor = new Color( 200, 200, 200 )
            };

            PdfPCell _Reciever = new PdfPCell( new Phrase( LocalisedString.Reciever ) )
            {
                HorizontalAlignment = 1,
                VerticalAlignment = 1,
                BackgroundColor = new Color( 200, 200, 200 )
            };




            _PDFSenderTable.AddCell( _Sender );
            _PDFRecieverTable.AddCell( _Reciever );

            InsertSenderHeaderData( _PDFSenderTable, fatturaElettronicaHeader.CedentePrestatore );
            InsertRecieverHeaderData( _PDFRecieverTable, fatturaElettronicaHeader.CessionarioCommittente );






            PdfPCell _PDFHeaderSenderCell = new PdfPCell( _PDFSenderTable );
            PdfPCell _PDFHeaderReciverCell = new PdfPCell( _PDFRecieverTable );


            _PDFHeaderSenderCell.Border = Rectangle.NO_BORDER;
            _PDFHeaderReciverCell.Border = Rectangle.NO_BORDER;


            PdfPTable _PDFHeaderContainer = new PdfPTable( 3 );

            _PDFHeaderContainer.SetWidths( new float[] { 12f, 1f, 12f } );

            _PDFHeaderContainer.AddCell( _PDFHeaderSenderCell );
            _PDFHeaderContainer.AddCell( new PdfPCell( ) { Border = Rectangle.NO_BORDER } );
            _PDFHeaderContainer.AddCell( _PDFHeaderReciverCell );


            return _PDFHeaderContainer;
        }

        private void InsertRecieverHeaderData( PdfPTable _PDFRecieverTable, CessionarioCommittente _FatturaRecieverHeader )
        {
            _PDFRecieverTable.AddCell( new PdfPCell( new Phrase( _FatturaRecieverHeader.DatiAnagrafici.Anagrafica.Denominazione ) ) { Border = Rectangle.NO_BORDER } ); // First address line
            _PDFRecieverTable.AddCell( new PdfPCell( new Phrase( _FatturaRecieverHeader.Sede.Indirizzo + " " + _FatturaRecieverHeader.Sede?.NumeroCivico ) ) { Border = Rectangle.NO_BORDER } ); // Second address line
            _PDFRecieverTable.AddCell( new PdfPCell( new Phrase( _FatturaRecieverHeader.Sede.CAP + " " + _FatturaRecieverHeader.Sede.Comune + " (" + _FatturaRecieverHeader.Sede.Provincia + ") - " + _FatturaRecieverHeader.Sede.Nazione ) ) { Border = Rectangle.NO_BORDER } ); // Third address line
            _PDFRecieverTable.AddCell( new PdfPCell( new Phrase( "P.IVA: " + _FatturaRecieverHeader.DatiAnagrafici.IdFiscaleIVA.IdPaese + " " + _FatturaRecieverHeader.DatiAnagrafici.IdFiscaleIVA.IdCodice ) ) { Border = Rectangle.NO_BORDER } ); // Fiscal ID Code + Fiscal ID
            _PDFRecieverTable.AddCell( new PdfPCell( new Phrase( "C.F.: " + _FatturaRecieverHeader.DatiAnagrafici.CodiceFiscale ) ) { Border = Rectangle.NO_BORDER } ); // Fiscal Code

            

               
        }

        private void InsertSenderHeaderData( PdfPTable _PDFSenderTable, CedentePrestatore _FatturaSenderHeader )
        {
            _PDFSenderTable.AddCell( new PdfPCell( new Phrase( _FatturaSenderHeader.DatiAnagrafici.Anagrafica?.Denominazione ) ) { Border = Rectangle.NO_BORDER } ); // First address line
            _PDFSenderTable.AddCell( new PdfPCell( new Phrase( _FatturaSenderHeader.Sede.Indirizzo + " " + _FatturaSenderHeader.Sede.NumeroCivico ) ) { Border = Rectangle.NO_BORDER } ); // Second address line
            _PDFSenderTable.AddCell( new PdfPCell( new Phrase( _FatturaSenderHeader.Sede.CAP + " " + _FatturaSenderHeader.Sede.Comune + " (" + _FatturaSenderHeader.Sede.Provincia + ") - " + _FatturaSenderHeader.Sede.Nazione ) ) { Border = Rectangle.NO_BORDER } ); // Third address line
            _PDFSenderTable.AddCell( new PdfPCell( new Phrase( "P.IVA: " + _FatturaSenderHeader.DatiAnagrafici.IdFiscaleIVA.IdPaese + " " + _FatturaSenderHeader.DatiAnagrafici.IdFiscaleIVA.IdCodice ) ) { Border = Rectangle.NO_BORDER } ); // Fiscal ID Code + Fiscal ID
            _PDFSenderTable.AddCell( new PdfPCell( new Phrase( "C.F.: " + _FatturaSenderHeader.DatiAnagrafici.CodiceFiscale ) ) { Border = Rectangle.NO_BORDER } ); // Fiscal Code
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
