namespace SharpPDF.Lib {
    // 9 Text
    public class FontOperator : Operator {
        public string Code { get; }
        public float Size { get; }
        
        public FontOperator(string code, float size) {
            Code = code;
            Size = size;
        }

        public override string ToString() {
            return $"/{Code} {floatToString(Size)} Tf";
        }
    }
}