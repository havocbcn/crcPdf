using FluentAssertions;
using System.IO;
using Xunit;

namespace crcPdf.Tests.Outline {   
    public class PdfOutlineShould : crcPdfTest {    
        [Fact]
        public void ReadEmptyOutlines() =>            
            // 12.3.3 Document outline
            crcPdfShould(
                Given: File.ReadAllBytes("samples/sample.pdf"),
                Then: pdf => { 
                    pdf.Outlines.Count.Should().Be(0);

                      using (var fileStream = File.Create("outline2.pdf"))
                    {
                        pdf.Save(fileStream);
                    }
                }
            );

        [Fact]
        public void CreateATwoPageWithTwoOutlines() =>
            crcPdf(
                Given: pdf => { 
                    var firstPage = pdf.Pages.AddPage();
                    var secondPage = pdf.Pages.AddPage();
                    pdf.Outlines.Add(
                        new DocumentOutlines("first", firstPage, 0, 792, 1, 
                            new DocumentOutlines("first-first", firstPage, null, 701, null), 
                            new DocumentOutlines("first-second", firstPage, null, 680, null,
                                new DocumentOutlines("first-second-first", firstPage, 200, 3, 1)),
                            new DocumentOutlines("first-third", firstPage, 2, 300, 2)), 
                        new DocumentOutlines("second", secondPage, 100, 200, 3));
                    },
                Then: pdf => { 
                    pdf.Outlines.Count.Should().Be(6);
                    
                    pdf.Outlines.First.Title.Should().Be("first");
                    pdf.Outlines.First.X.Should().Be(0);
                    pdf.Outlines.First.Y.Should().Be(792);
                    pdf.Outlines.First.Zoom.Should().Be(1);
                    pdf.Outlines.First.Count.Should().Be(4);
                    pdf.Outlines.First.Parent.Should().Be(pdf.Outlines);

                    pdf.Outlines.Last.Title.Should().Be("second");

                    pdf.Outlines.First.Next.Title.Should().Be("second");
                    pdf.Outlines.First.Next.Prev.Title.Should().Be("first");
                    
                    pdf.Outlines.First.First.Title.Should().Be("first-first");
                    pdf.Outlines.First.First.X.Should().BeNull();
                    pdf.Outlines.First.First.Y.Should().Be(701);
                    pdf.Outlines.First.First.Zoom.Should().BeNull();

                }
            );
    }
}
