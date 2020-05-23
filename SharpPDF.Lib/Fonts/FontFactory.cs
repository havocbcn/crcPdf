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

using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SharpPDF.Lib.Fonts {
    /// <summary>
    /// Get a font
    /// </summary>
	public class FontFactory {
		private readonly Dictionary<string, DocumentFont> m_lstFont = new Dictionary<string, DocumentFont>();
		private readonly Dictionary<string, string> dctFontRegistered = new Dictionary<string, string>();
		private readonly object lck = new object();
		private readonly Dictionary<string,string> baseFontsNames = new Dictionary<string, string> {
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

   		internal DocumentFont GetFont(PDFObjects pdf, PdfObject pdfObject)
        {
            var dic = pdf.GetObject<DictionaryObject>(pdfObject);
			
            if (IsBaseFont(pdf, dic)) {
                var name = pdf.GetObject<NameObject>(dic.Dictionary["BaseFont"]).Value;

                lock (lck)
                {
                    if (m_lstFont.ContainsKey(name)) {
                        return m_lstFont[name];
                    }

                    var font = new DocumentBaseFont(pdf, name);
                    m_lstFont.Add(name, font);
                    return font;
                }
            } else if (IsSubsetFont(pdf, dic)) {
				var name = pdf.GetObject<NameObject>(dic.Dictionary["BaseFont"]).Value;
                lock (lck) {
					if (m_lstFont.ContainsKey(name)) {
						return m_lstFont[name];
					}

					var font = new DocumentTtfSubsetFont(pdf, dic);
					m_lstFont.Add(name, font);
					return font;
				}
			} else if (IsTrueTypeFont(pdf, dic)) {
				var name = pdf.GetObject<NameObject>(dic.Dictionary["BaseFont"]).Value;
                lock (lck) {
					if (m_lstFont.ContainsKey(name)) {
						return m_lstFont[name];
					}

					var font = new DocumentTtfFont(pdf, dic);
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

		private static bool IsSubsetFont(PDFObjects pdf, DictionaryObject dic)
         	=> dic.Dictionary.ContainsKey("BaseFont") &&
                            dic.Dictionary.ContainsKey("ToUnicode");

        internal DocumentFont GetFont(PDFObjects pdf, string name, bool IsBold, bool IsItalic, EEmbedded embedded)
		{	
			string normalizedName = GetName(name, IsBold, IsItalic);        			

			// base fonts
			if (baseFontsNames.ContainsKey(normalizedName)) {
				var fontBaseName = baseFontsNames[normalizedName];
                lock (lck) {					
					if (m_lstFont.ContainsKey(fontBaseName)) {
						return m_lstFont[fontBaseName];
					}

					var font = new DocumentBaseFont(pdf, fontBaseName);
				    m_lstFont.Add(fontBaseName, font);
					return font;
                }
			}

            // unknown, or disk or systemfonts
            DocumentFont ttffont = null;
			lock (lck) {
				if (embedded == EEmbedded.NotEmbedded) {
					if (File.Exists(name)) {
						ttffont = new DocumentTtfFont(pdf, name);
					} else {
						LoadSystemFonts();

						if (!dctFontRegistered.ContainsKey(name)) {
							throw new PdfException(PdfExceptionCodes.FONT_NOT_FOUND, "Font " + name + " not found");
						}

						ttffont = new DocumentTtfFont(pdf, dctFontRegistered[name]);
					}				
				} else {
					if (File.Exists(name)) {
						ttffont = new DocumentTtfSubsetFont(pdf, name);
					} else {
						LoadSystemFonts();

						if (!dctFontRegistered.ContainsKey(name)) {
							throw new PdfException(PdfExceptionCodes.FONT_NOT_FOUND, "Font " + name + " not found");
						}
						
						ttffont = new DocumentTtfSubsetFont(pdf, dctFontRegistered[name]);
					}
				
				}

				m_lstFont.Add(normalizedName, ttffont);
			}
			return ttffont;
		}

		private void LoadSystemFonts() {
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

		private void LoadFonts(string folder)
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
