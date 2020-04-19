using FluentAssertions;
using System.IO;
using SharpPDF.Lib;
using Xunit;

namespace SharpPDF.Tests {
    public class DocumentFontShould : SharpPdfTest {   
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
            SharpPdf(
                Given: pdf => { pdf.Catalog.Pages
                    .AddPage()                        
                        .SetFont(fontName, 12, bold, italic)
                        .SetPosition(10, 15)
                        .AddLabel("Hola"); },
                Then: pdf => { 
                    pdf.Catalog.Pages.PageSons[0].Font[0].Should().BeOfType<DocumentBaseFont>();
                    if (bold || italic)
                        fontName += "-";
                    if (bold)
                        fontName += "Bold";
                    if (italic)
                        fontName += "Italic";
                    pdf.Catalog.Pages.PageSons[0].Font[0].Name = fontName;
                    }
            );

            [Fact]
        public void ReadBaseFontFromFile() =>            
            SharpPdfShould(
                Given: File.ReadAllBytes("samples/microsample.pdf"),
                Then: pdf => { 
                    pdf.Catalog.Pages.PageSons[0].Font[0].Should().BeOfType<DocumentBaseFont>();
                    pdf.Catalog.Pages.PageSons[0].Font[0].Name = "Times-Roman";
                }
            );

        [Theory]
        [InlineData("OpenSans-Regular")]        
        public void LoadTTFFonts(string fontName) =>
            SharpPdf(
                Given: pdf => { pdf.Catalog.Pages
                    .AddPage()                        
                        .SetFont(fontName, 12)
                        .SetPosition(10, 15)
                        .AddLabel("Hola"); },
                Then: pdf => { 
                    pdf.Catalog.Pages.PageSons[0].Font[0].Should().BeOfType<DocumentTtfFont>();
                    pdf.Catalog.Pages.PageSons[0].Font[0].Name = fontName;
    
                    using (var fs = new FileStream("openSans.pdf", FileMode.Create)) {
                        pdf.WriteTo(fs);
                    }
                    }
            );
    }
}