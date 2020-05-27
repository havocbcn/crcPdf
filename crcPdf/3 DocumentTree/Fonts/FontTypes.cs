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

using System;

namespace crcPdf.Fonts {
	/// <summary>
	/// Type of font
	/// </summary>
	[Flags] internal enum FontTypes {
		/// <summary>
		/// All glyphs have the same width
		/// </summary>
		FixedPitch = 1,

		/// <summary>
		/// Glyph have serifs
		/// </summary>
		Serif = 1 << 1,

		/// <summary>
		/// Font contains glyphs outside the Adobe Standard Latin charactes sets
		/// </summary>
		Symbolic = 1 << 2,

		/// <summary>
		/// Glyphs resemble cursive handwriting
		/// </summary>
		Script = 1 << 3,

		/// <summary>
		/// Font uses the Adobe standard Latin character set or a subset of it.
		/// </summary>
		Nonsymbolic = 1 << 5,

		/// <summary>
		/// Glyphs have dominant vertical strokes that are slanted.
		/// </summary>
		Italic = 1 << 6,

		/// <summary>
		/// Font contains no lowercase letters; typically used for display purposes, such as for titles or headlines. 
		/// </summary>
		AllCap = 1 << 16,

		/// <summary>
		/// Font  contains  both  uppercase  and  lowercase  letters. 
		/// </summary>
		SmallCap = 1 << 17,

		/// <summary>
		/// The ForceBold flag (bit 19) shall determine whether bold glyphs shall be painted with extra pixels even at 
		/// very small text sizes by a conforming reader. If the ForceBold flag is set, features of bold glyphs may be
		///  thickened at small text sizes. 
		/// </summary>
		ForceBold = 1 << 18
	}
}
