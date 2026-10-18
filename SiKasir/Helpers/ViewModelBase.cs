using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SiKasir.Helpers
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void Raise([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        protected void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            field = value;
            Raise(name);
        }
    }
}