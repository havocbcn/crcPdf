using System.Globalization;

namespace crcPdf {
    public class IntegerObject : RealObject {
        private readonly int intValue;

        public IntegerObject(int value) : base(value) {
            this.intValue = value;
        }

        public IntegerObject(Tokenizer tokenizer) : base(0) {
            string tokenContent = tokenizer.TokenExcludedCommentsAndWhitespaces().ToString();

            if (!int.TryParse(tokenContent, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out intValue)) {
                throw new PdfException(PdfExceptionCodes.INVALID_NUMBER_TOKEN, "Number cannot be cast to integer");
            }

            this.floatValue = intValue;
        }

        public int IntValue => intValue;

        public override string ToString() {
            return intValue.ToString();
        }

        public override byte[] Save(Compression compression) {            
            return GetBytes(this.ToString());
        }
    }
}