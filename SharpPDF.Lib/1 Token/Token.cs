using System;

namespace SharpPDF.Lib
{
    public class Token
    {        
        public Token(byte[] token, CharacterSetType characterSetClass = CharacterSetType.Regular)
        {
            this._token = token;
            this.characterSetClass = characterSetClass;
        }
        private readonly byte[] _token;

        public byte[] token => _token;        

        public readonly CharacterSetType characterSetClass;

        public override string ToString() => System.Text.ASCIIEncoding.ASCII.GetString(_token);

        public long ToLong() => Convert.ToInt64(ToString());
    }
}