namespace SharpPDF.Lib {
    // 9.4.2 Text-Positioning Operators
    public class TextPositioningOperator : Operator {
        public float X { get; }
        public float Y { get; }
        
        public TextPositioningOperator(float x, float y) {
            this.X = x;
            this.Y = y;
        }

        public override string ToString() {
            return $"{X} {Y} Td";
        }
    }
}