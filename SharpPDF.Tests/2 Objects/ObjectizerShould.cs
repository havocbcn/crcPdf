using System.IO;
using SharpPDF.Lib;
using Xunit;
using FluentAssertions;
using System.Text;

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
                        
            BooleanObject actual = (BooleanObject)objectizer.NextObject();
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

            Assert.Throws<PdfException>(() => { objectizer.NextObject(); });
        }

        [Theory]
        [InlineData("123", 123)]
        [InlineData("43445", 43445)]
        [InlineData("0", 0)]
        public void ObtainIntegerNumber(string fragment, int? expected)
        {
            /// 7.3.3 Numeric Objects
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            
            Objectizer objectizer = new Objectizer(feed); 

            IntegerObject actual = (IntegerObject)objectizer.NextObject();
            Assert.Equal(expected, actual.IntValue);
        }

        [Theory]
        [InlineData("+17", 17)]
        [InlineData("-98", -98)]
        public void ObtainIntegerNumberWithSign(string fragment, int? expected)
        {
            /// 7.3.3 Numeric Objects
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            
            Objectizer objectizer = new Objectizer(feed); 

            IntegerObject actual = (IntegerObject)objectizer.NextObject();
            Assert.Equal(expected, actual.IntValue);
        }

        [Theory]
        [InlineData("hola")]
        [InlineData("17+")]
        [InlineData("17-")]
        [InlineData("1-7")]
        public void ObtainIntegerNumberFromATextIsNotAllowed(string fragment)
        {
            /// 7.3.3 Numeric Objects
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            
            Objectizer objectizer = new Objectizer(feed); 
            
            Assert.Throws<PdfException>(() => { objectizer.NextObject(); });
        }

        
        [Theory]
        [InlineData("34.5", 34.5f)]
        [InlineData("-3.62", -3.62f)]
        [InlineData("+123.6", 123.6f)]
        [InlineData("4.", 4f)]
        [InlineData("-.002", -0.002f)]
        [InlineData("0.0", 0.0f)]
        public void ObtainRealNumber(string fragment, float expected)
        {
            /// 7.3.3 Numeric Objects
            var bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            var feed = new Tokenizer(new MemoryStream(bytes));
            
            var objectizer = new Objectizer(feed); 

            var actual = (RealObject)objectizer.NextObject();
            Assert.Equal(expected, actual.FloatValue);
        }

         [Theory]
        [InlineData("( This is a string )", " This is a string ")]
        public void ObtainLiteralString(string fragment, string expected)
        {
            // 7.3.4 String objects
            // Example 1
            var bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            var feed = new Tokenizer(new MemoryStream(bytes));
            
            var objectizer = new Objectizer(feed); 

            var actual = (StringObject)objectizer.NextObject();
            Assert.Equal(expected, actual.Value);
        }

          

        [Theory]
        [InlineData("( This is a string )", " This is a string ")]
        [InlineData(@"( Strings may contain newlines
and such . )",@" Strings may contain newlines
and such . ")]
        [InlineData(@"( Strings may contain balanced parentheses ( ) and special characters ( * ! & } ^ % and so on ) . )", @" Strings may contain balanced parentheses ( ) and special characters ( * ! & } ^ % and so on ) . ")]
        [InlineData("()", "")]
        public void ObtainLiteralStringWithBalancedParenthesis(string fragment, string expected)
        {
            // 7.3.4 String objects
            var bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            var feed = new Tokenizer(new MemoryStream(bytes));
            
            var objectizer = new Objectizer(feed); 

            var actual = (StringObject)objectizer.NextObject();
            Assert.Equal(expected, actual.Value);
        }

        [Theory]
        [InlineData(@"(abc\n123)", "abc\n123")]
        [InlineData(@"(abc\r123)", "abc\r123")]
        [InlineData(@"(abc\t123)", "abc\t123")]
        [InlineData(@"(abc\b123)", "abc\b123")]
        [InlineData(@"(abc\f123)", "abc\f123")]
        [InlineData(@"(abc\(123)", "abc(123")]
        [InlineData(@"(abc\)123)", "abc)123")]
        [InlineData(@"(abc\\123)", "abc\\123")]
        [InlineData(@"(abc\K123)", "abc\\K123")]
        public void UseReverseSolidusAsEscapeCharacterInLiteralString(string fragment, string expected)
        {
            // 7.3.4.2 Literal Strings
            // Table 3
           var bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            var feed = new Tokenizer(new MemoryStream(bytes));
            
            var objectizer = new Objectizer(feed); 

            var actual = (StringObject)objectizer.NextObject();
            Assert.Equal(expected, actual.Value);
        }           

        [Theory]
        [InlineData(@"(These \
two strings \
are the same.)", "These two strings are the same.")]
        [InlineData(@"(These \
)", "These ")]
        public void UseReverseSolidusAsLineContinuatorInLiteralString(string fragment, string expected)
        {
            // 7.3.4.2 Literal Strings
            // EXAMPLE 2
            var bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            var feed = new Tokenizer(new MemoryStream(bytes));
            
            var objectizer = new Objectizer(feed); 

            var actual = (StringObject)objectizer.NextObject();
            Assert.Equal(expected, actual.Value);
        }

        [Theory]
        [InlineData("(This string contains \\245two octal characters\\103.)", "This string contains ¥two octal charactersC.")]
        public void UseReverseSolidusAsOctalInLiteralString(string fragment, string expected)
        {
            // 7.3.4.2 Literal Strings
            // EXAMPLE 4
            var bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            var feed = new Tokenizer(new MemoryStream(bytes));
            
            var objectizer = new Objectizer(feed); 

            var actual = (StringObject)objectizer.NextObject();
            Assert.Equal(expected, actual.Value);
        }   

        [Theory]
        [InlineData(@"(\0533)", "+3")]
        [InlineData(@"(\053)", "+")]
        [InlineData(@"(\53)", "+")]
        public void UseReverseSolidusAsOctalWith3_2Or1NumberInLiteralString(string fragment, string expected)
        {
            // 7.3.4.2 Literal Strings
            // EXAMPLE 5
            var bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            var feed = new Tokenizer(new MemoryStream(bytes));
            
            var objectizer = new Objectizer(feed); 

            var actual = (StringObject)objectizer.NextObject();
            Assert.Equal(expected, actual.Value);
        }
        
        [Theory]
        [InlineData(@"<CAFE>", "\xCA\xFE")]
        [InlineData(@"<caFE>", "\xCA\xFE")]
        [InlineData(@"<0123>", "\x01\x23")]
        public void ReadHexadecimalStrings(string fragment, string expected)
        {
            // 7.3.4.3 Hexadecimal Strings
            // EXAMPLE 1
            var bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            var feed = new Tokenizer(new MemoryStream(bytes));
            
            var objectizer = new Objectizer(feed); 

            var actual = (StringObject)objectizer.NextObject();
            Assert.Equal(expected, actual.Value);
        }

        [Theory]
        [InlineData("<CA FE>", "\xCA\xFE")]
        [InlineData("<CA\nFE>", "\xCA\xFE")]
        [InlineData("<CA\tFE>", "\xCA\xFE")]
        [InlineData("<CA\rFE>", "\xCA\xFE")]
        [InlineData("<CA\fFE>", "\xCA\xFE")]
        [InlineData("<C\nA\fF E>", "\xCA\xFE")]
        public void ReadHexadecimalStringsIgnoringEscapeSequence(string fragment, string expected)
        {
            // 7.3.4.3 Hexadecimal Strings
            // EXAMPLE 1
            var bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            var feed = new Tokenizer(new MemoryStream(bytes));
            
            var objectizer = new Objectizer(feed); 

            var actual = (StringObject)objectizer.NextObject();
            Assert.Equal(expected, actual.Value);
        }

        [Theory]
        [InlineData(@"<CAF>", "\xCA\xF0")]
        public void ReadHexadecimalStringsFillingWithZeroes(string fragment, string expected)
        {
            // 7.3.4.3 Hexadecimal Strings
            // EXAMPLE 2
            var bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            var feed = new Tokenizer(new MemoryStream(bytes));
            
            var objectizer = new Objectizer(feed); 

            var actual = (StringObject)objectizer.NextObject();
            Assert.Equal(expected, actual.Value);
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
        [InlineData(@"/Name1]", "Name1")]

        public void ReadNameObjects(string fragment, string expected)
        {
            // 7.3.5 Name Objects
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            Objectizer objectizer = new Objectizer(feed);
            
            NameObject actual = (NameObject)objectizer.NextObject();
            Assert.Equal(expected, actual.Value);
        }

        [Fact]
        public void ReadIndirectObjects()
        {
            // 7.3.10 Indirect Objects
            string fragment = "12 0 obj true endobj";
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            Objectizer objectizer = new Objectizer(feed);
            
            IndirectObject actual = (IndirectObject)objectizer.NextObject();
            Assert.Equal(12, actual.Number);
            Assert.Equal(0, actual.Generation);

            var childs = actual.Childs<BooleanObject>();
            Assert.Single(childs);
            Assert.True(childs[0].Value);
        }

          [Fact]
        public void ReadIndirectObjectsMustHaveAValue()
        {
            // 7.3.10 Indirect Objects
            string fragment = "12 0 obj endobj";
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            Objectizer objectizer = new Objectizer(feed);
            
            Assert.Throws<PdfException>(() => { objectizer.NextObject(); });            
        }

         [Fact]
        public void ReadIndirectReference()
        {
            // 7.3.10 Indirect Objects
            string fragment = @"1 0 R";
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            var feed = new Tokenizer(new MemoryStream(bytes));
            var objectizer = new Objectizer(feed);
           
            var actual = (IndirectReferenceObject)objectizer.NextObject();
            Assert.Empty(actual.Childs<PdfObject>());            

            Assert.Equal(1, actual.Number);
            Assert.Equal(0, actual.Generation);
        }       

        [Theory]
        [InlineData("<</Type /Example>>")]
        [InlineData("<</Type /Example >>")]
        [InlineData("<< /Type /Example>>")]
        [InlineData("<< /Type /Example >>")]
        public void ReadAMinimalDictionary(string fragment)
        {
            // 7.3.7 Dictionary Objects
            byte[] bytes = UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            Objectizer objectizer = new Objectizer(feed);
            
            objectizer.NextObject().Should().BeOfType<DictionaryObject>();
        }        

        [Theory]
        [InlineData("[549 3.14 false (Ralph) /SomeName]")]
        [InlineData("[549 3.14 false (Ralph) /SomeName ]")]
        [InlineData("[ 549 3.14 false (Ralph) /SomeName]")]
        [InlineData("[ 549 3.14 false (Ralph) /SomeName ]")]
        public void ReadArray(string fragment)
        {
            // 7.3.6 Array Objects
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            var feed = new Tokenizer(new MemoryStream(bytes));
            var objectizer = new Objectizer(feed);
           
            var actual = (ArrayObject)objectizer.NextObject();
            Assert.Equal(5, actual.Childs<PdfObject>().Length);

            Assert.Equal(549, actual.Child<IntegerObject>(0).IntValue);
            Assert.Equal(3.14f, actual.Child<RealObject>(1).FloatValue);
            Assert.False(actual.Child<BooleanObject>(2).Value);
            Assert.Equal("Ralph", actual.Child<StringObject>(3).Value);
            Assert.Equal("SomeName", actual.Child<NameObject>(4).Value);        
        }

        [Fact]
        public void ReadArrayWithNoElements()
        {
            // 7.3.6 Array Objects
            string fragment = @"[]";
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            Objectizer objectizer = new Objectizer(feed);
           
            var actual = (ArrayObject)objectizer.NextObject();
            
            actual.Childs<PdfObject>().Should().BeEmpty();
        }
        
        
    }
}