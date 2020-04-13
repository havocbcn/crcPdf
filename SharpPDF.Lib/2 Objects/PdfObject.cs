using System.Collections.Generic;

namespace SharpPDF.Lib {
    public abstract class PdfObject {
        internal List<PdfObject> childs = new List<PdfObject>();
        public T[] Childs<T>() where T : PdfObject {
            List<T> lst = new List<T>();

            foreach (var ch in childs) {
                lst.Add((T)ch);
            }
            
            return lst.ToArray();
        }

         public T Child<T>(int pos) where T : PdfObject {
             if (pos >= childs.Count)
                return null;
            return childs[pos] as T;
        }
    }
}