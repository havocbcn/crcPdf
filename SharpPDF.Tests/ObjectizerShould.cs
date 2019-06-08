using System;
using System.IO;
using SharpPDF.Lib;
using Xunit;
using System.Linq;

namespace SharpPDF.Tests
{
    public class ObjectizerShould
    {     

        [Theory]
        [InlineData("true", true)]
        [InlineData("false", false)]
        public void ObtainBoolean(string fragment, bool? expected)
        {
            /// 7.3.2 Boolean objects
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            Objectizer objectizer = new Objectizer(feed);
            
            BooleanObject actual = (BooleanObject)objectizer.GetObject();
            Assert.Equal(ObjectType.Boolean, actual.Type());
            Assert.Equal(expected, actual.Value);
        }

        [Theory]
        [InlineData("True")]
        [InlineData("False")]
        [InlineData(")")]
        [InlineData("")]
        public void DoNotAllowInvalidBoolean(string fragment)
        {
            /// 7.3.2 Boolean objects
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            
            Objectizer objectizer = new Objectizer(feed); 

            Assert.Throws<PdfException>(() => { objectizer.GetObject(); });
        }

        [Theory]
        [InlineData("123", 123)]
        [InlineData("43445", 43445)]
        [InlineData("+17", 17)]
        [InlineData("-98", -98)]
        [InlineData("0", 0)]
        public void ObtainIntegerNumber(string fragment, int? expected)
        {
            /// 7.3.3 Numeric Objects
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            
            Objectizer objectizer = new Objectizer(feed); 

            IntegerObject actual = (IntegerObject)objectizer.GetObject();
            Assert.Equal(ObjectType.Integer, actual.Type());
            Assert.Equal(expected, actual.Value);
        }

        [Fact]
        public void ObtainIntegerNumberFromATextIsNotAllowed()
        {
            /// 7.3.3 Numeric Objects
            string fragment = "hola";
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            
            Objectizer objectizer = new Objectizer(feed); 
            
            Assert.Throws<PdfException>(() => { objectizer.GetObject(); });
        }

        [Theory]
        [InlineData(@"/Name1", "Name1")]
        [InlineData(@"/A;Name_With-Various***Characters?", "A;Name_With-Various***Characters?")]
        [InlineData(@"/1.2", "1.2")]
        [InlineData(@"/$$", "$$")]
        [InlineData(@"/@pattern", "@pattern")]
        [InlineData(@"/.notdef", ".notdef")]
        [InlineData(@"/Lime#20Green", "Lime Green")]
        [InlineData(@"/paired#28#29parentheses", "paired()parentheses")]
        [InlineData(@"/The_Key_of_F#23_Minor", "The_Key_of_F#_Minor")]
        [InlineData(@"/A#42", "AB")]

        public void ReadNameObjects(string fragment, string expected)
        {
            // 7.3.5 Name Objects
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            Objectizer objectizer = new Objectizer(feed);
            
            NameObject actual = (NameObject)objectizer.GetObject();
            Assert.Equal(ObjectType.Name, actual.Type());
            Assert.Equal(expected, actual.Value());
        }

        [Fact]
        public void ReadIndirectObjects()
        {
            // 7.3.10 Indirect Objects
            string fragment = "12 0 obj true endobj";
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            Objectizer objectizer = new Objectizer(feed);
            
            IndirectObject actual = (IndirectObject)objectizer.GetObject();
            Assert.Equal(12, actual.Number);
            Assert.Equal(0, actual.Generation);

            var childs = actual.Childs();
            Assert.Equal(1, childs.ToArray().Count());

            Assert.Equal(ObjectType.Boolean, childs.ToArray()[0].Type());
            Assert.Equal(true, ((BooleanObject)(childs.ToArray()[0])).Value);
        }

          [Fact]
        public void ReadIndirectObjectsMustHaveAValue()
        {
            // 7.3.10 Indirect Objects
            string fragment = "12 0 obj endobj";
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            Objectizer objectizer = new Objectizer(feed);
            
            Assert.Throws<PdfException>(() => { objectizer.GetObject(); });            
        }
    }
}