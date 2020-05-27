using System.Globalization;

namespace crcPdf {
    // Table 59 – Path Construction Operators
    public class RectangleOperator : PathConstructionOperator {
        public float X { get; }
        public float Y { get; }
        public float Width { get; }
        public float Height { get; }
        
        public RectangleOperator(float x, float y, float width, float height) {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public override string ToString() 
            => $"{X.ToString(CultureInfo.InvariantCulture)} {Y.ToString(CultureInfo.InvariantCulture)} {Width.ToString(CultureInfo.InvariantCulture)} {Height.ToString(CultureInfo.InvariantCulture)} re";
    }
}