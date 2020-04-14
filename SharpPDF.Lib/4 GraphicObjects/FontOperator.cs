namespace SharpPDF.Lib {
    // 9 Text
    public class FontOperator : Operator {
        public string Code { get; }
        public int Size { get; }
        
        public FontOperator(string code, int size) {
            Code = code;
            Size = size;
        }

        public override string ToString() {
            return $"/{Code} {Size} Tf";
        }
    }
}