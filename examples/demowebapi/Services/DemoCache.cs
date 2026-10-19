using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace demowebapi.Services
{
    /// <summary>
    /// Two level cache of parsed demos: gzip compressed JSON responses in memory (short lived, survives F5)
    /// and on disk (bounded by total size, least recently used files are evicted).
    /// </summary>
    public sealed class DemoCache
    {
        private const string FileExtension = ".json.gz";

        private readonly DemoCacheOptions _options;
        private readonly IMemoryCache? _memory;
        private readonly ILogger<DemoCache> _logger;
        private readonly SemaphoreSlim _evictionLock = new(1, 1);
        private string? _directory;

        public DemoCache(IOptions<DemoCacheOptions> options, IMemoryCache memoryCache, IHostEnvironment environment, ILogger<DemoCache> logger)
        {
            _options = options.Value;
            _logger = logger;
            _memory = _options.MemorySizeMB > 0 ? memoryCache : null;

            if (!string.IsNullOrWhiteSpace(_options.Directory))
            {
                var directory = Path.GetFullPath(_options.Directory, environment.ContentRootPath);
                try
                {
                    Directory.CreateDirectory(directory);
                    _directory = directory;
                    _logger.LogInformation("Disk cache: {Directory}, limit {Limit} MB; memory cache: {Memory} MB for {Ttl}",
                        directory, _options.MaxDiskSizeMB, _options.MemorySizeMB, _options.MemoryTtl);
                }
                catch (Exception e) when (e is IOException or UnauthorizedAccessException)
                {
                    _logger.LogWarning(e, "Disk cache directory {Directory} is not available, the disk cache is disabled", directory);
                }
            }
        }

        /// <summary>
        /// Returns the cached gzip compressed JSON for the key, or null
        /// </summary>
        public async Task<byte[]?> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            var id = IdFor(key);

            if (_memory != null && _memory.TryGetValue(id, out byte[]? bytes) && bytes != null)
            {
                _logger.LogDebug("Memory cache hit {Id}", id);
                return bytes;
            }

            if (_directory == null)
                return null;

            var path = Path.Combine(_directory, id + FileExtension);
            try
            {
                if (!File.Exists(path))
                    return null;

                bytes = await File.ReadAllBytesAsync(path, cancellationToken);
                // the write time is the "last used" mark for the eviction
                File.SetLastWriteTimeUtc(path, DateTime.UtcNow);
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
                _logger.LogWarning(e, "Could not read the cache file {Path}", path);
                return null;
            }

            _logger.LogDebug("Disk cache hit {Id}", id);
            PutInMemory(id, bytes);
            return bytes;
        }

        /// <summary>
        /// Stores the gzip compressed JSON for the key (memory and disk)
        /// </summary>
        public async Task SetAsync(string key, byte[] bytes)
        {
            var id = IdFor(key);
            PutInMemory(id, bytes);

            if (_directory == null)
                return;

            var path = Path.Combine(_directory, id + FileExtension);
            var temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                // write to a temporary file and move it into place, so readers never see a partial file
                await File.WriteAllBytesAsync(temp, bytes);
                File.Move(temp, path, overwrite: true);
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
                _logger.LogWarning(e, "Could not write the cache file {Path}", path);
                try { File.Delete(temp); } catch (Exception) { /* best effort */ }
                return;
            }

            Evict();
        }

        private void PutInMemory(string id, byte[] bytes)
        {
            _memory?.Set(id, bytes, new MemoryCacheEntryOptions
            {
                Size = bytes.Length,
                SlidingExpiration = _options.MemoryTtl,
            });
        }

        /// <summary>
        /// Deletes the least recently used files while the cache is above its size limit
        /// </summary>
        private void Evict()
        {
            // another request is already doing it
            if (_directory == null || !_evictionLock.Wait(0))
                return;

            try
            {
                var limit = (long)_options.MaxDiskSizeMB * 1024 * 1024;
                var files = new DirectoryInfo(_directory)
                    .EnumerateFiles("*" + FileExtension)
                    .Select(f => (f.FullName, f.Length, f.LastWriteTimeUtc))
                    .ToList();
                var total = files.Sum(f => f.Length);
                if (total <= limit)
                    return;

                var deleted = 0;
                foreach (var file in files.OrderBy(f => f.LastWriteTimeUtc))
                {
                    try
                    {
                        File.Delete(file.FullName);
                        total -= file.Length;
                        deleted++;
                    }
                    catch (Exception e) when (e is IOException or UnauthorizedAccessException)
                    {
                        _logger.LogDebug(e, "Could not delete the cache file {Path}", file.FullName);
                    }
                    if (total <= limit)
                        break;
                }
                _logger.LogInformation("Disk cache above {Limit} MB, deleted {Count} least recently used files", _options.MaxDiskSizeMB, deleted);
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
                _logger.LogWarning(e, "Disk cache eviction failed");
            }
            finally
            {
                _evictionLock.Release();
            }
        }

        private static string IdFor(string key) =>
            Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(key)));
    }
}
