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
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace crcPdf {
    public class DictionaryObject : PdfObject {
        private readonly Dictionary<string, PdfObject> dictionary = new Dictionary<string, PdfObject>();
        private readonly byte[] stream;
        private readonly string streamContent;

        public DictionaryObject(string content) {
            streamContent = content;
        }

        public DictionaryObject(Dictionary<string, PdfObject> values) {
            this.dictionary = values;
            
            foreach (KeyValuePair<string, PdfObject> kvp in values) {
                childs.Add(new NameObject(kvp.Key));
                childs.Add(kvp.Value);
            }
        }

        public DictionaryObject(Dictionary<string, PdfObject> values, byte[] stream) {
            this.dictionary = values;
            
            foreach (KeyValuePair<string, PdfObject> kvp in values) {
                childs.Add(new NameObject(kvp.Key));
                childs.Add(kvp.Value);
            }

            this.stream = stream;
        }
        
        public DictionaryObject(Tokenizer tokenizer) {
            Token token;

            if (!TokenValidator.IsDelimiter(tokenizer.TokenExcludedCommentsAndWhitespaces(), "<")) {
                throw new PdfException(PdfExceptionCodes.INVALID_DICTIONARY, "Expected <");
            }
            if (!TokenValidator.IsDelimiter(tokenizer.TokenExcludedComments(), "<")) {
                throw new PdfException(PdfExceptionCodes.INVALID_DICTIONARY, "Expected <");
            }

            while (!tokenizer.IsNextTokenExcludedCommentsAndWhitespaces(">")) {
                ReadKeyValue(tokenizer);            
            }

            tokenizer.TokenExcludedCommentsAndWhitespaces();            // first >

            if (!TokenValidator.IsDelimiter(tokenizer.TokenExcludedComments(), ">")) {
                throw new PdfException(PdfExceptionCodes.INVALID_DICTIONARY, "Expected >");
            }

            if (HasStream) {
                if (!TokenValidator.Validate(tokenizer.TokenExcludedCommentsAndWhitespaces(), "stream")) {
                    throw new PdfException(PdfExceptionCodes.INVALID_DICTIONARY_STREAM, "A dictionary with Lenght hasnt stream object");
                }                

                if (!tokenizer.ReadIntro()) {
                    throw new PdfException(PdfExceptionCodes.INVALID_DICTIONARY_STREAM, "A stream must be followed by \\n or \\r\\n");
                }

                stream = tokenizer.ReadStream(streamLength.Value);
                if (dictionary.ContainsKey("Filter")) {
                    if (((NameObject)dictionary["Filter"]).Value == "FlateDecode") {
                        int predictor = 1;
                        
                        if (dictionary.ContainsKey("Predictor")) {
                            predictor = ((IntegerObject)dictionary["Predictor"]).IntValue;
                        }

                        stream = Deflate(stream, predictor, 0);
                    }
                } 

                token = tokenizer.TokenExcludedCommentsAndWhitespaces();
                if (!TokenValidator.Validate(token, "endstream")) {
                    throw new PdfException(PdfExceptionCodes.INVALID_DICTIONARY_STREAM, $"Expected endstream but {token} found");
                }
            }
        }

        public Dictionary<string, PdfObject> Dictionary => dictionary;
        public byte[] Stream => stream;

        private int? streamLength;

        private bool HasStream => stream != null || streamLength.HasValue;        

        private void ReadKeyValue(Tokenizer tokenizer) {
            var read = new Objectizer(tokenizer);

            var key = read.NextObject() as NameObject;
            if (key == null) {
                throw new PdfException(PdfExceptionCodes.DICTIONARY_KEY_NAMEOBJECT, "A name object expected as key in dictionary");
            }
            
            childs.Add(key);

            var value = read.NextObject();
            childs.Add(value);

            dictionary.Add(key.Value, value);

            if (key.Value == "Length") {
                var lengthObject = value as IntegerObject;
                if (lengthObject == null) {
                    throw new PdfException(PdfExceptionCodes.DICTIONARY_VALUE_LENGTH_INTEGER, "A length value must be an integer number");
                }

                streamLength = lengthObject.IntValue;
            }
        }

        public override string ToString() {
            if (streamContent != null) {
                return $"<<{string.Join(" ", childs)}>>stream\n{streamContent}\nendstream";    
            } else if (HasStream) {                
                return $"<<{string.Join(" ", childs)}>>stream\n{BitConverter.ToString(stream).Replace("-","")}\nendstream";    
            }
            return $"<<{string.Join(" ", childs)}>>";
        }

        public override byte[] Save(Compression compression) {
            if (streamContent != null) {
                byte[] a2;
                if ((compression & Compression.Compress) == Compression.Compress) {
                    a2 = Flate(System.Text.Encoding.GetEncoding(1252).GetBytes(streamContent));
                    childs.Add(new NameObject("Filter"));
                    childs.Add(new NameObject("FlateDecode"));
                } else {
                    a2 = System.Text.Encoding.GetEncoding(1252).GetBytes(streamContent);
                }

                byte[] a3 = GetBytes($"\nendstream");

                childs.Add(new NameObject("Length"));
                childs.Add(new IntegerObject(a2.Length));                

                byte[] a1 = GetBytes($"<<{string.Join(" ", childs)}>>\nstream\n");
               
                return Join(a1, a2, a3);
            }
            else if (HasStream) {
                byte[] a1 = GetBytes($"<<{string.Join(" ", childs)}>>\nstream\n");
                byte[] a2 = stream;
                byte[] a3 = GetBytes($"\nendstream");
               
                return Join(a1, a2, a3);
            }
            return GetBytes($"<<{string.Join(" ", childs)}>>");
        }
        
        private static byte[] Flate(byte[] b) {            
            MemoryStream msOut = new MemoryStream();

            msOut.WriteByte(120);
            msOut.WriteByte(156);
       
            using (MemoryStream originalFileStream = new MemoryStream(b)) {
                using (DeflateStream compressionStream = new DeflateStream(msOut, CompressionMode.Compress)) {                    
                    originalFileStream.CopyTo(compressionStream);
                }
            }

            return msOut.ToArray();
		}

        private static byte[] Deflate(byte[] b, int predictor, int finalColumnCount) {
            byte[] result;
            using (MemoryStream msOut = new MemoryStream()) {
                using (MemoryStream inputStream = new MemoryStream(b)) {
                    inputStream.ReadByte();
                    inputStream.ReadByte();

                    using (DeflateStream gzip = new DeflateStream(inputStream, CompressionMode.Decompress)) {
                        gzip.CopyTo(msOut);
                    }
                }

                result = msOut.ToArray();
            }

            if (predictor == 1) {
                return result;
            } else if (predictor > 10) {
                int deflatedColumnCount = finalColumnCount + 1;
                int rowsCount = result.Length / deflatedColumnCount;
                if (rowsCount * (finalColumnCount + 1) != result.Length) {
                    throw new PdfException(PdfExceptionCodes.INVALID_FILTER, "decompressed stream length are not correct to use png filter");
                }

                byte[] finalResult = new byte[rowsCount * finalColumnCount];
                // https://stackoverflow.com/questions/23813941/reading-a-pdf-version-1-5-how-to-handle-cross-reference-stream-dictionary
                // https://www.w3.org/TR/PNG-Filters.html
                // byte[] forms a bidimensional array, width is worBytesWidth
                // first byte is a filter:
                // 0: None    |0|123  => 123
                // 1: sub     not implemented
                // 2: Up:     |2|123  => 246
                // 3: Average not implemented
                // 4: Paeth   not implemented
                int rowIndex = 0;
                while (rowIndex < rowsCount)
                {
                    switch (result[rowIndex * deflatedColumnCount])
                    {
                        case 0:
                            for (int j = 0; j < finalColumnCount; j++)
                            {
                                finalResult[rowIndex * finalColumnCount + j] = result[rowIndex * deflatedColumnCount + j + 1];
                            }
                            break;
                        case 2:
                            for (int j = 0; j < finalColumnCount; j++)
                            {
                                int value;
                                if (rowIndex == 0)
                                {
                                    value = result[rowIndex * deflatedColumnCount + j + 1];
                                }
                                else
                                {
                                    value = result[rowIndex * deflatedColumnCount + j + 1] + finalResult[(rowIndex - 1) * finalColumnCount + j];
                                }
                                finalResult[rowIndex * finalColumnCount + j] = (byte)(value % 256);
                            }
                            break;
                        default:
                            throw new PdfException(PdfExceptionCodes.COMPRESSION_NOT_IMPLEMENTED, "decompress filter " + result[rowIndex * deflatedColumnCount] + " not implemented");
                    }

                    rowIndex++;
                }

                return finalResult;
            }
            else
            {
                throw new PdfException(PdfExceptionCodes.COMPRESSION_NOT_IMPLEMENTED, $"predictor decompress {predictor} not implemented");
            }
        }
    }
}