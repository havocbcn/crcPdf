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

using System.IO;

namespace crcPdf {
    /// <summary>
    /// Class to choose: read or create a new Pdf (simple or expert mode)
    /// </summary>
    public class Pdf {
        /// <summary>
        /// Load a pdf and return its catalog
        /// </summary>
        /// <param name="stream">Used to read a byte stream representing a Pdf</param>
        /// <returns>The pdf catalog that represent the Pdf</returns>
        public static DocumentCatalog Load(Stream stream) 
            => new PdfReader().Analyze(new Tokenizer(stream));

        /// <summary>
        /// Creates a new Pdf using the expert mode. You will have to create each bloc manually
        /// </summary>
        /// <returns></returns>        
        public static DocumentCatalog CreateExpert() 
            => new DocumentCatalog();

        /// <summary>
        /// Create a simple Pdf using a normal mode. You will have assitance that creates all the 
        /// blocs automatically
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static SimplePdf CreateSimple(int width, int height)
            => new SimplePdf(width, height);
    }
}