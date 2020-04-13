// This file is part of SharpReport.
// 
// SharpReport is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// SharpReport is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with SharpReport.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;

namespace SharpPDF.Lib.Fonts
{
    /// <summary>
    /// Get a font
    /// </summary>
	public static class XrefFontFactory {
		private static Dictionary<string, DocumentFont> m_lstFont = new Dictionary<string, DocumentFont>();

		private static Dictionary<string, string> dctFontRegistered = new Dictionary<string, string>();
		private static object lck = new object();

		private static Dictionary<string,string> baseFontsNames = new Dictionary<string, string> {
			{ "timesnewroman", "Times-Roman"},
			{ "times", "Times-Roman"},
			{ "timesroman", "Times-Roman"},
			{ "times-roman", "Times-Roman"},
			{ "zapfdingbats", "ZapfDingbats"},
			{ "courier", "Courier"},
			{ "helvetica", "Helvetica"},
			{ "symbol", "Symbol"},
		};

		private static string GetName(string name, bool IsBold, bool IsItalic, EEmbedded embedded) {
			string normalizedName = name.ToLower().Replace(" ","");

			if (IsBold)
				normalizedName += "b";
			else
				normalizedName += "_";

			if (IsItalic)
				normalizedName += "i";
			else
				normalizedName += "_";

            if (embedded == EEmbedded.Embedded)
                normalizedName += "e";
            else
                normalizedName += "_";

			return normalizedName;
		}

		private static DocumentFont GetBaseFont(PDFObjects pdf,string name, bool IsBold, bool IsItalic) {
			DocumentFont font;

			switch (baseFontsNames[name.ToLower().Replace(" ","")]) {
                case "Times-Roman":
                    font = GetTimesRomanFont(pdf, IsBold, IsItalic);
                    break;
                case "Courier":
                    font = GetCourierFont(pdf, IsBold, IsItalic);
                    break;
                case "Helvetica":
                    font = GetHelveticaFont(pdf, IsBold, IsItalic);
                    break;
                default:
					font = new DocumentBaseFont(pdf, name);
					break;				
			}
			return font;
		}

        private static DocumentFont GetHelveticaFont(PDFObjects pdf,bool IsBold, bool IsItalic)
        {
            if (IsBold && IsItalic)
                return new DocumentBaseFont(pdf, "Helvetica-BoldOblique");
            else if (IsBold)
                return new DocumentBaseFont(pdf, "Helvetica-Bold");
            else if (IsItalic)
                return new DocumentBaseFont(pdf, "Helvetica-Oblique");
            else
                return new DocumentBaseFont(pdf, "Helvetica");
        }
     
        private static DocumentFont GetCourierFont(PDFObjects pdf,bool IsBold, bool IsItalic)
        {
            if (IsBold && IsItalic)
                return new DocumentBaseFont(pdf, "Courier-BoldOblique");
            else if (IsBold)
                return new DocumentBaseFont(pdf, "Courier-Bold");
            else if (IsItalic)
                return new DocumentBaseFont(pdf, "Courier-Oblique");
            else
                return new DocumentBaseFont(pdf, "Courier");
        }

        private static DocumentFont GetTimesRomanFont(PDFObjects pdf,bool IsBold, bool IsItalic)
        {
            if (IsBold && IsItalic)
                return new DocumentBaseFont(pdf, "Times-BoldItalic");
            else if (IsBold)
                return new DocumentBaseFont(pdf, "Times-Bold");
            else if (IsItalic)
                return new DocumentBaseFont(pdf, "Times-Italic");
            else
                return new DocumentBaseFont(pdf, "Times-Roman");          
        }

   		internal static DocumentFont GetFont(PDFObjects pdf, PdfObject pdfObject)
        {
			var dic = pdf.GetObject<DictionaryObject>(pdfObject);
			
			if (dic.Dictionary.ContainsKey("BaseFont")) {
				return new DocumentBaseFont(pdf, pdfObject);
			}

			// TODO
			throw new PdfException(PdfExceptionCodes.INVALID_FONT, $"Not supported font type");
		}

        internal static DocumentFont GetFont(PDFObjects pdf, string name, bool IsBold, bool IsItalic, EEmbedded embedded)
		{	
			string normalizedName = GetName(name, IsBold, IsItalic, embedded);
            
			// cache
            lock (lck) {
    			if (m_lstFont.ContainsKey(normalizedName)) {
    				return m_lstFont[normalizedName];
    			}
            }

			// base fonts
			if (baseFontsNames.ContainsKey(name.ToLower().Replace(" ","")))
			{
				DocumentFont font;
                lock (lck) {
					font = GetBaseFont(pdf, name, IsBold, IsItalic);				
				    m_lstFont.Add(normalizedName, font);
                }

				return font;
			}

            // unknown, or disk or systemfonts
            DocumentFont ttffont = null;
				lock (lck) {

/*
				switch(embedded) {
					case EEmbedded.NotEmbedded:
						if (File.Exists(name)) {
							ttffont = new XrefFontTtf(name);
						} else {
							LoadSystemFonts();

							if (!dctFontRegistered.ContainsKey(name))
								throw new FontException("Font " + name + " not found");

							ttffont = new XrefFontTtf(dctFontRegistered[name]);
						}
						break;
					case EEmbedded.Embedded:
						if (File.Exists(name)) {
							ttffont = new XrefFontTtfSubset(name, useBase64);
						} else {
							LoadSystemFonts();

							if (!dctFontRegistered.ContainsKey(name))
								throw new FontException("Font " + name + " not found");

							ttffont = new XrefFontTtfSubset(dctFontRegistered[name], useBase64);
						}
						break;
				}
*/
			    m_lstFont.Add(normalizedName, ttffont);
            }
			return ttffont;
		}

		private static void LoadSystemFonts() {
			if (dctFontRegistered.Count == 0) 
			{
				lock (lck) {
					LoadFonts("./");
					LoadFonts("c:/windows/fonts");
					LoadFonts("c:/winnt/fonts");
					LoadFonts("d:/windows/fonts");
					LoadFonts("d:/winnt/fonts");

					LoadFonts("/usr/share/X11/fonts");
					LoadFonts("/usr/X/lib/X11/fonts");
					LoadFonts("/usr/openwin/lib/X11/fonts");
					LoadFonts("/usr/share/fonts");
					LoadFonts("/usr/X11R6/lib/X11/fonts");
					LoadFonts("/Library/Fonts");
					LoadFonts("/System/Library/Fonts");
				}
			}
		}

		private static void LoadFonts(string folder)
		{
			if (!Directory.Exists(folder))
				return;

			foreach (string file in Directory.GetFiles(folder, "*.ttf")) {
				string filenaWithoutExtension = Path.GetFileNameWithoutExtension(file);
				if (!dctFontRegistered.ContainsKey(filenaWithoutExtension))
					dctFontRegistered.Add(filenaWithoutExtension, file);
			}

			foreach (string dir in Directory.GetDirectories(folder))
				LoadFonts(dir);
			
		}
	}
}
