using FluentAssertions;
using System.IO;
using Xunit;

namespace crcPdf.Tests.Outline {   
    //14.2 Procedure Sets
    public class PdfProcSet : crcPdfTest {    
        [Fact]
        public void ReadEmptyProcset() =>            
            // 12.3.3 Document outline
            crcPdfShould(
                Given: File.ReadAllBytes("samples/sample.pdf"),
                Then: pdf => { 
                    pdf.Pages.PageSons[0].Procsets.Should().HaveCount(2);
                    pdf.Pages.PageSons[0].Procsets[0].Should().Be("PDF");
                    pdf.Pages.PageSons[0].Procsets[1].Should().Be("Text");
                }
            );

    }
}
