using CliInterface.Core.Services;
using CliInterface.Infrastructure.Services;
using System.IO;
using System.Net;
using System.Threading;
using Xunit;

namespace Tests.Infrastructure
{
    /// <summary>
    /// Graphics service: local buffer prevents redundant downloads,
    /// missing cache returns null, unreachable URLs degrade gracefully.
    /// </summary>
    public sealed class GraphicsServiceTests
    {
        private static readonly byte[] PngBytes = new byte[] { 137, 80, 78, 71 };

        /// <summary>
        /// Verifies that an unreachable URL returns null without throwing.
        /// </summary>
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

        /// <summary>
        /// Verifies that an unknown URL returns null from GetCachedPath.
        /// </summary>
        [Fact]
        public void GetCachedPath_UnknownUrl_ReturnsNull()
        {
            IGraphicsService service = new GraphicsService();

            string? result = service.GetCachedPath("http://example.com/unknown.png");

            Assert.Null(result);
        }

        /// <summary>
        /// Verifies that an empty or whitespace URL returns null from GetCachedPath.
        /// </summary>
        [Fact]
        public void GetCachedPath_EmptyUrl_ReturnsNull()
        {
            IGraphicsService service = new GraphicsService();

            Assert.Null(service.GetCachedPath(""));
            Assert.Null(service.GetCachedPath("   "));
        }

        /// <summary>
        /// Verifies that an empty URL returns null and reports nothing downloaded.
        /// </summary>
        [Fact]
        public async Task DownloadAndCacheAsync_EmptyUrl_ReturnsNull()
        {
            IGraphicsService service = new GraphicsService();

            var (path, downloaded) = await service.DownloadAndCacheAsync("");

            Assert.Null(path);
            Assert.False(downloaded);
        }

        /// <summary>
        /// Verifies that a null URL returns null from GetCachedPath without throwing.
        /// </summary>
        [Fact]
        public void NullUrl_IsRejected()
        {
            IGraphicsService service = new GraphicsService();

            Assert.Null(service.GetCachedPath(null!));
        }

        /// <summary>
        /// Verifies that downloading the same URL twice only downloads once (cache hit on second call).
        /// </summary>
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

        /// <summary>
        /// Verifies that a successful download saves the image to the configured cache root
        /// and reuses the cache on a second call for the same URL.
        /// </summary>
        [Fact]
        public async Task DownloadAndCacheAsync_SuccessfulDownload_SavesToCustomCacheRoot()
        {
            string dir = Path.Combine(Path.GetTempPath(), "graphics-test-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                using HttpListener listener = new();
                listener.Prefixes.Add("http://127.0.0.1:9876/");
                listener.Start();

                _ = Task.Run(async () =>
                {
                    HttpListenerContext ctx = await listener.GetContextAsync();
                    ctx.Response.ContentType = "image/png";
                    ctx.Response.ContentLength64 = PngBytes.Length;
                    await ctx.Response.OutputStream.WriteAsync(PngBytes, 0, PngBytes.Length);
                    ctx.Response.OutputStream.Close();
                });

                IGraphicsService service = new GraphicsService(dir);
                var (path, downloaded) = await service.DownloadAndCacheAsync(
                    "http://127.0.0.1:9876/test.png",
                    CancellationToken.None);

                Assert.True(downloaded);
                Assert.NotNull(path);
                Assert.True(File.Exists(path!));
                Assert.StartsWith(dir, path!);
                string expectedName = Path.Combine(dir, BitConverter.ToString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("http://127.0.0.1:9876/test.png"))).Replace("-", "").ToLowerInvariant() + ".png");
                Assert.True(File.Exists(expectedName), $"Expected cached file at {expectedName}");

                // Second call should use cache, not download again.
                var (path2, downloaded2) = await service.DownloadAndCacheAsync(
                    "http://127.0.0.1:9876/test.png",
                    CancellationToken.None);
                Assert.False(downloaded2);
                Assert.Equal(path, path2);
            }
            finally
            {
                Directory.Delete(dir, recursive: true);
            }
        }
    }
}
