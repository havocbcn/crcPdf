namespace crcPdf {
    public interface IDocumentTree {
   
        IndirectReferenceObject IndirectReferenceObject(PDFObjects pdfObjects);
        
        void Load(PDFObjects pdf, PdfObject pdfObject);

        void OnSaveEvent(IndirectObject indirectObject, PDFObjects pdfObjects);        
    }
}