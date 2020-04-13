using System.Collections.Generic;

namespace SharpPDF.Lib {
    // 9.4.1 General
    public class TextObject : IGraphicObject {
        readonly List<ITextOperator> textOperations = new List<ITextOperator>();

        public ITextOperator[] Operations => textOperations.ToArray();

        public void AddLabel(string text) => textOperations.Add(new TextOperator(text));

        internal void SetPosition(int x, int y) => textOperations.Add(new TextPositioningOperator(x, y));

        internal void SetFont(string code, int size) => textOperations.Add(new FontOperator(code, size));

        internal void AddOperator(ITextOperator textOperator) => textOperations.Add(textOperator);

        public override string ToString() => $"BT {string.Join(" ", textOperations)} ET";
    }
}