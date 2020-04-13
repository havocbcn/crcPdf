using FluentAssertions;
using System;
using System.IO;
using SharpPDF.Lib;
using Xunit;

namespace SharpPDF.Tests.Outline {   
    public class PdfOutlineShould {    
        [Fact]
        public void ReadEmptyOutlines() =>            
            // 12.3.3 Document outline
            SharpPdfShould(
                Given: File.ReadAllBytes("samples/sample.pdf"),
                Then: pdf => { 
                    pdf.Catalog.Outlines.Count.Should().Be(0);
                }
            );

        private void SharpPdfShould(byte[] Given, 
                                    Action<SharpPdf> When = null, 
                                    Action<SharpPdf> Then = null)
        {            
            SharpPdf pdf = new SharpPdf(new MemoryStream(Given));

            if (When != null)
                When(pdf);

            if (Then != null)
                Then(pdf);
        }
    }
}
