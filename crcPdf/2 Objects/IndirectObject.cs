// This file is part of crcPdf.
// 
// crcPdf is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// crcPdf is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with crcPdf.  If not, see <http://www.gnu.org/licenses/>.
using System.Globalization;

namespace crcPdf {
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

        public override byte[] Save(Compression compression) {            
            byte[] a1 = GetBytes($"{number} {generation} obj ");
            byte[] a2 = childs[0].Save(compression);
            byte[] a3 = GetBytes($" endobj\n");

            return Join(a1, a2, a3);
        }
    }
}