using System;
using FluentAssertions;
using SharpPDF.Lib;
using Xunit;

namespace SharpPDF.Tests.Outline {   
    public class PathOperatorsShould : SharpPdfTest {    
        [Fact]
        public void CreateAPathFilled() =>            
            SharpPdf(
                Given: pdf => { pdf.Catalog.Pages
                    .AddPage()                        
                        .AddRectangle(10, 11, 12, 14.5f)
                        .AddStroke()
                        .AddFill();
                    },
                Then: pdf => { 
                    pdf.Catalog.Pages.PageSons[0].Contents.PageOperators.Should().HaveCount(3);
                    pdf.Catalog.Pages.PageSons[0].Contents.PageOperator<RectangleOperator>(0).X.Should().Be(10);
                    pdf.Catalog.Pages.PageSons[0].Contents.PageOperator<RectangleOperator>(0).Y.Should().Be(11);
                    pdf.Catalog.Pages.PageSons[0].Contents.PageOperator<RectangleOperator>(0).Width.Should().Be(12);
                    pdf.Catalog.Pages.PageSons[0].Contents.PageOperator<RectangleOperator>(0).Height.Should().Be(14.5f);

                    pdf.Catalog.Pages.PageSons[0].Contents.PageOperator<StrokeOperator>(1).Should().BeOfType<StrokeOperator>();

                    pdf.Catalog.Pages.PageSons[0].Contents.PageOperator<FillOperator>(2).Should().BeOfType<FillOperator>();
                }
            );

        [Fact]
        public void APathOperatorMustBeFollowedByPaintOperators() 
        {
            SharpPdf pdf = new SharpPdf();
            Action act = () => pdf.Catalog.Pages
                    .AddPage()                        
                        .AddRectangle(10, 10, 12, 14.5f)
                        .SetLineCap(LineCapStyle.ButtCap)
                        .AddStroke();

            act.Should().Throw<PdfException>()
                .WithMessage("INVALID_OPERATOR: A Path Painting Operator must appear after a Path Construction Operator or another Path Painting Operator");
        }
    }
}