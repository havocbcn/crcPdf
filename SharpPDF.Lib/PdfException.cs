using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace SharpPDF.Lib
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