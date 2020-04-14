using System.IO;

namespace SharpPDF.Lib {
    public class SharpPdf {
        readonly Tokenizer tokenizer;
        
        PDFObjects pdfObjects = new PDFObjects();

        public DocumentCatalog Catalog { get; private set; }
        
        public SharpPdf(MemoryStream ms) {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            tokenizer = new Tokenizer(ms);
            Analyze();
        }

        public SharpPdf() {      
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);      
            Catalog = new DocumentCatalog(pdfObjects);
        }

        private void Analyze() {
            AnalyzeHeader();
            long xrefPosition = XrefPosition();
            ReadXRef(xrefPosition);
        }

        private void ReadXRef(long xrefPosition) {
            tokenizer.MoveToPosition(xrefPosition);

            Token token;

            token = tokenizer.Token();
            if (!TokenValidator.Validate(token, CharacterSetType.Regular, "xref")) {
                throw new PdfException(PdfExceptionCodes.INVALID_XREF, "Expected xref");
            }

            token = tokenizer.Token();
            if (!TokenValidator.IsWhiteSpace(token)) { 
                throw new PdfException(PdfExceptionCodes.INVALID_XREF, "after xref must be a whitspace");
            }

            tokenizer.GetInteger();                         // objectNumber
            tokenizer.TokenExcludedComments();
            int numberOfEntries = tokenizer.GetInteger();
            tokenizer.TokenExcludedComments();

            for (int i = 0; i < numberOfEntries; i++)
            {
                int pos = tokenizer.GetInteger();           // position
                tokenizer.TokenExcludedComments();          // whitespace
                tokenizer.GetInteger();                     // generation
                tokenizer.TokenExcludedComments();          // whitespace
                string type = tokenizer.Token().ToString(); // f or n
                tokenizer.TokenExcludedComments();          // whitespace

                if (type != "f" && type != "n") {
                    throw new PdfException(PdfExceptionCodes.INVALID_XREF, "only xref f or n entries allowed");
                }
                
                if (type == "f") {    // free element
                    continue;
                }

                tokenizer.SavePosition();
                tokenizer.MoveToPosition(pos);

                IndirectObject obj = new IndirectObject(tokenizer);
                pdfObjects.AddObject(obj);
                tokenizer.RestorePosition();
            }

            if ( tokenizer.Token().ToString() != "trailer") {
                throw new PdfException(PdfExceptionCodes.INVALID_TRAILER, "expected trailer");
            }

            var trailer = new DictionaryObject(tokenizer);
            var rootIndirect = (IndirectReferenceObject)trailer.Dictionary["Root"];

            Catalog = pdfObjects.GetDocument<DocumentCatalog>(rootIndirect);
        }

        private long XrefPosition()
        {
            tokenizer.MoveToEnd();
            tokenizer.MoveToPreviousLine();
            tokenizer.MoveToPreviousLine();
            tokenizer.MoveToPreviousLine();
           
            Token token;

            token = tokenizer.Token();
            if (!TokenValidator.Validate(token, CharacterSetType.Regular, "startxref")) {
                throw new PdfException(PdfExceptionCodes.INVALID_EOF, "Expected startxref");
            }

            token = tokenizer.Token();
            if (!TokenValidator.IsWhiteSpace(token)) {
                throw new PdfException(PdfExceptionCodes.INVALID_EOF, "Expected a whitespace between starxref and position");
            }

            token = tokenizer.Token();
            if (!TokenValidator.IsRegularNumber(token)) {
                throw new PdfException(PdfExceptionCodes.INVALID_EOF, "startxref position expected");
            }

            long xrefPosition = token.ToLong();

            token = tokenizer.Token();
            if (!TokenValidator.IsWhiteSpace(token)) {
                throw new PdfException(PdfExceptionCodes.INVALID_EOF, "Expected a whitespace between starxref position and EOF");
            }

            token = tokenizer.Token();
            if (!TokenValidator.IsDelimiter(token, "%")) {
                throw new PdfException(PdfExceptionCodes.INVALID_EOF, "Expected %%EOF at end of the file");
            }

            token = tokenizer.Token();
            if (!TokenValidator.IsDelimiter(token, "%")) {
                throw new PdfException(PdfExceptionCodes.INVALID_EOF, "Expected %%EOF at end of the file");
            }

            token = tokenizer.Token();
            if (!TokenValidator.Validate(token, CharacterSetType.Regular, "EOF")) {
                throw new PdfException(PdfExceptionCodes.INVALID_EOF, "Expected %%EOF at end of the file");
            }

            return xrefPosition;
        }

        private void AnalyzeHeader()
        {
            Token token = tokenizer.Token();
            if (!TokenValidator.IsDelimiter(token, "%")) {
                throw new PdfException(PdfExceptionCodes.HEADER_NOT_FOUND, "Header not found");
            }

            token = tokenizer.Token();

            if (!TokenValidator.Validate(token, CharacterSetType.Regular, "PDF-1.1", "PDF-1.2", "PDF-1.3", "PDF-1.4", "PDF-1.5", "PDF-1.6", "PDF-1.7")) {
                throw new PdfException(PdfExceptionCodes.HEADER_NOT_FOUND, "Header not found");            
            }
        }

        public void WriteTo(MemoryStream ms)
        {
            pdfObjects.WriteTo(ms, Catalog);
        }
    }
}