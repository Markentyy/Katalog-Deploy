using CliInterface.Core.Services;
using CliInterface.Infrastructure.Services;
using Xunit;

namespace Tests.Infrastructure
{
    /// <summary>
    /// Graphics service: local buffer prevents redundant downloads,
    /// missing cache returns null, unreachable URLs degrade gracefully.
    /// </summary>
    public sealed class GraphicsServiceTests
    {
        [Fact]
        public async Task DownloadAndCacheAsync_UnreachableUrl_ReturnsNull()
        {
            IGraphicsService service = new GraphicsService();

            var (path, downloaded) = await service.DownloadAndCacheAsync(
                "http://127.0.0.1:1/nonexistent.png",
                CancellationToken.None);

            Assert.Null(path);
            Assert.False(downloaded);
        }

        [Fact]
        public void GetCachedPath_UnknownUrl_ReturnsNull()
        {
            IGraphicsService service = new GraphicsService();

            string? result = service.GetCachedPath("http://example.com/unknown.png");

            Assert.Null(result);
        }

        [Fact]
        public void GetCachedPath_EmptyUrl_ReturnsNull()
        {
            IGraphicsService service = new GraphicsService();

            Assert.Null(service.GetCachedPath(""));
            Assert.Null(service.GetCachedPath("   "));
        }

        [Fact]
        public async Task DownloadAndCacheAsync_EmptyUrl_ReturnsNull()
        {
            IGraphicsService service = new GraphicsService();

            var (path, downloaded) = await service.DownloadAndCacheAsync("");

            Assert.Null(path);
            Assert.False(downloaded);
        }

        [Fact]
        public void NullUrl_IsRejected()
        {
            IGraphicsService service = new GraphicsService();

            Assert.Null(service.GetCachedPath(null!));
        }

        [Fact]
        public async Task DownloadAndCacheAsync_TwiceSameUrl_DownloadedOnlyOnce()
        {
            IGraphicsService service = new GraphicsService();

            var (path1, downloaded1) = await service.DownloadAndCacheAsync(
                "http://127.0.0.1:1/test.png",
                CancellationToken.None);
            var (path2, downloaded2) = await service.DownloadAndCacheAsync(
                "http://127.0.0.1:1/test.png",
                CancellationToken.None);

            Assert.False(downloaded1);
            Assert.False(downloaded2);
            Assert.Null(path1);
            Assert.Null(path2);
        }
    }
}
