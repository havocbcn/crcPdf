using System.IO;
using crcPdf;
using Xunit;

namespace crcPdf.Tests {
    public class TokenizerShould {        

        [Theory]
        [InlineData("abc")]
        public void GetLiteralToken(string fragment) {
            // 7.2.2 Character set - White Characters
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            Assert.Equal("abc", feed.TokenExcludedComments().ToString());
            Assert.Throws<PdfException>(() => {feed.TokenExcludedComments(); });
        }

        [Theory]
        [InlineData("abc\0123")]
        [InlineData("abc\t123")]
        [InlineData("abc\n123")]
        [InlineData("abc\f123")]
        [InlineData("abc\r123")]
        [InlineData("abc 123")]
        public void SeparateTokenTypes(string fragment)
        {
            // 7.2.2 Character set - White Characters
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            Assert.Equal("abc", feed.TokenExcludedComments().ToString());
            Assert.Equal(CharacterSetType.WhiteSpace, feed.TokenExcludedComments().characterSetClass);
            Assert.Equal("123", feed.TokenExcludedComments().ToString());
        }

         [Theory]
        [InlineData("abc\0123>qwe")]
        [InlineData("abc\t123]qwe")]
        [InlineData("abc\n123/qwe")]
        [InlineData("abc\f123[qwe")]
        [InlineData("abc\r123<qwe")]
        [InlineData("abc 123>qwe")]
        public void SaveAndRestorePosition(string fragment)
        { 
            // 7.2.2 Character set - White Characters
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            feed.SavePosition();
            feed.SavePosition();
            Assert.Equal("abc", feed.TokenExcludedComments().ToString());
            feed.RestorePosition();
            Assert.Equal(CharacterSetType.Regular, feed.TokenExcludedComments().characterSetClass);
            Assert.Equal(CharacterSetType.WhiteSpace, feed.TokenExcludedComments().characterSetClass);
            feed.SavePosition();
            Assert.Equal("123", feed.TokenExcludedComments().ToString());
            feed.RestorePosition();
            Assert.Equal(CharacterSetType.Regular, feed.TokenExcludedComments().characterSetClass);
            Assert.Equal(CharacterSetType.Delimiter, feed.TokenExcludedComments().characterSetClass);
            feed.SavePosition();
            Assert.Equal("qwe", feed.TokenExcludedComments().ToString());
            feed.RestorePosition();
            Assert.Equal(CharacterSetType.Regular, feed.TokenExcludedComments().characterSetClass);
            feed.RestorePosition();
            Assert.Equal("abc", feed.TokenExcludedComments().ToString());
        }

        [Fact]
        public void TreatAnySequenceOfConsecutiveWhiteSpaceAsOneCharacter()
        {
            // 7.2.2 Character set - White Characters
            string fragment = "abc  123";
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(fragment);
            Tokenizer feed = new Tokenizer(new MemoryStream(bytes));
            
            Assert.Equal("abc", feed.TokenExcludedComments().ToString());
            Assert.Equal(CharacterSetType.WhiteSpace, feed.TokenExcludedComments().characterSetClass);
            Assert.Equal("123", feed.TokenExcludedComments().ToString());
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

            Token token = feed.TokenExcludedComments();
            Assert.Equal("abc", token.ToString());
            Assert.Equal(CharacterSetType.Regular, token.characterSetClass);

            token = feed.TokenExcludedComments();
            Assert.Equal(separator, token.ToString());
            Assert.Equal(CharacterSetType.Delimiter, token.characterSetClass);

            token = feed.TokenExcludedComments();
            Assert.Equal("123", token.ToString());
            Assert.Equal(CharacterSetType.Regular, token.characterSetClass);
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

            Assert.Equal("abc", feed.TokenExcludedComments().ToString());
            Assert.Equal(CharacterSetType.WhiteSpace, feed.TokenExcludedComments().characterSetClass);
            Assert.Equal("123", feed.TokenExcludedComments().ToString());
        }
    }
}
