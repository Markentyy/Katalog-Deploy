using System.Security.Cryptography;
using System.Text;
using CliInterface.Core.Services;

namespace CliInterface.Infrastructure.Services
{
    /// <summary>
    /// Downloads character artwork from a URL and caches it locally.
    /// </summary>
    /// <remarks>
    /// Before each download, the local cache is checked: if the file already
    /// exists, it is reused and no HTTP request is made.
    /// </remarks>
    public sealed class GraphicsService : IGraphicsService
    {
        private static readonly HttpClient HttpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        /// <summary>
        /// Gets the root directory where cached graphics are stored.
        /// </summary>
        public string CacheRoot { get; }

        private readonly string _cacheRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsService"/> class.
        /// </summary>
        /// <param name="cacheRoot">Optional override for the cache directory.</param>
        public GraphicsService(string? cacheRoot = null)
        {
            _cacheRoot = cacheRoot ?? DefaultCacheRoot();
            CacheRoot = _cacheRoot;
            Directory.CreateDirectory(_cacheRoot);
        }

        /// <inheritdoc />
        public async Task<(string? Path, bool Downloaded)> DownloadAndCacheAsync(string url, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(url))
                return (null, false);

            string? cached = GetCachedPath(url);
            if (cached is not null)
                return (cached, false);

            string? downloaded = await DownloadAsync(url, cancellationToken);
            return (downloaded, downloaded is not null);
        }

        /// <inheritdoc />
        public string? GetCachedPath(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            string fileName = HashUrl(url) + ".png";
            string path = Path.Combine(_cacheRoot, fileName);
            return File.Exists(path) ? path : null;
        }

        private async Task<string?> DownloadAsync(string url, CancellationToken cancellationToken)
        {
            try
            {
                using HttpResponseMessage response = await HttpClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();

                byte[] imageBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                string fileName = HashUrl(url) + ".png";
                string path = Path.Combine(_cacheRoot, fileName);
                File.WriteAllBytes(path, imageBytes);
                return path;
            }
            catch (HttpRequestException)
            {
                return null;
            }
            catch (TaskCanceledException)
            {
                return null;
            }
        }

        private static string HashUrl(string url)
        {
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(url));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        private static string DefaultCacheRoot()
        {
            string baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appDir = Path.Combine(baseDir, "DataPersistence", "graphics");
            Directory.CreateDirectory(appDir);
            return appDir;
        }
    }
}
