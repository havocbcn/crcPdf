using System.Globalization;

namespace SharpPDF.Lib {
    public class IndirectObject : PdfObject
    {
        private readonly int number;
        private readonly int generation;

        public IndirectObject(Tokenizer tokenizer) {
            ReadNumber(tokenizer.TokenExcludedCommentsAndWhitespaces(), ref number);
            ExpectAWhiteSpace(tokenizer.TokenExcludedComments());
            ReadNumber(tokenizer.TokenExcludedComments(), ref generation);
            if (generation < 0) {
                throw new PdfException(PdfExceptionCodes.INVALID_GENERATION, "Generation must be positive");
            }

            ExpectAText(tokenizer.TokenExcludedCommentsAndWhitespaces(), "obj");

            Objectizer analyzeChilds = new Objectizer(tokenizer);

            childs.Add(analyzeChilds.NextObject());

            ExpectAText(tokenizer.TokenExcludedCommentsAndWhitespaces(), "endobj");
        }

        public IndirectObject(int number) {
            this.number = number;
            generation = 0;            
        }

        public override int GetHashCode() => number.GetHashCode();

        public override bool Equals(object obj) {
            var indirect = (obj as IndirectObject);
            if (indirect == null) {
                return false;
            }
            
            return indirect.number == this.number;
        }

        public int Number => number;
        public int Generation => generation;

        public static implicit operator IndirectReferenceObject(IndirectObject a) => new IndirectReferenceObject(a.Number);
        
        private static void ExpectAText(Token token, string expected) {
            if (token.characterSetClass != CharacterSetType.Regular || token.ToString() != expected) {
                throw new PdfException(PdfExceptionCodes.INVALID_INDIRECTOBJECT_TOKEN, "Expected '" + expected + "' but '" + token.ToString() + "' appeared");
            }
        }

        private static void ExpectAWhiteSpace(Token token) {
            if (token.characterSetClass != CharacterSetType.WhiteSpace) {
                throw new PdfException(PdfExceptionCodes.INVALID_INDIRECTOBJECT_TOKEN, "Expected a whitespace");
            }
        }

        private static void ReadNumber(Token token, ref int number) {
            if (!int.TryParse(token.ToString(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out number)) {
                throw new PdfException(PdfExceptionCodes.INVALID_INDIRECTOBJECT_TOKEN, "Expected a number");
            }
        }

        public override string ToString() {
            return $"{number} {generation} obj {childs[0].ToString()} endobj\n";
        }
    }
}