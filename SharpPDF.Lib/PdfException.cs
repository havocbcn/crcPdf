using System;
using System.Runtime.Serialization;

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