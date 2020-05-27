// This file is part of crcPdf.
// 
// crcPdf is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// crcPdf is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with crcPdf.  If not, see <http://www.gnu.org/licenses/>.
using System.IO;

namespace crcPdf {
    public class crcPdf {
        readonly Tokenizer tokenizer;
        
        PDFObjects pdfObjects = new PDFObjects();

        public DocumentCatalog Catalog { get; private set; }        
        
        public crcPdf(MemoryStream ms) {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            tokenizer = new Tokenizer(ms);
            Analyze();
        }

        public crcPdf() {      
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);      
            Catalog = new DocumentCatalog(pdfObjects);
        }

        private void Analyze() {
            AnalyzeHeader();
            ReadXRef(XrefPosition());
        }

        private void ReadXRef(long xrefPosition) {
            tokenizer.MoveToPosition(xrefPosition);

            Token token = tokenizer.Token();
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

            for (int i = 0; i < numberOfEntries; i++) {
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

        private long XrefPosition() {
            tokenizer.MoveToEnd();
            tokenizer.MoveToPrevious('s');
           
            Token token = tokenizer.Token();
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
            if (!TokenValidator.IsDelimiter(token)) {
                token = tokenizer.Token();
            }

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

        private void AnalyzeHeader() {
            Token token = tokenizer.Token();
            if (!TokenValidator.IsDelimiter(token, "%")) {
                throw new PdfException(PdfExceptionCodes.HEADER_NOT_FOUND, "Header not found");
            }

            token = tokenizer.Token();

            if (!TokenValidator.Validate(token, CharacterSetType.Regular, "PDF-1.1", "PDF-1.2", "PDF-1.3", "PDF-1.4", "PDF-1.5", "PDF-1.6", "PDF-1.7")) {
                throw new PdfException(PdfExceptionCodes.HEADER_NOT_FOUND, "Header not found");            
            }
        }

        public void WriteTo(Stream ms) 
            => pdfObjects.WriteTo(ms, Catalog, Compression.Compress);        

        public void WriteTo(Stream ms, Compression compression) 
            => pdfObjects.WriteTo(ms, Catalog, compression);
    }
}