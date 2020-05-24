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
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPDF.Lib {
    public class StringObject : PdfObject {
        public StringObject(Tokenizer tokenizer) {            
            Token delimiter = tokenizer.TokenExcludedCommentsAndWhitespaces();
            if (delimiter.characterSetClass != CharacterSetType.Delimiter)
                throw new PdfException(PdfExceptionCodes.EXPECTED_DELIMITER, "String without a delimiter");

            string initialDelimiter = delimiter.ToString();
            if (initialDelimiter == "(") {
                GetLiteralString(tokenizer);
            } else if (initialDelimiter == "<") {
                GetHexadecimalString(tokenizer);
            } else {
                throw new PdfException(PdfExceptionCodes.EXPECTED_DELIMITER, "String must use <,> or (,) as delimiter");          
            }
        }

        public StringObject(string text) {
            this.value = text;
        }

        private string value;
        
        private readonly Dictionary<char, char> CharacterToEscapeCharacters = new Dictionary<char, char> {
            {'n','\n'}, 
            {'r', '\r'},
            {'t', '\t'},
            {'b', '\b'},
            {'f', '\f'},
            {'(', '('},
            {')', ')'},
            {'\\', '\\'}
        };

        public string Value => value;
        private void GetLiteralString(Tokenizer tokenizer) {
            int depth = 1;
            StringBuilder token = new StringBuilder();

            Token nextToken = tokenizer.Token();
            bool lastTokenHasReverseSolidus = false;

            while (depth > 0) {
                string nextTokenText = nextToken.ToString();

                if (!lastTokenHasReverseSolidus) {
                    if (nextTokenText == "(") {
                        depth++;
                    } else if (nextTokenText == ")") {
                        depth--;
                    }
                }

                if (depth > 0) {
                    token.Append(nextTokenText);
                    if (nextTokenText.EndsWith(@"\")) {
                        lastTokenHasReverseSolidus = true;
                    } else {
                        lastTokenHasReverseSolidus = false;
                    }

                    nextToken = tokenizer.Token();
                }                
            }
            value = EscapeALiteralString(token.ToString());
        }

        
        private void GetHexadecimalString(Tokenizer tokenizer) {
            Token nextToken = tokenizer.TokenExcludedComments();

            StringBuilder token = new StringBuilder();

            while (nextToken.ToString() != ">") {                
                token.Append(HexadecimalEscaper.FilterNonHexCharAndUpperCase(nextToken.ToString()));
                
                nextToken = tokenizer.TokenExcludedComments();
            }
            value = HexadecimalEscaper.ConvertHexToString(token.ToString());
        }

        private string EscapeALiteralString(string literalString) {
            StringBuilder literalStringEscaped = new StringBuilder();
            int i = 0;
            while (i < literalString.Length) {
                if (literalString[i] == '\\' && i < literalString.Length - 1) {
                    if (CharacterToEscapeCharacters.ContainsKey(literalString[i + 1])) {
                        literalStringEscaped.Append(CharacterToEscapeCharacters[literalString[i + 1]]);
                        i += 2;
                    }
                    else if (IsNewLine(literalString, i + 1)) {
                        i = ReverseSolidAsLineContinuator(literalString, i);
                    }
                    else if (IsNumber(literalString, i + 1)) {
                        i = OctalEscape(literalString, literalStringEscaped, i);
                    }
                    else {
                        literalStringEscaped.Append(literalString[i]);
                        i++;
                    }
                }
                else {
                    literalStringEscaped.Append(literalString[i]);
                    i++;
                }
            }
            return literalStringEscaped.ToString();
        }

        private int ReverseSolidAsLineContinuator(string literalString, int i) {
            i += 2;     // /the / and the /n or /r
            if (IsNewLine(literalString, i)) {
                i++;    // additional /n or /r
            }
            return i;
        }

        private int OctalEscape(string literalString, StringBuilder literalStringEscaped, int i)
        {
            i++;
            string octalNumber = "";
            octalNumber += literalString[i];

            if (IsNumber(literalString, i + 1)) {
                i++;
                octalNumber += literalString[i];
            }

            if (IsNumber(literalString, i + 1)) {
                i++;
                octalNumber += literalString[i];
            }

            literalStringEscaped.Append((char)Convert.ToInt32(octalNumber, 8));
            i++;
            return i;
        }
        
        private static bool IsNewLine(string literalString, int i)
            => i < literalString.Length && (literalString[i] == '\n' || literalString[i] == '\r');        

        private static bool IsNumber(string literalString, int i)
            => i < literalString.Length && (literalString[i] >= '0' && literalString[i] <= '9');

        public override string ToString() {
            return $"({Value})";
        }
        
        public override byte[] Save(Compression compression) {            
            return GetBytes(this.ToString());
        }
    }
}