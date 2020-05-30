namespace crcPdf {
    public interface IDocumentTree2 {   
        void Load(PDFObjects pdf, PdfObject pdfObject);

        void OnSaveEvent(IndirectObject indirectObject, PDFObjects pdfObjects);                
    }
}


