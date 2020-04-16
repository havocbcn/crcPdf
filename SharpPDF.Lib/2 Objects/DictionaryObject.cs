using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace SharpPDF.Lib {
    public class DictionaryObject : PdfObject {
        private readonly Dictionary<string, PdfObject> dictionary = new Dictionary<string, PdfObject>();
        private readonly byte[] stream;

        public DictionaryObject(string content) {
            stream = System.Text.ASCIIEncoding.ASCII.GetBytes(content);
            streamLength = stream.Length;
            dictionary.Add("Length", new IntegerObject(streamLength.Value));
            foreach (KeyValuePair<string, PdfObject> kvp in dictionary) {
                childs.Add(new NameObject(kvp.Key));
                childs.Add(kvp.Value);
            }
        }

        public DictionaryObject(Dictionary<string, PdfObject> values) {
            this.dictionary = values;
            
            foreach (KeyValuePair<string, PdfObject> kvp in values) {
                childs.Add(new NameObject(kvp.Key));
                childs.Add(kvp.Value);
            }
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
            
            if (!TokenValidator.IsDelimiter(tokenizer.TokenExcludedCommentsAndWhitespaces(), ">")) {
                throw new PdfException(PdfExceptionCodes.INVALID_DICTIONARY, "Expected >");
            }
            if (!TokenValidator.IsDelimiter(tokenizer.TokenExcludedComments(), ">")) {
                throw new PdfException(PdfExceptionCodes.INVALID_DICTIONARY, "Expected >");
            }

            if (HasStream) {
                if (!TokenValidator.Validate(tokenizer.TokenExcludedCommentsAndWhitespaces(), "stream")) {
                    throw new PdfException(PdfExceptionCodes.INVALID_DICTIONARY_STREAM, "A dictionary with Lenght hasnt stream object");
                }                

                if (!TokenValidator.IsWhiteSpace(tokenizer.Token(), "\n", "\r\n")) {
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

        private bool HasStream => streamLength.HasValue;        

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

            if (((NameObject)key).Value == "Length") {
                var lengthObject = value as IntegerObject;
                if (lengthObject == null) {
                    throw new PdfException(PdfExceptionCodes.DICTIONARY_VALUE_LENGTH_INTEGER, "A length value must be an integer number");
                }

                streamLength = lengthObject.IntValue;
            }
        }

        public override string ToString() {
            if (HasStream) {                
                return $"<<{string.Join(" ", childs)}>>stream\n{System.Text.Encoding.GetEncoding(1252).GetString(stream)}\nendstream";    
            }
            return $"<<{string.Join(" ", childs)}>>";
        }
        
        private byte[] Deflate(byte[] b, int predictor, int finalColumnCount) {
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