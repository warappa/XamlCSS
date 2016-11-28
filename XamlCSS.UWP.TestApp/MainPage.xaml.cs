using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace XamlCSS.UWP.TestApp
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
        public MainPage()
		{   
            this.InitializeComponent();

			this.DataContext = new
			{
				Test = "Test String from DataContext!"
			};

			// SwitchLayout();
		}

		private void SwitchLayout()
		{
			var app = Application.Current as App;

			app.currentStyle = app.currentStyle == app.cssStyle1 ? app.cssStyle2 : app.cssStyle1;

			var sheet = CssParsing.CssParser.Parse(app.currentStyle);

			Css.SetStyleSheet(thegrid, sheet);
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			SwitchLayout();
		}

		private int count = 0;
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
	}
}
