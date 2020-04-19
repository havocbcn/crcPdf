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
using SharpPDF.Lib.Fonts;

namespace SharpPDF.Lib {
	public abstract class DocumentFont : IDocumentTree  {		
		protected DocumentFont(PDFObjects pdf) : base(pdf) {
			StemV = 80;
			boundingBox = new short[4];
		}

		/// <summary>
		/// The maximum height above the baseline reached by glyphs in this font.  
		/// The height of glyphs for accented characters shall be excluded. 
		/// </summary>
		public short Ascendent { get; set; }

		/// <summary>
		/// The maximum depth below the baseline reached by glyphs in this font. 
		/// The value shall be a negative number. 
		/// </summary>
		public short Descendent { get; set; }

		public string Name { get; set; }

		public string FullPath { get; set; }

		public float GetAscendent(float size) => Ascendent * 0.001f * size;

		public float GetDescendent(float size) => Descendent * 0.001f * size;

		public float GetWidthPointKerned(string text, float size) {
			float currentSize = 0.0f;
			float kerning = 0.0f;
			int previousChar = -1;

			foreach (char ch in text) {
				if (dctCharCodeToGlyphID.ContainsKey((int)ch)) {
					currentSize += Glypth[dctCharCodeToGlyphID[(int)ch]].width;
				}

				if (previousChar >= 0) {
					int key = (previousChar << 16) + (int)ch;
					if (dctKerning.ContainsKey(key)) {
						kerning += dctKerning[key];
					}
				}

				previousChar = ch;
			}


			return (currentSize + kerning) * 0.001f * size;
		}


		/// <summary>
		/// The width.
		/// </summary>
		public int Width { get; set; }

        internal FontGlyph[] Glypth;

		internal Dictionary<int, short> dctKerning = new Dictionary<int, short>();

		protected Dictionary<int, int> dctCharCodeToGlyphID = new Dictionary<int, int>();

		/// <summary>
		/// The boundingBox of all glyphs
		/// specifying the lower-left x, lower-left y, upper-right x, and upper-right y coordinates of the rectangle.
		/// </summary>
		public short[] boundingBox { get; }

		/// <summary>
		/// 0 for short offsets (Offset16), 1 for long (Offset32).
		/// </summary>
		internal short indexToLocFormat { get; set; }

		/// <summary>
		/// The angle, expressed in degrees counterclockwise from the vertical, of the dominant vertical strokes of the font. 
		/// EXAMPLE: The 9-o’clock position is  90  degrees,  and  the  3-o’clock position is –90 degrees. 
		/// The value shall be negative for fonts that slope to the right, as almost all italic fonts do.
		/// </summary>
		internal int ItalicAngle { get; set; }

		/// <summary>
		/// The spacing between baselines of consecutive lines of text. Default value: 0. 
		/// </summary>
		internal int Leading { get; set; }

		/// <summary>
		/// The vertical coordinate of the top of flat capital letters, measured from the baseline. 
		/// </summary>
		internal int CapHeight = 729;
        
		/// <summary>
		/// The thickness, measured horizontally, of the dominant vertical stems of glyphs in the font. 
        /// https://stackoverflow.com/questions/35485179/stemv-value-of-the-truetype-font
        /// This value is not used
		/// </summary>
		public int StemV { get; set; }

		/// <summary>
		/// TTF Font
		/// </summary>
		protected byte[] TTFFont;


        internal int FirstChar;
		private bool anyTextSet = false;

        internal int LastChar  = -1;
        internal readonly HashSet<int> hashChar = new HashSet<int>();

        internal bool isUnicode;

        internal virtual int GetGlyphId(int ch) => dctCharCodeToGlyphID[ch];        

        internal virtual FontGlyph GetGlyph(int gliphtId) => Glypth[gliphtId];

        /// <summary>
        /// Escriben texto con esta fuente, me apunto cosas
        /// </summary>
        /// <param name="text">Text.</param>
        public virtual void SetText(string text) {
            foreach (char c in text) {
                AddNewChar(c);
            }
        }

        /// <summary>
        /// Get the font byte array
        /// </summary>
        /// <returns>The font.</returns>
        public virtual byte[] GetFont() {
            return new byte[0];
        }

        protected void AddNewChar(char c) {
            hashChar.Add((int)c);

            if (!anyTextSet || c < FirstChar) {
                FirstChar = c;
				anyTextSet = true;
			}
			if (c > LastChar) {
                LastChar = c;
			}
        }
    }
}
