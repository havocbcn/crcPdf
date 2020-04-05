using System.Collections.Generic;

namespace SharpPDF.Lib {
    public class BooleanObject : IPdfObject {
        private readonly bool value;

        public BooleanObject(Tokenizer tokenizer)
        {
            if (tokenizer.TokenExcludedCommentsAndWhitespaces().ToString() == "true")
                value = true;
            else 
                value = false;
        }
        
        public bool Value => value;
        public ObjectType ObjectType => Lib.ObjectType.Boolean;
        public IPdfObject[] Childs() => new IPdfObject[0];
    }
}