using System;
using System.Collections.Generic;

namespace SharpPDF.Lib {
    // 9.4.1 General
    public class TextObject : IGraphicObject {
        private List<ITextOperator> textOperations = new List<ITextOperator>();

        public TextObject() {
            
        }

        public ITextOperator[] Operations => textOperations.ToArray();

        public void AddLabel(string text) {
            textOperations.Add(new TextOperator(text));
        }

        internal void SetPosition(int x, int y) {
            textOperations.Add(new TextPositioningOperator(x, y));
        }

        public override string ToString() {
            return $"BT {string.Join(" ", textOperations)} ET";
        }

        internal void AddOperator(ITextOperator textOperator)
        {
            textOperations.Add(textOperator);
        }
    }
}