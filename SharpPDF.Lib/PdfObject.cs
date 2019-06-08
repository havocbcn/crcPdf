using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SharpPDF.Lib
{
    public interface IPdfObject
    {
        ObjectType Type();

        void Analyze();

        IEnumerable<IPdfObject> Childs();
    }
}