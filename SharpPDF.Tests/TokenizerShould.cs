using System;
using System.IO;
using SharpPDF.Lib;
using Xunit;

namespace SharpPDF.Tests
{
    public class TokenizerShould
    {        
        [Theory]
        [InlineData("abc")]
        public void GetLiteralToken(string fragment)
        {
            // 7.2.2 Character set - White Characters
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            Assert.Equal("abc", feed.GetToken().ToString());
        }

        [Theory]
        [InlineData("abc\0123>qwe")]
        [InlineData("abc\t123]qwe")]
        [InlineData("abc\n123/qwe")]
        [InlineData("abc\f123[qwe")]
        [InlineData("abc\r123<qwe")]
        [InlineData("abc 123>qwe")]
        public void SeparateTokenTypes(string fragment)
        {
            // 7.2.2 Character set - White Characters
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            Assert.Equal("abc", feed.GetToken().ToString());
            Assert.Equal(CharacterSetType.WhiteSpace, feed.GetToken().characterSetClass);
            Assert.Equal("123", feed.GetToken().ToString());
            Assert.Equal(CharacterSetType.Delimiter, feed.GetToken().characterSetClass);
            Assert.Equal("qwe", feed.GetToken().ToString());
            Assert.Equal(CharacterSetType.EndOfFile, feed.GetToken().characterSetClass);
        }

         [Theory]
        [InlineData("abc\0123>qwe")]
        [InlineData("abc\t123]qwe")]
        [InlineData("abc\n123/qwe")]
        [InlineData("abc\f123[qwe")]
        [InlineData("abc\r123<qwe")]
        [InlineData("abc 123>qwe")]
        public void GoBack(string fragment)
        { 
            // 7.2.2 Character set - White Characters
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            Assert.Equal("abc", feed.GetToken().ToString());
            feed.GoBack();
            Assert.Equal(CharacterSetType.Regular, feed.GetToken().characterSetClass);
            Assert.Equal(CharacterSetType.WhiteSpace, feed.GetToken().characterSetClass);
            Assert.Equal("123", feed.GetToken().ToString());
            feed.GoBack();
            Assert.Equal(CharacterSetType.Regular, feed.GetToken().characterSetClass);
            Assert.Equal(CharacterSetType.Delimiter, feed.GetToken().characterSetClass);
            Assert.Equal("qwe", feed.GetToken().ToString());
            feed.GoBack();
            Assert.Equal(CharacterSetType.Regular, feed.GetToken().characterSetClass);
            Assert.Equal(CharacterSetType.EndOfFile, feed.GetToken().characterSetClass);
            feed.GoBack();  // EOF
            feed.GoBack();  // qwe
            feed.GoBack();  // delimiter
            feed.GoBack();  // 123
            feed.GoBack();  // separator
            feed.GoBack();  // abc
            Assert.Equal("abc", feed.GetToken().ToString());
        }


        [Theory]
        [InlineData("abc\0123")]
        [InlineData("abc\t123")]
        [InlineData("abc\n123")]
        [InlineData("abc\f123")]
        [InlineData("abc\r123")]
        [InlineData("abc 123")]
        public void SeparateSyntaticConstructsByWhiteSpaces(string fragment)
        {
            // 7.2.2 Character set - White Characters
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            Assert.Equal("abc", feed.GetToken().ToString());
            Assert.Equal(CharacterSetType.WhiteSpace, feed.GetToken().characterSetClass);
            Assert.Equal("123", feed.GetToken().ToString());
        }
        

        [Fact]
        public void TreatAnySequenceOfConsecutiveWhiteSpaceAsOneCharacter()
        {
            // 7.2.2 Character set - White Characters
            string fragment = "abc  123";
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            
            Assert.Equal("abc", feed.GetToken().ToString());
            Assert.Equal(CharacterSetType.WhiteSpace, feed.GetToken().characterSetClass);
            Assert.Equal("123", feed.GetToken().ToString());
        }

        [Theory]
        [InlineData("abc(123","(")]
        [InlineData("abc)123",")")]
        [InlineData("abc<123","<")]
        [InlineData("abc>123",">")]
        [InlineData("abc[123","[")]
        [InlineData("abc]123","]")]
        [InlineData("abc}123","}")]
        [InlineData("abc{123","{")]
        [InlineData("abc/123","/")]
        public void SeparateSyntaticConstructsByDelimiterCharacters(string fragment, string separator)
        {
            // 7.2.2 Character set - Delimiter Characters
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));

            Token token = feed.GetToken();
            Assert.Equal("abc", token.ToString());
            Assert.Equal(CharacterSetType.Regular, token.characterSetClass);

            token = feed.GetToken();
            Assert.Equal(separator, token.ToString());
            Assert.Equal(CharacterSetType.Delimiter, token.characterSetClass);

            token = feed.GetToken();
            Assert.Equal("123", token.ToString());
            Assert.Equal(CharacterSetType.Regular, token.characterSetClass);

            token = feed.GetToken();
            Assert.Equal(CharacterSetType.EndOfFile, token.characterSetClass);
        }


        [Theory]
        [InlineData(@"abc% comment ( ) blah blah blah
123")]
        [InlineData(@"abc% comment ( /%) blah blah blah
123")]
        public void IgnoreComments(string fragment)
        {
            /// 7.2.3 Comments
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));

            Assert.Equal("abc", feed.GetToken().ToString());
            Assert.Equal(CharacterSetType.WhiteSpace, feed.GetToken().characterSetClass);
            Assert.Equal("123", feed.GetToken().ToString());
        }
    


       

        [Theory]
        [InlineData("34.5", 34.5f)]
        [InlineData("-3.62", -3.62f)]
        [InlineData("+123.6", 123.6f)]
        [InlineData("4.", 4f)]
        [InlineData("-.002", -0.002f)]
        [InlineData("0.0", 0.0f)]
        [InlineData("abc", null)]
        public void ObtainRealNumber(string fragment, float? expected)
        {
            /// 7.3.3 Numeric Objects
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            
            Assert.Equal(expected, feed.GetRealNumber());
        }

        [Theory]
        [InlineData("Not a valid string")]
        [InlineData("[Not a valid string]")]
        [InlineData("(Not a valid string")]        
        [InlineData("<Not a valid string")]        
        [InlineData("<")]        
        [InlineData("(")]        
        public void StringMustStartAndEndWithAParenthesisOrLessThan(string fragment)
        {
            /// 7.3.4 String objects
            /// 7.3.4.1 General
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            
            Assert.Throws<PdfException>(() => {feed.GetString(); });
        }

        [Theory]
        [InlineData("( This is a string )", " This is a string ")]
        public void ObtainLiteralString(string fragment, string expected)
        {
            // 7.3.4 String objects
            // Example 1
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            
            Assert.Equal(expected, feed.GetString());
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
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            
            Assert.Equal(expected, feed.GetString());
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
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            
            Assert.Equal(expected, feed.GetString());
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
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            
            Assert.Equal(expected, feed.GetString());
        }

         [Theory]
        [InlineData("(This string contains \\245two octal characters\\103.)", "This string contains ¥two octal charactersC.")]
        public void UseReverseSolidusAsOctalInLiteralString(string fragment, string expected)
        {
            // 7.3.4.2 Literal Strings
            // EXAMPLE 4
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            
            Assert.Equal(expected, feed.GetString());
        }   

         [Theory]
        [InlineData(@"(\0533)", "+3")]
        [InlineData(@"(\053)", "+")]
        [InlineData(@"(\53)", "+")]
        public void UseReverseSolidusAsOctalWith3_2Or1NumberInLiteralString(string fragment, string expected)
        {
            // 7.3.4.2 Literal Strings
            // EXAMPLE 5
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            
            Assert.Equal(expected, feed.GetString());
        }
        
        [Theory]
        [InlineData(@"<CAFE>", "\xCA\xFE")]
        [InlineData(@"<caFE>", "\xCA\xFE")]
        [InlineData(@"<0123>", "\x01\x23")]
        public void ReadHexadecimalStrings(string fragment, string expected)
        {
            // 7.3.4.3 Hexadecimal Strings
            // EXAMPLE 1
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            
            Assert.Equal(expected, feed.GetString());
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
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            
            Assert.Equal(expected, feed.GetString());
        }

        [Theory]
        [InlineData(@"<CAF>", "\xCA\xF0")]
        public void ReadHexadecimalStringsFillingWithZeroes(string fragment, string expected)
        {
            // 7.3.4.3 Hexadecimal Strings
            // EXAMPLE 2
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            
            Assert.Equal(expected, feed.GetString());
        }

     
    }
}
