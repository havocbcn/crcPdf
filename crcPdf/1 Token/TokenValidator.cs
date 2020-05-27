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
using System.Linq;

namespace crcPdf {
    public static class TokenValidator {    
        public static bool Validate(Token token, CharacterSetType tokenType, params string[] contents) 
            => token.characterSetClass == tokenType && contents.Contains(token.ToString());

        public static bool Validate(Token token, params string[] contents) 
            => Validate(token, CharacterSetType.Regular, contents);

        public static bool IsDelimiter(Token token, params string[] contents)
            => IsDelimiter(token) && contents.Contains(token.ToString());

        public static bool IsDelimiter(Token token)
            => token.characterSetClass == CharacterSetType.Delimiter;
        
        private static bool IsRegular (Token token)
            => token.characterSetClass == CharacterSetType.Regular;

        internal static bool IsRegularNumber(Token token) 
            => IsRegular(token) && IsRegularNumber(token.ToString());        

        private static bool IsRegularNumber(string token) 
            => token.All(ch => ch >= '0' && ch <= '9');
        
        internal static bool IsIntegerNumber(Token token) 
            => IsRegular(token) && IsIntegerNumber(token.ToString());

        private static bool IsIntegerNumber(string token) 
            => token.All(ch => (ch >= '0' && ch <= '9') || ch =='+' || ch == '-');

        internal static bool IsWhiteSpace(Token token) 
            => token.characterSetClass == CharacterSetType.WhiteSpace;

        internal static bool IsWhiteSpace(Token token, params string[] contents) 
            => IsWhiteSpace(token) && contents.Contains(token.ToString());

        internal static bool IsRealNumber(Token token)
            => IsRegular(token) && IsRealNumber(token.ToString());

        private static bool IsRealNumber(string token)
            => token.All(ch => (ch >= '0' && ch <= '9') || ch =='+' || ch == '-' || ch=='.');
    }
}