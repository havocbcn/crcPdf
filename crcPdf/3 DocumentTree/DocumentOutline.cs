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
namespace crcPdf {
    public class DocumentOutline : DocumentTree {
        public DocumentOutline()
        {
            
        }
        public override void Load(PDFObjects pdf, PdfObject pdfObject) { 
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

        public DocumentOutline first { get; private set; }
        public DocumentOutline last { get; private set; }
        public DocumentOutline parent { get; private set; }
        public DocumentOutline prev { get; private set; }
        public DocumentOutline next { get; private set; }
        public int Count { get; private set; }
        public string title { get; private set; }

        public override void OnSaveEvent(IndirectObject indirectObject, PDFObjects pdfObjects)
        {
            // TODO
        }
    }
}