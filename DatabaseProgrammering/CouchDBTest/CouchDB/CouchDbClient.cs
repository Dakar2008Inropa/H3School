using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CouchDBTest.CouchDB
{
    public sealed class CouchDbClient : IDisposable
    {
        private readonly HttpClient _http;

        private readonly Uri _dbUri;

        private readonly JsonSerializerOptions _jsonOptions;

        public CouchDbClient(CouchDbOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            if (string.IsNullOrWhiteSpace(options.BaseUri))
                throw new ArgumentException("BaseUri is required.", nameof(options));
            if (string.IsNullOrWhiteSpace(options.DatabaseName))
                throw new ArgumentException("DatabaseName is required.", nameof(options));

            string trimmedBase = options.BaseUri.TrimEnd('/');
            string combined = trimmedBase + "/" + options.DatabaseName;
            _dbUri = new Uri(combined, UriKind.Absolute);

            _http = new HttpClient();
            _http.Timeout = TimeSpan.FromSeconds(30);

            if (!string.IsNullOrWhiteSpace(options.Username) || !string.IsNullOrWhiteSpace(options.Password))
            {
                string user = options.Username ?? string.Empty;
                string pass = options.Password ?? string.Empty;
                string raw = user + ":" + pass;
                string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64);
            }

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<TDocument> CreateAsync<TDocument>(TDocument document, CancellationToken cancellationToken)
            where TDocument : CouchDocument
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            string json = SerializeForCreate(document);
            using StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _dbUri);
            request.Content = content;

            using HttpResponseMessage response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                string details = await ReadSafeAsync(response, cancellationToken).ConfigureAwait(false);
                throw new InvalidOperationException("CouchDB create failed: " + (int)response.StatusCode + " — " + response.ReasonPhrase + " — " + details);
            }

            string payload = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            CouchWriteResponse write = JsonSerializer.Deserialize<CouchWriteResponse>(payload, _jsonOptions) ?? new CouchWriteResponse();

            if (!write.Ok)
                throw new InvalidOperationException("CouchDB create returned non-OK response.");

            document.Id = write.Id;
            document.Rev = write.Rev;
            return document;
        }

        public async Task<TDocument> GetAsync<TDocument>(string id, CancellationToken cancellationToken)
            where TDocument : CouchDocument, new()
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Id is required.", nameof(id));

            Uri uri = new Uri(_dbUri.AbsoluteUri.TrimEnd('/') + "/" + Uri.EscapeDataString(id), UriKind.Absolute);

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            using HttpResponseMessage response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                string details = await ReadSafeAsync(response, cancellationToken).ConfigureAwait(false);
                throw new InvalidOperationException("CouchDB get failed: " + (int)response.StatusCode + " — " + response.ReasonPhrase + " — " + details);
            }

            string payload = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            TDocument doc = JsonSerializer.Deserialize<TDocument>(payload, _jsonOptions);
            if (doc == null)
                throw new InvalidOperationException("Failed to deserialize CouchDB document.");

            return doc;
        }

        public async Task<IReadOnlyList<TDocument>> GetAllAsync<TDocument>(int limit, CancellationToken cancellationToken)
            where TDocument : CouchDocument, new()
        {
            if (limit <= 0)
                limit = 50;

            string url = _dbUri.AbsoluteUri.TrimEnd('/') + "/_all_docs?include_docs=true&limit=" + limit.ToString();
            Uri uri = new Uri(url, UriKind.Absolute);

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            using HttpResponseMessage response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                string details = await ReadSafeAsync(response, cancellationToken).ConfigureAwait(false);
                throw new InvalidOperationException("CouchDB _all_docs failed: " + (int)response.StatusCode + " — " + response.ReasonPhrase + " — " + details);
            }

            string payload = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            AllDocsResult<TDocument> result = JsonSerializer.Deserialize<AllDocsResult<TDocument>>(payload, _jsonOptions);

            if (result == null || result.Rows == null || result.Rows.Count == 0)
                return Array.Empty<TDocument>();

            List<TDocument> docs = result.Rows
                .Where(r => r != null && r.Document != null)
                .Select(r => r.Document)
                .ToList();

            return docs;
        }

        public async Task<TDocument> UpdateAsync<TDocument>(TDocument document, CancellationToken cancellationToken)
            where TDocument : CouchDocument
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            if (string.IsNullOrWhiteSpace(document.Id))
                throw new ArgumentException("Document Id is required.", nameof(document));
            if (string.IsNullOrWhiteSpace(document.Rev))
                throw new ArgumentException("Document Rev is required for update.", nameof(document));

            Uri uri = new Uri(_dbUri.AbsoluteUri.TrimEnd('/') + "/" + Uri.EscapeDataString(document.Id), UriKind.Absolute);

            string json = JsonSerializer.Serialize(document, _jsonOptions);
            using StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, uri);
            request.Content = content;

            using HttpResponseMessage response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                string details = await ReadSafeAsync(response, cancellationToken).ConfigureAwait(false);
                throw new InvalidOperationException("CouchDB update failed: " + (int)response.StatusCode + " — " + response.ReasonPhrase + " — " + details);
            }

            string payload = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            CouchWriteResponse write = JsonSerializer.Deserialize<CouchWriteResponse>(payload, _jsonOptions) ?? new CouchWriteResponse();
            if (!write.Ok)
                throw new InvalidOperationException("CouchDB update returned non-OK response.");

            document.Rev = write.Rev;
            return document;
        }

        public async Task DeleteAsync(string id, string rev, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Id is required.", nameof(id));
            if (string.IsNullOrWhiteSpace(rev))
                throw new ArgumentException("Rev is required for delete.", nameof(rev));

            string url = _dbUri.AbsoluteUri.TrimEnd('/') + "/" + Uri.EscapeDataString(id) + "?rev=" + Uri.EscapeDataString(rev);
            Uri uri = new Uri(url, UriKind.Absolute);

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);
            using HttpResponseMessage response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                string details = await ReadSafeAsync(response, cancellationToken).ConfigureAwait(false);
                throw new InvalidOperationException("CouchDB delete failed: " + (int)response.StatusCode + " — " + response.ReasonPhrase + " — " + details);
            }
        }

        private static async Task<string> ReadSafeAsync(HttpResponseMessage response, CancellationToken ct)
        {
            try
            {
                string text = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(text))
                    return text.Length > 400 ? text.Substring(0, 400) : text;
                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private string SerializeForCreate<TDocument>(TDocument document) where TDocument : CouchDocument
        {
            JsonNode node = JsonSerializer.SerializeToNode(document, _jsonOptions);
            JsonObject obj = node as JsonObject;
            if (obj == null)
            {
                return JsonSerializer.Serialize(document, _jsonOptions);
            }

            if (string.IsNullOrWhiteSpace(document.Id) && obj.ContainsKey("_id"))
            {
                obj.Remove("_id");
            }

            if (string.IsNullOrWhiteSpace(document.Rev) && obj.ContainsKey("_rev"))
            {
                obj.Remove("_rev");
            }

            return obj.ToJsonString();
        }

        public void Dispose()
        {
            _http.Dispose();
        }
    }
}