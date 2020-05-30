using System.Globalization;

namespace crcPdf {
    // 9.3 Text State Parameters and Operators
    public class Operator {
        public string floatToString(float number)
            => number.ToString(CultureInfo.InvariantCulture);
    }
}