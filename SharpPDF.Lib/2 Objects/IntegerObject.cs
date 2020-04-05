using System.Globalization;

namespace SharpPDF.Lib {
    public class IntegerObject : IPdfObject {
        private readonly int value;

        public IntegerObject(int value) {
            this.value = value;
        }

        public IntegerObject(Tokenizer tokenizer) {
            string tokenContent = tokenizer.TokenExcludedCommentsAndWhitespaces().ToString();

            if (!int.TryParse(tokenContent, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value))                
                throw new PdfException(PdfExceptionCodes.INVALID_NUMBER_TOKEN, "Number cannot be cast to integer");
        }


        public int Value => value;
        public ObjectType ObjectType => Lib.ObjectType.Integer;
        public IPdfObject[] Childs() => new IPdfObject[0];

        public override string ToString() {
            return value.ToString();
        }

    }
}