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

namespace crcPdf {
	public class DocumentTtfDescriptorFont : DocumentTree  {
        private readonly DocumentTtfFontBase font;

        public DocumentTtfDescriptorFont(DocumentTtfFontBase font) {
            this.font = font;
        }

        public override void OnSaveEvent(IndirectObject indirectObject, PDFObjects pdfObjects)
        {
            var entries = new Dictionary<string, PdfObject> {
                { "Type", new NameObject("FontDescriptor") },
                { "StemV", new IntegerObject(font.StemV) },
                { "Flags", new IntegerObject((int)font.Flags) },
                { "FontName", new NameObject(font.Name) },
                { "FontBBox", new ArrayObject(new List<PdfObject> {
                    new IntegerObject(font.boundingBox[0]),
                    new IntegerObject(font.boundingBox[1]),
                    new IntegerObject(font.boundingBox[2]),
                    new IntegerObject(font.boundingBox[3])
                })},
                { "ItalicAngle", new IntegerObject(font.ItalicAngle) },
                { "Ascent", new IntegerObject(font.Ascendent) },
                { "Descent", new IntegerObject(font.Descendent) },
                { "CapHeight", new IntegerObject(font.CapHeight) },
            };   

            if (font.Leading > 0) {
                entries.Add("Leading", new IntegerObject(font.Leading));
            }           

            if (font.IsEmbedded) {
                var fontContent = new DocumentFontContent(font.FontByteArray);
                entries.Add("FontFile2", fontContent.IndirectReferenceObject(pdfObjects));
            }
            
            indirectObject.SetChild(new DictionaryObject(entries));
        }
    }
}
