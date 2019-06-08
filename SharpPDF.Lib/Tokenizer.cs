using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SharpPDF.Lib
{
    // 7.2.1 General
    // 7.2.2 Character Set
    public class Tokenizer
    {        
        private const byte NUL = 0x00;
        private const byte HorizontalTab = 0x09;
        private const byte LineFeed = 0x0A;
        private const byte FormFeed = 0x0C;
        private const byte CarriageReturn = 0x0D;
        private const byte Space = 0x20;        
        private const byte CommentDelimiter = (byte)'%';
        private readonly List<int> WhiteSpaceCharacters = new List<int>
        {
            NUL, HorizontalTab, LineFeed, FormFeed, CarriageReturn, Space
        };

        private readonly List<int> DelimiterCharacters = new List<int>
        {
            '(', ')', '<', '>', '[', ']', '{','}', '/', CommentDelimiter
        };

        private readonly List<int> CommentSpecialCharacters = new List<int>
        {
            Space, HorizontalTab
        };

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

        private readonly   Stream fragment;

        public Tokenizer(Stream fragment)
        {
            this.fragment = fragment;
        }

        public float? GetRealNumber()
        {
            string nextString = GetToken().ToString();
            float result;
            if (float.TryParse(nextString, 
                                NumberStyles.AllowDecimalPoint |
                                NumberStyles.AllowLeadingSign |
                                NumberStyles.AllowTrailingSign |
                                NumberStyles.AllowThousands, 
                                CultureInfo.InvariantCulture, 
                                out result))
                return result;
            return null;
        }


        public string GetString()
        {
            Token delimiter = GetToken();
            if (delimiter.characterSetClass != CharacterSetType.Delimiter)
                throw new PdfException(PdfExceptionCodes.EXPECTED_DELIMITER, "String without a delimiter");

            string initialDelimiter = delimiter.ToString();
            if (initialDelimiter == "(")
            {
                return GetLiteralString();
            }
            else if (initialDelimiter == "<")
            {
                return GetHexadecimalString();
            }

            throw new PdfException(PdfExceptionCodes.EXPECTED_DELIMITER, "String must use <,> or (,) as delimiter");
        }

        private string GetHexadecimalString()
        {

            Token nextToken = GetToken();

            if (nextToken.characterSetClass == CharacterSetType.EndOfFile)
                throw new PdfException(PdfExceptionCodes.FILE_ABRUPTLY_TERMINATED, "String ends abruptly, EOF detected");

            StringBuilder token = new StringBuilder();
            HexadecimalEscaper hexEscaper = new HexadecimalEscaper();

            while (nextToken.ToString() != ">")
            {                
                token.Append(hexEscaper.FilterNonHexCharAndUpperCase(nextToken.ToString()));
                
                nextToken = GetToken();

                if (nextToken.characterSetClass == CharacterSetType.EndOfFile)
                    throw new PdfException(PdfExceptionCodes.FILE_ABRUPTLY_TERMINATED, "String ends abruptly, EOF detected");

            }
            return hexEscaper.ConvertHexToString(token.ToString());
        }        

        private string GetLiteralString()
        {
            int depth = 1;
            StringBuilder token = new StringBuilder();

            Token nextToken = GetToken(false);
            bool lastTokenHasReverseSolidus = false;

            while (depth > 0)
            {
                if (nextToken.characterSetClass == CharacterSetType.EndOfFile)
                    throw new PdfException(PdfExceptionCodes.FILE_ABRUPTLY_TERMINATED, "String ends abruptly, EOF detected");

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
                }

                nextToken = GetToken(false);
            }
            return EscapeALiteralString(token.ToString());
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

        public void GoBack()
        {            
            redoList.Push(tokenList.Pop());            
        }

        private readonly Stack<Token> tokenList = new Stack<Token>();
        private readonly Stack<Token> redoList = new Stack<Token>();

        public Token GetToken(bool ignoreComments = true)
        {
            if (redoList.Count > 0)                
            {                
                Token tokenPreviouslySaved = redoList.Pop();
                tokenList.Push(tokenPreviouslySaved);
                return tokenPreviouslySaved;
            }

            Token token = GetTokenInternal(ignoreComments);

            tokenList.Push(token);

            return token;
        } 

        private Token GetTokenInternal(bool ignoreComments)
        {
            List<byte> bytesRead = new List<byte>();
            int byteRead;            
            CharacterSetType type = CharacterSetType.Unknown;

            while ((byteRead = fragment.ReadByte()) != -1)
            {
                if (ignoreComments && (byteRead == CommentDelimiter))
                {
                    IgnoreComment();
                    continue;
                }
                
                CharacterSetType byteReadType = GetCharacterSetClass(byteRead);
                if (type == CharacterSetType.Unknown)
                {
                    type = byteReadType;
                    bytesRead.Add((byte)byteRead);

                    if (type == CharacterSetType.Delimiter)
                    {
                        return new Token(bytesRead.ToArray(), type);    
                    }
                }
                else if (type != byteReadType)
                {
                    fragment.Seek(-1, SeekOrigin.Current);
                    return new Token(bytesRead.ToArray(), type);
                }
                else
                {
                     bytesRead.Add((byte)byteRead);    
                }
            }

            if (bytesRead.Count == 0)
                return new Token(null, CharacterSetType.EndOfFile);

            return new Token(bytesRead.ToArray());
        }

        private CharacterSetType GetCharacterSetClass(int byteRead)
        {
            if (WhiteSpaceCharacters.Contains(byteRead))
                return CharacterSetType.WhiteSpace;
            if (DelimiterCharacters.Contains(byteRead))
                return CharacterSetType.Delimiter;
            return CharacterSetType.Regular;
        }

        private void IgnoreComment()
        {
            int byteRead;
            while ((byteRead = fragment.ReadByte()) != -1)
            {
                if (WhiteSpaceCharacters.Contains(byteRead) &&
                    !CommentSpecialCharacters.Contains(byteRead))
                    return;
            }
        }
    }
}
