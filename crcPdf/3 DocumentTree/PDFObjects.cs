// This file is part of crcPdf.
// 
// crcPdf is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// crcPdf is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with crcPdf.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace crcPdf {
    public class PDFObjects {
        private readonly HashSet<IndirectObject> objects = new HashSet<IndirectObject>();
        private readonly Dictionary<Guid, IndirectObject> guidToIndirect = new Dictionary<Guid, IndirectObject>();
        private readonly Dictionary<IndirectObject, DocumentTree> cache = new Dictionary<IndirectObject, DocumentTree>();
        private int lastNumber;

        internal IndirectObject CacheGuid(Guid guid) 
            => guidToIndirect.ContainsKey(guid) ? guidToIndirect[guid] : null;

        internal IndirectObject CreateIndirectObject(Guid guid) {
            if (guidToIndirect.ContainsKey(guid)) {
                return guidToIndirect[guid];
            }

            var indirect =  new IndirectObject(++lastNumber);
            
            objects.Add(indirect);
            guidToIndirect.Add(guid, indirect);

            return indirect;
        } 

        internal void AddObject(IndirectObject id) {
            objects.Add(id);
            if (id.Number > lastNumber) {
                lastNumber = id.Number + 1;
            }
        }    

        internal void CleanObjects() {
            objects.Clear();
            lastNumber = 0;
        }    

        /// <summary>
        /// From an indirect object -1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj-, 
        /// construct a Document -DocumentCatalog in the example-
        /// </summary>
        public T GetDocument<T>(IndirectObject obj) where T : DocumentTree {
            if (cache.ContainsKey(obj)) {
                return cache[obj] as T;
            }

            var a = (T)Activator.CreateInstance(typeof(T));
            
            cache.Add(obj, a);

            a.Load(this, obj.childs[0]);

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
        public string GetType(IndirectReferenceObject indirectObj) 
            => objects.TryGetValue(indirectObj, out IndirectObject obj) ? GetType(obj) : null;        

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
        public T GetDocument<T>(IndirectReferenceObject indirectObj) where T : DocumentTree {
            IndirectObject obj;
            if (!objects.TryGetValue(indirectObj, out obj)) {
                return default(T);            
            }
            return GetDocument<T>(obj);
        } 

        /// <summary>
        /// From an unknown object: indirect object, indirect reference, dictionary or so on
        /// construct a Document -DocumentCatalog for example- using the objects list
        /// previously loaded
        /// </summary>
        internal T GetDocument<T>(PdfObject obj) where T : DocumentTree {
            if (obj is IndirectObject) {
                return GetDocument<T>(obj as IndirectObject);
            }
            if (obj is IndirectReferenceObject) {
                return GetDocument<T>(obj as IndirectReferenceObject);
            }

            var a = (T)Activator.CreateInstance(typeof(T));            
            a.Load(this, obj);
            return a;
        } 

        /// <summary>
        /// From an unknown object: indirect object, indirect reference, dictionaryobject or so on
        /// transform to T -DictionaryObject for example- using the objects list
        /// previously loaded if necessary.
        /// If is impossible to convert, an error is throw
        /// </summary>
        /// <param name="obj">The resource to obtain the data</param>
        /// <typeparam name="T">Document desired</typeparam>
        /// <returns>The final object transformed to correct type</returns>
        internal T GetObject<T>(PdfObject obj) where T : PdfObject {
            if (obj is IndirectObject) {
                return GetObject<T>(obj as IndirectObject);
            }
            if (obj is IndirectReferenceObject) {
                return GetObject<T>(obj as IndirectReferenceObject);
            }

            return obj as T;
        } 

        internal T GetObject<T>(IndirectObject obj) where T : PdfObject 
            => obj.childs[0] as T;
        
        internal T GetObject<T>(IndirectReferenceObject indirectObj) where T : PdfObject {
            IndirectObject obj;
            if (!objects.TryGetValue(indirectObj, out obj)) {
                throw new PdfException(PdfExceptionCodes.INVALID_CONTENT, $"Impossible to cast");
            }

            return GetObject<T>(obj);
        } 

        private static void Write(Stream stream, string text) {			
			byte[] textByte = Encoding.GetEncoding(1252).GetBytes(text);
			stream.Write(textByte, 0, textByte.Length);
		}

        internal void WriteTo(Stream ms, DocumentCatalog catalog, Compression compression) {   
            Write(ms, "%PDF-1.3\n");

            // if a pdf was loaded, all objects where loaded too
            // now we will go to save a new pdf with new objects
            // and objects will be created from catalog, the catalog
            // will be saved and reference another objects that will
            // trigger the object generation
            CleanObjects();

            // start creating all Documents, obtaining the indirect reference of the catalog
            // will call for indirect references of pageTree, those to page's, and so on. In each
            // step it will save the new Document in this
            var catalogIndirectReference = catalog.IndirectReferenceObject(this);            
            
            // now we can save all generated objects            
            var childPos = new List<long>();
            foreach (var child in objects) {
                childPos.Add(ms.Length);
                ms.Write(child.Save(compression));
            }

            var xrefPos = ms.Length;
            
            Write(ms, $"xref\n0 {objects.Count + 1}\n0000000000 65535 f\n"); // +1 for the free record
            int i = 0;
            foreach (var child in objects) {
                Write(ms, $"{childPos[i++].ToString("D10")} 00000 n\n");
            }

            Write(ms, $"trailer <</Root {catalogIndirectReference} /Size {objects.Count + 1}>>\nstartxref\n{xrefPos}\n%%EOF");
        }
    }
}