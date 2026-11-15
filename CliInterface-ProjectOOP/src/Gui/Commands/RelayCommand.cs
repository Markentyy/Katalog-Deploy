using System;
using System.Windows.Input;

namespace Gui.Commands
{
    /// <summary>
    /// A simple implementation of <see cref="ICommand"/> that delegates
    /// execution to an <see cref="Action"/>.
    /// </summary>
    public sealed class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="execute">The action to execute when the command is invoked.</param>
        public RelayCommand(Action<object?> execute)
        {
            ArgumentNullException.ThrowIfNull(execute);
            _execute = execute;
        }

        /// <summary>
        /// Occurs when changes occur that affect whether the command should execute.
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        /// <returns>True if the command can execute, otherwise false.</returns>
        public bool CanExecute(object? parameter) => true;

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        public void Execute(object? parameter) => _execute(parameter);
    }
}
