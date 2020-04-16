using System.Globalization;

namespace SharpPDF.Lib {
    // 9.3 Text State Parameters and Operators
    public class Operator {
        public string Number(float number)
            => number.ToString(CultureInfo.InvariantCulture);
    }
}