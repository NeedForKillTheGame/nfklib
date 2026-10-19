using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using demowebapi.Models;
using demowebapi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace demowebapi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class DemoController : ControllerBase
    {
        public const string HttpClientName = "demo";
        public const long MaxDemoSize = 64 * 1024 * 1024;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DemoCache _cache;
        private readonly DemoSourceOptions _source;
        private readonly JsonSerializerOptions _json;
        private readonly ILogger<DemoController> _logger;

        public DemoController(IHttpClientFactory httpClientFactory, DemoCache cache, IOptions<DemoSourceOptions> source,
            IOptions<JsonOptions> jsonOptions, ILogger<DemoController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            _source = source.Value;
            _json = jsonOptions.Value.JsonSerializerOptions;
            _logger = logger;
        }

        /// <summary>
        /// Parse demo from url
        /// </summary>
        /// <param name="url">Absolute http(s) url of a NFK demo (*.ndm) file</param>
        /// <param name="type">Returned data: empty (summary only) | chat | map | full</param>
        /// <param name="cancellationToken"></param>
        [HttpGet(Name = "GetDemo")]
        [ProducesResponseType(typeof(DemoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status415UnsupportedMediaType)]
        public async Task<IActionResult> Get([FromQuery] string url, [FromQuery] string? type = null, CancellationToken cancellationToken = default)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                return Problem(statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid demo file url",
                    detail: "Query parameter \"url\" must be an absolute http(s) url of a demo file.");
            }
            if (!_source.IsAllowed(uri))
            {
                return Problem(statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid demo file url",
                    detail: $"Demo files from \"{uri.Host}\" are not allowed.");
            }

            var kind = DemoParser.NormalizeType(type);
            var cacheKey = $"url|{uri.AbsoluteUri}|{kind}";
            if (await _cache.GetAsync(cacheKey, cancellationToken) is { } cached)
                return CachedJson(cached);

            byte[] data;
            try
            {
                var client = _httpClientFactory.CreateClient(HttpClientName);
                data = await client.GetByteArrayAsync(uri, cancellationToken);
            }
            catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
            {
                _logger.LogWarning(e, "Could not download demo from {Url}", uri);
                return Problem(statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid demo file url",
                    detail: $"Could not download \"{uri}\": {e.Message}");
            }

            using var stream = new MemoryStream(data, writable: false);
            return await ParseAsync(stream, kind, Path.GetFileName(uri.AbsolutePath), uri.ToString(), cacheKey);
        }

        /// <summary>
        /// Parse demo from file
        /// </summary>
        /// <param name="demofile">NFK demo (*.ndm) file, multipart/form-data</param>
        /// <param name="type">Returned data: empty (summary only) | chat | map | full</param>
        /// <param name="cancellationToken"></param>
        [HttpPost(Name = "PostDemo")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(MaxDemoSize)]
        [ProducesResponseType(typeof(DemoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status415UnsupportedMediaType)]
        public async Task<IActionResult> Post(IFormFile? demofile, [FromQuery] string? type = null, CancellationToken cancellationToken = default)
        {
            // accept the first uploaded file whatever the form field is called (like the old api did)
            var file = demofile ?? (Request.HasFormContentType ? Request.Form.Files.FirstOrDefault() : null);
            if (file is null || file.Length == 0)
            {
                return Problem(statusCode: StatusCodes.Status400BadRequest,
                    title: "Demo file is required",
                    detail: "Upload a demo file as multipart/form-data (field \"demofile\").");
            }

            using var stream = new MemoryStream((int)Math.Min(file.Length, MaxDemoSize));
            await file.CopyToAsync(stream, cancellationToken);
            stream.Position = 0;

            var kind = DemoParser.NormalizeType(type);
            var hash = Convert.ToHexStringLower(SHA256.HashData(stream.GetBuffer().AsSpan(0, (int)stream.Length)));
            var cacheKey = $"sha256|{hash}|{kind}";
            if (await _cache.GetAsync(cacheKey, cancellationToken) is { } cached)
                return CachedJson(cached);

            return await ParseAsync(stream, kind, file.FileName, file.FileName, cacheKey);
        }

        private async Task<IActionResult> ParseAsync(Stream stream, string kind, string? fileName, string source, string cacheKey)
        {
            var size = stream.Length;
            var started = Stopwatch.GetTimestamp();
            byte[] gzip;
            try
            {
                var data = DemoParser.Parse(stream, kind, fileName);
                gzip = GzipJson(data);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Could not parse demo {Source}", source);
                return Problem(statusCode: StatusCodes.Status415UnsupportedMediaType,
                    title: "Bad demo file",
                    detail: e.Message);
            }
            _logger.LogInformation("Parsed demo {Source} ({Size} bytes, type={Type}) in {Elapsed} ms, {Compressed} bytes gzip",
                source, size, kind, (int)Stopwatch.GetElapsedTime(started).TotalMilliseconds, gzip.Length);

            await _cache.SetAsync(cacheKey, gzip);
            return CachedJson(gzip);
        }

        private byte[] GzipJson(DemoDto data)
        {
            using var buffer = new MemoryStream();
            using (var gzip = new GZipStream(buffer, CompressionLevel.Optimal, leaveOpen: true))
            {
                JsonSerializer.Serialize(gzip, data, _json);
            }
            return buffer.ToArray();
        }

        /// <summary>
        /// Sends the cached gzip JSON as is when the client accepts gzip (almost always), otherwise decompressed.
        /// The ETag lets a browser reload (F5) end with 304 without a body.
        /// </summary>
        private IActionResult CachedJson(byte[] gzip)
        {
            var tag = Convert.ToHexStringLower(SHA256.HashData(gzip).AsSpan(0, 16));

            if (AcceptsGzip())
            {
                Response.Headers.ContentEncoding = "gzip";
                Response.Headers.Vary = HeaderNames.AcceptEncoding;
                return File(gzip, "application/json", lastModified: null, entityTag: new EntityTagHeaderValue($"\"{tag}-gzip\""));
            }

            using var source = new GZipStream(new MemoryStream(gzip, writable: false), CompressionMode.Decompress);
            var json = new MemoryStream();
            source.CopyTo(json);
            json.Position = 0;
            return File(json, "application/json", lastModified: null, entityTag: new EntityTagHeaderValue($"\"{tag}\""));
        }

        private bool AcceptsGzip()
        {
            foreach (var encoding in Request.GetTypedHeaders().AcceptEncoding)
            {
                if (encoding.Quality == 0)
                    continue;
                if (encoding.Value.Equals("gzip", StringComparison.OrdinalIgnoreCase) || encoding.Value.Equals("*", StringComparison.Ordinal))
                    return true;
            }
            return false;
        }
    }
}
