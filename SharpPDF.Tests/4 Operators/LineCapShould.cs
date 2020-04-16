using FluentAssertions;
using SharpPDF.Lib;
using Xunit;

namespace SharpPDF.Tests.Outline {   
    public class LineCapShould : SharpPdfTest {    
        [Fact]
        public void CreateALineCapAtPageDescriptionLevel() =>            
            SharpPdf(
                Given: pdf => { pdf.Catalog.Pages
                    .AddPage()                        
                        .SetLineCap(LineCapStyle.ButtCap) ;
                    },
                Then: pdf => { 
                    pdf.Catalog.Pages.PageSons[0].Contents.PageOperators.Should().HaveCount(1);
                    pdf.Catalog.Pages.PageSons[0].Contents.PageOperators[0].Should().BeOfType<LineCapOperator>();
                    ((LineCapOperator)pdf.Catalog.Pages.PageSons[0].Contents.PageOperators[0]).LineCap.Should().Be(LineCapStyle.ButtCap);
                }
            );

        [Fact]
        public void CreateALineCapAtTextObjectLevel() =>            
            SharpPdf(
                Given: pdf => { pdf.Catalog.Pages
                    .AddPage()                
                        .SetPosition(1, 2)
                        .SetLineCap(LineCapStyle.ButtCap);
                    },
                Then: pdf => { 
                    pdf.Catalog.Pages.PageSons[0].Contents.PageOperators.Should().HaveCount(1);
                    pdf.Catalog.Pages.PageSons[0].Contents.PageOperators[0].Should().BeOfType<TextObject>();
                    ((TextObject)pdf.Catalog.Pages.PageSons[0].Contents.PageOperators[0]).Operators.Should().HaveCount(2);
                    ((TextPositioningOperator)((TextObject)pdf.Catalog.Pages.PageSons[0].Contents.PageOperators[0]).Operators[0]).X.Should().Be(1);
                    ((TextPositioningOperator)((TextObject)pdf.Catalog.Pages.PageSons[0].Contents.PageOperators[0]).Operators[0]).Y.Should().Be(2);
                    ((LineCapOperator)((TextObject)pdf.Catalog.Pages.PageSons[0].Contents.PageOperators[0]).Operators[1]).LineCap.Should().Be(LineCapStyle.ButtCap);
                }
            );

        [Fact]
        public void CreateANonStrokingColourAtPageDescriptionLevel() =>            
            SharpPdf(
                Given: pdf => { pdf.Catalog.Pages
                    .AddPage()                        
                        .SetNonStrokingColour(0f, 1f, 0.5f);
                    },
                Then: pdf => { 
                    ((NonStrokingColourOperator)pdf.Catalog.Pages.PageSons[0].Contents.PageOperators[0]).R.Should().Be(0f);
                    ((NonStrokingColourOperator)pdf.Catalog.Pages.PageSons[0].Contents.PageOperators[0]).G.Should().Be(1f);
                    ((NonStrokingColourOperator)pdf.Catalog.Pages.PageSons[0].Contents.PageOperators[0]).B.Should().Be(0.5f);
                }
            );

        [Fact]
        public void CreateANonStrokingColourAtTextObjectLevel() =>            
            SharpPdf(
                Given: pdf => { pdf.Catalog.Pages
                    .AddPage()     
                        .SetPosition(1, 2)                   
                        .SetNonStrokingColour(0f, 1f, 0.5f);
                    },
                Then: pdf => { 
                    ((NonStrokingColourOperator)((TextObject)pdf.Catalog.Pages.PageSons[0].Contents.PageOperators[0]).Operators[1]).R.Should().Be(0f);
                    ((NonStrokingColourOperator)((TextObject)pdf.Catalog.Pages.PageSons[0].Contents.PageOperators[0]).Operators[1]).G.Should().Be(1f);
                    ((NonStrokingColourOperator)((TextObject)pdf.Catalog.Pages.PageSons[0].Contents.PageOperators[0]).Operators[1]).B.Should().Be(0.5f);
                }
            );
    }
}
