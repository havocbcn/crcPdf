using System.Text;

namespace SharpPDF.Lib {
    public class NameObject : PdfObject {
        private readonly string value;

        public NameObject(string value) {
            this.value = value;
        }

        public NameObject(Tokenizer tokenizer) {
            Token nextToken = tokenizer.TokenExcludedCommentsAndWhitespaces();
            
            if (nextToken.ToString() != "/")
                throw new PdfException(PdfExceptionCodes.INVALID_NAMEOBJECT_TOKEN, "Name object must start with a /");
            
            StringBuilder sb = new StringBuilder();

            while (tokenizer.IsNextTokenExcludedCommentsRegular())
            {
                nextToken = tokenizer.TokenExcludedComments();    
                sb.Append(nextToken.ToString());
            }

            value = Escape(sb.ToString());
        }

        public string Value => value;
        
        private static string Escape(string literalString) {
            StringBuilder literalStringEscaped = new StringBuilder();
            int i = 0;
            while (i < literalString.Length)
            {
                if (literalString[i] == '#' && i < literalString.Length - 2) {
                    HexadecimalEscaper hexEscaper = new HexadecimalEscaper();

                    literalStringEscaped.Append(hexEscaper.GetValue(literalString.Substring(i+1, 2)));
                    i+=3;
                }
                else {
                    literalStringEscaped.Append(literalString[i]);
                    i++;
                }
            }
            return literalStringEscaped.ToString();
        }

        public override string ToString() {
            return $"/{value}";
        }

        public override bool Equals(object obj) {   
            var other = obj as NameObject;
            return obj == null ? false : value != other.value;
        }
        
        public override int GetHashCode()
            => value.GetHashCode();

        public override byte[] Save(Compression compression) {            
            return GetBytes(this.ToString());
        }
    }
}