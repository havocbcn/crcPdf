using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SharpPDF.Lib
{
    public class IndirectObject : IPdfObject
    {
        public IndirectObject(Tokenizer tokenizer) 
        {
            this.tokenizer = tokenizer;
        }

        private int number;
        private int generation;
        private readonly Tokenizer tokenizer;

        private IPdfObject child;

        public int Number => number;
        public int Generation => generation;

        public IEnumerable<IPdfObject> Childs()
        {
            yield return child;
        }

        public void Analyze()
        {            
            ReadNumber(tokenizer.GetToken(), ref number);
            ExpectAWhiteSpace(tokenizer.GetToken());
            ReadNumber(tokenizer.GetToken(), ref generation);
            if (generation < 0)
                throw new PdfException(PdfExceptionCodes.INVALID_GENERATION, "Generation must be positive");

            ExpectAWhiteSpace(tokenizer.GetToken());
            ExpectAText(tokenizer.GetToken(), "obj");
            ExpectAWhiteSpace(tokenizer.GetToken());

            Objectizer analyzeChilds = new Objectizer(tokenizer);

            child = analyzeChilds.GetObject();

            ExpectAWhiteSpace(tokenizer.GetToken());
            ExpectAText(tokenizer.GetToken(), "endobj");
        }

        private void ExpectAText(Token token, string expected)
        {
            if (token.characterSetClass != CharacterSetType.Regular || token.ToString() != expected)
                throw new PdfException(PdfExceptionCodes.INVALID_INDIRECTOBJECT_TOKEN, "Expected '" + expected + "' but '" + token.ToString() + "' appeared");
        }

        private void ExpectAWhiteSpace(Token token)
        {
            if (token.characterSetClass != CharacterSetType.WhiteSpace)
                throw new PdfException(PdfExceptionCodes.INVALID_INDIRECTOBJECT_TOKEN, "Expected a whitespace");

        }

        private void ReadNumber(Token token, ref int number)
        {
            if (!int.TryParse(token.ToString(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out number))
                throw new PdfException(PdfExceptionCodes.INVALID_INDIRECTOBJECT_TOKEN, "Expected a number");
        }

        public ObjectType Type()
        {
            return ObjectType.Indirect;        
        }
    }
}