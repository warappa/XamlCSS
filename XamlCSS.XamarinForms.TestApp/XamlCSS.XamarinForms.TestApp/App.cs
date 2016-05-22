using Xamarin.Forms;
using XamlCSS.Windows.Media;

namespace XamlCSS.XamarinForms.TestApp
{
	public class App : Application
	{
		public App()
		{
			// The root page of your application
			MainPage = new MainPage();

			VisualTreeHelper.Initialize(this);
			Css.Initialize();
		}
		

		public string cssStyle1 = @"
Button
{
	TextColor: Red;
}
.container
{
	BackgroundColor: Yellow;
}
.container Button
{
	TextColor: Green;
}
.jumbo
{
	FontSize: 50;
}
Grid Grid Label:nth-of-type(1)
{
	Grid.Row: 0;
	Grid.Column: 1;
	Text: {Binding Test};
}
Grid Grid Label:nth-of-type(2)
{
	Grid.Row: 1;
	Grid.Column: 0;
	Text: {StaticResource testString};
}
";

		public string cssStyle2 = @"
ContentPage
{
	BackgroundColor: #333333;
	TextColor: #ffffff;
}
Button
{
	TextColor: Red;
	HeightRequest: 40;
	WidthRequest: 100;
}
.container
{
	TextColor: #aaaaaa;
}
.container Button
{
	TextColor: Olive;
}
#thebutton
{
	FontSize: 30;
}
.jumbo
{
	FontSize: 50;
	FontAttributes: Italic;
	HorizontalOptions: Center;
}
Grid Grid Label:nth-of-type(1)
{
	Grid.Row: 1;
	Grid.Column: 1;
}
Grid Grid Label:nth-of-type(2)
{
	Grid.Row: 0;
	Grid.Column: 0;
}
";
		public string currentStyle;
	}
}
