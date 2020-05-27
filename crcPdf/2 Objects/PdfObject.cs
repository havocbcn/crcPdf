using System.Collections.Generic;

namespace crcPdf {
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

        public abstract byte[] Save(Compression compression);  

        internal void SetChild(PdfObject child) {
            childs.Add(child);
        }

        internal static byte[] GetBytes(string str) 
            => System.Text.Encoding.GetEncoding(1252).GetBytes(str);


        internal static byte[] Join(byte[] a1, byte[] a2, byte[] a3) {
            byte[] bytes = new byte[a1.Length + a2.Length + a3.Length];
            System.Buffer.BlockCopy(a1, 0, bytes, 0, a1.Length);
            System.Buffer.BlockCopy(a2, 0, bytes, a1.Length, a2.Length);
            System.Buffer.BlockCopy(a3, 0, bytes, a1.Length + a2.Length, a3.Length);
            return bytes;
        }
    }
}