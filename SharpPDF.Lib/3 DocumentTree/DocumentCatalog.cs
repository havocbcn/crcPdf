using System.Collections.Generic;

namespace SharpPDF.Lib {
    // 7.7.2 Document Catalog
    public class DocumentCatalog : IDocumentTree {
        private DocumentPageTree pageTree;
        private DocumentOutline outlines;
        
        public DocumentCatalog(PDFObjects pdf) : base(pdf) {            
            this.pageTree = new DocumentPageTree(pdf);
        }

        public DocumentCatalog(PDFObjects pdf, PdfObject pdfObject) : base(pdf) {
            var dictionary = pdf.GetObject<DictionaryObject>(pdfObject);
            pageTree = pdf.GetDocument<DocumentPageTree>(dictionary.Dictionary["Pages"]);

            if (dictionary.Dictionary.ContainsKey("Outlines")) {
                outlines = pdf.GetDocument<DocumentOutline>(dictionary.Dictionary["Outlines"]);
            }
        }

        public override void OnSaveEvent(IndirectObject indirectObject)
        {
             var dic = new Dictionary<string, PdfObject> {
                { "Type", new NameObject("Catalog") },
                { "Pages", pageTree.IndirectReferenceObject }
            };

            if (Outlines != null) {
                dic.Add("Outlines", outlines.IndirectReferenceObject);
            }

            indirectObject.SetChild(new DictionaryObject(dic));
        }

        public DocumentPageTree Pages => pageTree;
        public DocumentOutline Outlines => outlines;
    }
}