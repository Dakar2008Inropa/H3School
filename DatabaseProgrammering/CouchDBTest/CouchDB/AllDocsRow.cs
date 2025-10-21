using System.Text.Json.Serialization;

namespace CouchDBTest.CouchDB
{
    public sealed class AllDocsRow<TDocument>
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public object Value { get; set; } = new object();

        [JsonPropertyName("doc")]
        public TDocument Document { get; set; } = default!;
    }
}