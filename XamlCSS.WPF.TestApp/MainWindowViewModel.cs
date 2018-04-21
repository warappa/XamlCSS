using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace XamlCSS.WPF.TestApp
{
    public class MainMenuItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName]string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public string Icon
        {
            get;
            set;
        }

        private string text = null;
        public string Text
        {
            get
            {
                return text;
            }

            set
            {
                text = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<MainMenuItem> subMenuItems = null;
        public ObservableCollection<MainMenuItem> SubMenuItems
        {
            get
            {
                return subMenuItems;
            }

            set
            {
                subMenuItems = value;
                OnPropertyChanged();
            }
        }

        private bool isSubMenuVisible;
        public bool IsSubMenuVisible
        {
            get
            {
                return isSubMenuVisible;
            }

            set
            {
                isSubMenuVisible = value;
                OnPropertyChanged();
            }
        }

        public MainMenuItem Parent
        {
            get;
            set;
        }

        public string TargetUri
        {
            get;
            private set;
        }

        public MainMenuItem() 
        {
        }

        public MainMenuItem(string icon, string text, string targetUri, IEnumerable<MainMenuItem> subMenuItems = null)
        {
            this.Icon = icon;
            this.Text = text;
            this.TargetUri = targetUri;
            this.SubMenuItems = new ObservableCollection<MainMenuItem>(subMenuItems ?? new List<MainMenuItem>());
            foreach (var item in this.SubMenuItems)
            {
                item.Parent = this;
            }
        }
    }

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            Init();
        }

        private List<MainMenuItem> menuItems;
        public List<MainMenuItem> MenuItems
        {
            get { return menuItems; }
            set { menuItems = value; OnPropertyChanged(); }
        }

        private string message;
        public string Message
        {
            get => message;
            set
            {
                message = value;
                OnPropertyChanged();
            }
        }

        private int testInt;
        public int TestInt
        {
            get => testInt;
            set
            {
                testInt = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<string> testList = new ObservableCollection<string>();
        public ObservableCollection<string> TestList
        {
            get => testList;
            set
            {
                testList = value;
                OnPropertyChanged();
            }
        }
        private void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public void Init()
        {
            this.MenuItems = new List<MainMenuItem>{
                new MainMenuItem("a", "Start", "Start"),
                new MainMenuItem("a", "Personen", "Persons"),
                new MainMenuItem("a", "Gebiete", "Territories"),
                new MainMenuItem("a", "Themen", "Topics",
                new[]{
                    new MainMenuItem("a", "Test Sub Menu 1", ""),
                    new MainMenuItem("a", "Test Sub Menu 2", "")
                }),
                new MainMenuItem("a", "Angebote", "Offerings"),
                new MainMenuItem("a", "Bericht", "Report"),
                new MainMenuItem("a", "Planer", "Planning"),
                new MainMenuItem("a", "Backup", "Backup"),
                new MainMenuItem("a", "Styling", "Styling")
            };
        }
    }
}
