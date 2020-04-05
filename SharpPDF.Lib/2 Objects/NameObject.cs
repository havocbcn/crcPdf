using System.Text;

namespace SharpPDF.Lib {
    public class NameObject : IPdfObject {
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

        public ObjectType ObjectType => Lib.ObjectType.Name;

        private string Escape(string literalString)
        {
            StringBuilder literalStringEscaped = new StringBuilder();
            int i = 0;
            while (i < literalString.Length)
            {
                if (literalString[i] == '#' && i < literalString.Length - 2)
                {
                    HexadecimalEscaper hexEscaper = new HexadecimalEscaper();

                    literalStringEscaped.Append(hexEscaper.GetValue(literalString.Substring(i+1, 2)));
                    i+=3;
                }
                else
                {
                    literalStringEscaped.Append(literalString[i]);
                    i++;
                }
            }
            return literalStringEscaped.ToString();
        }

        public IPdfObject[] Childs() => new IPdfObject[0];

        public override string ToString() {
            return $"/{value}";
        }
    }
}