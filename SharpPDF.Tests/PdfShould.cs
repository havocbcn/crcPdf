using FluentAssertions;
using System;
using System.IO;
using System.Linq;
using SharpPDF.Lib;
using Xunit;
using System.Text;

namespace SharpPDF.Tests
{
    public class PdfShould
    {     
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
                Given: header + @"
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
%%EOF",
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
        public void ReadASimpleObjectTree()
        {
            string pdfFile = @"%PDF-1.1
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
%%EOF";
            
            byte[] bytes = UTF8Encoding.UTF8.GetBytes(pdfFile);
            var pdf = new SharpPdf(new MemoryStream(bytes));

            pdf.Childs.Should().HaveCount(4);
        }      


        [Fact]
        public void ObtainASimpleCatalog() =>
            // 7.7.2 Document Catalog
            SharpPdfShould(
                Given: @"%PDF-1.1
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
%%EOF",
                When: pdf => { },
                Then: pdf => {                     
                    pdf.Catalog.Should().NotBeNull();
                    }
            );

        [Fact]
        public void ObtainASimplePageTree() =>
            // 7.7.3 Page Tree
            SharpPdfShould(
                Given: @"%PDF-1.1
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
%%EOF",
                Then: pdf => { 
                    pdf.Catalog.Pages.PageSons.Should().HaveCount(1);
                    pdf.Catalog.Pages.PageSons[0].Parent.Should().Be(pdf.Catalog.Pages);
                    }
            );

        [Fact]
        public void ObtainAHierarchyPageTree() =>
            // 7.7.3 Page Tree
            SharpPdfShould(
                Given: @"%PDF-1.1
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
%%EOF",
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
                Given: @"%PDF-1.1
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
%%EOF",
                Then: pdf => { 
                    pdf.Catalog.Pages.PageSons[0].Contents.Should().NotBeNull();
                }
            );

        [Fact]
        public void WriteAnEmptyPdf() =>
            // 7.5.2 File Header
            SharpPdf(
                Given: pdf => { pdf.Catalog.Pages
                    .AddPage()                        
                        .SetPosition(10, 15)
                        .AddLabel("Hola"); },
                Then: pdf => { 
                    pdf.Catalog.Pages.PageSons.Should().HaveCount(1);
                    pdf.Catalog.Pages.PageSons[0].Contents.GraphicObject.Should().HaveCount(1);
                    pdf.Catalog.Pages.PageSons[0].Contents.GraphicObject[0].Should().BeOfType<TextObject>();
                    ((TextObject)pdf.Catalog.Pages.PageSons[0].Contents.GraphicObject[0]).Operations.Should().HaveCount(2);
                    ((TextObject)pdf.Catalog.Pages.PageSons[0].Contents.GraphicObject[0]).Operations[0].ToString().Should().Be("10 15 Td");
                    ((TextObject)pdf.Catalog.Pages.PageSons[0].Contents.GraphicObject[0]).Operations[1].ToString().Should().Be("(Hola) Tj");
                }
            );
 
        private void SharpPdfShouldGiveAnError(string Given, Action<SharpPdf> When)
        {            
            string pdfFile = Given;

            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(pdfFile);
            SharpPdf pdf = new SharpPdf(new MemoryStream(bytes));

            Assert.Throws<PdfException>(() => { When(pdf); });
        }

        private void SharpPdfShould(string Given, Action<SharpPdf> When = null, Action<SharpPdf> Then = null)
        {            
            string pdfFile = Given;

            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(pdfFile);
            SharpPdf pdf = new SharpPdf(new MemoryStream(bytes));

            if (When != null)
                When(pdf);

            if (Then != null)
                Then(pdf);
        }

        private void SharpPdf(Action<SharpPdf> Given, Action<SharpPdf> When = null, Action<SharpPdf> Then = null)
        {            
            SharpPdf pdfWriter = new SharpPdf();
            Given(pdfWriter);

            MemoryStream ms = new MemoryStream();
            pdfWriter.WriteTo(ms);
            ms.Seek(0, SeekOrigin.Begin);
            SharpPdf pdfRead = new SharpPdf(ms);

            if (When != null)
                When(pdfRead);

            if (Then != null)
                Then(pdfRead);
        }
    }
}