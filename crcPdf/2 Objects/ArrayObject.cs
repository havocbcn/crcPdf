// This file is part of crcPdf.
// 
// crcPdf is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// crcPdf is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with crcPdf.  If not, see <http://www.gnu.org/licenses/>.
using System.Collections.Generic;

namespace crcPdf {
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