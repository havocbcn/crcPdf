namespace SharpPDF.Lib {
    public abstract class DocumentImage : IDocumentTree {

        public DocumentImage(PDFObjects pdf) : base(pdf) {
        }

        public DocumentImage(PDFObjects pdf, PdfObject pdfObject) : base(pdf) { 
            var contents = pdf.GetObject<DictionaryObject>(pdfObject);            
        }

        public abstract int Width { get; }
        public abstract int Height { get; }
    }
}