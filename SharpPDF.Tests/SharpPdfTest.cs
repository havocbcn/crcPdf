using FluentAssertions;
using System;
using System.IO;
using SharpPDF.Lib;
using Xunit;

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