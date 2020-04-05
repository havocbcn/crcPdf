using System.Collections.Generic;

namespace SharpPDF.Lib {
    // 7.7.2 Document Catalog
    public class DocumentCatalog : IDocumentTree
    {
        private readonly SharpPdf pdf;
        private DocumentPageTree pageTree;
        private readonly IndirectObject indirectObject;
        
        public DocumentCatalog(SharpPdf pdf) {
            this.pdf = pdf;
            this.pageTree = new DocumentPageTree(pdf);

            this.indirectObject = pdf.CreateIndirectObject();
            this.indirectObject.SetChild(new DictionaryObject(
                new Dictionary<string, IPdfObject>
                {
                    { "Type", new NameObject("Catalog") },
                    { "Pages", pageTree.IndirectReferenceObject },
                }
            ));

            pdf.AddChild(indirectObject, this);
        }

        public DocumentCatalog(IndirectObject indirectObject, SharpPdf pdf) {
            this.indirectObject = indirectObject;
            this.pdf = pdf;        
            pdf.LoadCompleteEvent += new SharpPdf.LoadCompleteHandler(onLoadComplete);    
        }

        private void onLoadComplete() {
            var dic = indirectObject.Childs()[0] as DictionaryObject;

            var pageReference = (IndirectReferenceObject)dic.Dictionary["Pages"];
            pageTree = (DocumentPageTree)pdf.Childs[pageReference];
        }

        public IndirectReferenceObject IndirectReferenceObject => indirectObject;
        
        public DocumentPageTree Pages => pageTree;
    }
}