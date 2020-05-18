// This file is part of SharpPDF.
// 
// SharpPDF is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// SharpPDF is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with SharpPDF.  If not, see <http://www.gnu.org/licenses/>.
using System.Collections.Generic;

namespace SharpPDF.Lib {
	public class DocumentDescendantFont : IDocumentTree  {		
        private readonly DocumentTtfFontBase font;

        public DocumentDescendantFont(PDFObjects pdf, DocumentTtfFontBase font) : base(pdf) {
            this.font = font;
        }

        public DocumentDescendantFont(PDFObjects pdf, PdfObject pdfObject) : base(pdf) {
            var dic = pdf.GetObject<DictionaryObject>(pdfObject);            

            //font.StemV = pdf.GetObject<IntegerObject>(dic.Dictionary["FirstChar"]).IntValue;
        }


        public override void OnSaveEvent(IndirectObject indirectObject)
        {          
            var widths = new List<PdfObject>();            
            int[] glyphList = new int[this.font.hashChar.Count];
            font.hashChar.CopyTo(glyphList);
            
            for (int i = 0; i < glyphList.Length; i++) {
                glyphList[i] = font.GetGlyphId(glyphList[i]);
            }

            int previousGlypthId = -100;
            var arrayObjects = new List<PdfObject>();
            foreach (int glyphIndex in glyphList) { 
                if (previousGlypthId == glyphIndex - 1) {
                    arrayObjects.Add(new IntegerObject(font.GetGlyph(glyphIndex).width));
                } else {
                    if (arrayObjects.Count > 0)
                        widths.Add(new ArrayObject(arrayObjects));
                    widths.Add(new IntegerObject(glyphIndex));
                    arrayObjects = new List<PdfObject>();
                    arrayObjects.Add(new IntegerObject(font.GetGlyph(glyphIndex).width));
                }
            }
            if (arrayObjects.Count > 0)
                widths.Add(new ArrayObject(arrayObjects));
        

            var descriptor = new DocumentTtfDescriptorFont(pdfObjects, font);
            
            var entries = new Dictionary<string, PdfObject> {
                { "Type", new NameObject("Font") },
                { "Subtype", new NameObject("CIDFontType2") },
                { "BaseFont", new NameObject(font.Name) },
                { "CIDSystemInfo", new DictionaryObject(new Dictionary<string, PdfObject> {
                    { "Registry", new StringObject("Adobe") },
                    { "Ordering", new StringObject("Identity") },
                    { "Supplement", new IntegerObject(0) }
                })},
                { "CIDToGIDMap", new NameObject("Identity") },
                { "FontDescriptor", descriptor.IndirectReferenceObject },
                { "DW", new IntegerObject(this.font.Width) },
                { "W", new ArrayObject(widths) },
            };   
   
            indirectObject.SetChild(new DictionaryObject(entries));
        }	

    }
}
