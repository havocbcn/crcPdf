// This file is part of crcPdf.
// 
// crcPdf is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// crcPdf is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with crcPdf.  If not, see <http://www.gnu.org/licenses/>.
using System;

namespace crcPdf {
    public class Token {        
        public Token(byte[] token) {
            this.token = token;
            this.characterSetClass = CharacterSetType.Regular;
        }

        public Token(byte[] token, CharacterSetType characterSetClass) {
            this.token = token;
            this.characterSetClass = characterSetClass;
        }

        public byte[] token { get; }

        public CharacterSetType characterSetClass { get; }

        public override string ToString() 
            => System.Text.ASCIIEncoding.ASCII.GetString(token);

        public long ToLong() 
            => Convert.ToInt64(ToString());
    }
}