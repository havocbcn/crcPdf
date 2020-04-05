using System.Collections.Generic;
using System.Linq;

namespace SharpPDF.Lib {
    public class DocumentPageTree : IDocumentTree {
        private readonly SharpPdf pdf;
        private readonly IndirectObject indirectObject;
        private DocumentPageTree parent;
        private List<DocumentPageTree> pageTreeSons;
        private List<DocumentPage> pageSons;

        public DocumentPageTree(SharpPdf pdf)
        {
            this.pdf = pdf;
            pageSons = new List<DocumentPage>();
            pageTreeSons = new List<DocumentPageTree>();
            indirectObject = pdf.CreateIndirectObject();
            pdf.SaveEvent += new SharpPdf.LoadCompleteHandler(onSaveEvent);
            pdf.AddChild(indirectObject, this);
        }

        private void onSaveEvent()
        {            
            List<IPdfObject> kids = new List<IPdfObject>();
            kids.AddRange(pageTreeSons.Select(p => p.IndirectReferenceObject));
            kids.AddRange(pageSons.Select(p => p.IndirectReferenceObject));            

            this.indirectObject.SetChild(new DictionaryObject(
                new Dictionary<string, IPdfObject>
                {
                    { "Type", new NameObject("Pages") },
                    { "Kids", new ArrayObject(kids) },
                    { "Count", new IntegerObject(kids.Count) },
                }
            ));
        }

        public DocumentPageTree(IndirectObject indirectObject, SharpPdf pdf)
        {
            this.pdf = pdf;
            this.indirectObject = indirectObject;

            pdf.LoadCompleteEvent += new SharpPdf.LoadCompleteHandler(onLoadComplete);
        }

        private void onLoadComplete()
        {
            var dic = indirectObject.Childs()[0] as DictionaryObject;            

            if (dic.Dictionary.ContainsKey("Parent")) {
                var parentReference = (IndirectReferenceObject)dic.Dictionary["Parent"];
                parent = (DocumentPageTree)pdf.Childs[parentReference];
            }

            var dicKids = (ArrayObject)dic.Dictionary["Kids"];
            var KidsIndirect = dicKids.Childs();

            var kidsReference = new IndirectReferenceObject[KidsIndirect.Length];

            for (int i = 0; i < ((IntegerObject)dic.Dictionary["Count"]).Value; i++)
                kidsReference[i] = (IndirectReferenceObject)KidsIndirect[i];

            pageTreeSons = new List<DocumentPageTree>();
            foreach (var kid in kidsReference)
            {
                var child = pdf.Childs[kid];
                if (child is DocumentPageTree)
                    pageTreeSons.Add((DocumentPageTree)child);
            }

            pageSons = new List<DocumentPage>();
            foreach (var kid in kidsReference)
            {
                var child = pdf.Childs[kid];
                if (child is DocumentPage)
                    pageSons.Add((DocumentPage)child);
            }
        }

        public DocumentPageTree[] PageTreeSons => pageTreeSons.ToArray();
        public DocumentPage[] PageSons => pageSons.ToArray();

        public DocumentPageTree Parent => parent;

        public IndirectReferenceObject IndirectReferenceObject => indirectObject;

        public Document AddPage()
        {
            var page = new DocumentPage(pdf, this);
            pageSons.Add(page);            

            return page.Contents;

            /*
            XrefResources resources = new XrefResources();
			contents = new XrefContents(m_useCompression);
            XrefPage page = new XrefPage(resources, contents, width, height);
            pageTree.AddPage(page);
            */
        }


    }
}