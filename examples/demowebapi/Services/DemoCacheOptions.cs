namespace demowebapi.Services
{
    /// <summary>
    /// Settings of the parsed demo cache ("DemoCache" configuration section).
    /// Environment variables: DemoCache__Directory, DemoCache__MaxDiskSizeMB, DemoCache__MemorySizeMB, DemoCache__MemoryTtl.
    /// </summary>
    public sealed class DemoCacheOptions
    {
        public const string SectionName = "DemoCache";

        /// <summary>
        /// Directory of the disk cache (gzip compressed JSON responses). A relative path is resolved against
        /// the content root. Empty disables the disk cache.
        /// </summary>
        public string? Directory { get; set; } = "cache";

        /// <summary>
        /// Maximum total size of the disk cache in megabytes; the least recently used files are deleted above it.
        /// </summary>
        public int MaxDiskSizeMB { get; set; } = 512;

        /// <summary>
        /// Size of the in-memory cache in megabytes (compressed responses). 0 disables it.
        /// </summary>
        public int MemorySizeMB { get; set; } = 32;

        /// <summary>
        /// How long a response stays in memory after its last use.
        /// </summary>
        public TimeSpan MemoryTtl { get; set; } = TimeSpan.FromMinutes(5);
    }

    /// <summary>
    /// Settings of the demo download ("DemoSource" configuration section).
    /// </summary>
    public sealed class DemoSourceOptions
    {
        public const string SectionName = "DemoSource";

        /// <summary>
        /// Hosts the "url" query parameter may point to (e.g. "stats.needforkill.ru"). Empty allows any host.
        /// </summary>
        public string[] AllowedHosts { get; set; } = Array.Empty<string>();

        public bool IsAllowed(Uri uri)
        {
            if (AllowedHosts.Length == 0)
                return true;

            foreach (var host in AllowedHosts)
            {
                if (string.Equals(host, uri.Host, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
