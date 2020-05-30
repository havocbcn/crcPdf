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
using System.Text;

namespace crcPdf {
    public class NameObject : PdfObject {
        private readonly string value;

        public NameObject(string value) {
            this.value = value;
        }

        public NameObject(Tokenizer tokenizer) {
            Token nextToken = tokenizer.TokenExcludedCommentsAndWhitespaces();
            
            if (nextToken.ToString() != "/") {
                throw new PdfException(PdfExceptionCodes.INVALID_NAMEOBJECT_TOKEN, "Name object must start with a /");
            }

            var sb = new StringBuilder();

            while (tokenizer.IsNextTokenExcludedCommentsRegular()) {
                nextToken = tokenizer.TokenExcludedComments();    
                sb.Append(nextToken.ToString());
            }

            value = Escape(sb.ToString());
        }

        public string Value 
            => value;
        
        private static string Escape(string literalString) {
            StringBuilder literalStringEscaped = new StringBuilder();
            int i = 0;
            while (i < literalString.Length)
            {
                if (literalString[i] == '#' && i < literalString.Length - 2) {
                    HexadecimalEscaper hexEscaper = new HexadecimalEscaper();

                    literalStringEscaped.Append(hexEscaper.GetValue(literalString.Substring(i+1, 2)));
                    i+=3;
                }
                else {
                    literalStringEscaped.Append(literalString[i]);
                    i++;
                }
            }
            return literalStringEscaped.ToString();
        }

        public override string ToString() 
            => $"/{value}";

        public override bool Equals(object obj)
            => obj == null ? false : value != (obj as NameObject).value;
        
        public override int GetHashCode()
            => value.GetHashCode();

        public override byte[] Save(Compression compression)            
            => GetBytes(this.ToString());
    }
}