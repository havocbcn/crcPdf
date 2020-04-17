using System;

namespace SharpPDF.Lib {
    [Flags] 
    public enum Compression {
        None = 0,
        Compress = 1,
        Optimize = 2
    }
}