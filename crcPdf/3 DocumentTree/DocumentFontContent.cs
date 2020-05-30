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
	public class DocumentFontContent : DocumentTree  {		
		private readonly byte[] font;
        public DocumentFontContent(PDFObjects pdf, PdfObject pdfObject){
            var dic = pdf.GetObject<DictionaryObject>(pdfObject);
            font = dic.Stream;
        }

        public DocumentFontContent() {
        }

        public DocumentFontContent(byte[] font) {
            this.font = font;
        }

        public override void OnSaveEvent(IndirectObject indirectObject, PDFObjects pdf)
        {          
            var entries = new Dictionary<string, PdfObject> {
                { "Length", new IntegerObject(font.Length) },
                { "Length1", new IntegerObject(font.Length) }
            };   
               
            indirectObject.SetChild(new DictionaryObject(entries, font));
        }	

    }
}
