using System;
using System.Collections.Generic;
using System.IO;

namespace SharpPDF.Lib {
    // 9 Text
    public class GraphicObjectizer {
        private readonly Objectizer reader;

        public GraphicObjectizer(byte[] content) {
            var stream = new MemoryStream(content);
            this.reader = new Objectizer(new Tokenizer(stream));
        }

        private static readonly Dictionary<string, Func<List<IPdfObject>, ITextOperator>> textOperators = 
            new Dictionary<string, Func<List<IPdfObject>, ITextOperator>>()
        {
            { "Tj", (t) => { 
                    if (t.Count != 1)
                        throw new PdfException(PdfExceptionCodes.INVALID_CONTENT, "Unknown graphic objectizer found in strem content");

                    var parameter = t[0] as StringObject;
                    if (parameter == null)
                        throw new PdfException(PdfExceptionCodes.INVALID_CONTENT, $"Expected a string but {parameter.ObjectType.ToString()} found");
                                    
                    return new TextOperator(parameter.Value);
                }
            },
            { "Td", (t) => { 
                    if (t.Count != 2)
                        throw new PdfException(PdfExceptionCodes.INVALID_CONTENT, "Unknown graphic objectizer found in strem content");

                    var x = t[0] as IntegerObject;
                    var y = t[1] as IntegerObject;
                    if (x == null || y == null)
                        throw new PdfException(PdfExceptionCodes.INVALID_CONTENT, $"Expected: int int Td, but {x.ObjectType.ToString()} {y.ObjectType.ToString()} Td found");
                                    
                    return new TextPositioningOperator(x.Value, y.Value);
                }
            },
             { "Tf", (t) => { 
                    if (t.Count != 2)
                        throw new PdfException(PdfExceptionCodes.INVALID_CONTENT, "Unknown graphic objectizer found in strem content");

                    var font = t[0] as NameObject;
                    var size = t[1] as IntegerObject;
                    if (font == null || size == null)
                        throw new PdfException(PdfExceptionCodes.INVALID_CONTENT, $"Expected: name integer Tf, but {font.ObjectType.ToString()} {size.ObjectType.ToString()} Td found");
                                    
// TODO
                    return new TextOperator("hola");
                }
            }
        };

        private readonly Dictionary<string, Func<Objectizer, IGraphicObject>> objects = 
            new Dictionary<string, Func<Objectizer, IGraphicObject>>() {
            { "BT", (t) => { 
                    var textObject = new TextObject();
                    List<IPdfObject> parameters = new List<IPdfObject>();

                    var o = t.NextObject(true);
                    while (o.ToString() != "ET")
                    {
                        if (o is OperatorObject) {
                            if (textOperators.ContainsKey(o.ToString()))
                                textObject.AddOperator(textOperators[o.ToString()].Invoke(parameters));

                            parameters.Clear();
                        }
                        else 
                            parameters.Add(o);

                        o = t.NextObject(true);
                    }

                    return textObject;
                }
            }
        };

        internal List<IGraphicObject> ReadObjects() {

            List<IGraphicObject> objs = new List<IGraphicObject>();
            var o = reader.NextObject(true);

            while (!reader.IsEOF()) {
                if (!(o is OperatorObject))
                  throw new PdfException(PdfExceptionCodes.INVALID_CONTENT, "Expected an object operator like BT");
            
                if (objects.ContainsKey(o.ToString()))
                    objs.Add(objects[o.ToString()].Invoke(reader));
                else 
                    throw new PdfException(PdfExceptionCodes.INVALID_CONTENT, $"Unknown operator {o.ToString()}");                        
                
                if (!reader.IsEOF())
                    o = reader.NextObject(true);                
            }            

            return objs;              
        }
    }
}