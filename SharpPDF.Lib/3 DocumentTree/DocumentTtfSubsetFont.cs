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
using System.Linq;
using System.Text;
using SharpPDF.Lib.Fonts;

namespace SharpPDF.Lib  {
    public class DocumentTtfSubsetFont : DocumentTtfFontBase {
		private const string m_sixHash = "IKWLZJ";

        public string sixHash => m_sixHash;
		private const string m_fontName = "FreeSans";
        public string fontName => m_fontName;

        public DocumentTtfSubsetFont(PDFObjects pdf, DictionaryObject dic) : base(pdf, dic) {                    
                 
        }
            
        public DocumentTtfSubsetFont(PDFObjects pdf, string FullPath) : base(pdf, FullPath) {			

            IsEmbedded = true;
            
            isUnicode = true;

            GlyphToChar firstChar = new GlyphToChar {
                character = 0,
                oldGlyphId = 0,
                newGlyphId = 0
            };
            lstGlyphToChar.Add(firstChar);

            dctCharToGlyphOldNew.Add(0, firstChar);
            dctNewGlyphIdToGlyphOldNew.Add(0, firstChar);

            this.Flags = FontTypes.Symbolic;
            this.Name = m_sixHash + "+" + m_fontName;
        }


        public override byte[] GetFont() {
            return SubsetTTFFont;
        }

        private byte[] SubsetTTFFont;


        private readonly string[] tablesNames = { "cmap", "cvt ", "fpgm", "glyf", "head", "hhea", "hmtx", "loca", "maxp", "prep" };

        public class GlyphToChar {
            public int? character {get; set;}
            public int oldGlyphId {get; set;}

            public int newGlyphId {get; set;}
            public byte[] bytes  {get; set;}
        }

        internal override int GetGlyphId(int ch) {
            return dctCharToGlyphOldNew[ch].newGlyphId;
        }

        internal override FontGlyph GetGlyph(int gliphtId) {
            return Glypth[dctNewGlyphIdToGlyphOldNew[gliphtId].oldGlyphId];
        }

        public override string SetText(string text) {
            foreach (char ch in text) {
                // si ya existe este caracter, no hago nada
                if (hashChar.Contains(ch))
                    continue;

                // nuevo caracter
                int counter = dctNewGlyphIdToGlyphOldNew.Count;

                // este caracter, con este antiguo glyph, tendrá uno nuevo
                GlyphToChar glyphConversion = new GlyphToChar {
                    character = ch,
                    oldGlyphId = dctCharCodeToGlyphID[ch],
                    newGlyphId = counter
                };

                lstGlyphToChar.Add(glyphConversion);

                dctCharToGlyphOldNew.Add(ch, glyphConversion);
                dctNewGlyphIdToGlyphOldNew.Add(counter, glyphConversion);

                AddNewChar(ch);
            }

            var sb = new StringBuilder();
            foreach (char c in text) {                
                sb.Append((char)GetGlyphId(c));
            }

            return sb.ToString();
        }


        readonly List<GlyphToChar> lstGlyphToChar = new List<GlyphToChar>();
        readonly Dictionary<int, GlyphToChar> dctCharToGlyphOldNew = new Dictionary<int, GlyphToChar>();
        readonly Dictionary<int, GlyphToChar> dctNewGlyphIdToGlyphOldNew = new Dictionary<int, GlyphToChar>();

        // Bit 0: If this is set, the arguments are 16-bit (uint16 or int16); otherwise, they are bytes (uint8 or int8).
        private const int ARG_1_AND_2_ARE_WORDS = 0x0001; 

        // Bit 3: This indicates that there is a simple scale for the component. Otherwise, scale = 1.0.
        private const int WE_HAVE_A_SCALE = 0x0008;

        // Bit 5: Indicates at least one more glyph after this one.
        private const int MORE_COMPONENTS = 0x0020;  

        // Bit 6: The x direction will use a different scale from the y direction.
        private const int WE_HAVE_AN_X_AND_Y_SCALE = 0x0040;

        // Bit 7: There is a 2 by 2 transformation that will be used to scale the component.
        private const int WE_HAVE_A_TWO_BY_TWO = 0x0080;

        private byte[] Subset(byte[] font)
        {
            // regular: 1 char is 1 glyph 
            // composite: 1 char is 1 glyph that points to many glyphs
            int glyphToCharIndex = 0;
            while (glyphToCharIndex < lstGlyphToChar.Count)
            {
                GlyphToChar glyphChar = lstGlyphToChar[glyphToCharIndex];
                glyphToCharIndex++;

                // obtengo el glyph en sí
                glyphChar.bytes = new byte[Glypth[glyphChar.oldGlyphId].lengthFile];
                Array.Copy(font, Glypth[glyphChar.oldGlyphId].offsetFile, glyphChar.bytes, 0, Glypth[glyphChar.oldGlyphId].lengthFile);

                int fontOffset = Glypth[glyphChar.oldGlyphId].offsetFile;
                int endPtsOfContours = GetUInt16(font, ref fontOffset);
                if (endPtsOfContours >= 0)  // If numberOfContours is negative, a composite glyph description is used.
                    continue;
                fontOffset += 8;

                while (true) {
                    int flags = GetUInt16(font, ref fontOffset);        // component flag

                    int glyphIndexOffset = fontOffset;
                    int glyphIndex = GetUInt16(font, ref fontOffset);  // glyph index of component

                    // nuevo caracter
                    int counter = dctNewGlyphIdToGlyphOldNew.Count;

                    // este caracter, con este antiguo glyph, tendrá uno nuevo
                    GlyphToChar compositeGlyph = new GlyphToChar {
                        character = null,
                        oldGlyphId = glyphIndex,
                        newGlyphId = counter
                    };

                    lstGlyphToChar.Add(compositeGlyph);
                    dctNewGlyphIdToGlyphOldNew.Add(counter, compositeGlyph);

                    // se escribe en este glyph, donde referencia a glyphIndex el newGlyph
                    SetUInt16(glyphChar.bytes, glyphIndexOffset - Glypth[glyphChar.oldGlyphId].offsetFile, counter);

                    if ((flags & MORE_COMPONENTS) == 0) {
                        break;
                    }

                    if ((flags & ARG_1_AND_2_ARE_WORDS) == 0) {
                        fontOffset += 2;
                    } else {
                        fontOffset += 4;
                    }

                    if ((flags & WE_HAVE_A_SCALE) == WE_HAVE_A_SCALE) {
                        fontOffset += 2;
                    } else if ((flags & WE_HAVE_AN_X_AND_Y_SCALE) == WE_HAVE_AN_X_AND_Y_SCALE) {
                        fontOffset += 4;
                    } else if ((flags & WE_HAVE_A_TWO_BY_TWO) == WE_HAVE_A_TWO_BY_TWO) {
                        fontOffset += 8;
                    }
                }
            }

            Dictionary<string, Table> dctTablesUsed = new Dictionary<string, Table>();

            // de todas las tablas cargadas, sólo grabaremos las que PDF necesita
            foreach (string table in tablesNames)
            {
                if (dctTables.ContainsKey(table)) {
                    dctTablesUsed.Add(table, dctTables[table]);
                }
            }

            int tableOffset = 16 * dctTablesUsed.Count + 12;

            byte[] hhea = SetHhea(font, dctTablesUsed);
            byte[] maxp = SetMaxp(font, dctTablesUsed);
            byte[] hmtx = SetHmtx();
            byte[] cmap = SetCmap();
            List<int> glyphOffset;
            byte[] glyph = SetGlyph(out glyphOffset);
            byte[] head = SetHead(font);
            byte[] loca = SetLoca(glyphOffset, head, glyph.Length);

            int fontSubsetSize = 0;
            fontSubsetSize += 12;   // initial header
            fontSubsetSize += 16 * dctTablesUsed.Count;   // initial header pointers
            // de las tablas que vamos a grabar
            foreach (KeyValuePair<string, Table> kvp in dctTablesUsed)
            {
                switch (kvp.Key)
                {
                    case "hhea":
                        fontSubsetSize += Get4bytePadding(hhea.Length);
                        break;
                    case "maxp":
                        fontSubsetSize += Get4bytePadding(maxp.Length);
                        break;
                    case "hmtx":
                        fontSubsetSize += Get4bytePadding(hmtx.Length);
                        break;
                    case "cmap":
                        fontSubsetSize += Get4bytePadding(cmap.Length);
                        break;
                    case "glyf":
                        fontSubsetSize += Get4bytePadding(glyph.Length);
                        break;
                    case "loca":
                        fontSubsetSize += Get4bytePadding(loca.Length);
                        break;
                    case "head":
                        fontSubsetSize += head.Length;
                        break;
                    default:
                        fontSubsetSize += Get4bytePadding(dctTables[kvp.Key].length);
                        break;
                }
            }

            byte[] fontSubset = new byte[Get4bytePadding(fontSubsetSize)];
            int fontSubsetPos = 0;

            // -- Header --
            fontSubsetPos = SetUInt32(fontSubset, fontSubsetPos, 0x00010000);
            fontSubsetPos = SetUInt16(fontSubset, fontSubsetPos, dctTablesUsed.Count);

            CalculateBinarySearchRegisters(dctTablesUsed.Count, 16, 4, out int entrySelector, out int searchRange, out int rangeShift);

            fontSubsetPos = SetUInt16(fontSubset, fontSubsetPos, searchRange);
            fontSubsetPos = SetUInt16(fontSubset, fontSubsetPos, entrySelector);
            fontSubsetPos = SetUInt16(fontSubset, fontSubsetPos, rangeShift);

            foreach (KeyValuePair<string, Table> kvp in dctTablesUsed)
            {
                fontSubsetPos = SetString(fontSubset, fontSubsetPos, kvp.Key);
                switch (kvp.Key)
                {
                    case "hhea":
                        fontSubsetPos = WriteHeader(fontSubset, fontSubsetPos, hhea, ref tableOffset);
                        break;
                    case "maxp":
                        fontSubsetPos = WriteHeader(fontSubset, fontSubsetPos, maxp, ref tableOffset);
                        break;
                    case "hmtx":
                        fontSubsetPos = WriteHeader(fontSubset, fontSubsetPos, hmtx, ref tableOffset);
                        break;
                    case "cmap":
                        fontSubsetPos = WriteHeader(fontSubset, fontSubsetPos, cmap, ref tableOffset);
                        break;
                    case "glyf":
                        fontSubsetPos = WriteHeader(fontSubset, fontSubsetPos, glyph, ref tableOffset);
                        break;
                    case "loca":
                        fontSubsetPos = WriteHeader(fontSubset, fontSubsetPos, loca, ref tableOffset);
                        break;
                    case "head":
                        fontSubsetPos = WriteHeader(fontSubset, fontSubsetPos, head, ref tableOffset);
                        break;
                    default:
                        fontSubsetPos = SetUInt32(fontSubset, fontSubsetPos, kvp.Value.checksum);
                        fontSubsetPos = SetUInt32(fontSubset, fontSubsetPos, tableOffset);
                        fontSubsetPos = SetUInt32(fontSubset, fontSubsetPos, kvp.Value.length);

                        tableOffset += Get4bytePadding(kvp.Value.length);
                        break;
                }
            }

            // -- Tables --
            int checkSumAdjustmentOffset = 0;
            foreach (KeyValuePair<string, Table> kvp in dctTablesUsed)
            {
                switch (kvp.Key)
                {
                    case "hhea":
                        Array.Copy(hhea, 0, fontSubset, fontSubsetPos, hhea.Length);
                        fontSubsetPos += Get4bytePadding(hhea.Length);
                        break;
                    case "maxp":
                        Array.Copy(maxp, 0, fontSubset, fontSubsetPos, maxp.Length);
                        fontSubsetPos += Get4bytePadding(maxp.Length);
                        break;
                    case "hmtx":
                        Array.Copy(hmtx, 0, fontSubset, fontSubsetPos, hmtx.Length);
                        fontSubsetPos += Get4bytePadding(hmtx.Length);
                        break;
                    case "cmap":
                        Array.Copy(cmap, 0, fontSubset, fontSubsetPos, cmap.Length);
                        fontSubsetPos += Get4bytePadding(cmap.Length);
                        break;
                    case "glyf":
                        Array.Copy(glyph, 0, fontSubset, fontSubsetPos, glyph.Length);
                        fontSubsetPos += Get4bytePadding(glyph.Length);
                        break;
                    case "loca":
                        Array.Copy(loca, 0, fontSubset, fontSubsetPos, loca.Length);
                        fontSubsetPos += Get4bytePadding(loca.Length);
                        break;
                    case "head":
                        checkSumAdjustmentOffset = fontSubsetPos + 8;
                        Array.Copy(head, 0, fontSubset, fontSubsetPos, head.Length);
                        fontSubsetPos += Get4bytePadding(head.Length);
                        break;
                    default:
                        Array.Copy(font, kvp.Value.offset, fontSubset, fontSubsetPos, kvp.Value.length);
                        fontSubsetPos += Get4bytePadding(kvp.Value.length);
                        break;
                }
            }

            SetUInt32(fontSubset, checkSumAdjustmentOffset, (int)CalculateFileChecksum(fontSubset));  // checkSumAdjustment

            return fontSubset;
        }

        private byte[] SetLoca(List<int> glyphOffset, byte[] head, int glyphLength)
        {
            byte[] loca = null;
            
            if (glyphLength> 128000) {
                loca = new byte[glyphOffset.Count * 2];

                SetUInt16(head, 50, 0);

                int pos = 0;
                foreach (int offset in glyphOffset) {
                    pos = SetUInt16(loca, pos, offset);
                }
            } else {
            
                loca = new byte[glyphOffset.Count * 4];

                SetUInt16(head, 50, 1);

                int pos = 0;
                foreach (int offset in glyphOffset) {
                    pos = SetUInt32(loca, pos, offset);
                }
            }
            return loca;
        }

        private byte[] SetHead(byte[] font)
        {
            byte[] head = new byte[dctTables["head"].length];
            Array.Copy(font, dctTables["head"].offset, head, 0, dctTables["head"].length);
            SetUInt32(head, 8, 0);  // checkSumAdjustment
            return head;
        }

        private byte[] SetGlyph(out List<int> glyphOffset)
        {
            int glyphSize = lstGlyphToChar.Sum(glyph => Get4bytePadding(glyph.bytes.Length));
            byte[] glyph = new byte[glyphSize];
            glyphOffset = new List<int>();
            int pos = 0;
            foreach (GlyphToChar glyphChar in lstGlyphToChar)
            {
                glyphOffset.Add(pos);
                Array.Copy(glyphChar.bytes, 0, glyph, pos, glyphChar.bytes.Length);
                pos += Get4bytePadding(glyphChar.bytes.Length);
            }
            glyphOffset.Add(pos);     // loca table will have a +1 loca element

            return glyph;
        }

        private byte[] SetCmap() {
            byte[] cmap = new byte[24 + 14 + lstGlyphToChar.Count * 12 + 262];
            int pos = 0;
            pos = SetUInt16(cmap, pos, 0);  // version     0
            pos = SetUInt16(cmap, pos, 2);  // numTables   2

            pos = SetUInt16(cmap, pos, 1);  // platformID  4
            pos = SetUInt16(cmap, pos, 0);  // encodingID  6
            pos = SetUInt32(cmap, pos, 20); // offset      8

            pos = SetUInt16(cmap, pos, 1);  // platformID  12
            pos = SetUInt16(cmap, pos, 10); // encodingID  14
            pos = SetUInt32(cmap, pos, 282); // offset      16

            pos = SetUInt16(cmap, pos, 0);  // 20 Subtable format; set to 0
            pos = SetUInt16(cmap, pos, 262);// 22 This is the length in bytes of the subtable.
            pos = SetUInt16(cmap, pos, 0);  // 24 Please see "Note on the language field in 'cmap' subtables"

            for (int i = 0; i < 256; i++) {
                if (dctNewGlyphIdToGlyphOldNew.ContainsKey(i)) {
                    // 20 Subtable format
                    pos = SetUInt8(cmap, pos, dctNewGlyphIdToGlyphOldNew[i].newGlyphId);  
                } else {
                    // 20 Subtable format; set to 0
                    pos = SetUInt8(cmap, pos, 0);  
                }
            }

            pos = SetUInt16(cmap, pos, 12); // 20 Subtable format; set to 12.
            pos = SetUInt16(cmap, pos, 0);  // 14 Reserved; set to 0
            // 16 Byte length of this subtable (including the header)
            pos = SetUInt32(cmap, pos, dctCharToGlyphOldNew.Keys.Count * 12 + 6);  
            pos = SetUInt32(cmap, pos, 0);  // 20 Please see “Note on the language field in 'cmap' subtables“ 
            pos = SetUInt32(cmap, pos, dctCharToGlyphOldNew.Keys.Count);  // 24 Number of groupings which follow

            foreach (GlyphToChar glyphChar in dctCharToGlyphOldNew.Values) {
                // 0 First character code in this group
                pos = SetUInt32(cmap, pos, glyphChar.character.Value);  
                // 4 Last character code in this group
                pos = SetUInt32(cmap, pos, glyphChar.character.Value);  
                // 8 Glyph index corresponding to the starting character code
                pos = SetUInt32(cmap, pos, glyphChar.newGlyphId);       
            }

            return cmap;
        }

        private byte[] SetHmtx()
        {
            byte[] hmtx = new byte[Get4bytePadding(4 * lstGlyphToChar.Count)];
            int pos = 0;

            foreach (GlyphToChar glyphChar in lstGlyphToChar) {
                pos = SetUInt16(hmtx, pos, Glypth[glyphChar.oldGlyphId].width * unitsPerEm / 1000);
                pos = SetUInt16(hmtx, pos, Glypth[glyphChar.oldGlyphId].leftSideBearing);
            }

            return hmtx;
        }

        private byte[] SetMaxp(byte[] font, Dictionary<string, Table> dctTablesUsed) {
            if (!dctTablesUsed.ContainsKey("maxp")) {
                return  new byte[0];
            }
            
            byte[] maxp = new byte[dctTablesUsed["maxp"].length];
            Array.Copy(font, dctTablesUsed["maxp"].offset, maxp, 0, dctTablesUsed["maxp"].length);

            // numGlyphs = number of new glyphs
            SetUInt16(maxp, 4, lstGlyphToChar.Count);

            return maxp;
        }

        private byte[] SetHhea(byte[] font, Dictionary<string, Table> dctTablesUsed)
        {
            if (dctTablesUsed.ContainsKey("hhea"))
            {
                byte[] hhea = new byte[dctTablesUsed["hhea"].length];
                Array.Copy(font, dctTablesUsed["hhea"].offset, hhea, 0, dctTablesUsed["hhea"].length);

                // numberOfHMetrics = number of new glyphs
                SetUInt16(hhea, 34, lstGlyphToChar.Count);

                return hhea;
            }

            return new byte[0];
        }

        private int WriteHeader(byte[] fontSubset, int fontSubsetPos, byte[] origin, ref int tableOffset) {
            fontSubsetPos = SetUInt32(fontSubset, fontSubsetPos, CheckSum(origin));
            fontSubsetPos = SetUInt32(fontSubset, fontSubsetPos, tableOffset);
            fontSubsetPos = SetUInt32(fontSubset, fontSubsetPos, origin.Length);

            tableOffset += Get4bytePadding(origin.Length);

            return fontSubsetPos;
        }

        private int Get4bytePadding(int length) {
            // align to 4 bytes: 0, 4, 8....
            // (0 + 3) and 0x111111...1100 = 0
            // (1 + 3) and 0x111111...1100 = 4
            // (2 + 3) and 0x111111...1100 = 4
            // (3 + 3) and 0x111111...1100 = 4
            // (4 + 3) and 0x111111...1100 = 4
            // (4 + 3) and 0x111111...1100 = 8
            // (5 + 3) and 0x111111...1100 = 8
            return length + 3 & (~3);
        }


        // Calculate searchRange, entrySelector and rangeShift
        private void CalculateBinarySearchRegisters(int count, int size, int logSize, out int entrySelector, out int searchRange, out int rangeShift) {
            
            entrySelector = -logSize;
            searchRange = 1;
            while (2 * searchRange < count * size) {
                entrySelector++;
                searchRange *= 2;
            }
            rangeShift = count * size - searchRange;
        }

        private short GetUInt16(byte[] font, ref int filePosition) {
            short result = (short) (font[filePosition] << 8 | font[filePosition+1]);
            filePosition += 2;
            return result;
        }

        private UInt32 GetUInt32(byte[] font, ref int filePosition)
        {
            if (filePosition + 4 <= font.Length) {
                UInt32 result = (UInt32) (font[filePosition] << 24 | font[filePosition+1] << 16 | font[filePosition+2] << 8 | font[filePosition+3]);
                filePosition += 4;
                return result;
            } else {
                UInt32 result = 0;
                int bytePosition = 24;
                while (filePosition < font.Length) {
                    result += (UInt32) (font[filePosition] << bytePosition);
                    filePosition ++;
                    bytePosition -= 8;
                }                
                return result;
            }
        }

        private long CalculateFileChecksum(byte[] font) {
            long sum = 0;
            int filePosition = 0;
            var nLongs = ((font.Length + 3) & ~3) >> 2;

            while (nLongs-- > 0)
                sum += GetUInt32(font, ref filePosition);

            return 0xB1B0AFBA - sum; 
        }

        private UInt32 CheckSum(byte[] block) {
            UInt32 sum = 0;
            int filePosition = 0;
            var nLongs = ((block.Length + 3) & ~3) >> 2;

            while (nLongs-- > 0)
                sum += GetUInt32(block, ref filePosition);

            return sum;            
        }

        private int SetUInt32(byte[] dest, int pos, UInt32 val) {
            dest[pos] = (byte)(val >> 24);
            dest[pos+1] = (byte)(val >> 16);
            dest[pos+2] = (byte)(val >> 8);
            dest[pos+3] = (byte)(val);
            return pos+4;
        }

         private int SetUInt32(byte[] dest, int pos, int val) {
            dest[pos] = (byte)(val >> 24);
            dest[pos+1] = (byte)(val >> 16);
            dest[pos+2] = (byte)(val >> 8);
            dest[pos+3] = (byte)(val);
            return pos+4;
        }

        private int SetUInt16(byte[] dest, int pos, int val) {
            dest[pos] = (byte)(val >> 8);
            dest[pos+1] = (byte)(val);
            return pos+2;
        }

        private int SetUInt8(byte[] dest, int pos, int val) {
            dest[pos] = (byte)(val);
            return pos+1;
        }

        private int SetString(byte[] dest, int pos, string val) {
            foreach (char ch in val) {
                dest[pos] = (byte)ch;
                pos++;
            }
            return pos;
        }

        public override void OnSaveEvent(IndirectObject indirectObject)
        {         

            // TTFfont is the font in ttf format has to be rewritten
            // a good guide: http://www.4real.gr/technical-documents-ttf-subset.html
            SubsetTTFFont = Subset(TTFFont);

/*
            var descendant = new DocumentDescendantFont(pdfObjects, this);  
            var cmap = new DocumentCmapFont(pdfObjects, this);  

            var entries = new Dictionary<string, PdfObject> {
                { "Type", new NameObject("Font") },
                { "Subtype", new NameObject("Type0") },
                { "Encoding", new NameObject("Identity-H") },
                { "DescendantFonts", new ArrayObject(new List<PdfObject> { descendant.IndirectReferenceObject }) },
                { "ToUnicode", new ArrayObject(new List<PdfObject> { cmap.IndirectReferenceObject }) },
                { "BaseFont", new NameObject(this.Name) }
            };   
   
            indirectObject.SetChild(new DictionaryObject(entries));
            */

            var widths = new List<PdfObject>();            
            widths.Add(new IntegerObject(this.Width));
            foreach (int i in hashChar) { 
                if (!dctCharCodeToGlyphID.ContainsKey(i)) {
                    widths.Add(new IntegerObject(this.Width));
                } else {
                    widths.Add(new IntegerObject(Glypth[dctCharCodeToGlyphID[i]].width));
                }
            }

            var descriptor = new DocumentTtfDescriptorFont(pdfObjects, this);
            var cmap = new DocumentCmapFont(pdfObjects, this);  

            var entries = new Dictionary<string, PdfObject> {
                { "Type", new NameObject("Font") },
                { "Subtype", new NameObject("TrueType") },
                { "BaseFont", new NameObject(Name) },
                { "FirstChar", new IntegerObject(0) },
                { "LastChar", new IntegerObject(hashChar.Count) },
                { "Widths", new ArrayObject(widths) },
                { "FontDescriptor", descriptor.IndirectReferenceObject },
                { "ToUnicode", cmap.IndirectReferenceObject },
            };   


            indirectObject.SetChild(new DictionaryObject(entries));
        }	
	}
}
