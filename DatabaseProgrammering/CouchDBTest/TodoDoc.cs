using CouchDBTest.CouchDB;
using System.Text.Json.Serialization;

namespace CouchDBTest
{
    public sealed class TodoDoc : CouchDocument
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("isDone")]
        public bool IsDone { get; set; }
    }
}