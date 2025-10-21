namespace CouchDBTest.CouchDB
{
    public sealed class AllDocsResult<TDocument>
    {
        public int TotalRows { get; set; }
        public int Offset { get; set; }
        public List<AllDocsRow<TDocument>> Rows { get; set; } = new List<AllDocsRow<TDocument>>();
    }
}