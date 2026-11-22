using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using CliInterface.Core.GameSystem;
using CliInterface.Core.Services;
using CliInterface.Presentation;
using Gui.Commands;

namespace Gui.ViewModels
{
    /// <summary>
    /// View model for the main catalog window. Holds the character list,
    /// search state and selection, and exposes commands for search and
    /// navigation.
    /// </summary>
    public sealed class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly CharacterCatalogPresenter _presenter;
        private readonly IGraphicsService _graphicsService;

        private string _searchTerm = string.Empty;
        private bool _isLoading;
        private string? _error;
        private CharacterStats? _selectedCharacter;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
        /// </summary>
        /// <param name="presenter">The catalog presenter for data loading.</param>
        /// <param name="graphicsService">The graphics service for image caching.</param>
        public MainWindowViewModel(
            CharacterCatalogPresenter presenter,
            IGraphicsService graphicsService)
        {
            ArgumentNullException.ThrowIfNull(presenter);
            ArgumentNullException.ThrowIfNull(graphicsService);
            _presenter = presenter;
            _graphicsService = graphicsService;

            Characters = new ObservableCollection<CharacterStats>();
            SearchCommand = new RelayCommand(_ => Search());
        }

        /// <summary>
        /// Gets the characters currently displayed in the catalog.
        /// </summary>
        public ObservableCollection<CharacterStats> Characters { get; }

        /// <summary>
        /// Gets or sets the current search term.
        /// </summary>
        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                if (_searchTerm != value)
                {
                    _searchTerm = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FilteredCharacters));
                    OnPropertyChanged(nameof(MatchCount));
                }
            }
        }

        /// <summary>
        /// Gets the characters matching the current search term, or all
        /// characters when the search term is empty.
        /// </summary>
        public IReadOnlyList<CharacterStats> FilteredCharacters =>
            string.IsNullOrWhiteSpace(_searchTerm)
                ? Characters
                : Characters.Where(c =>
                    c.Name.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();

        /// <summary>
        /// Gets the number of matching characters.
        /// </summary>
        public int MatchCount => FilteredCharacters.Count;

        /// <summary>
        /// Gets the total number of loaded characters.
        /// </summary>
        public int TotalCount => Characters.Count;

        /// <summary>
        /// Gets or sets the currently selected character.
        /// </summary>
        public CharacterStats? SelectedCharacter
        {
            get => _selectedCharacter;
            set
            {
                if (_selectedCharacter != value)
                {
                    _selectedCharacter = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets whether a load operation is in progress.
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the last error message, or null when no error occurred.
        /// </summary>
        public string? Error
        {
            get => _error;
            private set
            {
                if (_error != value)
                {
                    _error = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the command that executes the character search.
        /// </summary>
        public ICommand SearchCommand { get; }

        /// <summary>
        /// Gets the cached graphics path for the given character, or null.
        /// </summary>
        /// <param name="character">The character to get the graphics path for.</param>
        /// <returns>The cached graphics path, or null when not cached.</returns>
        public string? GetCachedGraphicsPath(CharacterStats character)
        {
            if (string.IsNullOrWhiteSpace(character.GraphicsUrl))
                return null;
            return _graphicsService.GetCachedPath(character.GraphicsUrl);
        }

        /// <summary>
        /// Loads all characters from the catalog service.
        /// </summary>
        public async Task LoadCharactersAsync()
        {
            IsLoading = true;
            Error = null;
            try
            {
                IReadOnlyList<CharacterStats> characters = await _presenter.LoadCharactersAsync();
                Characters.Clear();
                foreach (CharacterStats character in characters)
                    Characters.Add(character);
            }
            catch (Exception ex)
            {
                Error = $"Could not load characters: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                OnPropertyChanged(nameof(MatchCount));
            }

            // Pre-download and cache graphics for all loaded characters,
            // then notify the UI so card images that were previously
            // unbound (cache not yet available) are re-evaluated.
            var downloadTasks = new List<Task>();
            foreach (CharacterStats character in Characters)
            {
                if (!string.IsNullOrWhiteSpace(character.GraphicsUrl))
                {
                    downloadTasks.Add(_graphicsService.DownloadAndCacheAsync(character.GraphicsUrl));
                }
            }
            if (downloadTasks.Count > 0)
            {
                try
                {
                    await Task.WhenAll(downloadTasks);
                }
                catch
                {
                    // Graphics download failures are non-fatal.
                }
                OnPropertyChanged(nameof(FilteredCharacters));
                OnPropertyChanged(nameof(MatchCount));
            }
        }

        /// <summary>
        /// Executes the character search by filtering the loaded list.
        /// </summary>
        private void Search()
        {
            OnPropertyChanged(nameof(FilteredCharacters));
            OnPropertyChanged(nameof(MatchCount));
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
