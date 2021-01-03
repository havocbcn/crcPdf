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
using System.Collections.Generic;

namespace crcPdf {
    // 7.3 Objects
    public class Objectizer {
        private readonly Tokenizer tokenizer;

        public Objectizer(Tokenizer tokenizer) {
            this.tokenizer = tokenizer;
        }

        private readonly Dictionary<string, Func<Tokenizer, PdfObject>> tokenToObject = 
            new Dictionary<string, Func<Tokenizer, PdfObject>>
        {
            { "null", (t) => { return new NullObject(t); }},
            { "true", (t) => { return new BooleanObject(t); }},
            { "false", (t) => { return new BooleanObject(t); }},
            { "[", (t) => { return new ArrayObject(t); }},
            { "/", (t) => { return new NameObject(t); }},
            { "(", (t) => { return new StringObject(t); }},
            { "<", (t) => {            
                t.SavePosition();           
                t.TokenExcludedCommentsAndWhitespaces();    // se lee el < inicial
                string secondToken = t.TokenExcludedComments().ToString();
                if (secondToken == "<") {
                    t.RestorePosition();
                    return new DictionaryObject(t);
                }
                else {
                    t.RestorePosition();
                    return new StringObject(t);
                }
                }
            }
        };

        public PdfObject NextObject()
            => NextObject(false);

        public PdfObject NextObject(bool allowOperators)
        {
            tokenizer.SavePosition();            
            Token token = tokenizer.TokenExcludedCommentsAndWhitespaces();

            if (tokenToObject.ContainsKey(token.ToString())) {                
                tokenizer.RestorePosition();
                return tokenToObject[token.ToString()].Invoke(tokenizer);
            }

            if (TokenValidator.IsRegularNumber(token)) {
                if (tokenizer.IsEOF())
                {
                    tokenizer.RestorePosition();
                    return new IntegerObject(tokenizer);
                }

                Token secondToken = tokenizer.TokenExcludedCommentsAndWhitespaces();
                if (tokenizer.IsEOF() && allowOperators) {
                    tokenizer.RestorePosition();
                    return new IntegerObject(tokenizer);
                }
                Token thirdToken = tokenizer.TokenExcludedCommentsAndWhitespaces();

                if (TokenValidator.IsRegularNumber(secondToken) && thirdToken.ToString() == "obj")
                {
                    tokenizer.RestorePosition();
                    return new IndirectObject(tokenizer);
                }
                else if (TokenValidator.IsRegularNumber(secondToken) && thirdToken.ToString() == "R")
                {
                    tokenizer.RestorePosition();
                    return new IndirectReferenceObject(tokenizer);
                }

                tokenizer.RestorePosition();
                return new IntegerObject(tokenizer);
            }
            else if (TokenValidator.IsIntegerNumber(token)) {
                tokenizer.RestorePosition();
                return new IntegerObject(tokenizer);
            }
            else if (TokenValidator.IsRealNumber(token)) {
                tokenizer.RestorePosition();
                return new RealObject(tokenizer);
            }
            else if (allowOperators) {
                tokenizer.RestorePosition();
                return new OperatorObject(tokenizer);
            }

            throw new PdfException(PdfExceptionCodes.UNKNOWN_TOKEN, "Not known structure:" + token.ToString());                
        }

        internal bool IsEOF() => tokenizer.IsEOF();
    }
}