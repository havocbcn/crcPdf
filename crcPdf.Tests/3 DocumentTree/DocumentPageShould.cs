using FluentAssertions;
using System.IO;
using Xunit;

namespace crcPdf.Tests.DocumentTree {   
    //14.2 Procedure Sets
    public class DocumentPageShould : crcPdfTest {    
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

        [Fact]
        public void SetMediaBoxExpert() {
            // Table 30 – Entries in a page object
            var mediaBox = new Rectangle(0, 1, 2, 3);
            var pdf = Pdf.CreateExpert();
            pdf.Pages.AddPage();
            pdf.Pages.PageSons[0].SetMediaBox(mediaBox);
            pdf.Pages.PageSons[0].MediaBox.Should().Be(mediaBox);
        }


        [Fact]
        public void SetMediaBoxSimple() {
            // Table 30 – Entries in a page object
            int width = 2;
            int height = 3;
            var mediaBox = new Rectangle(0, 1, width, height);
            var pdf = Pdf.CreateSimple(width, height);
            pdf.NewPage();
            pdf.Catalog.Pages.SetMediaBox(mediaBox);
            pdf.Catalog.Pages.MediaBox.Should().Be(mediaBox);
        }
    }
}
