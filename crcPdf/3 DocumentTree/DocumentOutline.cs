namespace crcPdf {
    public class DocumentOutline : IDocumentTree {

        public DocumentOutline(PDFObjects pdf) : base(pdf) {
        }

        public DocumentOutline(PDFObjects pdf, PdfObject pdfObject) : base(pdf) { 
            var contents = pdf.GetObject<DictionaryObject>(pdfObject);
            foreach (var value in contents.Dictionary) {
                switch (value.Key) {
                    case "Type":
                        break;
                    case "Title":
                        title = pdf.GetObject<StringObject>(value.Value).Value;
                        break;
                    case "Parent":
                        parent = pdf.GetDocument<DocumentOutline>(value.Value);
                        break;
                    case "First":
                        first = pdf.GetDocument<DocumentOutline>(value.Value);
                        break;
                    case "Last":
                        last = pdf.GetDocument<DocumentOutline>(value.Value);
                        break;
                    case "Prev":
                        prev = pdf.GetDocument<DocumentOutline>(value.Value);
                        break;
                    case "Next":
                        next = pdf.GetDocument<DocumentOutline>(value.Value);
                        break;
                    case "Count":
                        Count = pdf.GetObject<IntegerObject>(value.Value).IntValue;
                        break;
                    default:
                        throw new PdfException(PdfExceptionCodes.UNKNOWN_ENTRY, $"Outlines contain an unknown entry: {value.Key}");                        
                }
            }
        } 

        public DocumentOutline first { get; }
        public DocumentOutline last { get; }
        public DocumentOutline parent { get; }
        public DocumentOutline prev { get; }
        public DocumentOutline next { get; }
        public int Count { get; }
        public string title { get; }

        public override void OnSaveEvent(IndirectObject indirectObject)
        {
            // TODO
        }
    }
}