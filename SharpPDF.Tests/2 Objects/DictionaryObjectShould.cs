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
            actual.Childs().Should().HaveCount(2);
            actual.Childs()[0].Should().BeOfType<NameObject>();

            Assert.Equal("Type", ((NameObject)actual.Childs()[0]).Value);
            Assert.Equal("Example", ((NameObject)actual.Childs()[1]).Value);
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
            Assert.Equal(ObjectType.Dictionary, actual.ObjectType);
            Assert.Equal(12, actual.Childs().Length);


            Assert.Equal("Type", ((NameObject)actual.Childs()[0]).Value);
            Assert.Equal("Example", ((NameObject)actual.Childs()[1]).Value);        

            Assert.Equal("Subtype", ((NameObject)actual.Childs()[2]).Value);
            Assert.Equal("DictionaryExample", ((NameObject)actual.Childs()[3]).Value);        

            Assert.Equal("Version", ((NameObject)actual.Childs()[4]).Value);
            Assert.Equal(0.01f, ((RealObject)actual.Childs()[5]).Value);        

            Assert.Equal("IntegerItem", ((NameObject)actual.Childs()[6]).Value);
            Assert.Equal(12, ((IntegerObject)actual.Childs()[7]).Value); 

            Assert.Equal("StringItem", ((NameObject)actual.Childs()[8]).Value);
            Assert.Equal("a string", ((StringObject)actual.Childs()[9]).Value);        

            Assert.Equal("Subdictionary", ((NameObject)actual.Childs()[10]).Value);

            Assert.Equal("Item1", ((NameObject)actual.Childs()[11].Childs()[0]).Value);
            Assert.Equal(0.4f, ((RealObject)actual.Childs()[11].Childs()[1]).Value);

            Assert.Equal("Item2", ((NameObject)actual.Childs()[11].Childs()[2]).Value);
            Assert.True(((BooleanObject)actual.Childs()[11].Childs()[3]).Value);

            Assert.Equal("LastItem", ((NameObject)actual.Childs()[11].Childs()[4]).Value);
            Assert.Equal("not!", ((StringObject)actual.Childs()[11].Childs()[5]).Value);

            Assert.Equal("VeryLastItem", ((NameObject)actual.Childs()[11].Childs()[6]).Value);
            Assert.Equal("OK", ((StringObject)actual.Childs()[11].Childs()[7]).Value);
        }
        
        [Theory]
        [InlineData("<</Length /No>>stream\r\n0123456789\r\nendstream")]
        [InlineData("<</Length /10>>other")]
        [InlineData("<</Length /10>>")]
        [InlineData("<</Length 10>>stream0123456789\nendstream")]
        [InlineData("<</Length 10>>stream\n0123456789endstream")]
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
        public void ReadStreams(string fragment)
        {
            // 7.3.8 Stream Objects            
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            Objectizer objectizer = new Objectizer(feed);
           
            DictionaryObject actual = (DictionaryObject)objectizer.NextObject();
            Assert.Equal(ObjectType.Dictionary, actual.ObjectType);
            Assert.Equal(2, actual.Childs().Length);


            Assert.Equal("Length", ((NameObject)actual.Childs()[0]).Value);
            Assert.Equal(10, ((IntegerObject)actual.Childs()[1]).Value);        

            Assert.Equal(System.Text.UTF8Encoding.UTF8.GetBytes("0123456789"), actual.Stream);
        }
    }
}
