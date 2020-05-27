using System.IO;
using crcPdf;
using Xunit;
using FluentAssertions;

namespace crcPdf.Tests {
    public class IndirectReferenceObjectShould {     
        [Fact]
        public void ACreatedNumberMustBePositive() {
            Assert.Throws<PdfException>(() => { var a = new IndirectReferenceObject(-1); });
        }   

        [Theory]
        [InlineData("-1 0 R")]
        public void AReadNumberMustBePositive(string fragment) {
            // 7.3.7 Dictionary Objects
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));            
            
            Assert.Throws<PdfException>(() => { var a = new IndirectReferenceObject(feed); });
        }  

        [Theory]
        [InlineData("1 -1 R")]
        public void AReadGenerationMustBePositive(string fragment) {
            // 7.3.7 Dictionary Objects
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));            
            
            Assert.Throws<PdfException>(() => { var a = new IndirectReferenceObject(feed); });
        }  

        [Theory]
        [InlineData("1 0 NO_R")]
        public void ReferenceMustBeR(string fragment) {
            // 7.3.7 Dictionary Objects
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));            
            
            Assert.Throws<PdfException>(() => { var a = new IndirectReferenceObject(feed); });
        }  

        [Theory]
        [InlineData("1 0R")]
        public void WhiteSpacesOk(string fragment) {
            // 7.3.7 Dictionary Objects
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));            
            
            Assert.Throws<PdfException>(() => { var a = new IndirectReferenceObject(feed); });
        }  

        [Theory]
        [InlineData("A 0 R")]
        [InlineData("0 A R")]
        public void MustBeNumbers(string fragment) {
            // 7.3.7 Dictionary Objects
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));            
            
            Assert.Throws<PdfException>(() => { var a = new IndirectReferenceObject(feed); });
        }  
        

        [Fact]
        public void TwoInstanceMustBeTheSame() {
            var instance1 = new IndirectReferenceObject(1);
            var instance2 = new IndirectReferenceObject(1);

            instance1.Should().Equals(instance2);
        }        
  
    }
}