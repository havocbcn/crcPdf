using System.Linq;

namespace crcPdf {
    // 9.4.3 Text Showing Operators
    public class TextOperator : Operator {
        readonly string text;
        
        public TextOperator(string text) {
            this.text = text;
        }

        public override string ToString() {
            return $"({text.Replace(@"\", @"\\").Replace("(", @"\(").Replace(")", @"\)")}) Tj";
        }
    }
}