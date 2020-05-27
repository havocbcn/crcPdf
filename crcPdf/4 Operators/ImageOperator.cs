namespace crcPdf {    
    public class ImageOperator : Operator {
        public string Code { get; }
        
        public ImageOperator(string code) {
            Code = code;
        }

        public override string ToString() {
            return $"/{Code} Do";
        }
    }
}