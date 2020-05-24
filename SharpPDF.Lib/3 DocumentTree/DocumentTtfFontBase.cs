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
using System;
using System.Collections.Generic;
using System.IO;
using SharpPDF.Lib.Fonts;

namespace SharpPDF.Lib {
	public abstract class DocumentTtfFontBase : DocumentFont {
		internal FontTypes Flags { get; set; }

		internal bool IsEmbedded { get; set; }

        internal Dictionary<string, Table> dctTables = new Dictionary<string, Table>();

        protected DocumentTtfFontBase(PDFObjects pdf, string FontFullPath) : base(pdf) {
            FullPath = FontFullPath;
            Flags = FontTypes.Nonsymbolic;
            boundingBox[0] = -1166;
            boundingBox[1] = -638;
            boundingBox[2] = 2260;
            boundingBox[3] = 1050;

            Ascendent = 800;
            Descendent = -200;

            Width = 1000;

            Process(File.ReadAllBytes(FontFullPath));
        }

		protected DocumentTtfFontBase(PDFObjects pdf, DictionaryObject dic) : base(pdf) {			
            var descriptorDictionary = pdf.GetObject<DictionaryObject>(dic.Dictionary["FontDescriptor"]).Dictionary;

            this.StemV = pdf.GetObject<IntegerObject>(descriptorDictionary["StemV"]).IntValue;
            this.Name  = pdf.GetObject<NameObject>(descriptorDictionary["FontName"]).Value;
            var boundingBoxArray = pdf.GetObject<ArrayObject>(descriptorDictionary["FontBBox"]).childs;

            this.boundingBox[0] = (short)pdf.GetObject<IntegerObject>(boundingBoxArray[0]).IntValue;
            this.boundingBox[1] = (short)pdf.GetObject<IntegerObject>(boundingBoxArray[1]).IntValue;
            this.boundingBox[2] = (short)pdf.GetObject<IntegerObject>(boundingBoxArray[2]).IntValue;
            this.boundingBox[3] = (short)pdf.GetObject<IntegerObject>(boundingBoxArray[3]).IntValue;

            this.ItalicAngle = pdf.GetObject<IntegerObject>(descriptorDictionary["ItalicAngle"]).IntValue;
            this.Ascendent = (short)pdf.GetObject<IntegerObject>(descriptorDictionary["Ascent"]).IntValue;
            this.Descendent = (short)pdf.GetObject<IntegerObject>(descriptorDictionary["Descent"]).IntValue;
            this.CapHeight = (short)pdf.GetObject<IntegerObject>(descriptorDictionary["CapHeight"]).IntValue;

            this.FirstChar = pdf.GetObject<IntegerObject>(dic.Dictionary["FirstChar"]).IntValue;
            this.LastChar = pdf.GetObject<IntegerObject>(dic.Dictionary["LastChar"]).IntValue;

            var widths = pdf.GetObject<ArrayObject>(dic.Dictionary["Widths"]);

			if (descriptorDictionary.ContainsKey("FontFile2")) {
				Process(pdf.GetObject<DictionaryObject>(descriptorDictionary["FontFile2"]).Stream);

				if (dic.Dictionary.ContainsKey("ToUnicode")) {
                    var cmap = pdf.GetObject<DictionaryObject>(dic.Dictionary["ToUnicode"]);
                    ReadCmap(cmap);
                }
            } else {
				var lstGlyph = new List<FontGlyph>();
				
				int i = 0;
				foreach (var width in widths.Childs<IntegerObject>()) {
					if (width.IntValue > 0) {
						lstGlyph.Add(
							new Fonts.FontGlyph {
								width =  width.IntValue
							}
						);
						hashChar.Add(FirstChar+i);
						dctCharCodeToGlyphID.Add(FirstChar+i, lstGlyph.Count-1);
					}
					i++;
				}
				Glypth = lstGlyph.ToArray();
			}			
 		}

        private void ReadCmap(DictionaryObject cmap) {
            var stream = new MemoryStream(cmap.Stream);
            var reader = new Objectizer(new Tokenizer(stream));

			 GlyphToChar firstChar = new GlyphToChar {
                character = 0,
                oldGlyphId = 0,
                newGlyphId = 0
            };
            lstGlyphToChar.Add(firstChar);

            dctCharToGlyphOldNew.Add(0, firstChar);
            dctNewGlyphIdToGlyphOldNew.Add(0, firstChar);

            while (!reader.IsEOF()) {
                var o = reader.NextObject(true);

                if (o is OperatorObject) {
                    var op = o as OperatorObject;
                    if (op.Value == "endcodespacerange") {
                        var numberOrEndCmap = reader.NextObject(true);
                        if (numberOrEndCmap is IntegerObject) {
                            var numberOfItems = numberOrEndCmap as IntegerObject;
                            var typeOfRange = reader.NextObject(true) as OperatorObject;
                            if (typeOfRange.Value == "beginbfchar") {
                                for (int i = 0; i < numberOfItems.IntValue; i++) {
                                    var codeInHex = reader.NextObject(true) as StringObject;
                                    int code = (int)codeInHex.Value[0];
                                    var unicodeInHex = reader.NextObject(true) as StringObject;
									if (unicodeInHex.Value.Length == 2) {
										int unicode = ((int)unicodeInHex.Value[0] << 8) + (int)unicodeInHex.Value[1];

										hashChar.Add(unicode);

										GlyphToChar glyph = new GlyphToChar {
											character = unicode,
											oldGlyphId = code,
											newGlyphId = code
										};

										lstGlyphToChar.Add(glyph);

										dctCharToGlyphOldNew.Add(unicode, glyph);
										dctNewGlyphIdToGlyphOldNew.Add(code, glyph);

										if (!dctCharCodeToGlyphID.ContainsKey(unicode)) {
											dctCharCodeToGlyphID.Add(unicode, code);
										}

									} else {
                                        // TODO
                                        throw new PdfException(PdfExceptionCodes.CMAP_TODO, "I do not undertand this cmap");
                                    }
                                }
                            } else if (typeOfRange.Value == "beginbfrange") {
                                for (int i = 0; i < numberOfItems.IntValue; i++) {
                                    var codeStartInHex = reader.NextObject(true) as StringObject;
                                    int codeStart = (int)codeStartInHex.Value[0];
                                    var codeEndInHex = reader.NextObject(true) as StringObject;
                                    int codeEnd = (int)codeEndInHex.Value[0];
                                    var unicodeInHex = reader.NextObject(true) as StringObject;
                                    if (unicodeInHex.Value.Length == 2) {
                                        int unicode = ((int)unicodeInHex.Value[0] << 8) + (int)unicodeInHex.Value[1];

                                        int counter = 0;
                                        for (int code = codeStart; code < codeEnd; code++) {
                                            hashChar.Add(code);

											GlyphToChar glyph = new GlyphToChar {
												character = unicode,
												oldGlyphId = code,
												newGlyphId = code
											};

											lstGlyphToChar.Add(glyph);

											dctCharToGlyphOldNew.Add(unicode, glyph);
											dctNewGlyphIdToGlyphOldNew.Add(code, glyph);

											if (!dctCharCodeToGlyphID.ContainsKey(unicode + counter))												
                                            	dctCharCodeToGlyphID.Add(unicode + counter, code);
                                            counter++;
                                        }
                                    } else {
                                        // TODO
                                        throw new PdfException(PdfExceptionCodes.CMAP_TODO, "I do not undertand this cmap");
                                    }
                                }
                            }
                            reader.NextObject(true);    // endbfchar or endbfrange
                        }
                    }
                }
            }
        }

        private void Process(byte[] stream) {
			TTFFont = stream;

			int version = GetUInt32();

			if (version != 0x00010000 && version != 0x4F54544F) {
				throw new PdfException(PdfExceptionCodes.FONT_NOT_SUPPORTED, $"TTF version not supported: {version.ToString("X4")}");
			}
						
			ushort numTables = GetUInt16();	// Number of tables.
			int searchRange = GetUInt16();
            Skip(2);						// entrySelector
            int rangeShift = GetUInt16();

            if (rangeShift != numTables * 16 - searchRange) {
				throw new PdfException(PdfExceptionCodes.FONT_ERROR, "rangeShift is not correct");
			}

			for (int i = 0; i < numTables; i++) {
				string tag = GetString(4);
				
				dctTables.Add(tag, new Table{
                    checksum = GetUInt32(),
					offset = GetUInt32(),
					length = GetUInt32()
				});
			}

			// kerning
			ProcessGLYPH(dctTables["glyf"], dctTables["loca"]);
			ProcessHead(dctTables["head"]);
			ProcessHHEA(dctTables["hhea"]);
			ProcessHMTX(dctTables["hmtx"]);
			ProcessCMAP(dctTables["cmap"]);
			ProcessNAME(dctTables["name"]);
		}

        private void ProcessNAME(Table table) {
            filePosition = table.offset;
			ushort format = GetUInt16();

			if (format == 0) {
				ushort counts = GetUInt16();
				ushort offset = GetUInt16();

				for (int i = 0; i < counts; i++) {
					ushort platformId = GetUInt16();
					ushort encodingId = GetUInt16();
					ushort languageId = GetUInt16();
					ushort nameId = GetUInt16();
					ushort length = GetUInt16();
					ushort stringOffset = GetUInt16();

					int actualFilePosition = filePosition;
					filePosition = table.offset + offset + stringOffset;
					string fontString = GetString(length);
					filePosition = actualFilePosition;

					if (nameId == 4 && // 4 	Full font name that reflects all family and relevant subfamily descriptors. The full font name is generally a combination of name IDs 1 and 2, or of name IDs 16 and 17, or a similar human-readable variant. 
						(languageId == 0 || languageId == 1033 /* english US */ || languageId == 2057 /* english UK */)) {
						if (string.IsNullOrEmpty(Name)) {
							Name = fontString;
						}
					}					
				}

			}

						// simplificación
			// TODO
			Name = ""; // Path.GetFileNameWithoutExtension(TTFFileName);


        }

        private void ProcessGLYPH(Table tableGlyph, Table tableLoca) {
			filePosition = tableLoca.offset;

			int[] glyphOffset;

			if (indexToLocFormat == 0) {
				int numEntries = tableLoca.length / 2;	

    			glyphOffset = new int[numEntries];

				for (int i = 0; i < numEntries; i++) {
					glyphOffset[i] = GetUInt16() << 1; // The actual local offset divided by 2 is stored. 
				}
			} else {
				int numEntries = tableLoca.length / 4;	

				glyphOffset = new int[numEntries];

				for (int i = 0; i < numEntries; i++) {
					glyphOffset[i] = GetUInt32();
				}
			}
				
			Glypth = new FontGlyph[glyphOffset.Length-1]; // In order to compute the length of the last glyph element, there is an extra entry after the last valid index.

			// In order to compute the length of the last glyph element, there is an extra entry after the last valid index.
			for (int i = 0; i < glyphOffset.Length-1; i++) {				
				// REMEMBER: filePosition = tableGlyph.offset + glyphOffset[i] + 2;

				Glypth[i] = new FontGlyph();
				Glypth[i].SetFilePosition(tableGlyph.offset + glyphOffset[i], glyphOffset[i+1] - glyphOffset[i]);

				// REMEMBER: Skip(8);  	// lowerX, lowerY, upperX, uppery				
			}
		}

       	private void ProcessHead(Table table) {
			filePosition = table.offset;
        
			ushort majorVersion = GetUInt16();
			ushort minorVersion = GetUInt16();

			if (majorVersion != 1 || minorVersion != 0) {
				throw new NotSupportedException("TTF head version not supported: " + majorVersion + "." + minorVersion);
			}

			Skip(8);		// fontRevision, checkSumAdjustment

			int magicNumber = GetUInt32();
			if (magicNumber != 0x5F0F3CF5) {
				throw new NotSupportedException("TTF version not supported head.magicNumber: " + magicNumber.ToString("X4"));
			}

			Skip(2);	// flags
						// Bit 0: Baseline for font at y=0
						// Bit 1: Left sidebearing point at x=0 (relevant only for TrueType rasterizers) — see the note below regarding variable fonts
						// Bit 2: Instructions may depend on point size
						// Bit 3: Force ppem to integer values for all internal scaler math; may use fractional ppem sizes if this bit is clear
						// Bit 4: Instructions may alter advance width (the advance widths might not scale linearly)
						// Bit 5: This bit is not used in OpenType, and should not be set in order to ensure compatible behavior on all platforms. If set, it may result in different behavior for vertical layout in some platforms. (See Apple's specification for details regarding behavior in Apple platforms.)
						// Bits 6–10: These bits are not used in Opentype and should always be cleared. (See Apple's specification for details regarding legacy used in Apple platforms.)
						// Bit 11: Font data is ‘lossless’ as a results of having been subjected to optimizing transformation and/or compression (such as e.g. compression mechanisms defined by ISO/IEC 14496-18, MicroType Express, WOFF 2.0 or similar) where the original font functionality and features are retained but the binary compatibility between input and output font files is not guaranteed. As a result of the applied transform, the ‘DSIG’ Table may also be invalidated.
						// Bit 12: Font converted (produce compatible metrics)
						// Bit 13: Font optimized for ClearType™. Note, fonts that rely on embedded bitmaps (EBDT) for rendering should not be considered optimized for ClearType, and therefore should keep this bit cleared.
						// Bit 14: Last Resort font. If set, indicates that the glyphs encoded in the cmap subtables are simply generic symbolic representations of code point ranges and don’t truly represent support for those code points. If unset, indicates that the glyphs encoded in the cmap subtables represent proper support for those code points.
						// Bit 15: Reserved, set to 0 

			unitsPerEm = GetUInt16();

			Skip(16);	// creationtime modifiedtime

			boundingBox[0] = GetInt16();
			boundingBox[1] = GetInt16();
			boundingBox[2] = GetInt16();
			boundingBox[3] = GetInt16();
			
			Skip(6);	// macStyle						
						// Bit 0: Bold (if set to 1)
						// Bit 1: Italic (if set to 1)
						// Bit 2: Underline (if set to 1)
						// Bit 3: Outline (if set to 1)
						// Bit 4: Shadow (if set to 1)
						// Bit 5: Condensed (if set to 1)
						// Bit 6: Extended (if set to 1)
						// Bits 7–15: Reserved (set to 0).
						// lowestRecPPEM
						// fontDirectionHint
						
			indexToLocFormat = GetInt16();
		}

		private void ProcessHHEA(Table table) {
			filePosition = table.offset;

			ushort majorVersion = GetUInt16();
			ushort minorVersion = GetUInt16();

			if (majorVersion != 1 || minorVersion != 0) {
				throw new NotSupportedException("TTF head version not supported: " + majorVersion + "." + minorVersion);
			}

			Ascendent = GetInt16();
			Descendent = GetInt16();

			Skip(26);	// LineGap:  Typographic line gap. Negative LineGap values are treated as zero in Windows 3.1, and in Mac OS System 6 and System 7.
						// advanceWidthMax: Maximum advance width value in 'hmtx' table.
						// minLeftSideBearing: Minimum left sidebearing value in 'hmtx' table.
						// minRightSideBearing: Minimum right sidebearing value; calculated as Min(aw - lsb - (xMax - xMin)).
						// xMaxExtent: Max(lsb + (xMax - xMin)).
						// caretSlopeRise: Used to calculate the slope of the cursor (rise/run); 1 for vertical.
						// caretSlopeRun: 0 for vertical.
						// caretOffset: The amount by which a slanted highlight on a glyph needs to be shifted to produce the best appearance. Set to 0 for non-slanted fonts	
						// 8						
						// metricDataFormat

			numberOfHMetrics = GetUInt16();
		}

		private void ProcessCMAP(Table table) {
			filePosition = table.offset;
			Skip(2);	// cmapVersion
			ushort cmapNumTables = GetUInt16();

            int CMAP_OFFSET = 0;

			for (int i = 0; i < cmapNumTables; i++) {
				ushort platformID = GetUInt16();
				ushort encodingID = GetUInt16();
				int offset = GetUInt32();
                if (platformID == 1 && encodingID == 0) {
                    CMAP_OFFSET = table.offset + offset;
                } else if (platformID == 3 && encodingID == 10) {
                    CMAP_OFFSET = table.offset + offset;					
				} else if (platformID == 3 && encodingID == 1) {
                    CMAP_OFFSET = table.offset + offset;					
				} else if (CMAP_OFFSET == 0) {
                    CMAP_OFFSET = table.offset + offset;					
				}
			}

            if (CMAP_OFFSET > 0) {
                filePosition = CMAP_OFFSET;
                int format = GetUInt16();

                switch (format) {
                    case 0:
                        ProcessCMAP0();
                        break;
                    case 4:
                        ProcessCMAP4();
                        break;
                    case 6:
                        ProcessCMAP6();
                        break;
                    case 12:
                        ProcessCMAP12();
                        break;
					default:
						throw new PdfException(PdfExceptionCodes.FONT_NOT_SUPPORTED, $"CMAP format {format} not supported");	
                }
            } else {
				throw new PdfException(PdfExceptionCodes.FONT_NOT_SUPPORTED, "CMAP platform not supported");
			}
		}

        private void ProcessCMAP0() {
            Skip(4);
            for (int i = 0; i < 256; i++) {
                int glyphId = GetUInt8();
                dctCharCodeToGlyphID.Add(i, glyphId);
                Glypth[glyphId].unicode = i;
            }
        }

        private void ProcessCMAP4() {
            int length = GetUInt16();           // This is the length in bytes of the subtable.
			Skip(2);				            // language, Please see "Note on the language field in 'cmap' subtables" in this document.
            int segmentCount = GetUInt16() >> 1;    // 2 x segCount.
            int searchRange = GetUInt16();
            Skip(2);							// entrySelector
            int rangeShift = GetUInt16();
            if (rangeShift != (segmentCount << 1) - searchRange) {
                throw new PdfException(PdfExceptionCodes.FONT_ERROR, "Invalid CMAP 4 format");
			}

            int[] endCode = new int[segmentCount];
            for (int i = 0; i < segmentCount; i++) {
                endCode[i] = GetUInt16();
            }

			Skip(2); 							// reservedPad 0

            ushort[] startCode = new ushort[segmentCount];
            for (int i = 0; i < segmentCount; i++) {
                startCode[i] = GetUInt16();
            }
            ushort[] idDelta = new ushort[segmentCount];
            for (int i = 0; i < segmentCount; i++) {
                idDelta[i] = GetUInt16();
            }
            ushort[] idRangeOffset = new ushort[segmentCount];
            for (int i = 0; i < segmentCount; i++) {
                idRangeOffset[i] = GetUInt16();
            }
            ushort[] glyphIdArray = new ushort[(length >> 1) - 8 - segmentCount << 2];
            for (int i = 0; i < glyphIdArray.Length; i++) {
                glyphIdArray[i] = GetUInt16();
            }

            for (ushort segmentIndex = 0; segmentIndex < segmentCount; segmentIndex++) {
                for (int charIndex = startCode[segmentIndex]; charIndex <= endCode[segmentIndex]; charIndex++) {
                    ushort glyphId; 
                    if (idRangeOffset[segmentIndex] == 0) {
                        glyphId = (ushort)(charIndex + idDelta[segmentIndex]);
						dctCharCodeToGlyphID.Add(charIndex, glyphId);
                    	Glypth[glyphId].unicode = charIndex;
                    } else {
						int j = (idRangeOffset[segmentIndex] >> 1) + (charIndex - startCode[segmentIndex]) - (segmentCount - segmentIndex);

						if (glyphIdArray[j] != 0) {
                        	glyphId = (ushort)(glyphIdArray[j] + idDelta[segmentIndex]);
							dctCharCodeToGlyphID.Add(charIndex, glyphId);

							if (glyphId < Glypth.Length) {
                    			Glypth[glyphId].unicode = charIndex;
							} else {
								Console.WriteLine($"error {glyphId} of {Glypth.Length}");
							}
						}
						// if 0 => missingGlyph
                    }

                }
            }
        }

        private void ProcessCMAP6() {
            Skip(4);
            int ini = GetUInt16();
            int count = GetUInt16();

            for (int i = ini; i < ini + count; i++) {
                int glyphId = GetUInt16();
                dctCharCodeToGlyphID.Add(i, glyphId);
                Glypth[glyphId].unicode = i;
            }
        }

		private void ProcessCMAP12() {
			Skip(10); 	// reserved (16)
						// length (32)
						// language (32)
			int numGroups = GetUInt32();

			for (int i = 0; i < numGroups; i++) {
				int startCharCode = GetUInt32();
				int endCharCode = GetUInt32();
				int startGlyphID = GetUInt32();

				int z = 0;
				for (int j = startCharCode; j <= endCharCode; j++) {
					dctCharCodeToGlyphID.Add(j, startGlyphID + z);
					Glypth[startGlyphID + z].unicode = j;
					z++;
				}
			}
		}

		private void ProcessHMTX(Table table)
		{
			filePosition = table.offset;

			int lastGlypthWidth = 0;
			ushort lastLeftSideBearing = 0;
			for (int i = 0; i < numberOfHMetrics; i++) {						
				lastGlypthWidth = GetUInt16();
				lastLeftSideBearing = GetUInt16();
				Glypth[i].SetWidthAndLeftSideBearing(lastGlypthWidth * 1000 / unitsPerEm, lastGlypthWidth, lastLeftSideBearing);
			}

			for (int i = numberOfHMetrics; i < Glypth.Length; i++) {
				Glypth[i].SetWidthAndLeftSideBearing(lastGlypthWidth * 1000 / unitsPerEm, lastGlypthWidth, lastLeftSideBearing);
			}
		}

        public override byte[] FontByteArray => TTFFont;

      	internal struct Table {
			internal int offset;
			internal int length;
            internal int checksum;
		}		


		/// <summary>
		/// Number of hMetric entries in 'hmtx' table
		/// </summary>
		private ushort numberOfHMetrics;

		/// <summary>
		/// Valid range is from 16 to 16384. This value should be a power of 2 for fonts that have TrueType outlines.
		/// </summary>
		internal ushort unitsPerEm;

		private int filePosition;

		private short GetInt16() {
			short result = (short) (TTFFont[filePosition] << 8 | TTFFont[filePosition+1]);
			filePosition += 2;
			return result;
		}

        private ushort GetUInt8() {           
            ushort result = (ushort) (TTFFont[filePosition]);
            filePosition += 1;
            return result;
        }

		private ushort GetUInt16() {			
			ushort result = (ushort) (TTFFont[filePosition] << 8 | TTFFont[filePosition+1]);
			filePosition += 2;
			return result;
		}

		private int GetUInt32() {
			int result = (TTFFont[filePosition] << 24 | TTFFont[filePosition+1] << 16 | TTFFont[filePosition+2] << 8 | TTFFont[filePosition+3]);
			filePosition += 4;
			return result;
		}

		private string GetString(int length) {
			string ret = System.Text.Encoding.ASCII.GetString(TTFFont, filePosition, length);
			filePosition += length;
			return ret;
		}

		private void Skip(int bytes) {
			filePosition += bytes;
		}
    }
}