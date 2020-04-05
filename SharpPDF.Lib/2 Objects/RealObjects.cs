using System.Globalization;

namespace SharpPDF.Lib {
    public class RealObject : IPdfObject {
        private readonly float value;

        public RealObject(Tokenizer tokenizer) {
            string tokenContent = tokenizer.TokenExcludedCommentsAndWhitespaces().ToString();
                        
            if (!float.TryParse(tokenContent, 
                                NumberStyles.AllowDecimalPoint |
                                NumberStyles.AllowLeadingSign |
                                NumberStyles.AllowTrailingSign |
                                NumberStyles.AllowThousands, 
                                CultureInfo.InvariantCulture, 
                                out value))
                throw new PdfException(PdfExceptionCodes.INVALID_NUMBER_TOKEN, $"Number {tokenContent} cannot be cast to a float number");
        }
        public float Value => value;
        public ObjectType ObjectType => Lib.ObjectType.Real;
        public IPdfObject[] Childs() => new IPdfObject[0];
    }
}