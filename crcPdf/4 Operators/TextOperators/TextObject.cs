using System;
using System.Collections.Generic;

namespace crcPdf {
    // 9.4.1 General
    public class TextObject : Operator {
        readonly List<Operator> operators = new List<Operator>();


        public Operator[] Operators => operators.ToArray();
        public T Operator<T>(int index) where T: Operator 
            => operators[index] as T;
        

        public void AddLabel(string text) => operators.Add(new TextOperator(text));

        public void SetPosition(int x, int y) => operators.Add(new TextPositioningOperator(x, y));

        public void SetFont(string code, float size) => operators.Add(new FontOperator(code, size));

        internal void AddOperator(Operator textOperator) => operators.Add(textOperator);

        public override string ToString() => $"BT {string.Join(" ", operators)} ET";

        public void SetLineCap(LineCapStyle lineCap) => operators.Add(new LineCapOperator(lineCap));

        public void SetNonStrokingColour(float r, float g, float b) => operators.Add(new NonStrokingColourOperator(r, g, b));
    }
}