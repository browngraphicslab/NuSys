using System.ComponentModel;

namespace NuSysApp
{
    /// <summary>
    /// This serves as a base class to all ViewModels and allows calls to RaisePropertyChanged.
    /// </summary>
    public abstract class BaseINPC : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            //if (handler != null)
            //{
            //    handler(this, new PropertyChangedEventArgs(propertyName));
            //}
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
