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
    public class IndirectReferenceObject : PdfObject {
        private readonly int number;
        private readonly int generation;

        public IndirectReferenceObject(int number) {
            this.number = number;
            if (number <= 0) {
                throw new PdfException(PdfExceptionCodes.INVALID_NUMBER, "Indirect number must be positive");
            }
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

        public override byte[] Save(Compression compression) {            
            return GetBytes(this.ToString());
        }

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