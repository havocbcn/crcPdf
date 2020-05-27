namespace crcPdf {
    public abstract class IDocumentTree {
        internal readonly PDFObjects pdfObjects;

        protected IDocumentTree(PDFObjects pdfObject) {
            this.pdfObjects = pdfObject;            
        }        

        private IndirectObject cache;

        public IndirectReferenceObject IndirectReferenceObject 
        {
            get {
                if (cache != null) {
                    return cache;
                }

                var indirectObject = pdfObjects.CreateIndirectObject();
                cache = indirectObject;

                OnSaveEvent(indirectObject);                
                return indirectObject;
            }
        }

        public abstract void OnSaveEvent(IndirectObject indirectObject);        
    }
}