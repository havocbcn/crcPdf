using System;
using System.Collections.Generic;
using System.IO;

namespace crcPdf {
    // 9 Text
    public class PageOperator {
        private readonly Objectizer reader;

        public PageOperator(byte[] content) {
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
                    $"Unknown operator found in stream content {String.Join(',', parameters)}");
            } 
        }
        
        private static readonly Dictionary<string, Func<List<PdfObject>, Operator>> textOperators = 
            new Dictionary<string, Func<List<PdfObject>, Operator>> {
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
                    return new TextPositioningOperator(x.Value, y.Value);
                }
            },
            { "Tm", (t) => { 
                    ExpectedParameters(t, 6);
                    var a = GetParameter<RealObject>(t, 0).Value;
                    var b = GetParameter<RealObject>(t, 1).Value;
                    var c = GetParameter<RealObject>(t, 2).Value;
                    var d = GetParameter<RealObject>(t, 3).Value;
                    var e = GetParameter<RealObject>(t, 4).Value;
                    var f = GetParameter<RealObject>(t, 5).Value;
                    return new TextMatrixOperation(a, b, c, d, e, f);
                }
            },
            { "Tf", (t) => { 
                    ExpectedParameters(t, 2);
                    var font = GetParameter<NameObject>(t, 0).Value;
                    var size = GetParameter<RealObject>(t, 1).Value;                                
                    return new FontOperator(font, size);
                }
            },
            // Table 107 – Text object operators0
            { "J", (parameters) => {                 
                    ExpectedParameters(parameters, 1);
                    var lineCap = GetParameter<IntegerObject>(parameters, 0).IntValue;
                    return new LineCapOperator((LineCapStyle)lineCap);
                }
            },
            // Table 74 – Colour Operators
            { "rg", (t) => { 
                    ExpectedParameters(t, 3);
                    var r = GetParameter<RealObject>(t, 0).Value;
                    var g = GetParameter<RealObject>(t, 1).Value;
                    var b = GetParameter<RealObject>(t, 2).Value;                                
                    return new NonStrokingColourOperator(r, g, b);
                }
            },
        };

        // Table 51 – Operator Categories
        private readonly Dictionary<string, Func<Objectizer, List<PdfObject>, Operator>> pageOperators = 
            new Dictionary<string, Func<Objectizer, List<PdfObject>, Operator>> {
            // Table 107 – Text object operators0
            { "J", (objectizer, parameters) => {                 
                    ExpectedParameters(parameters, 1);
                    var lineCap = GetParameter<IntegerObject>(parameters, 0).IntValue;
                    return new LineCapOperator((LineCapStyle)lineCap);
                }
            },
            // Table 74 – Colour Operators
            { "rg", (objectizer, parameters) => { 
                    ExpectedParameters(parameters, 3);
                    var r = GetParameter<RealObject>(parameters, 0).Value;
                    var g = GetParameter<RealObject>(parameters, 1).Value;
                    var b = GetParameter<RealObject>(parameters, 2).Value;                                
                    return new NonStrokingColourOperator(r, g, b);
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
                                parameters.Clear();
                            } else {
                                throw new PdfException(PdfExceptionCodes.INVALID_CONTENT, 
                                    $"Unknown graphic objectizer found in stream content: {o}");
                            }
                        } else  {
                            parameters.Add(o);
                        }

                        o = objectizer.NextObject(true);
                    }

                    return textObject;
                }
            },
            { "re", (objectizer, tParameters) => {                 
                    ExpectedParameters(tParameters, 4);

                    var x = GetParameter<RealObject>(tParameters, 0).Value;
                    var y = GetParameter<RealObject>(tParameters, 1).Value;
                    var width = GetParameter<RealObject>(tParameters, 2).Value;
                    var height = GetParameter<RealObject>(tParameters, 3).Value;

                    return new RectangleOperator(x, y, width, height);
                }
            },
            { "f", (objectizer, tParameters) => {                 
                    ExpectedParameters(tParameters, 0);                    
                    return new FillOperator();
                }
            },
            { "F", (objectizer, tParameters) => {                 
                    ExpectedParameters(tParameters, 0);                    
                    return new FillOperator();
                }
            },
            { "S", (objectizer, tParameters) => {                 
                    ExpectedParameters(tParameters, 0);                    
                    return new StrokeOperator();
                }
            },
            { "Do", (objectizer, tParameters) => {    
                    ExpectedParameters(tParameters, 1);
                    var image = GetParameter<NameObject>(tParameters, 0).Value;
                    return new ImageOperator(image);
                }
            },
            // Table 57 Graphics State Operators
            { "q", (objectizer, tParameters) => {    
                    ExpectedParameters(tParameters, 0);
                    return new SaveGraphOperator();
                }
            },
            { "Q", (objectizer, tParameters) => {    
                    ExpectedParameters(tParameters, 0);
                    return new RestoreGraphOperator();
                }
            },
            { "cm", (objectizer, parameters) => {    
                    ExpectedParameters(parameters, 6);
                    var a = GetParameter<RealObject>(parameters, 0).Value;
                    var b = GetParameter<RealObject>(parameters, 1).Value;
                    var c = GetParameter<RealObject>(parameters, 2).Value;
                    var d = GetParameter<RealObject>(parameters, 3).Value;
                    var e = GetParameter<RealObject>(parameters, 4).Value;
                    var f = GetParameter<RealObject>(parameters, 5).Value;
                    return new CurrentTransformationMatrixOperator(a, b, c, d, e, f);
                }
            },
        };

        internal List<Operator> ReadObjects() {
            List<Operator> objs = new List<Operator>();
            List<PdfObject> parameters = new List<PdfObject>();
                            
            while (!reader.IsEOF()) {
                var o = reader.NextObject(true);

                if (pageOperators.ContainsKey(o.ToString())) {
                    objs.Add(pageOperators[o.ToString()].Invoke(reader, parameters));
                    parameters.Clear();
                } else {
                    parameters.Add(o);
                }
            }
            
            return objs;              
        }
    }
}