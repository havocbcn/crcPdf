using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpPDF.Lib {
    public class DocumentPage : IDocumentTree {        
        private Document contents;
        private DocumentPageTree parent;
        private readonly SharpPdf pdf;
        private readonly IndirectObject indirectObject;
        DictionaryObject resources;

        public IndirectReferenceObject IndirectReferenceObject => indirectObject;

        public DocumentPage(IndirectObject indirectObject, SharpPdf pdf) {            
            this.indirectObject = indirectObject;
            this.pdf = pdf;                       
            pdf.LoadCompleteEvent += new SharpPdf.LoadCompleteHandler(onLoadComplete);
        }
    
        public DocumentPage(SharpPdf pdf, DocumentPageTree parent) {
            this.pdf = pdf;
            this.parent = parent;
            this.contents = new Document(pdf);
            this.indirectObject = pdf.CreateIndirectObject();
            pdf.SaveEvent += new SharpPdf.LoadCompleteHandler(onSaveEvent);            
            pdf.AddChild(indirectObject, this);
        }

        private void onSaveEvent()
        {
            this.indirectObject.SetChild(new DictionaryObject(
                new Dictionary<string, IPdfObject>
                {
                    { "Type", new NameObject("Page") },
                    { "Parent", parent.IndirectReferenceObject },
                    { "Contents", contents.IndirectReferenceObject },
                }
            ));
        }

        private void onLoadComplete() {
            var dic = indirectObject.Childs()[0] as DictionaryObject;
            var contentsReference = (IndirectReferenceObject)dic.Dictionary["Contents"];
            if (dic.Dictionary.ContainsKey("Parent")) {
                var parentReference = (IndirectReferenceObject)dic.Dictionary["Parent"];
                parent = (DocumentPageTree)pdf.Childs[parentReference];
            }
            if (dic.Dictionary.ContainsKey("Resources"))
                resources = (DictionaryObject)dic.Dictionary["Resources"];

            if (!pdf.Childs.ContainsKey(contentsReference))
                throw new PdfException(PdfExceptionCodes.INDIRECT_REFERENCE_MISSING, $"{Contents} is missing");
                    
            contents = (Document)pdf.Childs[contentsReference];
        }

        public Document Contents => contents;

        public DocumentPageTree Parent => parent;      
    }
}