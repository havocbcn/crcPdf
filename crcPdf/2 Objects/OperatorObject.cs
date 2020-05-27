namespace crcPdf {
    // in graphic or text objects there is special commands call operators
    public class OperatorObject : PdfObject {
        public OperatorObject(Tokenizer tokenizer) {            
            Token nextToken = tokenizer.TokenExcludedCommentsAndWhitespaces();

            Value = nextToken.ToString();
        }

        public string Value { get; private set; }

        public override string ToString() => Value;
        public override byte[] Save(Compression compression) {            
            return GetBytes(this.ToString());
        }
    }
}