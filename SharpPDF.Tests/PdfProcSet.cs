using FluentAssertions;
using System;
using System.IO;
using SharpPDF.Lib;
using Xunit;

namespace SharpPDF.Tests.Outline {   
    //14.2 Procedure Sets
    public class PdfProcSet {    
        [Fact]
        public void ReadEmptyProcset() =>            
            // 12.3.3 Document outline
            SharpPdfShould(
                Given: File.ReadAllBytes("samples/sample.pdf"),
                Then: pdf => { 
                    pdf.Catalog.Pages.PageSons[0].Procsets.Should().HaveCount(2);
                    pdf.Catalog.Pages.PageSons[0].Procsets[0].Should().Be("PDF");
                    pdf.Catalog.Pages.PageSons[0].Procsets[1].Should().Be("Text");
                }
            );

        private void SharpPdfShould(byte[] Given, Action<SharpPdf> When = null, Action<SharpPdf> Then = null)
        {            
            SharpPdf pdf = new SharpPdf(new MemoryStream(Given));

            if (When != null)
                When(pdf);

            if (Then != null)
                Then(pdf);
        }
    }
}
