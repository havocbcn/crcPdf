namespace SharpPDF.Lib {
    // 9.4.2 Text-Positioning Operators
    public class TextPositioningOperator : ITextOperator {
        private int x;
        private int y;
        
        public TextPositioningOperator(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public override string ToString() {
            return $"{x} {y} Td";
        }
    }
}