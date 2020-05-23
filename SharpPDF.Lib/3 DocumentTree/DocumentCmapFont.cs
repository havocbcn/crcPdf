// This file is part of SharpPdf.
// 
// SharpPdf is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// SharpPdf is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with SharpPdf.  If not, see <http://www.gnu.org/licenses/>.

using System.Text;

namespace SharpPDF.Lib
{
	/// <summary>
	/// Representa el mapeo de glyph a unicode (CMAP)
	/// </summary>
	public class DocumentCmapFont : IDocumentTree
	{		
        private readonly DocumentTtfFontBase font;

        public DocumentCmapFont(PDFObjects pdf, DocumentTtfFontBase font) : base(pdf) {
            this.font = font;
        }

        public DocumentCmapFont(PDFObjects pdf, PdfObject pdfObject) : base(pdf) {
        }

		public override void OnSaveEvent(IndirectObject indirectObject) {           
			//	/CIDInit		/ProcSet findresource begin 12
			//		
			// 	/CIDSystemInfo
			//		/Registry 	A string identifying the issuer of the character collection. 
			//					For information about assigning a registry identifier, contact the Adobe   
			//					Solutions Network or consult the ASN Web site (see the Bibliography).
			//		/Ordering	A string that uniquely names the character collection within the specified registry.
			//		/Supplement	The supplement number of the character collection. An original character collection
			//					has a supplement number of  0. Whenever additional CIDs are assigned in a character 
			//					collection, the supplement number shall be increased. Supplements shall not alter 
			//					the ordering of existing CIDs in the character collection. This value shall not be 
			//					used in determining compatibility between character collections. 
            StringBuilder sb = new StringBuilder();

            sb.Append(@"/CIDInit/ProcSet findresource begin 12 dict begin begincmap/CIDSystemInfo <</Registry (QWERT)/Ordering (ASDFG)/Supplement 0>> def /CMapName /QWERT def/CMapType 2 def 1 begincodespacerange
<00> <FF>
endcodespacerange\n");

            sb.Append(font.hashChar.Count + " beginbfchar\n");
            foreach (int i in font.hashChar) {
                sb.Append("<" + font.GetGlyphId(i).ToString("X2") + "> <" +  i.ToString("X4") + ">\n");    
            }

            sb.Append("endbfchar\nendcmap\nCMapName currentdict /CMap defineresource pop\nend end\n");
   
            indirectObject.SetChild(new DictionaryObject(sb.ToString()));
		}
	}
}
