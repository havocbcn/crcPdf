using System;
using System.Collections.Generic;

namespace SharpPDF.Lib {
    // 7.3 Objects
    public class Objectizer {
        private readonly Tokenizer tokenizer;

        public Objectizer(Tokenizer tokenizer) {
            this.tokenizer = tokenizer;
        }

        private readonly Dictionary<string, Func<Tokenizer, PdfObject>> tokenToObject = 
            new Dictionary<string, Func<Tokenizer, PdfObject>>()
        {
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

        public PdfObject NextObject(bool allowOperators = false)
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