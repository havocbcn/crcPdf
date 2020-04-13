using System;

namespace SharpPDF.Lib
{
    public class Token
    {        
        public Token(byte[] token)
        {
            this._token = token;
            this.characterSetClass = CharacterSetType.Regular;
        }

        public Token(byte[] token, CharacterSetType characterSetClass)
        {
            this._token = token;
            this.characterSetClass = characterSetClass;
        }

        private readonly byte[] _token;

        public byte[] token => _token;        

        public CharacterSetType characterSetClass { get; }

        public override string ToString() => System.Text.ASCIIEncoding.ASCII.GetString(_token);

        public long ToLong() => Convert.ToInt64(ToString());
    }
}