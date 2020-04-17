using System.Collections.Generic;

namespace SharpPDF.Lib {
    public class ArrayObject : PdfObject {
        public ArrayObject(List<PdfObject> childs) {
            this.childs = childs;
        }

        public ArrayObject(Tokenizer tokenizer) {
            if (!TokenValidator.IsDelimiter(tokenizer.TokenExcludedCommentsAndWhitespaces(), "[")) {
                throw new PdfException(PdfExceptionCodes.INVALID_ARRAY, "Expected [");
            }

            var read = new Objectizer(tokenizer);
            while (!tokenizer.IsNextTokenExcludedCommentsAndWhitespaces("]")) {
                childs.Add(read.NextObject());        
            }

            if (!TokenValidator.IsDelimiter(tokenizer.TokenExcludedCommentsAndWhitespaces(), "]")) {
                throw new PdfException(PdfExceptionCodes.INVALID_ARRAY, "Expected ]");
            }
        }    

        public override string ToString() {
            return $"[{string.Join(" ", childs)}]";
        }

        public override byte[] Save(Compression compression) {            
            List<byte> b = new List<byte>();
            b.Add((byte)'[');
            foreach (var child in childs) {
                b.AddRange(child.Save(compression));
            }
            b.Add((byte)']');

            return b.ToArray();
        }
    }
}