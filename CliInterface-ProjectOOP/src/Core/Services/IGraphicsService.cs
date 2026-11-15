namespace CliInterface.Core.Services
{
    /// <summary>
    /// Abstraction for downloading and caching character graphics.
    /// </summary>
    /// <remarks>
    /// The business layer (Core) does not know about HTTP or the file system.
    /// Implementations live in Infrastructure.
    /// </remarks>
    public interface IGraphicsService
    {
        /// <summary>
        /// Downloads the image at <paramref name="url"/> and saves it locally, or returns the cached path.
        /// Returns the local file path and whether the image was newly downloaded (not taken from cache).
        /// </summary>
        /// <param name="url">The source image URL.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task<(string? Path, bool Downloaded)> DownloadAndCacheAsync(string url, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the local cached file path for <paramref name="url"/>, or null when not cached.
        /// </summary>
        /// <param name="url">The source image URL.</param>
        string? GetCachedPath(string url);

        /// <summary>
        /// Gets the root directory where cached graphics are stored.
        /// </summary>
        string CacheRoot { get; }
    }
}
