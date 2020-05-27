namespace crcPdf {
    // Table 57
    public class CurrentTransformationMatrixOperator : Operator {
        public float A { get; }
        public float B { get; }
        public float C { get; }
        public float D { get; }
        public float E { get; }
        public float F { get; }
        
        public CurrentTransformationMatrixOperator(float a, float b, float c, float d, float e, float f) {
            this.A = a;
            this.B = b;
            this.C = c;
            this.D = d;
            this.E = e;
            this.F = f;
        }

        public override string ToString() {
            return $"{floatToString(A)} {floatToString(B)} {floatToString(C)} {floatToString(D)} {floatToString(E)} {floatToString(F)} cm";
        }
    }
}