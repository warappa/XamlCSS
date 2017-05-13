using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace XamlCSS.WPF.TestApp
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private string message;
        public string Message
        {
            get => message;
            set
            {
                message = value;
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
