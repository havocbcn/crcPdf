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

        public bool Validate(Token token, params string[] contents) => Validate(token, CharacterSetType.Regular, contents);

        public bool IsDelimiter(Token token, params string[] contents)
        {
            if (token.characterSetClass !=  CharacterSetType.Delimiter)
                return false;

            List<string> lstContents = new List<string>(contents);
            if (!lstContents.Contains(token.ToString()))
                return false;

            return true;
        }

        internal bool IsRegularNumber(Token token) {
             if (token.characterSetClass != CharacterSetType.Regular)
                return false;

            return IsRegularNumber(token.ToString());
        }

        private bool IsRegularNumber(string token) => token.All(ch => ch >= '0' && ch <= '9');
        
        internal bool IsIntegerNumber(Token token) {
             if (token.characterSetClass != CharacterSetType.Regular)
                return false;

            return IsIntegerNumber(token.ToString());
        }

        private bool IsIntegerNumber(string token) { 
            foreach(char ch in token)
            {
                if ((ch < '0' || ch >'9') && ch != '+' && ch != '-')
                    return false;
            }
            return true;
        }

        internal bool IsWhiteSpace(Token token) => token.characterSetClass ==  CharacterSetType.WhiteSpace;

        public bool IsWhiteSpace(Token token, params string[] contents)
        {
            if (token.characterSetClass !=  CharacterSetType.WhiteSpace)
                return false;

            List<string> lstContents = new List<string>(contents);
            if (!lstContents.Contains(token.ToString()))
                return false;

            return true;
        }

        internal bool IsRealNumber(Token token)
        {
            if (token.characterSetClass != CharacterSetType.Regular)
                return false;

            return IsRealNumber(token.ToString());
        }

         private bool IsRealNumber(string token)
        { 
            foreach(char ch in token)
            {
                if ((ch < '0' || ch >'9') && ch != '+' && ch != '-' && ch != '.')
                    return false;
            }
            return true;
        }
    }
}