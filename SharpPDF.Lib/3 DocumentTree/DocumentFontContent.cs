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
using System.IO;

namespace SharpPDF.Lib {
	public class DocumentFontContent : IDocumentTree  {		
		private readonly byte[] font;
        public DocumentFontContent(PDFObjects pdf, PdfObject pdfObject) : base(pdf) {
            var dic = pdf.GetObject<DictionaryObject>(pdfObject);
            font = dic.Stream;
        }

        public DocumentFontContent(PDFObjects pdf, byte[] font) : base(pdf) {
            this.font = font;
        }

        public override void OnSaveEvent(IndirectObject indirectObject)
        {          
            var entries = new Dictionary<string, PdfObject> {
                { "Length", new IntegerObject(font.Length) },
                { "Length1", new IntegerObject(font.Length) }
            };   
               
            indirectObject.SetChild(new DictionaryObject(entries, font));
        }	

    }
}
