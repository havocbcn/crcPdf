using System;
using System.Collections.Generic;

namespace SharpPDF.Lib
{
    internal static class DocumentTreeFactory
    {
        private static readonly Dictionary<string, Func<IndirectObject, SharpPdf, IDocumentTree>> documents = 
            new Dictionary<string, Func<IndirectObject, SharpPdf, IDocumentTree>>()
        {
            { "Catalog", (indirect, pdf) => { return new DocumentCatalog(indirect, pdf); } },
            { "Pages", (indirect, pdf) => { return new DocumentPageTree(indirect, pdf); } },
            { "Page", (indirect, pdf) => { return new DocumentPage(indirect, pdf); } }
        };

        internal static IDocumentTree Analyze(IndirectObject obj, SharpPdf pdf)
        {
            var dic = obj.Childs()[0] as DictionaryObject;            

            if (dic?.Dictionary?.ContainsKey("Type") ?? false)
                return documents[((NameObject)dic.Dictionary["Type"]).Value](obj, pdf);            

            return new Document(pdf, obj);            
        }
    }
}