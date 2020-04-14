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

        private static T GetParameter<T>(List<PdfObject> parameters, int index) where T : PdfObject  {
            var parameter = parameters[index] as T;

            if (parameter == null) {
                throw new PdfException(PdfExceptionCodes.INVALID_CONTENT, 
                    $"Expected a {typeof(T)} but {parameters[index].GetType()} with value {parameters[index].ToString()} is found");
            }
            return parameter;
        }

         private static void ExpectedParameters(List<PdfObject> parameters, int index) {            
            if (parameters.Count != index) {
                throw new PdfException(PdfExceptionCodes.INVALID_CONTENT, 
                    "Unknown graphic objectizer found in strem content");
            } 
        }

        private static readonly Dictionary<string, Func<List<PdfObject>, ITextOperator>> textOperators = 
            new Dictionary<string, Func<List<PdfObject>, ITextOperator>>() {
            { "Tj", (t) => {                                     
                    ExpectedParameters(t, 1);

                    var parameter = GetParameter<StringObject>(t, 0);
                                    
                    return new TextOperator(parameter.Value);
                }
            },
            { "Td", (t) => { 
                    ExpectedParameters(t, 2);

                    var x = GetParameter<RealObject>(t, 0);
                    var y = GetParameter<RealObject>(t, 1);
                                    
                    return new TextPositioningOperator(x.FloatValue, y.FloatValue);
                }
            },
             { "Tf", (t) => { 
                    ExpectedParameters(t, 2);

                    var font = GetParameter<NameObject>(t, 0).Value;
                    var size = GetParameter<IntegerObject>(t, 1).IntValue;
                                
                    return new FontOperator(font, size);
                }
            }
        };

        // Table 51 – Operator Categories
        private readonly Dictionary<string, Func<Objectizer, List<PdfObject>, IGraphicObject>> objects = 
            new Dictionary<string, Func<Objectizer, List<PdfObject>, IGraphicObject>>() {
            // Table 107 – Text object operators0
            { "J", (objectizer, parameters) => {                 
                    ExpectedParameters(parameters, 1);

                    var lineCap = GetParameter<IntegerObject>(parameters, 0).IntValue;

                    return new LineCapObject(lineCap);
                }
            },
            { "BT", (objectizer, tParameters) => {                 
                    ExpectedParameters(tParameters, 0);

                    var textObject = new TextObject();
                    List<PdfObject> parameters = new List<PdfObject>();

                    var o = objectizer.NextObject(true);
                    while (o.ToString() != "ET") {
                        if (o is OperatorObject) {
                            if (textOperators.ContainsKey(o.ToString())) {
                                textObject.AddOperator(textOperators[o.ToString()].Invoke(parameters));
                            }

                            parameters.Clear();
                        } else  {
                            parameters.Add(o);
                        }

                        o = objectizer.NextObject(true);
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
                } else {
                    parameters.Add(o);
                }

                if (!reader.IsEOF()) {
                    o = reader.NextObject(true);
                }
            }            

            return objs;              
        }
    }
}