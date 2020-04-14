using System.Collections.Generic;

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
            var validator = new TokenValidator();
            Token token;

            if (!validator.IsDelimiter(tokenizer.TokenExcludedCommentsAndWhitespaces(), "<")) {
                throw new PdfException(PdfExceptionCodes.INVALID_DICTIONARY, "Expected <");
            }
            if (!validator.IsDelimiter(tokenizer.TokenExcludedComments(), "<")) {
                throw new PdfException(PdfExceptionCodes.INVALID_DICTIONARY, "Expected <");
            }

            while (!tokenizer.IsNextTokenExcludedCommentsAndWhitespaces(">")) {
                ReadKeyValue(tokenizer);            
            }
            
            if (!validator.IsDelimiter(tokenizer.TokenExcludedCommentsAndWhitespaces(), ">")) {
                throw new PdfException(PdfExceptionCodes.INVALID_DICTIONARY, "Expected >");
            }
            if (!validator.IsDelimiter(tokenizer.TokenExcludedComments(), ">")) {
                throw new PdfException(PdfExceptionCodes.INVALID_DICTIONARY, "Expected >");
            }

            if (HasStream) {
                if (!validator.Validate(tokenizer.TokenExcludedCommentsAndWhitespaces(), "stream")) {
                    throw new PdfException(PdfExceptionCodes.INVALID_DICTIONARY_STREAM, "A dictionary with Lenght hasnt stream object");
                }                

                if (!validator.IsWhiteSpace(tokenizer.Token(), "\n", "\r\n")) {
                    throw new PdfException(PdfExceptionCodes.INVALID_DICTIONARY_STREAM, "A stream must be followed by \\n or \\r\\n");
                }

                stream = tokenizer.ReadStream(streamLength.Value);

                token = tokenizer.TokenExcludedCommentsAndWhitespaces();
                if (!validator.Validate(token, "endstream")) {
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

            dictionary.Add(((NameObject)key).Value, value);

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
    }
}