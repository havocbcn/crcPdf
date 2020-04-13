namespace SharpPDF.Lib {
    // in graphic or text objects there is special commands call operators
    public class OperatorObject : PdfObject {
        public OperatorObject(Tokenizer tokenizer) {            
            Token nextToken = tokenizer.TokenExcludedCommentsAndWhitespaces();

            Value = nextToken.ToString();
        }

        public string Value { get; private set; }
        public PdfObject[] Childs() => new PdfObject[0];

        public override string ToString() {
            return Value;
        }
    }
}