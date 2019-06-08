using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SharpPDF.Lib
{
    public class SharpPdf
    {
        Tokenizer tokenizer;

        public SharpPdf(MemoryStream ms)
        {
            Tokenizer tokenizer = new Tokenizer(ms);
        }

        public IEnumerable<IPdfObject> GetChilds()
        {
            throw new NotImplementedException();
        }
    }
}