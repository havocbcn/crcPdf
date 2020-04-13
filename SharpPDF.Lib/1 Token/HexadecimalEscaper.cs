using System.Text;

namespace SharpPDF.Lib {
    public class HexadecimalEscaper {
        private const int FromaToAInAscii = 32;   

        public string ConvertHexToString(string rawHexString) {
            StringBuilder hexString  = new StringBuilder();
            
            for (int i = 0; i < rawHexString.Length; i+=2) {
                if (i+1 < rawHexString.Length)
                    hexString.Append((char)((GetHexVal(rawHexString[i]) << 4) + (GetHexVal(rawHexString[i+1]))));
                else
                    hexString.Append((char)(GetHexVal(rawHexString[i]) << 4));
            }
            
            return hexString.ToString();
        }

        public string FilterNonHexCharAndUpperCase(string rawHex) {
            StringBuilder sb = new StringBuilder();

            foreach (char c in rawHex) {
                if (c >= '0' && c <='9')
                    sb.Append(c);
                else if (c >= 'A' && c <= 'Z')
                    sb.Append(c);
                else if (c >= 'a' && c <= 'z')
                    sb.Append((char)(c - FromaToAInAscii));                
            }

            return sb.ToString();
        }

        public string GetValue(string asciiHexadecimal) 
            => ConvertHexToString(FilterNonHexCharAndUpperCase(asciiHexadecimal));

        private int GetHexVal(int val) 
            => val - (val <= '9' ? '0' : 'A'-10);
    }
}