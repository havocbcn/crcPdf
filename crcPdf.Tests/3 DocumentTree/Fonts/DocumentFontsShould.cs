// This file is part of crcPdf.
// 
// crcPdf is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// crcPdf is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with crcPdf.  If not, see <http://www.gnu.org/licenses/>.
using FluentAssertions;
using System.IO;
using Xunit;
using crcPdf.Fonts;

namespace crcPdf.Tests {
    public class DocumentFontShould : crcPdfTest {   
        [Theory]
        [InlineData("Times-Roman", false, false, "Times-Roman")]
        [InlineData("Times Roman", false, true, "Times-Italic")]
        [InlineData("Times-Roman", true, false, "Times-Bold")]
        [InlineData("Times-Roman", true, true, "Times-BoldItalic")]
        [InlineData("Courier", false, false, "Courier")]
        [InlineData("Courier", false, true, "Courier-Oblique")]
        [InlineData("Courier", true, false, "Courier-Bold")]
        [InlineData("Courier", true, true, "Courier-BoldOblique")]
        [InlineData("Helvetica", false, false, "Helvetica")]
        [InlineData("Helvetica", false, true, "Helvetica-Oblique")]
        [InlineData("Helvetica", true, false, "Helvetica-Bold")]
        [InlineData("Helvetica", true, true, "Helvetica-BoldOblique")]
        [InlineData("Symbol", false, false, "Symbol")]
        [InlineData("Symbol", false, true, "Symbol")]
        [InlineData("Symbol", true, false, "Symbol")]
        [InlineData("Symbol", true, true, "Symbol")]
        [InlineData("ZapfDingbats", false, false, "ZapfDingbats")]
        [InlineData("ZapfDingbats", false, true, "ZapfDingbats")]
        [InlineData("ZapfDingbats", true, false, "ZapfDingbats")]
        [InlineData("ZapfDingbats", true, true, "ZapfDingbats")]
        public void LoadBaseFonts(string fontName, bool bold, bool italic, string baseFontName) =>
            crcPdf(
                Given: pdf => { pdf.Pages
                    .AddPage()
                        .SetFont(fontName, 12, bold, italic)
                        .SetPosition(10, 15)
                        .AddLabel("Hola"); 
                },
                Then: pdf => { 
                    var textOperators = pdf.Pages.PageSons[0].Contents.PageOperator<TextObject>(0);
                    var fontCode = textOperators.Operator<FontOperator>(0).Code;                    
                    pdf.Pages.PageSons[0].Font[fontCode].Should().BeOfType<DocumentBaseFont>();                    
                    pdf.Pages.PageSons[0].Font[fontCode].Name.Should().Be(baseFontName);
                    }
            );

            [Fact]
        public void ReadBaseFontFromFile() =>            
            crcPdfShould(
                Given: File.ReadAllBytes("samples/microsample.pdf"),
                Then: pdf => { 
                    var textOperators = pdf.Pages.PageSons[0].Contents.PageOperator<TextObject>(0);
                    var fontCode = textOperators.Operator<FontOperator>(0).Code;    
                    pdf.Pages.PageSons[0].Font[fontCode].Should().BeOfType<DocumentBaseFont>();
                    pdf.Pages.PageSons[0].Font[fontCode].Name.Should().Be("Times-Roman");
                }
            );

        [Theory]
        [InlineData("OpenSans-Regular")]        
        public void LoadTTFFonts(string fontName) =>
            crcPdf(
                Given: pdf => { pdf.Pages
                    .AddPage()                        
                        .SetFont(fontName, 12)
                        .SetPosition(10, 15)
                        .AddLabel("Hola"); },
                Then: pdf => { 
                    var textOperators = pdf.Pages.PageSons[0].Contents.PageOperator<TextObject>(0);
                    var fontCode = textOperators.Operator<FontOperator>(0).Code;    
                    pdf.Pages.PageSons[0].Font[fontCode].Should().BeOfType<DocumentTtfFont>();
                    pdf.Pages.PageSons[0].Font[fontCode].Name.Should().Be("OpenSans");
    
                    using (var fs = new FileStream("openSans.pdf", FileMode.Create)) {
                        pdf.Save(fs);
                    }
                    }
            );

        [Theory]
        [InlineData("OpenSans-Regular")]        
        public void LoadSubsetTTFFonts(string fontName) =>
            crcPdf(
                Given: pdf => { pdf.Pages
                    .AddPage()                        
                        .SetFont(fontName, 12, Embedded.Yes)
                        .SetPosition(10, 15)
                        .AddLabel("Α α:Alpha. Β β: Beta. Γ γ: Gamma. Δ δ: Delta"); },
                Then: pdf => { 
                    var textOperators = pdf.Pages.PageSons[0].Contents.PageOperator<TextObject>(0);
                    var fontCode = textOperators.Operator<FontOperator>(0).Code;    
                    pdf.Pages.PageSons[0].Font[fontCode].Should().BeOfType<DocumentTtfSubsetFont>();
                    pdf.Pages.PageSons[0].Font[fontCode].Name.Should().EndWith("OpenSans");
    
                    using (var fs = new FileStream("openSansSubset.pdf", FileMode.Create)) {
                        pdf.Save(fs);
                    }
                    }
            );

        [Theory]
        [InlineData("OpenSans-Regular")]        
        public void MixTTFFontsAndSubset(string fontName) =>
            crcPdf(
                Given: pdf => { pdf.Pages
                    .AddPage()                        
                        .SetFont(fontName, 12, Embedded.No)
                        .SetPosition(10, 15)
                        .AddLabel("Α α:Alpha. Β β: Beta. Γ γ: Gamma. Δ δ: Delta")
                        .SetFont(fontName, 12, Embedded.Yes)
                        .SetPosition(100, 150)
                        .AddLabel("Α α:Alpha. Β β: Beta. Γ γ: Gamma. Δ δ: Delta"); },
                Then: pdf => { 
                    var textOperators = pdf.Pages.PageSons[0].Contents.PageOperator<TextObject>(0);
                    var fontCode1 = textOperators.Operator<FontOperator>(0).Code;  
                    var fontCode2 = textOperators.Operator<FontOperator>(3).Code;    

                    pdf.Pages.PageSons[0].Font[fontCode1].Should().BeOfType<DocumentTtfFont>();
                    pdf.Pages.PageSons[0].Font[fontCode1].Name.Should().Be("OpenSans");
                    pdf.Pages.PageSons[0].Font[fontCode2].Should().BeOfType<DocumentTtfSubsetFont>();
                    pdf.Pages.PageSons[0].Font[fontCode2].Name.Should().EndWith("OpenSans");
    
                    using (var fs = new FileStream("openSansSubsetMix.pdf", FileMode.Create)) {
                        pdf.Save(fs);
                    }
                    }
            );
    }
}