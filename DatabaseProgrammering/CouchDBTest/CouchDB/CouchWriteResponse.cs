using System.Text.Json.Serialization;

namespace CouchDBTest.CouchDB
{
    public sealed class CouchWriteResponse
    {
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("rev")]
        public string Rev { get; set; } = string.Empty;
    }
}