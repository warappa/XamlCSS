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
@import ""Resources/baseStyle.scss"";

Button
{
	Foreground: Red;
}
.container
{
	Background: Yellow;
 
    Button
    {
	    Foreground: Brown;
    }
}
.jumbo
{
	FontSize: 50;
}
Grid Grid 
{
    TextBlock:nth-of-type(1)
    {
	    Grid.Row: 0;
	    Grid.Column: 1;
	    Text: #Binding Message;
    }

    TextBlock:nth-of-type(2)
    {
	    Grid.Row: 1;
	    Grid.Column: 0;
	    Text: #StaticResource testString;
    }
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
Grid Grid 
{
    TextBlock:nth-of-type(1)
    {
	    Grid.Row: 1;
	    Grid.Column: 0;
    }

    TextBlock:nth-of-type(2)
    {
	    Grid.Row: 0;
	    Grid.Column: 0;
    }
}
";
        public string currentStyle;

        public App()
        {

            //dynamic t = this;
            //var u = t.Resources;

            
            Css.Initialize();
            InitializeComponent();
        }
    }
}
