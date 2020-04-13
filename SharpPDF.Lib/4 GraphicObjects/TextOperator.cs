namespace SharpPDF.Lib {
    // 9 Text
    public class TextOperator : ITextOperator {
        readonly string text;
        
        public TextOperator(string text) {
            this.text = text;
        }

        public override string ToString() {
            return $"({text}) Tj";
        }
    }
}