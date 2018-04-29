using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace XamlCSS.XamarinForms.TestApp
{
    [XamlCSS.Linker.Preserve(AllMembers = true)]
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private string test;
        public string Test
        {
            get => test;
            set
            {
                test = value;
                OnPropertyChange();
            }
        }

        private int testInt;
        public int TestInt
        {
            get => testInt;
            set
            {
                testInt = value;
                OnPropertyChange();
            }
        }

        private ObservableCollection<string> testList = new ObservableCollection<string>();
        public ObservableCollection<string> TestList
        {
            get => testList;
            set
            {
                testList = value;
                OnPropertyChange();
            }
        }

        private void OnPropertyChange([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
