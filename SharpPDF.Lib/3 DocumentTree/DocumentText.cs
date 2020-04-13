using System.Collections.Generic;
using System.Linq;

namespace SharpPDF.Lib {
    public class DocumentText : IDocumentTree {        
        List<IGraphicObject> graphicObjects = new List<IGraphicObject>();

        public DocumentText(PDFObjects pdf) : base(pdf) {
        }

        public DocumentText(PDFObjects pdf, PdfObject pdfObject)  : base(pdf){            
            var dic = pdf.GetObject<DictionaryObject>(pdfObject);
                
            if (dic.Stream != null)
                this.graphicObjects = new GraphicObjectizer(dic.Stream).ReadObjects();                    
        } 

        private TextObject GetOrAddLastTextObject() {
            var textObject = graphicObjects.LastOrDefault() as TextObject;

            if (textObject == null) {
                textObject = new TextObject();
                graphicObjects.Add(textObject);
            }

            return textObject;
        }

        public void AddLabel(string text) => GetOrAddLastTextObject().AddLabel(text);

        public void SetPosition(int x, int y) => GetOrAddLastTextObject().SetPosition(x, y);

        public void SetFont(string code, int size) => GetOrAddLastTextObject().SetFont(code, size);

        public override void OnSaveEvent(IndirectObject indirectObject)
        {
            string text = string.Join("\n", graphicObjects);
            indirectObject.SetChild(new DictionaryObject(text));
        }

        public IGraphicObject[] GraphicObject => graphicObjects.ToArray();
    }
}