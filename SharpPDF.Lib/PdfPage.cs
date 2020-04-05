using System.Collections.Generic;
using System.Linq;

namespace SharpPDF.Lib
{
    public class PdfPage
    {
        List<PdfElement> elements = new List<PdfElement>();

        public PdfLabel[] Label => elements.OfType<PdfLabel>().ToArray();
    }
}