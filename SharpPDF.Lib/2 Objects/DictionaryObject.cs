using System.Collections.Generic;
using System.Linq;

namespace SharpPDF.Lib
{
    public class DictionaryObject : IPdfObject
    {
        private readonly List<IPdfObject> childs = new  List<IPdfObject>();
        private readonly Dictionary<string, IPdfObject> dictionary = new Dictionary<string, IPdfObject>();
        private readonly byte[] stream = null;

        public DictionaryObject(string content) {
            stream = System.Text.ASCIIEncoding.ASCII.GetBytes(content);
            streamLength = stream.Length;
            dictionary.Add("Length", new IntegerObject(streamLength.Value));
            foreach (KeyValuePair<string, IPdfObject> kvp in dictionary) {
                childs.Add(new NameObject(kvp.Key));
                childs.Add(kvp.Value);
            }
        }

        public DictionaryObject(Dictionary<string, IPdfObject> values) {
            this.dictionary = values;
            
            foreach (KeyValuePair<string, IPdfObject> kvp in values) {
                childs.Add(new NameObject(kvp.Key));
                childs.Add(kvp.Value);
            }
        }
        
        public DictionaryObject(Tokenizer tokenizer) {
            var validator = new TokenValidator();

            if (!validator.IsDelimiter(tokenizer.TokenExcludedCommentsAndWhitespaces(), "<"))
                throw new PdfException(PdfExceptionCodes.INVALID_DICTIONARY, "Expected <");
            if (!validator.IsDelimiter(tokenizer.TokenExcludedComments(), "<"))
                throw new PdfException(PdfExceptionCodes.INVALID_DICTIONARY, "Expected <");

            while (!tokenizer.IsNextTokenExcludedCommentsAndWhitespaces(">"))
                ReadKeyValue(tokenizer);            
            
            if (!validator.IsDelimiter(tokenizer.TokenExcludedCommentsAndWhitespaces(), ">"))
                throw new PdfException(PdfExceptionCodes.INVALID_DICTIONARY, "Expected >");
            if (!validator.IsDelimiter(tokenizer.TokenExcludedComments(), ">"))
                throw new PdfException(PdfExceptionCodes.INVALID_DICTIONARY, "Expected >");

            if (HasStream)
            {   
                if (!validator.Validate(tokenizer.TokenExcludedCommentsAndWhitespaces(), "stream"))
                    throw new PdfException(PdfExceptionCodes.INVALID_DICTIONARY_STREAM, "A dictionary with Lenght hasnt stream object");

                if (!validator.IsWhiteSpace(tokenizer.Token(), "\n", "\r\n"))
                    throw new PdfException(PdfExceptionCodes.INVALID_DICTIONARY_STREAM, "A stream must be followed by \\n or \\r\\n");

                stream = tokenizer.ReadStream(streamLength.Value);

                if (!validator.IsWhiteSpace(tokenizer.Token(), "\n", "\r\n"))
                    throw new PdfException(PdfExceptionCodes.INVALID_DICTIONARY_STREAM, "A stream must be ended by \\n or \\r\\n");

                if (!validator.Validate(tokenizer.TokenExcludedCommentsAndWhitespaces(), "endstream"))
                    throw new PdfException(PdfExceptionCodes.INVALID_DICTIONARY_STREAM, "A dictionary with Lenght hasnt endstream object");

            }
        }

        public Dictionary<string, IPdfObject> Dictionary => dictionary;
        public byte[] Stream => stream;

        public ObjectType ObjectType => Lib.ObjectType.Dictionary;

        private int? streamLength = null;

        private bool HasStream => streamLength.HasValue;        

        private void ReadKeyValue(Tokenizer tokenizer)
        {
            var read = new Objectizer(tokenizer);

            var key = read.NextObject();
            if (key.ObjectType != Lib.ObjectType.Name)
                throw new PdfException(PdfExceptionCodes.DICTIONARY_KEY_NAMEOBJECT, "A name object expected as key in dictionary");

            childs.Add(key);

            var value = read.NextObject();
            childs.Add(value);

            dictionary.Add(((NameObject)key).Value, value);

            if (((NameObject)key).Value == "Length")
            {
                if (value.ObjectType!= Lib.ObjectType.Integer)
                    throw new PdfException(PdfExceptionCodes.DICTIONARY_VALUE_LENGTH_INTEGER, "A length value must be an integer number");

                streamLength = ((IntegerObject)value).Value;
            }
        }

        public IPdfObject[] Childs() => childs.ToArray();

        public override string ToString() {
            if (HasStream) {                
                return $"<<{string.Join(" ", childs)}>>stream\n{System.Text.Encoding.GetEncoding(1252).GetString(stream)}\nendstream";    
            }
            return $"<<{string.Join(" ", childs)}>>";
        }

    }
}