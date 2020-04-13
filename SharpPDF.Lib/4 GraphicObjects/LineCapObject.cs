namespace SharpPDF.Lib {
    // 8.4.3.3 Line Cap Style
    public class LineCapObject : IGraphicObject {
        public LineCapObject(int lineCap) {
            LineCap = lineCap;
        }

        public int LineCap { get; }

        public override string ToString() => $"{LineCap} J";
    }
}