using System.Collections.Generic;
using System.Linq;

namespace SharpPDF.Lib {
    public class TokenValidator {    
        public bool Validate(Token token, CharacterSetType tokenType, params string[] contents) {
            if (token.characterSetClass != tokenType)
                return false;

            List<string> lstContents = new List<string>(contents);
            if (!lstContents.Contains(token.ToString()))
                return false;

            return true;
        }

        public bool Validate(Token token, params string[] contents) 
            => Validate(token, CharacterSetType.Regular, contents);

        public bool IsDelimiter(Token token, params string[] contents)
            => token.characterSetClass == CharacterSetType.Delimiter && contents.Contains(token.ToString());

        internal bool IsRegularNumber(Token token) 
            => token.characterSetClass == CharacterSetType.Regular && IsRegularNumber(token.ToString());        

        private bool IsRegularNumber(string token) 
            => token.All(ch => ch >= '0' && ch <= '9');
        
        internal bool IsIntegerNumber(Token token) 
            => token.characterSetClass == CharacterSetType.Regular && IsIntegerNumber(token.ToString());

        private bool IsIntegerNumber(string token) 
            => token.All(ch => (ch >= '0' && ch <= '9') || ch =='+' || ch == '-');

        internal bool IsWhiteSpace(Token token) 
            => token.characterSetClass == CharacterSetType.WhiteSpace;

        internal bool IsWhiteSpace(Token token, params string[] contents) 
            => IsWhiteSpace(token) && contents.Contains(token.ToString());

        internal bool IsRealNumber(Token token)
            => token.characterSetClass == CharacterSetType.Regular && IsRealNumber(token.ToString());

        private bool IsRealNumber(string token)
            => token.All(ch => (ch >= '0' && ch <= '9') || ch =='+' || ch == '-' || ch=='.');
    }
}