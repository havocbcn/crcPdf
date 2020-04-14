using FluentAssertions;
using System;
using System.IO;
using SharpPDF.Lib;
using Xunit;
using System.Text;

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
            SharpPdf pdfRead = new SharpPdf(ms);

            if (When != null) {
                When(pdfRead);
            }

            if (Then != null) {
                Then(pdfRead);
            }
        }

        internal void SharpPdfShouldGiveAnError(string Given, Action<SharpPdf> When)
        {            
            string pdfFile = Given;

            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(pdfFile);
            SharpPdf pdf = new SharpPdf(new MemoryStream(bytes));

            Assert.Throws<PdfException>(() => { When(pdf); });
        }
    }
}