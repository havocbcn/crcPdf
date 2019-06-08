using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SharpPDF.Lib
{
    public class IntegerObject : IPdfObject
    {
        public IntegerObject(Tokenizer tokenizer) 
        {
            this.tokenizer = tokenizer;
        }

        private int value;
        private readonly Tokenizer tokenizer;

        public int Value => value;

        public ObjectType Type()
        {
            return ObjectType.Integer;
        }

        public void Analyze()
        {
            string tokenContent = tokenizer.GetToken().ToString();

            if (!int.TryParse(tokenContent, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value))                
                throw new PdfException(PdfExceptionCodes.INVALID_NUMBER_TOKEN, "Number cannot be cast to integer");
        }

        public IEnumerable<IPdfObject> Childs()
        {
            return new List<IPdfObject>();
        }
    }
}