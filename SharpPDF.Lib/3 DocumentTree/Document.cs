using System.Collections.Generic;
using System.Linq;

namespace SharpPDF.Lib {
    public class Document : IDocumentTree {
        private readonly SharpPdf pdf;
        private readonly IndirectObject indirectObject;
        List<IGraphicObject> graphicObjects = new List<IGraphicObject>();

        public Document(SharpPdf pdf) {
            this.pdf = pdf;
            this.indirectObject = pdf.CreateIndirectObject();
            pdf.SaveEvent += new SharpPdf.LoadCompleteHandler(onSaveEvent);
            pdf.AddChild(indirectObject, this);
        }

        private void onSaveEvent() {
            string text = string.Join("\n", graphicObjects);
            this.indirectObject.SetChild(new DictionaryObject(text));
        }

        public Document(SharpPdf pdf, IndirectObject obj) {            
            this.indirectObject = obj;
            this.pdf = pdf;

            ReadContent();
        } 

        public IndirectReferenceObject IndirectReferenceObject => indirectObject;

        public void ReadContent() {            
            var childs = indirectObject.Childs();
            if (childs == null || childs.Length == 0)
                return;
                
            var firstChild = indirectObject.Childs()[0];

            if (firstChild is DictionaryObject)
            {
                var dic = firstChild as DictionaryObject;

                this.graphicObjects = new GraphicObjectizer(dic.Stream).ReadObjects();;
            }            
        }

        public Document AddLabel(string text) {
            var textObject = graphicObjects.LastOrDefault() as TextObject;

            if (textObject == null) {
                textObject = new TextObject();
                graphicObjects.Add(textObject);
            }

            textObject.AddLabel(text);                
        
            return this;
        }

        public Document SetPosition(int x, int y) {            
            var textObject = graphicObjects.LastOrDefault() as TextObject;

            if (textObject == null) {
                textObject = new TextObject();
                graphicObjects.Add(textObject);
            }

            textObject.SetPosition(x, y);
            return this;
        }

        public IGraphicObject[] GraphicObject => graphicObjects.ToArray();
    }
}