using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CliInterface.Core.GameSystem;
using CliInterface.Core.Services;
using CliInterface.Infrastructure.Services;
using CliInterface.Presentation;
using Gui.Converters;
using Gui.ViewModels;

namespace Gui
{
    /// <summary>
    /// The Avalonia application class. Assembles dependencies and starts
    /// the main catalog window.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the application by loading XAML resources.
        /// </summary>
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <summary>
        /// Configures the application lifetime after the framework is initialized.
        /// </summary>
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Assemble dependencies.
                ICharacterCatalogService catalogService =
                    new GenshinCharacterCatalogService(
                        "http://localhost:5000/characters",
                        "data/characters.json");
                IGraphicsService graphicsService = new GraphicsService();
                CharacterCatalogPresenter presenter = new(catalogService);

                // Register resources for XAML lookup.
                Resources.Add("GraphicsUrlConverter", new GraphicsUrlConverter(graphicsService));

                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(presenter, graphicsService)
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
