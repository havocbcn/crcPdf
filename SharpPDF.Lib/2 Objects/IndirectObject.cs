using System.Globalization;

namespace SharpPDF.Lib {
    public class IndirectObject : IPdfObject
    {
        private readonly int number;
        private readonly int generation;
        private IPdfObject child;

        public IndirectObject(Tokenizer tokenizer) 
        {
            ReadNumber(tokenizer.TokenExcludedCommentsAndWhitespaces(), ref number);
            ExpectAWhiteSpace(tokenizer.TokenExcludedComments());
            ReadNumber(tokenizer.TokenExcludedComments(), ref generation);
            if (generation < 0)
                throw new PdfException(PdfExceptionCodes.INVALID_GENERATION, "Generation must be positive");

            ExpectAWhiteSpace(tokenizer.TokenExcludedComments());
            ExpectAText(tokenizer.TokenExcludedComments(), "obj");
            ExpectAWhiteSpace(tokenizer.TokenExcludedComments());

            Objectizer analyzeChilds = new Objectizer(tokenizer);

            child = analyzeChilds.NextObject();

            ExpectAWhiteSpace(tokenizer.TokenExcludedComments());
            ExpectAText(tokenizer.TokenExcludedComments(), "endobj");
        }

        public IndirectObject(int number) {
            this.number = number;
            generation = 0;
            this.child = null;
        }

        public void SetChild(IPdfObject child) {
            this.child = child;
        }

        public override int GetHashCode() => number.GetHashCode();

        public override bool Equals(object obj)
        {
            var indirect = (obj as IndirectObject);
            if (indirect == null)
                return false;
            
            return indirect.number == this.number;
        }

        public int Number => number;
        public int Generation => generation;

        public IPdfObject[] Childs() => new IPdfObject[1] { child };

        public static implicit operator IndirectReferenceObject(IndirectObject a) => new IndirectReferenceObject(a.Number);
        
        private void ExpectAText(Token token, string expected)
        {
            if (token.characterSetClass != CharacterSetType.Regular || token.ToString() != expected)
                throw new PdfException(PdfExceptionCodes.INVALID_INDIRECTOBJECT_TOKEN, "Expected '" + expected + "' but '" + token.ToString() + "' appeared");
        }

        private void ExpectAWhiteSpace(Token token)
        {
            if (token.characterSetClass != CharacterSetType.WhiteSpace)
                throw new PdfException(PdfExceptionCodes.INVALID_INDIRECTOBJECT_TOKEN, "Expected a whitespace");

        }

        private void ReadNumber(Token token, ref int number)
        {
            if (!int.TryParse(token.ToString(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out number))
                throw new PdfException(PdfExceptionCodes.INVALID_INDIRECTOBJECT_TOKEN, "Expected a number");
        }

        public ObjectType ObjectType => Lib.ObjectType.Indirect;

        public override string ToString() {
            return $"{number} {generation} obj {child.ToString()} endobj\n";
        }
    }
}