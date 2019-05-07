using System.ComponentModel;
using System.Runtime.CompilerServices;
using GoogleTranslate.Desktop.Annotations;

namespace GoogleTranslate.Desktop
{
    public class TranslateModel : INotifyPropertyChanged
    {
        private string _translateResult;

        public string TranslateResult
        {
            get => _translateResult;
            set
            {
                _translateResult = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
