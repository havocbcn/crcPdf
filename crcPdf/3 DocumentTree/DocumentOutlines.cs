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
using System.Collections.Generic;

namespace crcPdf {
    public class DocumentOutlines : DocumentTree {
        public string Title { get; private set; }
        public DocumentPage Page { get; private set; }
        public float? X  { get; private set; }
        public float? Y { get; private set; }
        public float? Zoom { get; private set; }
        public DocumentOutlines First { get; private set; }
        public DocumentOutlines Last { get; private set; }
        public DocumentOutlines Prev { get; private set; }
        public DocumentOutlines Next { get; private set; }
        public DocumentOutlines Parent { get; private set; }
        public int Count { get; private set; }

        public DocumentOutlines()
        {
        }

        public DocumentOutlines(string title, DocumentPage page, float? x, float? y, float? zoom, params DocumentOutlines[] sons)
        {
            this.Title = title;
            this.Page = page;
            this.X = x;
            this.Y = y;
            this.Zoom = zoom;

            Add(sons);
        }

        public override void Load(PDFObjects pdf, PdfObject pdfObject) { 
            var contents = pdf.GetObject<DictionaryObject>(pdfObject);
            foreach (var value in contents.Dictionary) {
                switch (value.Key) {
                    case "Type":
                        break;                    
                    case "First":
                        First = pdf.GetDocument<DocumentOutlines>(value.Value);
                        break;
                    case "Last":
                        Last = pdf.GetDocument<DocumentOutlines>(value.Value);
                        break;
                    case "Prev":
                        Prev = pdf.GetDocument<DocumentOutlines>(value.Value);
                        break;
                    case "Next":
                        Next = pdf.GetDocument<DocumentOutlines>(value.Value);
                        break;
                    case "Parent":
                        Parent = pdf.GetDocument<DocumentOutlines>(value.Value);
                        break;
                    case "Count":
                        Count = pdf.GetObject<IntegerObject>(value.Value).IntValue;
                        break;
                    case "Title":
                        Title = pdf.GetObject<StringObject>(value.Value).Value;
                        break;
                    case "Dest":
                        var dest = pdf.GetObject<ArrayObject>(value.Value);
                        
                        Page = pdf.GetDocument<DocumentPage>(dest.childs[0]);

                        switch (pdf.GetObject<NameObject>(dest.childs[1]).Value) {
                            case "XYZ":
                                X = GetRealOrNullObject(dest.childs[2]);
                                Y = GetRealOrNullObject(dest.childs[3]);
                                Zoom = GetRealOrNullObject(dest.childs[4]);
                                break;
                            default:
                                throw new PdfException(PdfExceptionCodes.UNKNOWN_ENTRY, $"Outlines contain an unknown Dest entry: {pdf.GetObject<NameObject>(dest.childs[1]).Value}");
                        }

                        break;
                    default:
                        throw new PdfException(PdfExceptionCodes.UNKNOWN_ENTRY, $"Outlines contain an unknown entry: {value.Key}");                        
                }
            }
        }    

        public override void OnSaveEvent(IndirectObject indirectObject, PDFObjects pdfObjects)
        {
            var entries = new Dictionary<string, PdfObject>() { { "Count", new IntegerObject(Count) }};

            if (Parent == null) entries.Add("Type", new NameObject("Outlines"));
            if (Parent != null) {
                entries.Add("Parent", Parent.IndirectReferenceObject(pdfObjects));                
                entries.Add("Dest", new ArrayObject(new List<PdfObject>() {
                    Page.IndirectReferenceObject(pdfObjects),  
                    new NameObject("XYZ"),
                        GetRealOrNullObject(X),
                        GetRealOrNullObject(Y),
                        GetRealOrNullObject(Zoom)
                    }));
            }
            if (First != null) entries.Add("First", First.IndirectReferenceObject(pdfObjects));
            if (Last != null) entries.Add("Last", Last.IndirectReferenceObject(pdfObjects));
            if (Prev != null) entries.Add("Prev", Prev.IndirectReferenceObject(pdfObjects));
            if (Next != null) entries.Add("Next", Next.IndirectReferenceObject(pdfObjects));            
            if (Title != null) entries.Add("Title", new StringObject(Title));            
   
            indirectObject.SetChild(new DictionaryObject(entries));
        }
        
        public void Add(params DocumentOutlines[] sons) {
            if (sons == null || sons.Length == 0)
                return;

            First = sons[0];
            Last = sons[sons.Length-1];
            sons[0].Parent = this;    
            Count = 0;

            for (int i = 0; i < sons.Length; i++)
            {
                sons[i].Parent = this;
                if (i > 0)
                    sons[i].Prev = sons[i-1];

                if (i < sons.Length-1)
                    sons[i].Next = sons[i+1];

                Count += sons[i].Count + 1;
            }
        }

        private PdfObject GetRealOrNullObject(float? value)
        {
            if (value.HasValue)
                return new RealObject(value.Value);
            return new NullObject();
        }

        private float? GetRealOrNullObject(PdfObject obj)
        {
            if (obj is NullObject)
                return null;
            return (obj as RealObject).Value;
        }
    }
}