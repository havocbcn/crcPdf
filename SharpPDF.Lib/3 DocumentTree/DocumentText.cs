using System.Collections.Generic;
using System.Linq;

namespace SharpPDF.Lib {
    public class DocumentText : IDocumentTree {        
        List<Operator> pageOperators = new List<Operator>();

        public DocumentText(PDFObjects pdf) : base(pdf) {
        }

        public DocumentText(PDFObjects pdf, PdfObject pdfObject)  : base(pdf) {            
            var dic = pdf.GetObject<DictionaryObject>(pdfObject);
                
            if (dic.Stream != null) {
                this.pageOperators = new PageOperator(dic.Stream).ReadObjects();                    
            }
        } 

        private bool IsLastOperatorATextObject()
            => pageOperators.LastOrDefault() is TextObject;

        private TextObject GetOrAddLastTextObject() {
            var textObject = pageOperators.LastOrDefault() as TextObject;

            if (textObject == null) {
                textObject = new TextObject();
                pageOperators.Add(textObject);
            }

            return textObject;
        }

        public void AddLabel(string text) 
            => GetOrAddLastTextObject().AddLabel(text);

        public void SetPosition(int x, int y) 
            => GetOrAddLastTextObject().SetPosition(x, y);

        public void SetFont(string code, float size) 
            => GetOrAddLastTextObject().SetFont(code, size);

        public void SetLineCap(LineCapStyle lineCap) {
            if (IsLastOperatorATextObject()) {
                GetOrAddLastTextObject().SetLineCap(lineCap);
            } else {
                pageOperators.Add(new LineCapOperator(lineCap));
            }
        }

        public void SetNonStrokingColour(float r, float g, float b) {
            if (IsLastOperatorATextObject()) {
                GetOrAddLastTextObject().SetNonStrokingColour(r, g, b);
            } else {
                pageOperators.Add(new NonStrokingColourOperator(r, g, b));
            }
        }      

        public override void OnSaveEvent(IndirectObject indirectObject) {
            string text = string.Join(" ", pageOperators);
            indirectObject.SetChild(new DictionaryObject(text));
        }

        public Operator[] PageOperators => pageOperators.ToArray();
        public T PageOperator<T>(int index) where T: Operator => pageOperators[index] as T;

        public void AddRectangle(float x, float y, float width, float height)
            => pageOperators.Add(new RectangleOperator(x, y, width, height));

        public void AddFill() {
            if (!(pageOperators.LastOrDefault() is PathPaintingOperator || 
                pageOperators.LastOrDefault() is PathConstructionOperator)) {
                throw new PdfException(PdfExceptionCodes.INVALID_OPERATOR, "A Path Painting Operator must appear after a Path Construction Operator or another Path Painting Operator");
            }
            
            pageOperators.Add(new FillOperator());
        }

        public void AddStroke() {
            if (!(pageOperators.LastOrDefault() is PathPaintingOperator || 
                pageOperators.LastOrDefault() is PathConstructionOperator)) {
                throw new PdfException(PdfExceptionCodes.INVALID_OPERATOR, "A Path Painting Operator must appear after a Path Construction Operator or another Path Painting Operator");
            }

            pageOperators.Add(new StrokeOperator());
        }

        internal void AddImage(string imageCode) => pageOperators.Add(new ImageOperator(imageCode));

        public void SaveGraph() => pageOperators.Add(new SaveGraphOperator());

        public void RestoreGraph() => pageOperators.Add(new RestoreGraphOperator());

        public void CurrentTransformationMatrix(float a, float b, float c, float d, float e, float f) 
            => pageOperators.Add(new CurrentTransformationMatrixOperator(a, b, c, d, e, f));
    }
}