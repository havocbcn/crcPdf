namespace SharpPDF.Lib {
    public interface IPdfObject {
        ObjectType ObjectType { get; }

        IPdfObject[] Childs();
    }
}