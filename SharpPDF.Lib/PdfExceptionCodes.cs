using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SharpPDF.Lib
{
    public enum PdfExceptionCodes
    {
        FILE_ABRUPTLY_TERMINATED,
        INVALID_BOOLEAN_TOKEN,
        EXPECTED_DELIMITER,
        OBJECT_UNKNOWN,
        INVALID_NAMEOBJECT_TOKEN,
        UNKNOWN_TOKEN,
        INVALID_NUMBER_TOKEN,
        INVALID_INDIRECTOBJECT_TOKEN,
        INVALID_GENERATION,
    }  
}