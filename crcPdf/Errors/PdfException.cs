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
using System;
using System.Runtime.Serialization;

namespace crcPdf
{
    [Serializable]
    public class PdfException : Exception
    {
        // This protected constructor is used for deserialization.
        protected PdfException( SerializationInfo info, 
            StreamingContext context ) :
                base( info, context )
        { }

        public PdfException(PdfExceptionCodes code, string description)
            : base(code.ToString() + ": " + description)
        {

        }    
    }
}