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
        [InlineData("Times-Roman", false, false)]
        [InlineData("Times-Roman", false, true)]
        [InlineData("Times-Roman", true, false)]
        [InlineData("Times-Roman", true, true)]
        [InlineData("Courier", false, false)]
        [InlineData("Courier", false, true)]
        [InlineData("Courier", true, false)]
        [InlineData("Courier", true, true)]
        [InlineData("Helvetica", false, false)]
        [InlineData("Helvetica", false, true)]
        [InlineData("Helvetica", true, false)]
        [InlineData("Helvetica", true, true)]
        [InlineData("Symbol", false, false)]
        [InlineData("Symbol", false, true)]
        [InlineData("Symbol", true, false)]
        [InlineData("Symbol", true, true)]
        [InlineData("ZapfDingbats", false, false)]
        [InlineData("ZapfDingbats", false, true)]
        [InlineData("ZapfDingbats", true, false)]
        [InlineData("ZapfDingbats", true, true)]        
        public void LoadBaseFonts(string fontName, bool bold, bool italic) =>
            crcPdf(
                Given: pdf => { pdf.Pages
                    .AddPage()                        
                        .SetFont(fontName, 12, bold, italic)
                        .SetPosition(10, 15)
                        .AddLabel("Hola"); },
                Then: pdf => { 
                    pdf.Pages.PageSons[0].Font[0].Should().BeOfType<DocumentBaseFont>();
                    if (bold || italic)
                        fontName += "-";
                    if (bold)
                        fontName += "Bold";
                    if (italic)
                        fontName += "Italic";
                    pdf.Pages.PageSons[0].Font[0].Name = fontName;
                    }
            );

            [Fact]
        public void ReadBaseFontFromFile() =>            
            crcPdfShould(
                Given: File.ReadAllBytes("samples/microsample.pdf"),
                Then: pdf => { 
                    pdf.Pages.PageSons[0].Font[0].Should().BeOfType<DocumentBaseFont>();
                    pdf.Pages.PageSons[0].Font[0].Name = "Times-Roman";
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
                    pdf.Pages.PageSons[0].Font[0].Should().BeOfType<DocumentTtfFont>();
                    pdf.Pages.PageSons[0].Font[0].Name = fontName;
    
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
                    pdf.Pages.PageSons[0].Font[0].Should().BeOfType<DocumentTtfSubsetFont>();
                    pdf.Pages.PageSons[0].Font[0].Name = fontName;
    
                    using (var fs = new FileStream("openSansSubset.pdf", FileMode.Create)) {
                        pdf.Save(fs);
                    }
                    }
            );
    }
}