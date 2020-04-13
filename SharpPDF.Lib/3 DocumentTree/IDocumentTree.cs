namespace SharpPDF.Lib {
    public abstract class IDocumentTree {
        internal readonly PDFObjects pdfObjects;

        public IDocumentTree(PDFObjects pdfObject) {
            this.pdfObjects = pdfObject;            
        }        

        private IndirectObject cache;

        public IndirectReferenceObject IndirectReferenceObject 
        {
            get {
                if (cache != null)
                    return cache;

                var indirectObject = pdfObjects.CreateIndirectObject();
                cache = indirectObject;

                OnSaveEvent(indirectObject);                
                return indirectObject;
            }
        }

        public abstract void OnSaveEvent(IndirectObject indirectObject);        
    }
}