using FluentAssertions;
using System;
using System.IO;
using SharpPDF.Lib;
using Xunit;
using System.Text;

namespace SharpPDF.Tests {
    public class PdfShould : SharpPdfTest {     
        [Theory]
        [InlineData("%PDF-1.1")]
        [InlineData("%PDF-1.2")]
        [InlineData("%PDF-1.3")]
        [InlineData("%PDF-1.4")]
        [InlineData("%PDF-1.5")]
        [InlineData("%PDF-1.6")]
        [InlineData("%PDF-1.7")]
        public void AcceptCorrectHeaderVersion(string header)
        {
            // 7.5.2 File Header
            SharpPdfShould(
                Given: System.Text.UTF8Encoding.UTF8.GetBytes(header + @"
%¥±ë
1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj
2 0 obj << /Type /Pages /Kids [3 0 R] /Count 1 /MediaBox [0 0 300 144] >> endobj
3 0 obj << /Type /Page /Parent 2 0 R /Resources 
<< /Font 
 << /F1 << /Type /Font /Subtype /Type1 /BaseFont /Times-Roman >> >>
>> /Contents 4 0 R >> endobj
4 0 obj << /Length 39 >> stream
BT /F1 18 Tf 0 0 Td (Hello World) Tj ET
endstream endobj
xref
0 5
0000000000 65535 f 
0000000017 00000 n 
0000000066 00000 n 
0000000147 00000 n 
0000000303 00000 n 
trailer << /Root 1 0 R /Size 5 >>
startxref
392
%%EOF"),
                When: pdf => {  },
                Then: pdf => { pdf.Catalog.Should().NotBeNull(); }
            );
        }

        [Theory]
        [InlineData("PDF-1.1")]
        [InlineData("%HOLA")]
        [InlineData("%PDF-1.0")]
        [InlineData("%PDF-1.8")]
        [InlineData("%PDF-2.1")]
        public void RejectIncorrectHeadersAndEOFs(string header)
        {
            // 7.5.2 File Header
            string pdfFile =  header + @"
xref
0 1
0000000000 65535 f 
trailer << /Root 1 0 R /Size 1 >>
startxref
9
%%EOF";
            Assert.Throws<PdfException>(() =>
            {
                byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(pdfFile);
                var pdf = new SharpPdf(new MemoryStream(bytes));
            });
        }

        [Theory]
        [InlineData("%%EO")]
        [InlineData("%EOF")]
        [InlineData("EOF")]
        public void EndWithACorrectEndOfFileMarker(string eof)
        {
            // 7.5.5 File Trailer
            string pdfFile = @"%PDF-1.1
startxref
9
" + eof;
            Assert.Throws<PdfException>(() =>
            {
                byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(pdfFile);
                var pdf = new SharpPdf(new MemoryStream(bytes));
            });
        }

        [Fact]
        public void HaveStartxref()
        {
            // 7.5.5 File Trailer
            string pdfFile = @"%PDF-1.1
startxre
9
%%EOF";

            Assert.Throws<PdfException>(() =>
            {
                byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(pdfFile);
                var pdf = new SharpPdf(new MemoryStream(bytes));
            });
        }

         [Fact]
        public void HaveStartxrefWithNewLineAndPosition()
        {
            // 7.5.5 File Trailer
            string pdfFile = @"%PDF-1.1
startxref 9
%%EOF";

            Assert.Throws<PdfException>(() =>
            {
                byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(pdfFile);
                var pdf = new SharpPdf(new MemoryStream(bytes));
            });
        }

         [Fact]
        public void Havexrefposition()
        { 
            // 7.5.5 File Trailer
            string pdfFile = @"%PDF-1.1
startxref
NO
%%EOF";
            Assert.Throws<PdfException>(() =>
            {
                byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(pdfFile);
                var pdf = new SharpPdf(new MemoryStream(bytes));
            });
        }

        [Fact]
        public void AssureThatEndOfFileMarkerIsLastLine()
        {
            // 7.5.5 File Trailer
            string pdfFile =  @"%PDF-1.1
%%EOF";
            Assert.Throws<PdfException>(() =>
            {
                byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(pdfFile);
                var pdf = new SharpPdf(new MemoryStream(bytes));
            });
        }

        [Fact]
        public void DoNotAcceptWithoutXRef()
        {
            // 7.5.5 File Trailer
            string pdfFile =  @"%PDF-1.1
ef
startxref
9
%%EOF";
            Assert.Throws<PdfException>(() =>
            {
                byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(pdfFile);
                var pdf = new SharpPdf(new MemoryStream(bytes));
            });
        }

        [Fact]
        public void ObtainASimpleCatalog() =>
            // 7.7.2 Document Catalog
            SharpPdfShould(
                Given: File.ReadAllBytes("samples/microsample.pdf"),
                When: pdf => { },
                Then: pdf => {                     
                    pdf.Catalog.Should().NotBeNull();
                    }
            );

        [Fact]
        public void ObtainASimplePageTree() =>
            // 7.7.3 Page Tree
            SharpPdfShould(
                Given: File.ReadAllBytes("samples/microsample.pdf"),
                Then: pdf => { 
                    pdf.Catalog.Pages.PageSons.Should().HaveCount(1);
                    pdf.Catalog.Pages.PageSons[0].Parent.Should().Be(pdf.Catalog.Pages);
                    }
            );

        [Fact]
        public void ObtainAHierarchyPageTree() =>
            // 7.7.3 Page Tree
            SharpPdfShould(
                Given: System.Text.UTF8Encoding.UTF8.GetBytes(@"%PDF-1.1
1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj
2 0 obj << /Type /Pages /Kids [3 0 R 5 0 R] /Count 2 /MediaBox [0 0 100 200] >> endobj
3 0 obj << /Type /Pages /Kids [4 0 R] /Count 1 /MediaBox [0 0 300 144] >> endobj
4 0 obj << /Type /Page /Parent 3 0 R /Contents 6 0 R >> endobj
5 0 obj << /Type /Page /Parent 2 0 R /Contents 6 0 R >> endobj
6 0 obj << /Length 39 >> stream
BT /F1 18 Tf 0 0 Td (Hello World) Tj ET
endstream endobj
xref
0 7
0000000000 65535 f 
0000000009 00000 n 
0000000058 00000 n 
0000000145 00000 n 
0000000226 00000 n 
0000000289 00000 n 
0000000352 00000 n 
trailer << /Root 1 0 R /Size 7 >>
startxref
441
%%EOF"),
                Then: pdf => { 
                    pdf.Catalog.Pages.PageSons.Should().HaveCount(1);
                    pdf.Catalog.Pages.PageTreeSons.Should().HaveCount(1);
                    pdf.Catalog.Pages.PageTreeSons[0].PageSons.Should().HaveCount(1);                   
                }
            );

        [Fact]
        public void ObtainAPageContent() =>
            // 7.7.3 Page Tree
            SharpPdfShould(
                Given: File.ReadAllBytes("samples/microsample.pdf"),
                Then: pdf => { 
                    pdf.Catalog.Pages.PageSons[0].Contents.Should().NotBeNull();
                }
            );

        [Fact]
        public void WriteASimplePdf() =>
            SharpPdf(
                Given: pdf => { pdf.Catalog.Pages
                    .AddPage()                        
                        .SetFont("Times roman", 12, false, false)
                        .SetPosition(10, 15)
                        .AddLabel("Hola"); },
                Then: pdf => { 
                    pdf.Catalog.Pages.PageSons.Should().HaveCount(1);
                    pdf.Catalog.Pages.PageSons[0].Contents.GraphicObject.Should().HaveCount(1);
                    pdf.Catalog.Pages.PageSons[0].Contents.GraphicObject[0].Should().BeOfType<TextObject>();
                    ((TextObject)pdf.Catalog.Pages.PageSons[0].Contents.GraphicObject[0]).Operations.Should().HaveCount(3);
                    ((TextObject)pdf.Catalog.Pages.PageSons[0].Contents.GraphicObject[0]).Operations[0].ToString().Should().Be("/F0 12 Tf");
                    ((TextObject)pdf.Catalog.Pages.PageSons[0].Contents.GraphicObject[0]).Operations[1].ToString().Should().Be("10 15 Td");
                    ((TextObject)pdf.Catalog.Pages.PageSons[0].Contents.GraphicObject[0]).Operations[2].ToString().Should().Be("(Hola) Tj");
                }
            );

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
        public void UseSimpleFonts(string fontName, bool bold, bool italic) =>
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
        public void ReadBaseFontResourceDirect() =>
            // 7.7.3 Page Tree
            SharpPdfShould(
                Given: File.ReadAllBytes("samples/microsample.pdf"),
                Then: pdf => { 
                    pdf.Catalog.Pages.PageSons[0].Font[0].Should().BeOfType<DocumentBaseFont>();
                    pdf.Catalog.Pages.PageSons[0].Font[0].Name = "Times-Roman";
                }
            );
    }
}