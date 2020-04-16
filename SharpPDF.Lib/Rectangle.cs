using System.Collections.Generic;

namespace SharpPDF.Lib {
    public class Rectangle {
        public float X { get; }
        public float Y { get; }
        public float Width { get; }
        public float Height { get; }

        public Rectangle(float x, float y, float width, float height) {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        internal List<PdfObject> ToArrayObject()
            => new List<PdfObject>() {
                new RealObject(X),
                new RealObject(Y),
                new RealObject(Width),
                new RealObject(Height)
            };
    }
}