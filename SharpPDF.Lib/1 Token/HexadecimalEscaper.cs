// This file is part of SharpPdf.
// 
// SharpPdf is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// SharpPdf is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with SharpPdf.  If not, see <http://www.gnu.org/licenses/>.
using System.Text;

namespace SharpPDF.Lib {
    public class HexadecimalEscaper {
        private const int FromaToAInAscii = 32;   

        public static string ConvertHexToString(string rawHexString) {
            StringBuilder hexString  = new StringBuilder();
            
            for (int i = 0; i < rawHexString.Length; i += 2) {
                if (i + 1 < rawHexString.Length) {
                    hexString.Append((char)((GetHexVal(rawHexString[i]) << 4) + (GetHexVal(rawHexString[i+1]))));
                } else {
                    hexString.Append((char)(GetHexVal(rawHexString[i]) << 4));
                }
            }
            
            return hexString.ToString();
        }

        public static string FilterNonHexCharAndUpperCase(string rawHex) {
            StringBuilder sb = new StringBuilder();

            foreach (char c in rawHex) {
                if (c >= '0' && c <='9') {
                    sb.Append(c);
                } else if (c >= 'A' && c <= 'Z') {
                    sb.Append(c);
                } else if (c >= 'a' && c <= 'z') {
                    sb.Append((char)(c - FromaToAInAscii));                
                }
            }

            return sb.ToString();
        }

        public string GetValue(string asciiHexadecimal) 
            => ConvertHexToString(FilterNonHexCharAndUpperCase(asciiHexadecimal));

        private static int GetHexVal(int val) 
            => val - (val <= '9' ? '0' : 'A'-10);
    }
}