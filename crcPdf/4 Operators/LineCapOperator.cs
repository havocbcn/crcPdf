namespace crcPdf {
    // 8.4.3.3 Line Cap Style
    public class LineCapOperator : Operator {
        public LineCapOperator(LineCapStyle lineCap) {
            LineCap = lineCap;
        }

        public LineCapStyle LineCap { get; }

        public override string ToString() => $"{(int)LineCap} J";
    }
}