using Prism.Mvvm;

namespace DeepVision.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "DeepVision";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public MainWindowViewModel()
        {

        }
    }
}
