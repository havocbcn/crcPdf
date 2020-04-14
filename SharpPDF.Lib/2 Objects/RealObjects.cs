using System.Globalization;

namespace SharpPDF.Lib {
    public class RealObject : PdfObject {
        internal float floatValue;

        public RealObject(float value) {
            this.floatValue = value;
        }

        public RealObject(Tokenizer tokenizer) {
            string tokenContent = tokenizer.TokenExcludedCommentsAndWhitespaces().ToString();
                        
            if (!float.TryParse(tokenContent, 
                                NumberStyles.AllowDecimalPoint |
                                NumberStyles.AllowLeadingSign |
                                NumberStyles.AllowTrailingSign |
                                NumberStyles.AllowThousands, 
                                CultureInfo.InvariantCulture, 
                                out floatValue))
                throw new PdfException(PdfExceptionCodes.INVALID_NUMBER_TOKEN, $"Number {tokenContent} cannot be cast to a float number");
        }
        public float FloatValue => floatValue;    
    }
}