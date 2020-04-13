namespace SharpPDF.Lib {
    // 9.4.2 Text-Positioning Operators
    public class TextPositioningOperator : ITextOperator {
        readonly float x;
        readonly float y;
        
        public TextPositioningOperator(float x, float y) {
            this.x = x;
            this.y = y;
        }

        public override string ToString() {
            return $"{x} {y} Td";
        }
    }
}