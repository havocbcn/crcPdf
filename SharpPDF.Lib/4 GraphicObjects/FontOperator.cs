namespace SharpPDF.Lib {
    // 9 Text
    public class FontOperator : ITextOperator {
        readonly string code;
        readonly int size;
        
        public FontOperator(string code, int size) {
            this.code = code;
            this.size = size;
        }

        public override string ToString() {
            return $"/{code} {size} Tf";
        }
    }
}