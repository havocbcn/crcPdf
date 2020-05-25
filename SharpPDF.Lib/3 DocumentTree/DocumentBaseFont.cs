using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SharpPDF.Lib.Fonts;

namespace SharpPDF.Lib {
    public class DocumentBaseFont : DocumentFont {        
        public DocumentBaseFont(PDFObjects pdf, string fontName) : base(pdf) {
            Name = fontName;
            LoadFont(fontName);
        }

        public override void OnSaveEvent(IndirectObject indirectObject)
        {
            var entries = new Dictionary<string, PdfObject> {
                { "Type", new NameObject("Font") },
                { "Subtype", new NameObject("Type1") },
                { "BaseFont", new NameObject(this.Name) },
                { "Encoding", new NameObject("WinAnsiEncoding") }
            };   
            
            indirectObject.SetChild(new DictionaryObject(entries));
        }

        private void LoadFont(string fontName) {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "SharpPDF.Lib.Resources." + fontName + "_resource.txt";

            string[] result;
            Stream stream = assembly.GetManifestResourceStream(resourceName);
            using (StreamReader reader = new StreamReader(stream))
            {
                result = reader.ReadToEnd().Split('\n');
            }

            string[] parts = result[0].Split(',');
            Width = Convert.ToInt32(parts[0]);
            ItalicAngle = Convert.ToInt32(parts[1]);
            boundingBox[0] = Convert.ToInt16(parts[2]);
            boundingBox[1] = Convert.ToInt16(parts[3]);
            boundingBox[2] = Convert.ToInt16(parts[4]);
            boundingBox[3] = Convert.ToInt16(parts[5]);
            Ascendent = Convert.ToInt16(parts[6]);
            Descendent = Convert.ToInt16(parts[7]);

            parts = result[1].Split(',');
            for (int i = 0; i < parts.Length; i += 2)
            {
                dctCharCodeToGlyphID.Add(Convert.ToInt32(parts[i]), Convert.ToInt32(parts[i + 1]));
            }

            parts = result[2].Split(',');
            if (parts.Length > 1)
            {
                for (int i = 0; i < parts.Length; i += 2)
                {
                    dctKerning.Add(Convert.ToInt32(parts[i]), Convert.ToInt16(parts[i + 1]));
                }
            }

            parts = result[3].Split(',');
            Glypth = new FontGlyph[parts.Length / 6];
            int j = 0;
            for (int i = 0; i < parts.Length; i += 6)
            {
                Glypth[j] = new FontGlyph(
                    Convert.ToInt32(parts[i]),
                    Convert.ToInt32(parts[i + 1]));
                j++;
            }
        }     
    }
}