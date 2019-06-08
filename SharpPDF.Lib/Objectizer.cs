using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SharpPDF.Lib
{
    // 7.3 Objects
    public class Objectizer
    {
        private readonly Tokenizer tokenizer;

        public Objectizer(Tokenizer tokenizer)
        {
            this.tokenizer = tokenizer;
        }

        public IPdfObject GetObject()
        {
            Token token = tokenizer.GetToken();

            if (token.characterSetClass  == CharacterSetType.EndOfFile)
            {
                throw new PdfException(PdfExceptionCodes.FILE_ABRUPTLY_TERMINATED, "expected something else");    
            }

            IPdfObject pdfObject = null;
            if (token.ToString() == "/")
             {
                tokenizer.GoBack();
                pdfObject = new NameObject(tokenizer);
             }

            else if (token.ToString().StartsWith("true") ||
                token.ToString().StartsWith("false"))
            {
                tokenizer.GoBack();
                pdfObject = new BooleanObject(tokenizer);
            }           
            else if (token.IsRegularNumber)
            {
                Token secondToken = tokenizer.GetToken();
                if (secondToken.characterSetClass == CharacterSetType.WhiteSpace)
                {
                    Token thirdToken = tokenizer.GetToken();
                    if (thirdToken.IsRegularNumber)
                    {
                        Token fourthToken = tokenizer.GetToken();
                        if (fourthToken.characterSetClass == CharacterSetType.WhiteSpace)
                        {
                            Token fifthToken = tokenizer.GetToken();
                            if (fifthToken.characterSetClass == CharacterSetType.Regular &&
                                fifthToken.ToString() == "obj")
                            {
                                tokenizer.GoBack(); 
                                tokenizer.GoBack(); 
                                tokenizer.GoBack(); 
                                tokenizer.GoBack();
                                tokenizer.GoBack();
                                pdfObject = new IndirectObject(tokenizer);
                            }
                            else
                            {
                                tokenizer.GoBack(); 
                                tokenizer.GoBack(); 
                                tokenizer.GoBack(); 
                                tokenizer.GoBack();
                                tokenizer.GoBack();
                                throw new NotImplementedException();
                            }
                        }
                        else
                        {
                            tokenizer.GoBack(); 
                            tokenizer.GoBack(); 
                            tokenizer.GoBack();
                            tokenizer.GoBack();
                            pdfObject = new IntegerObject(tokenizer);
                        }
                    }
                    else
                    {
                        tokenizer.GoBack(); 
                        tokenizer.GoBack();
                        tokenizer.GoBack();
                        pdfObject = new IntegerObject(tokenizer);
                    }
                }
                else
                {
                    tokenizer.GoBack();
                    tokenizer.GoBack();
                    pdfObject = new IntegerObject(tokenizer);
                }
            }
            else if (token.IsNumber)
            {
                tokenizer.GoBack();
                pdfObject = new IntegerObject(tokenizer);
            }
            else
            {
                throw new PdfException(PdfExceptionCodes.UNKNOWN_TOKEN, "Not known structure:" + token.ToString());    
            }
            
            pdfObject.Analyze();

            return pdfObject;
        }
        
       
    }
}