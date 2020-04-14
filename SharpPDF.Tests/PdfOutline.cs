using FluentAssertions;
using System;
using System.IO;
using SharpPDF.Lib;
using Xunit;

namespace SharpPDF.Tests.Outline {   
    public class PdfOutlineShould : SharpPdfTest {    
        [Fact]
        public void ReadEmptyOutlines() =>            
            // 12.3.3 Document outline
            SharpPdfShould(
                Given: File.ReadAllBytes("samples/sample.pdf"),
                Then: pdf => { 
                    pdf.Catalog.Outlines.Count.Should().Be(0);
                }
            );
    }
}
