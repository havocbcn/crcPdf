using System.Text;

namespace SharpPDF.Lib {
    // in graphic or text objects there is special commands call operators
    public class OperatorObject : IPdfObject {
        public OperatorObject(Tokenizer tokenizer) {            
            StringBuilder sb = new StringBuilder();
            Token nextToken = tokenizer.TokenExcludedCommentsAndWhitespaces();

            Value = nextToken.ToString();
        }

        public string Value { get; private set; }
        public ObjectType ObjectType => Lib.ObjectType.Operator;
        public IPdfObject[] Childs() => new IPdfObject[0];

        public override string ToString() {
            return Value;
        }

    }
}