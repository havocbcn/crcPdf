using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SharpPDF.Lib
{
    public class NameObject : IPdfObject
    {
        public NameObject(Tokenizer tokenizer)
        {
            this.tokenizer = tokenizer;
        }

       
        private string value;
        private readonly Tokenizer tokenizer;

        public string Value()
        {
            return value;
        }

        public ObjectType Type()
        {
            return ObjectType.Name;
        }

        public void Analyze()
        {
            Token nextToken = tokenizer.GetToken();
            
            if (nextToken.ToString() != "/")
                throw new PdfException(PdfExceptionCodes.INVALID_NAMEOBJECT_TOKEN, "Name object must start with a /");

            nextToken = tokenizer.GetToken();

            StringBuilder sb = new StringBuilder();
            while (nextToken.characterSetClass == CharacterSetType.Regular ||
                nextToken.characterSetClass == CharacterSetType.Delimiter)
            {   
                sb.Append(nextToken.ToString());
                nextToken = tokenizer.GetToken();
            } 

            value = Escape(sb.ToString());
        }

         private string Escape(string literalString)
        {
            StringBuilder literalStringEscaped = new StringBuilder();
            int i = 0;
            while (i < literalString.Length)
            {
                if (literalString[i] == '#' && i < literalString.Length - 2)
                {
                    HexadecimalEscaper hexEscaper = new HexadecimalEscaper();

                    literalStringEscaped.Append(hexEscaper.GetValue(literalString.Substring(i+1, 2)));
                    i+=3;
                }
                else
                {
                    literalStringEscaped.Append(literalString[i]);
                    i++;
                }
            }
            return literalStringEscaped.ToString();
        }

        public IEnumerable<IPdfObject> Childs()
        {
            throw new NotImplementedException();
        }
    }
}