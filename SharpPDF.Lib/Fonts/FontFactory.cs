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

using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SharpPDF.Lib.Fonts {
    /// <summary>
    /// Get a font
    /// </summary>
	public static class FontFactory {
		private static Dictionary<string, DocumentFont> m_lstFont = new Dictionary<string, DocumentFont>();
		private static Dictionary<string, string> dctFontRegistered = new Dictionary<string, string>();
		private static object lck = new object();
		private static Dictionary<string,string> baseFontsNames = new Dictionary<string, string> {
			{ "timesnewromanbi", "Times-BoldItalic"},
			{ "timesnewromanb_", "Times-Bold"},
			{ "timesnewroman_i", "Times-Italic"},
			{ "timesnewroman__", "Times-Roman"},
			{ "timesbi", "Times-BoldItalic"},
			{ "timesb_", "Times-Bold"},
			{ "times_i", "Times-Italic"},
			{ "times__", "Times-Roman"},
			{ "timesromanbi", "Times-BoldItalic"},
			{ "timesromanb_", "Times-Bold"},
			{ "timesroman_i", "Times-Italic"},
			{ "timesroman__", "Times-Roman"},
			{ "times-romanbi", "Times-BoldItalic"},
			{ "times-romanb_", "Times-Bold"},
			{ "times-roman_i", "Times-Italic"},
			{ "times-roman__", "Times-Roman"},
			{ "times-roman", "Times-Roman"},
			{ "zapfdingbatsbi", "ZapfDingbats"},
			{ "zapfdingbatsb_", "ZapfDingbats"},
			{ "zapfdingbats_i", "ZapfDingbats"},
			{ "zapfdingbats__", "ZapfDingbats"},
			{ "courierbi", "Courier-BoldOblique"},
			{ "courierb_", "Courier-Bold"},
			{ "courier_i", "Courier-Oblique"},
			{ "courier__", "Courier"},
			{ "helveticabi", "Helvetica-BoldOblique"},
			{ "helveticab_", "Helvetica-Bold"},
			{ "helvetica_i", "Helvetica-Oblique"},
			{ "helvetica__", "Helvetica"},
			{ "symbolbi", "Symbol"},
			{ "symbol_i", "Symbol"},
			{ "symbolb_", "Symbol"},
			{ "symbol__", "Symbol"},
		};

		private static string GetName(string name, bool IsBold, bool IsItalic) 
			=> $"{name.ToLower(CultureInfo.InvariantCulture).Replace(" ","")}{(IsBold ? "b" : "_")}{(IsItalic ? "i" : "_")}";

		private static DocumentFont GetBaseFont(PDFObjects pdf, string normalizedName) 
			=> new DocumentBaseFont(pdf, baseFontsNames[normalizedName]);

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
			string normalizedName = GetName(name, IsBold, IsItalic);
            
			// cache
            lock (lck) {
    			if (m_lstFont.ContainsKey(normalizedName)) {
    				return m_lstFont[normalizedName];
    			}
            }

			// base fonts
			if (baseFontsNames.ContainsKey(normalizedName)) {
				DocumentFont font;
                lock (lck) {
					font = GetBaseFont(pdf, normalizedName);				
				    m_lstFont.Add(normalizedName, font);
                }

				return font;
			}

            // unknown, or disk or systemfonts
            DocumentFont ttffont = null;
				lock (lck) {


				switch(embedded) {
					case EEmbedded.NotEmbedded:
						if (File.Exists(name)) {
							//ttffont = new XrefFontTtf(name);
						} else {
							LoadSystemFonts();

							if (!dctFontRegistered.ContainsKey(name)) {
								throw new PdfException(PdfExceptionCodes.FONT_NOT_FOUND, "Font " + name + " not found");
							}

							//ttffont = new XrefFontTtf(dctFontRegistered[name]);
						}
						break;
					case EEmbedded.Embedded:
						if (File.Exists(name)) {
							//ttffont = new XrefFontTtfSubset(name, useBase64);
						} else {
							LoadSystemFonts();

							if (!dctFontRegistered.ContainsKey(name)) {
								throw new PdfException(PdfExceptionCodes.FONT_NOT_FOUND, "Font " + name + " not found");
							}
							
							//ttffont = new XrefFontTtfSubset(dctFontRegistered[name], useBase64);
						}
						break;
				}

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
			if (!Directory.Exists(folder)) {
				return;
			}

			foreach (string file in Directory.GetFiles(folder, "*.ttf")) {
				string filenaWithoutExtension = Path.GetFileNameWithoutExtension(file);
				if (!dctFontRegistered.ContainsKey(filenaWithoutExtension)) {
					dctFontRegistered.Add(filenaWithoutExtension, file);
				}
			}

			foreach (string dir in Directory.GetDirectories(folder)) {
				LoadFonts(dir);
			}			
		}
	}
}
