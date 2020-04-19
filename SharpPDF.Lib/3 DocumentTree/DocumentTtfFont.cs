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
	public class DocumentTtfFont : DocumentTtfFontBase  {		
		private readonly DocumentTtfDescriptorFont descriptor;
        public DocumentTtfFont(PDFObjects pdf, string FullPath) : base(pdf, FullPath) {
            descriptor = new DocumentTtfDescriptorFont(pdf, this);            
        }

        public DocumentTtfFont(PDFObjects pdf, DictionaryObject dic) : base(pdf, dic) {            
        
            descriptor = new DocumentTtfDescriptorFont(pdf, this);            

        }

        private static string GetName(PDFObjects pdf, DictionaryObject dic) =>
            pdf.GetObject<NameObject>(dic.Dictionary["BaseFont"]).Value;

        public override void OnSaveEvent(IndirectObject indirectObject)
        {
            var widths = new List<PdfObject>();            
            for (int i = FirstChar; i < LastChar+1; i++) {                
                if (!hashChar.Contains(i)) {
                    widths.Add(new IntegerObject(0));
                } else if (!dctCharCodeToGlyphID.ContainsKey(i)) {
                    widths.Add(new IntegerObject(this.Width));
                } else {
                    widths.Add(new IntegerObject(Glypth[dctCharCodeToGlyphID[i]].width));
                }
            }

            var entries = new Dictionary<string, PdfObject> {
                { "Encoding", new NameObject("WinAnsiEncoding") },
                { "Type", new NameObject("Font") },
                { "Subtype", new NameObject("TrueType") },
                { "BaseFont", new NameObject(Name) },
                { "FirstChar", new IntegerObject(FirstChar) },
                { "LastChar", new IntegerObject(LastChar) },
                { "Widths", new ArrayObject(widths) },
                { "FontDescriptor", descriptor.IndirectReferenceObject },
            };   
   
            indirectObject.SetChild(new DictionaryObject(entries));
        }	

    }
}
