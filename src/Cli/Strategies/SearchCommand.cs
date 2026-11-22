using CliInterface.Cli.Engine;
using CliInterface.Core.Catalog;
using CliInterface.Core.GameSystem;
using CliInterface.Cli.Rendering;

namespace CliInterface.Cli.Strategies
{
    /// <summary>
    /// Searches characters by name substring and prints the matches.
    /// All characters are shown when no search term is given.
    /// </summary>
    public sealed class SearchCommand : ICommand
    {
        private readonly CharacterWorld _world;
        private readonly CharacterTableRenderer _renderer;

        /// <summary>
        /// Gets the command verb.
        /// </summary>
        public string Name => "search";

        /// <summary>
        /// Gets the one line help summary.
        /// </summary>
        public string Summary => "Search characters by name substring.";

        /// <summary>
        /// Gets the detailed usage line.
        /// </summary>
        public string Usage => "search <substring>";

        /// <summary>
        /// Initializes a search command bound to the given world.
        /// </summary>
        /// <param name="world">The character world.</param>
        /// <param name="renderer">The character table renderer.</param>
        public SearchCommand(CharacterWorld world, CharacterTableRenderer renderer)
        {
            ArgumentNullException.ThrowIfNull(world);
            ArgumentNullException.ThrowIfNull(renderer);
            _world = world;
            _renderer = renderer;
        }

        /// <summary>
        /// Searches characters by name substring and prints matches.
        /// </summary>
        /// <param name="cli">The CLI facade for output.</param>
        /// <param name="command">The parsed command line.</param>
        public void Execute(Cli cli, ParsedCommand command)
        {
            command.ExpectArgCount(0, 1, Usage);
            command.ExpectOptions(Usage);

            string substring = command.Args.Count == 0
                ? string.Empty
                : command.RequireArg(0, "a name substring: search <substring>.");

            List<CharacterRow> rows = new();
            foreach (KeyValuePair<string, Character> entry in _world.Characters.All)
            {
                if (entry.Value.Name.Contains(substring, StringComparison.OrdinalIgnoreCase))
                {
                    rows.Add(new CharacterRow(
                        entry.Key,
                        entry.Value,
                        entry.Value.Inventory.Count,
                        _world.BookOf(entry.Key).Count));
                }
            }

            cli.Display(new CharacterTable(_world, rows, _renderer));
        }
    }
}
