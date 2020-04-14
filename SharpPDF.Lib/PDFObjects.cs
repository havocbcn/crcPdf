using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SharpPDF.Lib {
    public class PDFObjects {
        private readonly HashSet<IndirectObject> objects = new HashSet<IndirectObject>();

        private readonly List<IndirectObject> objectsToSave = new List<IndirectObject>();

        private readonly Dictionary<IndirectObject, IDocumentTree> cache = new Dictionary<IndirectObject, IDocumentTree>();

        private int lastNumber = 0;
        internal IndirectObject CreateIndirectObject() {
            var indirect =  new IndirectObject(++lastNumber);
            objectsToSave.Add(indirect);
            return indirect;
        } 

        internal void AddObject(IndirectObject id) {
            objects.Add(id);
            if (id.Number > lastNumber) {
                lastNumber = id.Number + 1;
            }
        }        

        /// <summary>
        /// From an indirect object -1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj-, 
        /// construct a Document -DocumentCatalog in the example-
        /// </summary>
        public T GetDocument<T>(IndirectObject obj) where T : IDocumentTree {
            if (cache.ContainsKey(obj)) {
                return cache[obj] as T;
            }

            var a = (T)Activator.CreateInstance(typeof(T), this, obj.childs[0]);

            cache.Add(obj, a);
            return a;
        } 

        /// <summary>
        /// From an indirect reference, I want to know the type of this object
        /// For example: 1 0 R
        /// and exists 1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj
        /// It will return "Catalog"
        /// If the obj referenced is not a dictionary, or not exists, for example
        /// 1 0 obj [ 1 2 3 ] endobj
        /// type will be null
        /// </summary>
        public string GetType(IndirectReferenceObject indirectObj) {
            IndirectObject obj;
            if (!objects.TryGetValue(indirectObj, out obj)) {
                return null;
            }
            return GetType(obj);
        }

        /// <summary>
        /// From an object reference, I want to know the type of this object
        /// For example: 1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj
        /// It will return "Catalog"
        /// If the obj referenced is not a dictionary, for example
        /// 1 0 obj [ 1 2 3 ] endobj
        /// type will be null
        /// </summary>
        public string GetType(IndirectObject obj) {        
            var dic = obj.Child<DictionaryObject>(0);
            if (dic == null) {
                return null;
            }
            if (dic.Dictionary.ContainsKey("Type")) {
                return GetObject<NameObject>(dic.Dictionary["Type"]).Value;
            }
            return null;
        }

        /// <summary>
        /// From an indirect reference object -1 0 R-, 
        /// construct a Document -DocumentCatalog in the example- using the objects list
        /// previously loaded
        /// </summary>
        public T GetDocument<T>(IndirectReferenceObject indirectObj) where T : IDocumentTree {
            IndirectObject obj;
            if (!objects.TryGetValue(indirectObj, out obj)) {
                return null;            
            }
            return GetDocument<T>(obj);
        } 

        /// <summary>
        /// From an unknown object: indirect object, indirect reference, dictionary or so on
        /// construct a Document -DocumentCatalog for example- using the objects list
        /// previously loaded
        /// </summary>
        public T GetDocument<T>(PdfObject obj) where T : IDocumentTree {
            if (obj is IndirectObject) {
                return GetDocument<T>(obj as IndirectObject);
            }
            if (obj is IndirectReferenceObject) {
                return GetDocument<T>(obj as IndirectReferenceObject);
            }

            var a = (T)Activator.CreateInstance(typeof(T), this, obj);
            return a;
        } 

        /// <summary>
        /// From an unknown object: indirect object, indirect reference, dictionaryobject or so on
        /// transform to T -DictionaryObject for example- using the objects list
        /// previously loaded if necessary
        /// if is impossible to convert, an error is throw
        /// </summary>
        public T GetObject<T>(PdfObject obj) where T : PdfObject {
            if (obj is IndirectObject) {
                return GetObject<T>(obj as IndirectObject);
            }
            if (obj is IndirectReferenceObject) {
                return GetObject<T>(obj as IndirectReferenceObject);
            }

            return obj as T;
        } 

        public T GetObject<T>(IndirectObject obj) where T : PdfObject {
            return obj.childs[0] as T;
        } 
        public T GetObject<T>(IndirectReferenceObject indirectObj) where T : PdfObject {
            IndirectObject obj;
            if (!objects.TryGetValue(indirectObj, out obj)) {
                throw new PdfException(PdfExceptionCodes.INVALID_CONTENT, $"Impossible to cast");
            }

            return GetObject<T>(obj);
        } 

        public void WriteTo(MemoryStream ms, DocumentCatalog catalog) {   
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("%PDF-1.6");
            List<int> childPos = new List<int>();

            var catalogIndirectReference = catalog.IndirectReferenceObject;            
            
            foreach (var child in objectsToSave) {
                childPos.Add(sb.Length);
                sb.Append(child.ToString());
            }

            int xrefPos = sb.Length;
            
            sb.AppendLine($"xref\n0 {objectsToSave.Count + 1}\n0000000000 65535 f"); // +1 for the free record
            int i = 0;
            foreach (var child in objectsToSave) {
                sb.AppendLine($"{childPos[i++].ToString("D10")} 00000 n");
            }

            sb.Append($"trailer <</Root {catalogIndirectReference} /Size {objectsToSave.Count + 1}>>\nstartxref\n{xrefPos}\n%%EOF");

            byte[] existingData = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            ms.Write(existingData, 0, existingData.Length); 
        }
    }
}