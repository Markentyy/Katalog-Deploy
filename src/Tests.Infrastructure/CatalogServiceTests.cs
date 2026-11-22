using CliInterface.Core.GameSystem;
using CliInterface.Infrastructure.Services;
using Xunit;

namespace Tests.Infrastructure
{
    /// <summary>
    /// Catalog service checks: local JSON fallback, case-insensitive lookup.
    /// </summary>
    public sealed class CatalogServiceTests
    {
        private static readonly string FallbackPath = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "../../../../../data/characters.json"));

        /// <summary>
        /// Verifies characters load from the local JSON fallback file.
        /// </summary>
        [Fact]
        public async Task LoadsCharactersFromFallbackFile()
        {
            ICharacterCatalogService service = new GenshinCharacterCatalogService(
                "http://invalid.invalid/api", FallbackPath);

            IReadOnlyList<CharacterStats> characters = await service.GetCharactersAsync();

            Assert.NotEmpty(characters);
            Assert.All(characters, c => Assert.False(string.IsNullOrWhiteSpace(c.Name)));
        }

        /// <summary>
        /// Verifies case-insensitive character lookup by name.
        /// </summary>
        [Fact]
        public async Task FindCharacterByNameCaseInsensitive()
        {
            ICharacterCatalogService service = new GenshinCharacterCatalogService(
                "http://invalid.invalid/api", FallbackPath);

            CharacterStats? character = await service.GetCharacterByNameAsync("diluc");

            Assert.NotNull(character);
            Assert.Equal("Diluc", character!.Name);
            Assert.Equal("Pyro", character.Element);
        }

        /// <summary>
        /// Verifies that an unknown character name returns null.
        /// </summary>
        [Fact]
        public async Task UnknownCharacterReturnsNull()
        {
            ICharacterCatalogService service = new GenshinCharacterCatalogService(
                "http://invalid.invalid/api", FallbackPath);

            CharacterStats? character = await service.GetCharacterByNameAsync("Неіснуючий");

            Assert.Null(character);
        }

        /// <summary>
        /// Verifies that an empty or whitespace API URL throws ArgumentException.
        /// </summary>
        [Fact]
        public void NullApiUrl_IsRejected()
        {
            Assert.Throws<ArgumentException>(() =>
                new GenshinCharacterCatalogService("", FallbackPath));
            Assert.Throws<ArgumentException>(() =>
                new GenshinCharacterCatalogService("   ", FallbackPath));
        }

        /// <summary>
        /// Verifies that an empty or whitespace fallback path throws ArgumentException.
        /// </summary>
        [Fact]
        public void NullFallbackPath_IsRejected()
        {
            Assert.Throws<ArgumentException>(() =>
                new GenshinCharacterCatalogService("http://example.com/api", ""));
        }
    }
}
