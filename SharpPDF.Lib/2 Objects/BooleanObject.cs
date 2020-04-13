using System.Collections.Generic;

namespace SharpPDF.Lib {
    public class BooleanObject : PdfObject {
        private readonly bool value;

        public BooleanObject(Tokenizer tokenizer)
        {
            if (tokenizer.TokenExcludedCommentsAndWhitespaces().ToString() == "true")
                value = true;
            else 
                value = false;
        }
        
        public bool Value => value;
        public PdfObject[] Childs() => new PdfObject[0];
    }
}