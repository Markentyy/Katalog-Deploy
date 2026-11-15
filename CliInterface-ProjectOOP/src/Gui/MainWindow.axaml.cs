using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Gui.ViewModels;
using System;

namespace Gui
{
    /// <summary>
    /// The main catalog window: displays characters, search input and results.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <summary>
        /// Called when the window is opened; starts loading characters.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            if (DataContext is MainWindowViewModel viewModel)
                _ = viewModel.LoadCharactersAsync();
        }
    }
}
