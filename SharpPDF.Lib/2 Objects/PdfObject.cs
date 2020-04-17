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
            if (pos >= childs.Count) {
                return null;
            }
            return childs[pos] as T;
        }

        internal void SetChild(PdfObject child) {
            childs.Add(child);
        }

        internal byte[] GetBytes(string str) 
            => System.Text.Encoding.GetEncoding(1252).GetBytes(str);

        public abstract byte[] Save(Compression compression);  

        internal byte[] Join(byte[] a1, byte[] a2, byte[] a3) {
            byte[] bytes = new byte[a1.Length + a2.Length + a3.Length];
            System.Buffer.BlockCopy(a1, 0, bytes, 0, a1.Length);
            System.Buffer.BlockCopy(a2, 0, bytes, a1.Length, a2.Length);
            System.Buffer.BlockCopy(a3, 0, bytes, a1.Length + a2.Length, a3.Length);
            return bytes;
        }

        internal byte[] Join(byte[] a1, byte[] a2) {
            byte[] bytes = new byte[a1.Length + a2.Length];
            System.Buffer.BlockCopy(a1, 0, bytes, 0, a1.Length);
            System.Buffer.BlockCopy(a2, 0, bytes, a1.Length, a2.Length);
            return bytes;
        }
    }
}