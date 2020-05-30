using System;

namespace crcPdf {
    public abstract class DocumentTree : IDocumentTree {
        
        internal readonly Guid guid = Guid.NewGuid();

        public override bool Equals(object obj)
        {           
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            
            return guid == (obj as DocumentTree).guid;
        }
        
        public override int GetHashCode()
            => guid.GetHashCode();
        
        public IndirectReferenceObject IndirectReferenceObject(PDFObjects pdfObjects) 
        {        
            var indirectObject = pdfObjects.CacheGuid(guid);
            if (indirectObject != null) {
                return indirectObject;
            }

            indirectObject = pdfObjects.CreateIndirectObject(guid);        
            OnSaveEvent(indirectObject, pdfObjects);                
            return indirectObject;         
        }    

        public virtual void Load(PDFObjects pdf, PdfObject pdfObject) { }

        public virtual void OnSaveEvent(IndirectObject indirectObject, PDFObjects pdfObjects) { }
    }
}