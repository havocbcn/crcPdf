using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPDF.Lib {
    public class StringObject : IPdfObject {
        public StringObject(Tokenizer tokenizer) {            
            Token delimiter = tokenizer.TokenExcludedCommentsAndWhitespaces();
            if (delimiter.characterSetClass != CharacterSetType.Delimiter)
                throw new PdfException(PdfExceptionCodes.EXPECTED_DELIMITER, "String without a delimiter");

            string initialDelimiter = delimiter.ToString();
            if (initialDelimiter == "(")
                GetLiteralString(tokenizer);
            else if (initialDelimiter == "<")
                GetHexadecimalString(tokenizer);
            else
                throw new PdfException(PdfExceptionCodes.EXPECTED_DELIMITER, "String must use <,> or (,) as delimiter");          
        }

        private string value;
        
        private readonly Dictionary<char, char> CharacterToEscapeCharacters = new Dictionary<char, char>
        {
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
        public ObjectType ObjectType => Lib.ObjectType.String;

        private void GetLiteralString(Tokenizer tokenizer)
        {
            int depth = 1;
            StringBuilder token = new StringBuilder();

            Token nextToken = tokenizer.Token();
            bool lastTokenHasReverseSolidus = false;

            while (depth > 0)
            {
                string nextTokenText = nextToken.ToString();

                if (!lastTokenHasReverseSolidus)
                {
                    if (nextTokenText == "(")
                        depth++;
                    else if (nextTokenText == ")")
                        depth--;
                }

                if (depth > 0)
                {
                    token.Append(nextTokenText);
                    if (nextTokenText.EndsWith(@"\"))
                        lastTokenHasReverseSolidus = true;
                    else
                        lastTokenHasReverseSolidus = false;

                    nextToken = tokenizer.Token();
                }                
            }
            value = EscapeALiteralString(token.ToString());
        }

        
        private void GetHexadecimalString(Tokenizer tokenizer)
        {

            Token nextToken = tokenizer.TokenExcludedComments();

            StringBuilder token = new StringBuilder();
            HexadecimalEscaper hexEscaper = new HexadecimalEscaper();

            while (nextToken.ToString() != ">")
            {                
                token.Append(hexEscaper.FilterNonHexCharAndUpperCase(nextToken.ToString()));
                
                nextToken = tokenizer.TokenExcludedComments();
            }
            value = hexEscaper.ConvertHexToString(token.ToString());
        }

        private string EscapeALiteralString(string literalString)
        {
            StringBuilder literalStringEscaped = new StringBuilder();
            int i = 0;
            while (i < literalString.Length)
            {
                if (literalString[i] == '\\' && i < literalString.Length - 1)
                {
                    if (CharacterToEscapeCharacters.ContainsKey(literalString[i + 1]))
                    {
                        literalStringEscaped.Append(CharacterToEscapeCharacters[literalString[i + 1]]);
                        i += 2;
                    }
                    else if (IsNewLine(literalString, i + 1))
                    {
                        i = ReverseSolidAsLineContinuator(literalString, i);
                    }
                    else if (IsNumber(literalString, i + 1))
                    {
                        i = OctalEscape(literalString, literalStringEscaped, i);
                    }
                    else
                    {
                        literalStringEscaped.Append(literalString[i]);
                        i++;
                    }
                }
                else
                {
                    literalStringEscaped.Append(literalString[i]);
                    i++;
                }
            }
            return literalStringEscaped.ToString();
        }

        private int ReverseSolidAsLineContinuator(string literalString, int i)
        {
            i += 2;     // /the / and the /n or /r
            if (IsNewLine(literalString, i))
                i++;    // additional /n or /r
            return i;
        }

        private int OctalEscape(string literalString, StringBuilder literalStringEscaped, int i)
        {
            i++;
            string octalNumber = "";
            octalNumber += literalString[i];

            if (IsNumber(literalString, i + 1))
            {
                i++;
                octalNumber += literalString[i];
            }

            if (IsNumber(literalString, i + 1))
            {
                i++;
                octalNumber += literalString[i];
            }

            literalStringEscaped.Append((char)Convert.ToInt32(octalNumber, 8));
            i++;
            return i;
        }

        
        private bool IsNewLine(string literalString, int i)
        {
            if (literalString.Length-1 < i)
                return false;

            return literalString[i] == '\n' || literalString[i] == '\r';
        }

        private bool IsNumber(string literalString, int i)
        {
            if (literalString.Length-1 < i)
                return false;

            return literalString[i] >= '0' && literalString[i] <= '9';
        }

        public IPdfObject[] Childs() => new IPdfObject[0];
    }
}