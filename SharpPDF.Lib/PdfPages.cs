using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SharpPDF.Lib
{
    public class PdfPages : IEnumerable
    {
        List<PdfPage> pages = new List<PdfPage>();

        public PdfPage this[int index] => pages[index];

        public IEnumerator GetEnumerator()
        {
            foreach (var page in pages)
                yield return page;
        }
    }
}