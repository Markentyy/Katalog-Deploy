using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using CliInterface.Core.Services;

namespace Gui.Converters
{
    /// <summary>
    /// Converts a character graphics URL to a cached <see cref="Bitmap"/>
    /// for display in the UI. Returns null when no URL or no cached path
    /// is available.
    /// </summary>
    public sealed class GraphicsUrlConverter : IValueConverter
    {
        private readonly IGraphicsService _graphicsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsUrlConverter"/> class.
        /// </summary>
        /// <param name="graphicsService">The graphics service for cache lookups.</param>
        public GraphicsUrlConverter(IGraphicsService graphicsService)
        {
            ArgumentNullException.ThrowIfNull(graphicsService);
            _graphicsService = graphicsService;
        }

        /// <summary>
        /// Converts a graphics URL to a cached <see cref="Bitmap"/>.
        /// </summary>
        /// <param name="value">The graphics URL.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">Optional parameter.</param>
        /// <param name="culture">The culture info.</param>
        /// <returns>The cached bitmap, or null when not available.</returns>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string url || string.IsNullOrWhiteSpace(url))
                return null;

            string? cached = _graphicsService.GetCachedPath(url);
            if (cached is null)
                return null;

            return new Bitmap(cached);
        }

        /// <summary>
        /// ConvertBack is not supported.
        /// </summary>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
