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

        private static readonly Dictionary<string, Func<List<PdfObject>, ITextOperator>> textOperators = 
            new Dictionary<string, Func<List<PdfObject>, ITextOperator>>()
        {
            { "Tj", (t) => { 
                    if (t.Count != 1)
                        throw new PdfException(PdfExceptionCodes.INVALID_CONTENT, "Unknown graphic objectizer found in strem content");

                    var parameter = t[0] as StringObject;
                    if (parameter == null)
                        throw new PdfException(PdfExceptionCodes.INVALID_CONTENT, $"Expected a string but {t[0].ToString()} is found");
                                    
                    return new TextOperator(parameter.Value);
                }
            },
            { "Td", (t) => { 
                    if (t.Count != 2)
                        throw new PdfException(PdfExceptionCodes.INVALID_CONTENT, "Unknown graphic objectizer found in strem content");

                    
                    float? x = (t[0] as IntegerObject)?.Value ?? (t[0] as RealObject)?.Value;
                    float? y = (t[1] as IntegerObject)?.Value ?? (t[1] as RealObject)?.Value;
                    if (!x.HasValue || !y.HasValue)
                        throw new PdfException(PdfExceptionCodes.INVALID_CONTENT, $"Expected: real real Td, but {t[0].GetType()} {t[1].GetType()} Td found");
                                    
                    return new TextPositioningOperator(x.Value, y.Value);
                }
            },
             { "Tf", (t) => { 
                    if (t.Count != 2)
                        throw new PdfException(PdfExceptionCodes.INVALID_CONTENT, "Unknown graphic objectizer found in strem content");

                    var font = t[0] as NameObject;
                    var size = t[1] as IntegerObject;
                    if (font == null || size == null)
                        throw new PdfException(PdfExceptionCodes.INVALID_CONTENT, $"Expected: name integer Tf, but {font.GetType()} {size.GetType()} Td found");
                                
                    return new FontOperator(font.Value, size.Value);
                }
            }
        };

        // Table 51 – Operator Categories
        private readonly Dictionary<string, Func<Objectizer, List<PdfObject>, IGraphicObject>> objects = 
            new Dictionary<string, Func<Objectizer, List<PdfObject>, IGraphicObject>>() {
            // Table 107 – Text object operators0
            { "J", (t, tParameters) => {                 
                    if (tParameters.Count != 1)
                         throw new PdfException(PdfExceptionCodes.INVALID_CONTENT, 
                            $"Unknown content found");

                    var lineCap = (tParameters[0] as IntegerObject).Value;
                    return new LineCapObject(lineCap);
                }
            },
            { "BT", (t, tParameters) => {                 
                    if (tParameters.Count != 0)
                         throw new PdfException(PdfExceptionCodes.INVALID_CONTENT, 
                            $"Unknown content found");

                    var textObject = new TextObject();
                    List<PdfObject> parameters = new List<PdfObject>();

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
            List<PdfObject> parameters = new List<PdfObject>();
            var o = reader.NextObject(true);

            while (!reader.IsEOF()) {           
                if (objects.ContainsKey(o.ToString())) {
                    objs.Add(objects[o.ToString()].Invoke(reader, parameters));
                    parameters.Clear();
                }
                else 
                    parameters.Add(o);
                
                if (!reader.IsEOF())
                    o = reader.NextObject(true);                
            }            

            return objs;              
        }
    }
}