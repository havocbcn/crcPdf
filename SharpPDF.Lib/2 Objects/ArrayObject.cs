using System.Collections.Generic;

namespace SharpPDF.Lib {
    public class ArrayObject : IPdfObject {        
        private readonly List<IPdfObject> childs = new  List<IPdfObject>();

        public ArrayObject(List<IPdfObject> childs) {
            this.childs = childs;
        }

        public ArrayObject(Tokenizer tokenizer) {
            var validator = new TokenValidator();

            if (!validator.IsDelimiter(tokenizer.TokenExcludedCommentsAndWhitespaces(), "["))
                throw new PdfException(PdfExceptionCodes.INVALID_ARRAY, "Expected [");

            var read = new Objectizer(tokenizer);
            while (!tokenizer.IsNextTokenExcludedCommentsAndWhitespaces("]"))
                childs.Add(read.NextObject());        

            if (!validator.IsDelimiter(tokenizer.TokenExcludedCommentsAndWhitespaces(), "]"))
                throw new PdfException(PdfExceptionCodes.INVALID_ARRAY, "Expected ]");
        }

        public ObjectType ObjectType => Lib.ObjectType.Array;

        public IPdfObject[] Childs() => childs.ToArray();

        public override string ToString() {
            return $"[{string.Join(" ", childs)}]";
        }
    }
}