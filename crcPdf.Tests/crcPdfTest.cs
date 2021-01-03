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
using System.Diagnostics;
using System.IO;

namespace crcPdf.Tests {
    public class crcPdfTest { 
        internal void crcPdfShould(byte[] Given, Action<DocumentCatalog> When = null, Action<DocumentCatalog> Then = null)
        {            
            DocumentCatalog pdf = Pdf.Load(new MemoryStream(Given));

            if (When != null) {
                When(pdf);
            }

            if (Then != null) {
                Then(pdf);
            }
        }

        internal void crcPdf(Action<DocumentCatalog> Given, Action<DocumentCatalog> When = null, Action<DocumentCatalog> Then = null)
        {            
            DocumentCatalog pdfWriter = Pdf.CreateExpert();
            Given(pdfWriter);

            MemoryStream ms = new MemoryStream();
            pdfWriter.Save(ms);

            LogInDebug(ms);

            ms.Seek(0, SeekOrigin.Begin);
            DocumentCatalog pdfRead = Pdf.Load(ms);

            if (When != null) {
                When(pdfRead);
            }

            if (Then != null) {
                Then(pdfRead);
            }
        }

        [Conditional("DEBUG")]
        private void LogInDebug(MemoryStream ms)
        {            
            ms.Seek(0, SeekOrigin.Begin);
            //   Console.Write(System.Text.UTF8Encoding.UTF8.GetString(ms.ToArray()));
        }
    }
}