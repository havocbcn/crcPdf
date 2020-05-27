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