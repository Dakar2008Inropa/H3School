using System.Text.Json.Serialization;

namespace CouchDBTest.CouchDB
{
    public class CouchDocument
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("_rev")]
        public string Rev { get; set; } = string.Empty;
    }
}