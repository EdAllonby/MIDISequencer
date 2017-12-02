using System;
using System.Windows.Input;
using JetBrains.Annotations;

namespace Sequencer.ViewModel.Command
{
    public class RelayCommand : ICommand
    {
        [CanBeNull] private Predicate<object> canExecute;
        [NotNull] private Action<object> execute;

        public RelayCommand([NotNull] Action<object> execute)
            : this(execute, DefaultCanExecute)
        {
        }

        public RelayCommand([NotNull] Action<object> execute, [NotNull] Predicate<object> canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
                CanExecuteChangedInternal += value;
            }

            remove
            {
                CommandManager.RequerySuggested -= value;
                CanExecuteChangedInternal -= value;
            }
        }

        public bool CanExecute(object parameter)
        {
            return canExecute != null && canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            execute(parameter);
        }

        public void OnCanExecuteChanged()
        {
            EventHandler handler = CanExecuteChangedInternal;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public void Destroy()
        {
            canExecute = _ => false;
            execute = _ => { };
        }

        private static bool DefaultCanExecute(object parameter)
        {
            return true;
        }

        private event EventHandler CanExecuteChangedInternal;
    }
}