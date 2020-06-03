using System.IO;
using FluentAssertions;
using Xunit;

namespace crcPdf.Tests.Image {       
    public class DocumentImageShould : crcPdfTest {    
        [Fact]
        public void CreateAnImage() =>            
           crcPdf(
                Given: pdf => { pdf.Pages
                    .AddPage()        
                        .SaveGraph() 
                        .CurrentTransformationMatrix(300, 0, 0, 500, 50, 100)
                        .AddImage("samples/image.jpg")
                        .RestoreGraph();
                    },
                Then: pdf => { 
                    pdf.Pages.PageSons[0].Contents.PageOperators[0].Should().BeOfType<SaveGraphOperator>();
                    pdf.Pages.PageSons[0].Contents.PageOperator<CurrentTransformationMatrixOperator>(1).A.Should().Be(300);
                    pdf.Pages.PageSons[0].Contents.PageOperator<CurrentTransformationMatrixOperator>(1).B.Should().Be(0);
                    pdf.Pages.PageSons[0].Contents.PageOperator<CurrentTransformationMatrixOperator>(1).C.Should().Be(0);
                    var imageCode = pdf.Pages.PageSons[0].Contents.PageOperator<ImageOperator>(2).Code;
                    pdf.Pages.PageSons[0].Image[imageCode].Width.Should().Be(787);
                    pdf.Pages.PageSons[0].Image[imageCode].Height.Should().Be(1024);
                    pdf.Pages.PageSons[0].Contents.PageOperators[3].Should().BeOfType<RestoreGraphOperator>();

                     using (var fs = new FileStream("image.pdf", FileMode.Create)) {
                        pdf.Save(fs);
                    }
                }
            );

                [Fact]
        public void ReadAndSaveRawImage() =>
            // 12.3.3 Document outline
            crcPdfShould(
                Given: File.ReadAllBytes("samples/rawImage.pdf"),
                Then: pdf => { 
                    pdf.Pages.PageSons.Should().HaveCount(1);

                    using (var fs = new FileStream("rawImage_output.pdf", FileMode.Create)) {
                        pdf.Save(fs, Compression.Compress | Compression.Optimize);
                    }
                }
            );
            

    }
}
