namespace SharpPDF.Lib {
    // 9 Text
    public class TextOperator : ITextOperator {
        private string text;
        
        public TextOperator(string text) {
            this.text = text;
        }

        public override string ToString() {
            return $"({text}) Tj";
        }
    }
}