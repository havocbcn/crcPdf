using System.Collections.Generic;
using System.Linq;

namespace SharpPDF.Lib {
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

        private DocumentOutline first;
        private DocumentOutline last;
        private DocumentOutline parent;
        private DocumentOutline prev;
        private DocumentOutline next;
        public int Count { get; private set; }
        private string title;

        public override void OnSaveEvent(IndirectObject indirectObject)
        {
            // TODO
        }
    }
}