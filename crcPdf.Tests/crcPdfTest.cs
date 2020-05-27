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
using System.IO;
using crcPdf;

namespace crcPdf.Tests {
    public class crcPdfTest { 
        internal void crcPdfShould(byte[] Given, Action<crcPdf> When = null, Action<crcPdf> Then = null)
        {            
            crcPdf pdf = new crcPdf(new MemoryStream(Given));

            if (When != null) {
                When(pdf);
            }

            if (Then != null) {
                Then(pdf);
            }
        }

        internal void crcPdf(Action<crcPdf> Given, Action<crcPdf> When = null, Action<crcPdf> Then = null)
        {            
            crcPdf pdfWriter = new crcPdf();
            Given(pdfWriter);

            MemoryStream ms = new MemoryStream();
            pdfWriter.WriteTo(ms);

            ms.Seek(0, SeekOrigin.Begin);
            crcPdf pdfRead = new crcPdf(ms);

            if (When != null) {
                When(pdfRead);
            }

            if (Then != null) {
                Then(pdfRead);
            }
        }
    }
}