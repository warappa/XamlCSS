using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using XamlCSS.CssParsing;

namespace XamlCSS.XamarinForms.TestApp
{
    public partial class MainPage : ContentPage
    {
        public MainPageViewModel ViewModel => BindingContext as MainPageViewModel;

        public MainPage()
        {
            this.BindingContext = new MainPageViewModel
            {
                Test = "Hello World from BindingContext!",
                TestInt = 10,
                TestList = new ObservableCollection<string> { "a", "b", "c" }
            };

            InitializeComponent();
        }

        private void SwitchLayout()
        {
            var app = Application.Current as App;
            app.currentStyle = app.currentStyle == app.cssStyle1 ? app.cssStyle2 : app.cssStyle1;

            Css.SetStyleSheet(thegrid, CssParser.Parse(app.currentStyle));
        }


        private void Button_Click(object sender, EventArgs e)
        {
            SwitchLayout();
        }

        private void Button_Click_1(object sender, EventArgs e)
        {
            var b = new Button() { Text = "Abc" };
            b.Clicked += B_Click;
            b.IsEnabled = new Random().Next(0, 2) == 0;
            stack.Children.Add(b);
        }

        private void B_Click(object sender, EventArgs e)
        {
            stack.Children.Remove(sender as Button);
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            var button = (sender as Button);
            if (button.Text == "Clicked!")
            {
                button.Text = "Click me";
                ViewModel.Test = "Hello World from BindingContext!";
            }
            else
            {
                button.Text = "Clicked!";
                ViewModel.Test = "...";
            }
        }
    }
}
