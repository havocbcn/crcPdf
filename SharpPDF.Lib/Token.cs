using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

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

        public bool IsNumber 
        { 
            get {
                foreach(byte ch in _token)
                {
                    if (ch != '-' && ch != '+' && ch < '0' && ch >'9')
                        return false;
                }
                return true;
            }
        }

         public bool IsRegularNumber 
        { 
            get {
                foreach(byte ch in _token)
                {
                    if (ch < '0' && ch >'9')
                        return false;
                }
                return true;
            }
        }

        public readonly CharacterSetType characterSetClass;

        public override string ToString()
        {
            return System.Text.ASCIIEncoding.ASCII.GetString(_token);
        }
    }
}