// This file is part of SharpPdf.
// 
// SharpPdf is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// SharpPdf is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with SharpPdf.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.IO;
using SharpPDF.Lib;

namespace SharpPDF.Tests {
    public class SharpPdfTest { 
        internal void SharpPdfShould(byte[] Given, Action<SharpPdf> When = null, Action<SharpPdf> Then = null)
        {            
            SharpPdf pdf = new SharpPdf(new MemoryStream(Given));

            if (When != null) {
                When(pdf);
            }

            if (Then != null) {
                Then(pdf);
            }
        }

        internal void SharpPdf(Action<SharpPdf> Given, Action<SharpPdf> When = null, Action<SharpPdf> Then = null)
        {            
            SharpPdf pdfWriter = new SharpPdf();
            Given(pdfWriter);

            MemoryStream ms = new MemoryStream();
            pdfWriter.WriteTo(ms);

            ms.Seek(0, SeekOrigin.Begin);
            Console.WriteLine(System.Text.UTF8Encoding.UTF8.GetString(ms.ToArray()));

            ms.Seek(0, SeekOrigin.Begin);
            SharpPdf pdfRead = new SharpPdf(ms);

            if (When != null) {
                When(pdfRead);
            }

            if (Then != null) {
                Then(pdfRead);
            }
        }
    }
}