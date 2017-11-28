using System;
using System.Windows;
using System.Windows.Controls;

namespace XamlCSS.WPF.TestApp
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel ViewModel => DataContext as MainWindowViewModel;

        public MainWindow()
        {
            this.DataContext = new MainWindowViewModel
            {
                Message = "Hello World from DataContext!"
            };

            InitializeComponent();
        }

        private void SwitchLayout()
        {
            var app = Application.Current as App;

            var currentStyle = (app.Resources["InternalStyle"] as StyleSheet).Content;

            (app.Resources["InternalStyle"] as StyleSheet).Content = currentStyle == app.cssStyle1 ? app.cssStyle2 : app.cssStyle1;

            if (styleEditor != null)
            {
                styleEditor.Close();
                styleEditor = null;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SwitchLayout();
        }

        private int count = 0;
        private StyleEditor styleEditor;

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var b = new Button() { Content = "Abc" + count++, Name = "B" + Guid.NewGuid().ToString("N") };
            b.Click += B_Click;
            stack.Children.Add(b);
        }

        private void B_Click(object sender, RoutedEventArgs e)
        {
            stack.Children.Remove(sender as Button);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if ((string)button.Content == "Click me")
            {
                button.Content = "Clicked!";
                ViewModel.Message = "...";
            }
            else
            {
                button.Content = "Click me";
                ViewModel.Message = "Hello World from DataContext!";
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (styleEditor == null ||
                styleEditor.IsVisible == false)
            {
                styleEditor = new StyleEditor();

                styleEditor.Show();
            }
            else
            {
                styleEditor.Close();
                styleEditor = null;
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            this.ToggleClass("light dark");
            //this.runCss3.ToggleClass("fa-css3");
        }
    }
}
