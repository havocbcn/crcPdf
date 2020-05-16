namespace SharpPDF.Lib {
    public abstract class DocumentImage : IDocumentTree {

        public DocumentImage(PDFObjects pdf) : base(pdf) {
        }

        public DocumentImage(PDFObjects pdf, PdfObject pdfObject) : base(pdf) {             
        }

        public abstract int Width { get; }
        public abstract int Height { get; }
    }
}