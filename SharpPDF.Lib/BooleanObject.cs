using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SharpPDF.Lib
{
    public class BooleanObject : IPdfObject
    {
        public BooleanObject(Tokenizer tokenizer) 
        {
            this.tokenizer = tokenizer;
        }

        private bool value;
        private readonly Tokenizer tokenizer;

        public bool Value => value;

        public ObjectType Type()
        {
            return ObjectType.Boolean;
        }

        public void Analyze()
        {
            string tokenContent = tokenizer.GetToken().ToString();
            if (tokenContent == "true")
                value = true;
            else if (tokenContent == "false")
                value = false;
            else
                throw new PdfException(PdfExceptionCodes.INVALID_BOOLEAN_TOKEN, "Only true or false for boolean");
        }

        public IEnumerable<IPdfObject> Childs()
        {
            return new List<IPdfObject>();
        }
    }
}