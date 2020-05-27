using FluentAssertions;
using System.IO;
using Xunit;

namespace crcPdf.Tests.Outline {   
    public class PdfOutlineShould : crcPdfTest {    
        [Fact]
        public void ReadEmptyOutlines() =>            
            // 12.3.3 Document outline
            crcPdfShould(
                Given: File.ReadAllBytes("samples/sample.pdf"),
                Then: pdf => { 
                    pdf.Catalog.Outlines.Count.Should().Be(0);
                }
            );
    }
}
