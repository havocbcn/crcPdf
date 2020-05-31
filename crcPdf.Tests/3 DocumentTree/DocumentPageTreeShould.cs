using FluentAssertions;
using Xunit;
using crcPdf;

namespace crcPdf.Tests.DocumentTree {   
    //7.7.3. Page Tree Nodes
    public class DocumentPageTreeShould : crcPdfTest {    
        [Fact]
        public void SetMediaBoxExpert() {
            // Table 30 – Entries in a page object
            var mediaBox = new Rectangle(0, 1, 2, 3);
            var pdf = Pdf.CreateExpert();
            pdf.Pages.SetMediaBox(mediaBox);
            pdf.Pages.MediaBox.Should().Be(mediaBox);
        }

        
    }
}
