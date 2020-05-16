using System.IO;
using FluentAssertions;
using SharpPDF.Lib;
using Xunit;

namespace SharpPDF.Tests.Outline {       
    public class DocumentImageShould : SharpPdfTest {    
        [Fact]
        public void CreateAnImage() =>            
           SharpPdf(
                Given: pdf => { pdf.Catalog.Pages
                    .AddPage()        
                        .SaveGraph() 
                        .CurrentTransformationMatrix(300, 0, 0, 500, 50, 100)
                        .AddImage("samples/image.jpg")
                        .RestoreGraph();
                    },
                Then: pdf => { 
                    pdf.Catalog.Pages.PageSons[0].Contents.PageOperators[0].Should().BeOfType<SaveGraphOperator>();
                    pdf.Catalog.Pages.PageSons[0].Contents.PageOperator<CurrentTransformationMatrixOperator>(1).A.Should().Be(300);
                    pdf.Catalog.Pages.PageSons[0].Contents.PageOperator<CurrentTransformationMatrixOperator>(1).B.Should().Be(0);
                    pdf.Catalog.Pages.PageSons[0].Contents.PageOperator<CurrentTransformationMatrixOperator>(1).C.Should().Be(0);
                    var imageCode = pdf.Catalog.Pages.PageSons[0].Contents.PageOperator<ImageOperator>(2).Code;
                    pdf.Catalog.Pages.PageSons[0].Image[imageCode].Width.Should().Be(787);
                    pdf.Catalog.Pages.PageSons[0].Image[imageCode].Height.Should().Be(1024);
                    pdf.Catalog.Pages.PageSons[0].Contents.PageOperators[3].Should().BeOfType<RestoreGraphOperator>();

                     using (var fs = new FileStream("image.pdf", FileMode.Create)) {
                        pdf.WriteTo(fs);
                    }
                }
            );

    }
}
