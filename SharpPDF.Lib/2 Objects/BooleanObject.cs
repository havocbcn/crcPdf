namespace SharpPDF.Lib {
    public class BooleanObject : PdfObject {
        private readonly bool value;

        public BooleanObject(Tokenizer tokenizer)
        {
            if (tokenizer.TokenExcludedCommentsAndWhitespaces().ToString() == "true") {
                value = true;            
            } else {
                value = false;
            }
        }
        
        public bool Value => value;

        public override string ToString() {
            return value.ToString();
        }

        public override byte[] Save(Compression compression) {            
            return GetBytes(this.ToString());
        }
    }
}