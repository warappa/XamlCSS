using System;
using System.Windows;

namespace XamlCSS.WPF.TestApp
{
	/// <summary>
	/// Interaktionslogik für "App.xaml"
	/// </summary>
	public partial class App : Application
	{
		public string cssStyle1 = @"
Button
{
	Foreground: Red;
}
.container
{
	Background: Yellow;
}
.container Button
{
	Foreground: Brown;
}
.jumbo
{
	FontSize: 50;
}
";

		public string cssStyle2 = @"
Window
{
	Background: #333333;
	Foreground: #ffffff;
}
Button
{
	Foreground: Red;
	Height: 40;
	Width: 100;
}
.container
{
	Background: #aaaaaa;
}
.container Button
{
	Foreground: Brown;
}
#thebutton
{
	FontSize: 30;
}
.jumbo
{
	FontSize: 50;
	FontStyle: Italic;
	HorizontalAlignment: Center;
}
";
		public string currentStyle;

		public App()
		{
			InitializeComponent();

			LoadedDetectionHelper.Initialize();
			
		}
	}
}
