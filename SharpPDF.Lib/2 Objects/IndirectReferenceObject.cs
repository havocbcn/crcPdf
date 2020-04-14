using System.Globalization;

namespace SharpPDF.Lib {
    public class IndirectReferenceObject : PdfObject {
        private readonly int number;
        private readonly int generation;

        public IndirectReferenceObject(int number) {
            this.number = number;
            generation = 0;            
        }

        public IndirectReferenceObject(Tokenizer tokenizer) {
            ReadNumber(tokenizer.TokenExcludedCommentsAndWhitespaces(), ref number);
            if (number <= 0) {
                throw new PdfException(PdfExceptionCodes.INVALID_NUMBER, "Indirect number must be positive");
            }
            
            ExpectAWhiteSpace(tokenizer.TokenExcludedComments());
            
            ReadNumber(tokenizer.TokenExcludedComments(), ref generation);
            if (generation < 0) {
                throw new PdfException(PdfExceptionCodes.INVALID_GENERATION, "Generation must be positive");
            }

            ExpectAWhiteSpace(tokenizer.TokenExcludedComments());            
            ExpectAText(tokenizer.TokenExcludedComments(), "R");
        }

        public override string ToString() => $"{number} {generation} R";

        public override int GetHashCode() => number.GetHashCode();

        public override bool Equals(object obj) {
            var indirect = (obj as IndirectReferenceObject);
            if (indirect == null) {
                return false;
            }
            
            return indirect.number == this.number;
        }

        public int Number => number;
        public int Generation => generation;
        public static implicit operator IndirectObject(IndirectReferenceObject a) => new IndirectObject(a.Number);    
        private void ExpectAText(Token token, string expected) {
            if (token.characterSetClass != CharacterSetType.Regular || token.ToString() != expected) {
                throw new PdfException(PdfExceptionCodes.INVALID_INDIRECTOBJECT_TOKEN, "Expected '" + expected + "' but '" + token.ToString() + "' appeared");
            }
        }

        private void ExpectAWhiteSpace(Token token) {
            if (token.characterSetClass != CharacterSetType.WhiteSpace) {
                throw new PdfException(PdfExceptionCodes.INVALID_INDIRECTOBJECT_TOKEN, "Expected a whitespace");
            }
        }

        private void ReadNumber(Token token, ref int number) {
            if (!int.TryParse(token.ToString(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out number)) {
                throw new PdfException(PdfExceptionCodes.INVALID_INDIRECTOBJECT_TOKEN, "Expected a number");
            }
        }
    }
}