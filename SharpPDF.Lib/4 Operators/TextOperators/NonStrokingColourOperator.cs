using System.Globalization;

namespace SharpPDF.Lib {
    // 9 Text
    public class NonStrokingColourOperator : Operator {
        public float R { get; }
        public float G { get; }
        public float B { get; }
        
        public NonStrokingColourOperator(float r, float g, float b) {
            if (r < 0 || r > 1 ||
                g < 0 || g > 1 ||
                b < 0 || b > 1) {
                    throw new PdfException(PdfExceptionCodes.INVALID_COLOR, $"the color ${r},{g},{b} must be numbers between 0 and 1");
            }
                
            R = r;
            G = g;
            B = b;
        }

        public override string ToString() {
            return $"{R.ToString(CultureInfo.InvariantCulture)} {G.ToString(CultureInfo.InvariantCulture)} {B.ToString(CultureInfo.InvariantCulture)} rg";
        }
    }
}