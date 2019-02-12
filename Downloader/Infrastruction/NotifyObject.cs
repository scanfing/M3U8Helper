using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace M3U8Downloader.Infrastruction
{
    public abstract class NotifyObject : INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Methods

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, args);
        }

        protected virtual void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            bool flag = Equals(storage, value);
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                storage = value;
                RaisePropertyChanged(propertyName);
                result = true;
            }
            return result;
        }

        #endregion Methods
    }
}