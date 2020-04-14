using System;
using System.IO;
using SharpPDF.Lib;
using Xunit;
using System.Linq;
using FluentAssertions;

namespace SharpPDF.Tests
{
    public class DictionaryObjectShould
    {     
        [Theory]
        [InlineData("/Type /Example>>")]
        [InlineData("</Type /Example >>")]
        [InlineData("<< /Type /Example>")]
        [InlineData("<< /Type /Example ")]
        [InlineData("<< 1 2 >>")]
        public void ThrowError(string fragment)
        {
            // 7.3.7 Dictionary Objects
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));            
            
            Assert.Throws<PdfException>(() => { var dictionaryObject = new DictionaryObject(feed); });
        }        

        [Theory]
        [InlineData("<</Type /Example>>")]
        [InlineData("<</Type /Example >>")]
        [InlineData("<< /Type /Example>>")]
        [InlineData("<< /Type /Example >>")]
        public void ReadAMinimalDictionary(string fragment)
        {
            // 7.3.7 Dictionary Objects
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));            
            
            var actual = new DictionaryObject(feed); 
            actual.Childs<PdfObject>().Should().HaveCount(2);
            actual.Childs<PdfObject>()[0].Should().BeOfType<NameObject>();

            Assert.Equal("Type", actual.Child<NameObject>(0).Value);
            Assert.Equal("Example", actual.Child<NameObject>(1).Value);
        }        

        [Fact]
        public void ReadDictionary()
        {
            // 7.3.7 Dictionary Objects
            string fragment = @"<< /Type /Example
/Subtype /DictionaryExample
/Version 0.01
/IntegerItem 12
/StringItem (a string)
/Subdictionary << /Item1 0.4
/Item2 true
/LastItem (not!)
/VeryLastItem (OK)
>>
>>";
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            Objectizer objectizer = new Objectizer(feed);
           
            DictionaryObject actual = (DictionaryObject)objectizer.NextObject();
            Assert.Equal(12, actual.Childs<PdfObject>().Length);


            Assert.Equal("Type", actual.Child<NameObject>(0).Value);
            Assert.Equal("Example", actual.Child<NameObject>(1).Value);        

            Assert.Equal("Subtype", actual.Child<NameObject>(2).Value);
            Assert.Equal("DictionaryExample", actual.Child<NameObject>(3).Value);

            Assert.Equal("Version", actual.Child<NameObject>(4).Value);
            Assert.Equal(0.01f, actual.Child<RealObject>(5).FloatValue);        

            Assert.Equal("IntegerItem", actual.Child<NameObject>(6).Value);
            Assert.Equal(12, actual.Child<IntegerObject>(7).IntValue); 

            Assert.Equal("StringItem", actual.Child<NameObject>(8).Value);
            Assert.Equal("a string", actual.Child<StringObject>(9).Value);        

            Assert.Equal("Subdictionary", actual.Child<NameObject>(10).Value);

            Assert.Equal("Item1", actual.Child<DictionaryObject>(11).Child<NameObject>(0).Value);
            Assert.Equal(0.4f, actual.Child<DictionaryObject>(11).Child<RealObject>(1).FloatValue);

            Assert.Equal("Item2", actual.Child<DictionaryObject>(11).Child<NameObject>(2).Value);
            Assert.True(actual.Child<DictionaryObject>(11).Child<BooleanObject>(3).Value);

            Assert.Equal("LastItem", actual.Child<DictionaryObject>(11).Child<NameObject>(4).Value);
            Assert.Equal("not!", actual.Child<DictionaryObject>(11).Child<StringObject>(5).Value);

            Assert.Equal("VeryLastItem", actual.Child<DictionaryObject>(11).Child<NameObject>(6).Value);
            Assert.Equal("OK", actual.Child<DictionaryObject>(11).Child<StringObject>(7).Value);
        }
        
        [Theory]
        [InlineData("<</Length /No>>stream\r\n0123456789\r\nendstream")]
        [InlineData("<</Length /10>>other")]
        [InlineData("<</Length /10>>")]
        [InlineData("<</Length 10>>stream0123456789\nendstream")]
        [InlineData("<</Length 10>>stream\r0123456789endstream")]
        [InlineData("<</Length 10>>stream 0123456789endstream")]
        [InlineData("<</Length 10>>stream\n0123456789\nendstrea")]
        [InlineData("<</Length 10>>stream\n0123456789\nabc")]
        [InlineData("<</Length 10>>strea\n0123456789\nendstream")]
        [InlineData("<</Length 10>>\n0123456789\nendstream")]
        public void ReadStreamsErrors(string fragment)
        {
            // 7.3.8 Stream Objects            
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            Objectizer objectizer = new Objectizer(feed);
            
            Assert.Throws<PdfException>(() => { objectizer.NextObject(); });
        }

        [Theory]
        [InlineData("<</Length 10>>stream\r\n0123456789\r\nendstream")]
        [InlineData("<</Length 10>>stream\n0123456789\nendstream")]
        [InlineData("<</Length 10>>stream\n0123456789endstream")] // is not completely valid, but some writers do
        public void ReadStreams(string fragment)
        {
            // 7.3.8 Stream Objects            
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            Objectizer objectizer = new Objectizer(feed);
           
            DictionaryObject actual = (DictionaryObject)objectizer.NextObject();
            Assert.Equal(2, actual.Childs<PdfObject>().Length);


            Assert.Equal("Length", actual.Child<NameObject>(0).Value);
            Assert.Equal(10, actual.Child<IntegerObject>(1).IntValue);        

            Assert.Equal(System.Text.UTF8Encoding.UTF8.GetBytes("0123456789"), actual.Stream);
        }
    }
}
