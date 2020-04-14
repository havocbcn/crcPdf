using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SharpPDF.Lib
{
    // 7.2.1 General
    // 7.2.2 Character Set
    public class Tokenizer {
        private const byte NUL = 0x00;
        private const byte HorizontalTab = 0x09;
        private const byte LineFeed = 0x0A;
        private const byte FormFeed = 0x0C;
        private const byte CarriageReturn = 0x0D;
        private const byte Space = 0x20;        
        private const byte CommentDelimiter = (byte)'%';
        private readonly List<int> WhiteSpaceCharacters = new List<int> {
            NUL, HorizontalTab, LineFeed, FormFeed, CarriageReturn, Space
        };
        private readonly List<int> DelimiterCharacters = new List<int> {
            '(', ')', '<', '>', '[', ']', '{','}', '/', CommentDelimiter
        };
        private readonly Stack<long> positionSaved = new Stack<long>();
        private readonly List<int> CommentSpecialCharacters = new List<int> {
            Space, HorizontalTab
        };
        private readonly Stream fragment;

        internal bool IsNextTokenExcludedCommentsAndWhitespaces(string text) {
            if (IsEOF()) {
                return false;
            }

            SavePosition();
            Token t = TokenExcludedCommentsAndWhitespaces();
            if (t.ToString() == text) {
                RestorePosition();
                return true;
            }
            RestorePosition();
            return false;
        }

        internal bool IsNextTokenExcludedCommentsRegular() {
            if (IsEOF()) {
                return false;
            }

            SavePosition();
            Token t = TokenExcludedComments();
            if (t.characterSetClass == CharacterSetType.Regular) {
                RestorePosition();
                return true;
            }
            RestorePosition();
            return false;
        }

        public Tokenizer(Stream fragment) => this.fragment = fragment;

        public int GetInteger() {
            string nextString = TokenExcludedComments().ToString();
            int value;
            if (!int.TryParse(nextString, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value)) {
                throw new PdfException(PdfExceptionCodes.INVALID_NUMBER_TOKEN, "Number cannot be cast to integer");
            }
            
            return value;
        }

        public void MoveToEnd() => fragment.Seek(0, SeekOrigin.End);

        public void MoveToPosition(long xrefPosition) => fragment.Seek(xrefPosition, SeekOrigin.Begin);

        public void MoveToPreviousLine() {
            fragment.Seek(-2, SeekOrigin.Current);

            while (fragment.ReadByte() != LineFeed) {
                fragment.Seek(-2, SeekOrigin.Current);
                if (fragment.Position == 0) {
                    throw new PdfException(PdfExceptionCodes.BOF, "Beginning Of File detected, but a LineFeed was expected");
                }
            }
        }

        internal bool IsEOF() => fragment.Position == fragment.Length;

        internal byte[] ReadStream(int streamLength) {
            byte[] buffer = new byte[streamLength];
            int readLength = fragment.Read(buffer, 0, streamLength);
            if (readLength != streamLength) {
                throw new PdfException(PdfExceptionCodes.INVALID_STREAM, "Trying to read " + streamLength + " bytes, but only " + readLength + "read");
            }
            return buffer;
        }

        public void SavePosition() => positionSaved.Push(fragment.Position); 

        public void RestorePosition() => fragment.Seek( positionSaved.Pop(), SeekOrigin.Begin);
        
        public Token TokenExcludedComments() => GetTokenInternal(true);

        public Token TokenExcludedCommentsAndWhitespaces() {
            var token = TokenExcludedComments();
            while (token.characterSetClass == CharacterSetType.WhiteSpace) {
                token = TokenExcludedComments();
            }

            return token;
        } 

        public Token Token() => GetTokenInternal(false);

        private Token GetTokenInternal(bool ignoreComments)
        {
            List<byte> bytesRead = new List<byte>();
            int byteRead;            
            CharacterSetType type = CharacterSetType.Unknown;

            while ((byteRead = fragment.ReadByte()) != -1) {
                if (ignoreComments && (byteRead == CommentDelimiter)) {
                    IgnoreComment();
                    continue;
                }
                
                CharacterSetType byteReadType = GetCharacterSetClass(byteRead);
                if (type == CharacterSetType.Unknown) {
                    type = byteReadType;
                    bytesRead.Add((byte)byteRead);

                    if (type == CharacterSetType.Delimiter) {
                        return new Token(bytesRead.ToArray(), type);    
                    }
                }
                else if (type != byteReadType) {
                    fragment.Seek(-1, SeekOrigin.Current);
                    return new Token(bytesRead.ToArray(), type);
                }
                else
                {
                     bytesRead.Add((byte)byteRead);    
                }
            }

            if (bytesRead.Count == 0) {
                throw new PdfException(PdfExceptionCodes.FILE_ABRUPTLY_TERMINATED, "EOF detected");
            }

            return new Token(bytesRead.ToArray());
        }

        private CharacterSetType GetCharacterSetClass(int byteRead) {
            if (WhiteSpaceCharacters.Contains(byteRead)) {
                return CharacterSetType.WhiteSpace;
            }
            if (DelimiterCharacters.Contains(byteRead)) {
                return CharacterSetType.Delimiter;
            }
            return CharacterSetType.Regular;
        }

        private void IgnoreComment()
        {
            int byteRead;
            while ((byteRead = fragment.ReadByte()) != -1)
            {
                if (WhiteSpaceCharacters.Contains(byteRead) &&
                    !CommentSpecialCharacters.Contains(byteRead)) {
                    return;
                }
            }
        }
    }
}
