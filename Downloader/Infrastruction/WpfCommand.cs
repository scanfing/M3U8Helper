using System;
using System.Windows.Input;

namespace M3U8Downloader.Infrastruction
{
    public class WpfCommand : ICommand
    {
        #region Fields

        private Func<bool> _canexcute;
        private Action _excute;

        #endregion Fields

        #region Constructors

        public WpfCommand(Action excute, Func<bool> canexcute = null)
        {
            _excute = excute;
            _canexcute = canexcute;
        }

        #endregion Constructors

        #region Events

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        #endregion Events

        #region Methods

        public virtual bool CanExecute(object parameter)
        {
            if (_canexcute == null)
                return true;
            else
                return _canexcute.Invoke();
        }

        public virtual void Execute(object parameter)
        {
            _excute?.Invoke();
        }

        #endregion Methods
    }

    public class WpfCommand<T> : WpfCommand
    {
        #region Fields

        private Func<T, bool> _canexcute;
        private Action<T> _excute;

        #endregion Fields

        #region Constructors

        public WpfCommand(Action<T> excute, Func<T, bool> canexcute = null) : base(null, null)
        {
            _excute = excute;
            _canexcute = canexcute;
        }

        #endregion Constructors

        #region Methods

        public override bool CanExecute(object parameter)
        {
            if (_canexcute == null)
                return true;
            else
                return _canexcute.Invoke((T)parameter);
        }

        public override void Execute(object parameter)
        {
            _excute?.Invoke((T)parameter);
        }

        #endregion Methods
    }
}