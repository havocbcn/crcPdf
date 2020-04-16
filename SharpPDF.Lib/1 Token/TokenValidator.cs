using System.Linq;

namespace SharpPDF.Lib {
    public class TokenValidator {    
        public static bool Validate(Token token, CharacterSetType tokenType, params string[] contents) 
            => token.characterSetClass == tokenType && contents.Contains(token.ToString());

        public static bool Validate(Token token, params string[] contents) 
            => Validate(token, CharacterSetType.Regular, contents);

        public static bool IsDelimiter(Token token, params string[] contents)
            => token.characterSetClass == CharacterSetType.Delimiter && contents.Contains(token.ToString());

         public static bool IsDelimiter(Token token)
            => token.characterSetClass == CharacterSetType.Delimiter;


        internal static bool IsRegularNumber(Token token) 
            => token.characterSetClass == CharacterSetType.Regular && IsRegularNumber(token.ToString());        

        private static bool IsRegularNumber(string token) 
            => token.All(ch => ch >= '0' && ch <= '9');
        
        internal static bool IsIntegerNumber(Token token) 
            => token.characterSetClass == CharacterSetType.Regular && IsIntegerNumber(token.ToString());

        private static bool IsIntegerNumber(string token) 
            => token.All(ch => (ch >= '0' && ch <= '9') || ch =='+' || ch == '-');

        internal static bool IsWhiteSpace(Token token) 
            => token.characterSetClass == CharacterSetType.WhiteSpace;

        internal static bool IsWhiteSpace(Token token, params string[] contents) 
            => IsWhiteSpace(token) && contents.Contains(token.ToString());

        internal static bool IsRealNumber(Token token)
            => token.characterSetClass == CharacterSetType.Regular && IsRealNumber(token.ToString());

        private static bool IsRealNumber(string token)
            => token.All(ch => (ch >= '0' && ch <= '9') || ch =='+' || ch == '-' || ch=='.');
    }
}