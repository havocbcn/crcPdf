// This file is part of crcPdf.
// 
// crcPdf is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// crcPdf is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with crcPdf.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using crcPdf.Fonts;
using crcPdf.Images;

namespace crcPdf {
    public class DocumentPage : DocumentTree {        
        private DocumentText contents;
        private DocumentPageTree parent;
        private readonly Dictionary<DocumentFont, string> fonts = new Dictionary<DocumentFont, string>();
        private readonly Dictionary<string, DocumentFont> reverseFonts = new Dictionary<string, DocumentFont>();
        private readonly Dictionary<DocumentImage, string> images = new Dictionary<DocumentImage, string>();        
        private readonly Dictionary<string, DocumentImage> reverseImages = new Dictionary<string, DocumentImage>();        
        private readonly List<string> procsets = new List<string>();
        public Rectangle MediaBox {get; private set; }
        public IReadOnlyDictionary<string, DocumentFont> Font => reverseFonts;
        public IReadOnlyDictionary<string, DocumentImage> Image => reverseImages;
        public DocumentFont CurrentFont { get; private set;}
        public string[] Procsets => procsets.ToArray();

        public DocumentPage(){            
        }        
        
        public DocumentPage(DocumentPageTree parent) {   
            this.parent = parent;
            this.contents = new DocumentText();
        }

        public override void Load(PDFObjects pdf, PdfObject pdfObject) {
            var dic = pdf.GetObject<DictionaryObject>(pdfObject);            

            if (dic.Dictionary.ContainsKey("Parent")) {                
                parent = pdf.GetDocument<DocumentPageTree>(dic.Dictionary["Parent"]);
            }

            if (dic.Dictionary.ContainsKey("MediaBox")) {                
                var mediaBox = pdf.GetObject<ArrayObject>(dic.Dictionary["MediaBox"]);
                
                MediaBox = new Rectangle(
                    pdf.GetObject<RealObject>(mediaBox.childs[0]).Value,
                    pdf.GetObject<RealObject>(mediaBox.childs[1]).Value,
                    pdf.GetObject<RealObject>(mediaBox.childs[2]).Value,
                    pdf.GetObject<RealObject>(mediaBox.childs[3]).Value);
            }

            if (dic.Dictionary.ContainsKey("Resources")) {
                var resources = pdf.GetObject<DictionaryObject>(dic.Dictionary["Resources"]);

                foreach (var resource in resources.Dictionary) {
                    switch (resource.Key) {
                        case "Font":
                            foreach (var font in pdf.GetObject<DictionaryObject>(resource.Value).Dictionary) {
                                var documentFont = FontFactory.GetFont(pdf, font.Value);
                                fonts.Add(documentFont, font.Key);
                                reverseFonts.Add(font.Key, documentFont);
                            }
                            break;
                        case "XObject":
                            foreach (var image in pdf.GetObject<DictionaryObject>(resource.Value).Dictionary) {
                                var documentImage = ImageFactory.GetImage(pdf, image.Value);
                                images.Add(documentImage, image.Key);
                                reverseImages.Add(image.Key, documentImage);
                            }
                            break;
                        case "ProcSet":
                            //14.2 Procedure Sets
                            var arrayobject = pdf.GetObject<ArrayObject>(resource.Value);
                            foreach (var item in arrayobject.Childs<NameObject>()) {
                                procsets.Add(item.Value);
                            }
                            break;
                        default:
                            // TODO
                            throw new PdfException(PdfExceptionCodes.INVALID_RESOURCE, $"Not supported resource found {resource.Key}");
                    } 
                }
            }

            contents = pdf.GetDocument<DocumentText>(dic.Dictionary["Contents"]);
        }

    
        public DocumentPage SetMediaBox(Rectangle rectangle) {
            MediaBox = rectangle;
            return this;
        }

        public DocumentPage SetTextPositioning(float x, float y) {
            contents.AddSetTextPositioning(x, y);
            return this;
        }

        public DocumentPage AddNonStrokingColour(float r, float g, float b) {
            contents.AddNonStrokingColour(r, g, b);
            return this;
        }

        public DocumentPage SetTextMatrix(float a, float b, float c, float d, float e, float f) {
            contents.SetTextMatrix(a, b, c, d, e, f);
            return this;
        }
      

        public DocumentText Contents 
            => contents;
      
        public DocumentPage AddLabel(string text) {
            if (CurrentFont == null) {
                throw new PdfException(PdfExceptionCodes.FONT_ERROR, $"A font must be set before writting");
            }
            
            contents.AddLabel(CurrentFont.SetText(text));
            return this;
        }

        public DocumentPage SetPosition(int x, int y) {            
            contents.SetPosition(x, y);
            return this;
        }

        public DocumentPage SetNonStrokingColour(float r, float g, float b)
        {
            contents.SetNonStrokingColour(r, g, b);
            return this;
        }

        public DocumentPage SetFont(string name, int size, bool isBold, bool isItalic) 
            => SetFont(name, size, isBold, isItalic, Embedded.No);

        public DocumentPage SetFont(string name, int size) 
            => SetFont(name, size, false, false, Embedded.No);

        public DocumentPage SetFont(string name, int size, Embedded embedded) 
            => SetFont(name, size, false, false, embedded);

        public DocumentPage SetFont(string name, int size, bool isBold, bool isItalic, Embedded embedded)
            => SetFont(FontFactory.GetFont(name, isBold, isItalic, embedded), size);

        public DocumentPage SetFont(DocumentFont font, int size) {
            if (!fonts.ContainsKey(font)) {
                var key = "F" + fonts.Count;
                fonts.Add(font, key);
                reverseFonts.Add(key, font);
            }

            CurrentFont = font;
            contents.SetFont(fonts[font], size);
            return this;
        }


        public DocumentPage SetLineCap(LineCapStyle lineCap)  {
            contents.SetLineCap(lineCap);
            return this;
        }

        public DocumentPage AddRectangle(float x, float y, float width, float height) {
            contents.AddRectangle(x, y, width, height);
            return this;
        }

        public DocumentPage AddStroke()
        {
            contents.AddStroke();
            return this;
        }

        public DocumentPage AddFill()
        {
            contents.AddFill();
            return this;
        }

        public DocumentPage AddImage(string fullFilePath) {
            var image = ImageFactory.GetImage(fullFilePath);

            if (!images.ContainsKey(image)) {
                var key = "I" + images.Count;
                images.Add(image, key);
                reverseImages.Add(key, image);
            }

            contents.AddImage(images[image]);
            return this;
        }

          public DocumentPage AddImage(byte[] image) {
            var documentIimage = ImageFactory.GetImage(image);

            if (!images.ContainsKey(documentIimage)) {
                var key = "I" + images.Count;
                images.Add(documentIimage, key);
                reverseImages.Add(key, documentIimage);
            }

            contents.AddImage(images[documentIimage]);
            return this;
        }

        public DocumentPage SaveGraph() {
            contents.SaveGraph();
            return this;
        }

        public DocumentPage RestoreGraph() {
            contents.RestoreGraph();
            return this;
        }

        public DocumentPage CurrentTransformationMatrix(float a, float b, float c, float d, float e, float f) {
            contents.CurrentTransformationMatrix(a, b, c, d, e, f);
            return this;
        }

        public override void OnSaveEvent(IndirectObject indirectObject, PDFObjects pdfObjects)
        {
            var resourceEntries = new Dictionary<string, PdfObject>();

            if (fonts.Count > 0) {                
                var dicFonts = new Dictionary<string, PdfObject>();

                foreach (var font in fonts) {
                    dicFonts.Add(font.Value, font.Key.IndirectReferenceObject(pdfObjects));
                }

                resourceEntries.Add("Font", new DictionaryObject(dicFonts));
            }

             if (images.Count > 0) {                
                var dicImages = new Dictionary<string, PdfObject>();

                foreach (var img in images) {
                    dicImages.Add(img.Value, img.Key.IndirectReferenceObject(pdfObjects));
                }

                resourceEntries.Add("XObject", new DictionaryObject(dicImages));
            }

            if (procsets.Count > 0) {
                List<PdfObject> lstObjects = new List<PdfObject>();
                foreach (var procSet in procsets) {
                    lstObjects.Add(new NameObject(procSet));
                }
                resourceEntries.Add("ProcSet", new ArrayObject(lstObjects));
            }

            var entries = new Dictionary<string, PdfObject> {
                { "Type", new NameObject("Page") },
                { "Parent", parent.IndirectReferenceObject(pdfObjects)},
                { "Contents", contents.IndirectReferenceObject (pdfObjects)}
            };                        

            if (resourceEntries.Count > 0) {                
                entries.Add("Resources", new DictionaryObject(resourceEntries));
            }

            if (MediaBox != null) {
                entries.Add("MediaBox", new ArrayObject(MediaBox.ToArrayObject()));
            }

            indirectObject.SetChild(new DictionaryObject(entries));
        }
    }
}