using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace XamlCSS.XamarinForms.TestApp
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();

			this.BindingContext = new
			{
				Test = "Hello World from BindingContext!",
				TestList = new List<string> { "a", "b", "c" }
			};

			this.Appearing += MainPage_Appearing;
		}

		private void MainPage_Appearing(object sender, EventArgs e)
		{
			this.Appearing -= MainPage_Appearing;

			SwitchLayout();
		}

		private void SwitchLayout()
		{
			var main = App.Current.MainPage;
			var app = Application.Current as App;
			app.currentStyle = app.currentStyle == app.cssStyle1 ? app.cssStyle2 : app.cssStyle1;

            var styler = new XamarinFormsStyler();
            styler.ApplyStyleAsync(app.currentStyle);
		}


		private void Button_Click(object sender, EventArgs e)
		{
			SwitchLayout();
		}

		private void Button_Click_1(object sender, EventArgs e)
		{
			var b = new Button() { Text = "Abc" };
			b.Clicked += B_Click;
			stack.Children.Add(b);
		}

		private void B_Click(object sender, EventArgs e)
		{
			stack.Children.Remove(sender as Button);
		}
	}
}
