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
using System.Globalization;
using System.IO;

namespace crcPdf.Fonts {
    /// <summary>
    /// Get a font
    /// </summary>
	public static class FontFactory {
		private static readonly Dictionary<string, DocumentFont> m_lstFont = new Dictionary<string, DocumentFont>();
		private static readonly Dictionary<string, string> dctFontRegistered = new Dictionary<string, string>();
		private static readonly object lck = new object();
		private static readonly Dictionary<string,string> baseFontsNames = new Dictionary<string, string> {
			{ "timesnewromanbiN", "Times-BoldItalic"},
			{ "timesnewromanb_N", "Times-Bold"},
			{ "timesnewroman_iN", "Times-Italic"},
			{ "timesnewroman__N", "Times-Roman"},
			{ "timesbiN", "Times-BoldItalic"},
			{ "timesb_N", "Times-Bold"},
			{ "times_iN", "Times-Italic"},
			{ "times__N", "Times-Roman"},
			{ "timesromanbiN", "Times-BoldItalic"},
			{ "timesromanb_N", "Times-Bold"},
			{ "timesroman_iN", "Times-Italic"},
			{ "timesroman__N", "Times-Roman"},
			{ "times-romanbiN", "Times-BoldItalic"},
			{ "times-romanb_N", "Times-Bold"},
			{ "times-roman_iN", "Times-Italic"},
			{ "times-roman__N", "Times-Roman"},
			{ "times-romanN", "Times-Roman"},
			{ "times romanbiN", "Times-BoldItalic"},
			{ "times romanb_N", "Times-Bold"},
			{ "times roman_iN", "Times-Italic"},
			{ "times roman__N", "Times-Roman"},
			{ "zapfdingbatsbiN", "ZapfDingbats"},
			{ "zapfdingbatsb_N", "ZapfDingbats"},
			{ "zapfdingbats_iN", "ZapfDingbats"},
			{ "zapfdingbats__N", "ZapfDingbats"},
			{ "courierbiN", "Courier-BoldOblique"},
			{ "courierb_N", "Courier-Bold"},
			{ "courier_iN", "Courier-Oblique"},
			{ "courier__N", "Courier"},
			{ "helveticabiN", "Helvetica-BoldOblique"},
			{ "helveticab_N", "Helvetica-Bold"},
			{ "helvetica_iN", "Helvetica-Oblique"},
			{ "helvetica__N", "Helvetica"},
			{ "symbolbiN", "Symbol"},
			{ "symbol_iN", "Symbol"},
			{ "symbolb_N", "Symbol"},
			{ "symbol__N", "Symbol"},
		};

		private static string GetName(string name, bool IsBold, bool IsItalic, Embedded embedded) 
			=> $"{name.ToLower(CultureInfo.InvariantCulture).Replace(" N","")}{(IsBold ? "b" : "_")}{(IsItalic ? "i" : "_")}{(embedded == Embedded.Yes ? "Y" : "N")}";

   		internal static DocumentFont GetFont(PDFObjects pdf, PdfObject pdfObject)
        {
            var dic = pdf.GetObject<DictionaryObject>(pdfObject);
			
            if (IsBaseFont(pdf, dic)) {
                var name = pdf.GetObject<NameObject>(dic.Dictionary["BaseFont"]).Value;

                lock (lck)
                {
                    if (m_lstFont.ContainsKey(name)) {
                        return m_lstFont[name];
                    }

                    var font = new DocumentBaseFont(name);
                    m_lstFont.Add(name, font);
                    return font;
                }
            } else if (IsSubsetFont(dic)) {
				var name = pdf.GetObject<NameObject>(dic.Dictionary["BaseFont"]).Value;
                lock (lck) {
					if (m_lstFont.ContainsKey(name)) {
						return m_lstFont[name];
					}

					var font = pdf.GetDocument<DocumentTtfSubsetFont>(pdfObject);
					m_lstFont.Add(name, font);
					return font;
				}
			} else if (IsTrueTypeFont(pdf, dic)) {
				var name = pdf.GetObject<NameObject>(dic.Dictionary["BaseFont"]).Value;
                lock (lck) {
					if (m_lstFont.ContainsKey(name)) {
						return m_lstFont[name];
					}

					var font = pdf.GetDocument<DocumentTtfFont>(pdfObject);
					m_lstFont.Add(name, font);
					return font;
				}
			} 
            
            throw new PdfException(PdfExceptionCodes.INVALID_FONT, $"Not supported font type");
        }

        private static bool IsBaseFont(PDFObjects pdf, DictionaryObject dic)
         	=> dic.Dictionary.ContainsKey("BaseFont") &&
                            dic.Dictionary.ContainsKey("Subtype") &&
                            pdf.GetObject<NameObject>(dic.Dictionary["Subtype"]).Value == "Type1";        

		private static bool IsTrueTypeFont(PDFObjects pdf, DictionaryObject dic)
         	=> dic.Dictionary.ContainsKey("BaseFont") &&
                            dic.Dictionary.ContainsKey("Subtype") &&
                            pdf.GetObject<NameObject>(dic.Dictionary["Subtype"]).Value == "TrueType";

		private static bool IsSubsetFont(DictionaryObject dic)
         	=> dic.Dictionary.ContainsKey("BaseFont") &&
                            dic.Dictionary.ContainsKey("ToUnicode");

        internal static DocumentFont GetFont(string name, bool IsBold, bool IsItalic, Embedded embedded)
		{	
			string normalizedName = GetName(name, IsBold, IsItalic, embedded);

			// base fonts
			if (baseFontsNames.ContainsKey(normalizedName)) {
				var fontBaseName = baseFontsNames[normalizedName];
                lock (lck) {					
					if (m_lstFont.ContainsKey(fontBaseName)) {
						return m_lstFont[fontBaseName];
					}

					var font = new DocumentBaseFont(fontBaseName);
				    m_lstFont.Add(fontBaseName, font);
					return font;
                }
			}

            // unknown, or disk or systemfonts
            DocumentFont ttffont = null;
			lock (lck) {
				if (m_lstFont.ContainsKey(normalizedName)) {
						return m_lstFont[normalizedName];
					}

				if (embedded == Embedded.No) {
					if (File.Exists(name)) {
						ttffont = new DocumentTtfFont(name);
					} else {
						LoadSystemFonts();

						if (!dctFontRegistered.ContainsKey(name)) {
							throw new PdfException(PdfExceptionCodes.FONT_NOT_FOUND, "Font " + name + " not found");
						}

						ttffont = new DocumentTtfFont(dctFontRegistered[name]);
					}				
				} else {
					if (File.Exists(name)) {
						ttffont = new DocumentTtfSubsetFont(name);
					} else {
						LoadSystemFonts();

						if (!dctFontRegistered.ContainsKey(name)) {
							throw new PdfException(PdfExceptionCodes.FONT_NOT_FOUND, "Font " + name + " not found");
						}
						
						ttffont = new DocumentTtfSubsetFont(dctFontRegistered[name]);
					}
				
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
