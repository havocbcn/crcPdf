using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpPDF.Lib
{
    public class SharpPdf
    {
        readonly Tokenizer tokenizer;

        readonly TokenValidator validator = new TokenValidator();

        private Dictionary<IndirectObject, IDocumentTree> childs = new Dictionary<IndirectObject, IDocumentTree>();
        
        public SharpPdf(MemoryStream ms) {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            tokenizer = new Tokenizer(ms);
            Analyze();
        }

        private int lastNumber = 0;
        internal IndirectObject CreateIndirectObject() => new IndirectObject(++lastNumber);
        internal void AddChild(IndirectObject id, IDocumentTree obj ) {
            childs.Add(id, obj);
        }
        public SharpPdf() {      
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);      
            Catalog = new DocumentCatalog(this);
        }

        public IDictionary<IndirectObject, IDocumentTree> Childs => childs;

        private void Analyze()
        {
            AnalyzeHeader();
            long xrefPosition = XrefPosition();
            ReadXRef(xrefPosition);
        }

        private void ReadXRef(long xrefPosition)
        {
            tokenizer.MoveToPosition(xrefPosition);

            Token token;

            token = tokenizer.Token();
            if (!validator.Validate(token, CharacterSetType.Regular, "xref"))
                throw new PdfException(PdfExceptionCodes.INVALID_XREF, "Expected xref");

            token = tokenizer.Token();
            if (!validator.IsWhiteSpace(token))
                throw new PdfException(PdfExceptionCodes.INVALID_XREF, "after xref must be a whitspace");

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

                if (type != "f" && type != "n")
                    throw new PdfException(PdfExceptionCodes.INVALID_XREF, "only xref f or n entries allowed");
                
                if (type == "f")    // free element
                    continue;

                tokenizer.SavePosition();
                tokenizer.MoveToPosition(pos);

                IndirectObject obj = new IndirectObject(tokenizer);
                childs.Add(obj, DocumentTreeFactory.Analyze(obj, this));
                tokenizer.RestorePosition();
            }

            // TODO: no se porque se ha leido ya el trailer, no debería
            if ( tokenizer.Token().ToString() != "trailer")
                throw new PdfException(PdfExceptionCodes.INVALID_TRAILER, "expected trailer");
            
            //if (!validator.IsDelimiter(tokenizer.TokenExcludedCommentsAndWhitespaces(), "trailer"))
              //  throw new PdfException(PdfExceptionCodes.INVALID_TRAILER, "Expected trailer");

            DictionaryObject trailer = new DictionaryObject(tokenizer);
            var rootIndirect = (IndirectReferenceObject)trailer.Dictionary["Root"];
            Catalog = (DocumentCatalog)this.childs[rootIndirect];

            LoadCompleteEvent();  
        }

        private long XrefPosition()
        {
            tokenizer.MoveToEnd();
            tokenizer.MoveToPreviousLine();
            tokenizer.MoveToPreviousLine();
            tokenizer.MoveToPreviousLine();
           
            Token token;

            token = tokenizer.Token();
            if (!validator.Validate(token, CharacterSetType.Regular, "startxref"))
                throw new PdfException(PdfExceptionCodes.INVALID_EOF, "Expected startxref");

            token = tokenizer.Token();
            if (!validator.IsWhiteSpace(token))
                throw new PdfException(PdfExceptionCodes.INVALID_EOF, "Expected a whitespace between starxref and position");

            token = tokenizer.Token();
            if (!validator.IsRegularNumber(token))
                throw new PdfException(PdfExceptionCodes.INVALID_EOF, "startxref position expected");

            long xrefPosition = token.ToLong();

            token = tokenizer.Token();
            if (!validator.IsWhiteSpace(token))
                throw new PdfException(PdfExceptionCodes.INVALID_EOF, "Expected a whitespace between starxref position and EOF");

            token = tokenizer.Token();
            if (!validator.IsDelimiter(token, "%"))
                throw new PdfException(PdfExceptionCodes.INVALID_EOF, "Expected %%EOF at end of the file");

            token = tokenizer.Token();
            if (!validator.IsDelimiter(token, "%"))
                throw new PdfException(PdfExceptionCodes.INVALID_EOF, "Expected %%EOF at end of the file");

            token = tokenizer.Token();
            if (!validator.Validate(token, CharacterSetType.Regular, "EOF"))
                throw new PdfException(PdfExceptionCodes.INVALID_EOF, "Expected %%EOF at end of the file");

            return xrefPosition;
        }

        private void AnalyzeHeader()
        {
            Token token = tokenizer.Token();
            if (!validator.IsDelimiter(token, "%"))            
                throw new PdfException(PdfExceptionCodes.HEADER_NOT_FOUND, "Header not found");

            token = tokenizer.Token();

            if (!validator.Validate(token, CharacterSetType.Regular, "PDF-1.1", "PDF-1.2", "PDF-1.3", "PDF-1.4", "PDF-1.5", "PDF-1.6", "PDF-1.7"))
                throw new PdfException(PdfExceptionCodes.HEADER_NOT_FOUND, "Header not found");            
        }

        public DocumentCatalog Catalog { get; private set; }
    
        internal delegate void LoadCompleteHandler();
        internal event LoadCompleteHandler LoadCompleteEvent;
        internal event LoadCompleteHandler SaveEvent;

        public void WriteTo(MemoryStream ms)
        {
            SaveEvent();
            
            string pdf = "%PDF-1.6\n";
            List<int> childPos = new List<int>();
            
            foreach (var child in childs) {
                childPos.Add(pdf.Length);
                pdf += child.Key.ToString();
            }

            int xrefPos = pdf.Length;

            
            pdf += $"xref\n0 {childs.Count + 1}\n0000000000 65535 f\n"; // +1 for the free record
            int i = 0;
            foreach (var child in childs) {
                pdf += $"{childPos[i++].ToString("D10")} 00000 n\n";
            }

            pdf += $"trailer <</Root {Catalog.IndirectReferenceObject} /Size {childs.Count + 1}>>\nstartxref\n{xrefPos}\n%%EOF";

            Console.WriteLine(pdf);
            byte[] existingData = System.Text.Encoding.UTF8.GetBytes(pdf);            
            ms.Write(existingData, 0, existingData.Length); 
        }
    }
}