using Xamarin.Forms;
using XamlCSS.Windows.Media;

namespace XamlCSS.XamarinForms.TestApp
{
    public class App : Application
    {
        public App()
        {
            Css.Initialize(this);

            Resources = new ResourceDictionary();
            Resources.Add("testString", "Hello World from StaticResource!");
            // The root page of your application
            MainPage = new MainPage();
        }


        public string cssStyle1 = @"
Button
{
	TextColor: Red;
}
.container
{
	BackgroundColor: Yellow;

    Button
    {
	    TextColor: Red;
    }
}
.jumbo
{
	FontSize: 50;
}
Grid Grid 
{
    Label:nth-of-type(1)
    {
	    Grid.Row: 0;
	    Grid.Column: 1;
	    Text: @Binding Test;
    }
    Label:nth-of-type(2)
    {
	    Grid.Row: 1;
	    Grid.Column: 0;
	    Text: @StaticResource testString;
    }

    Label:nth-of-type(3)
    {
	    Grid.Row: 1;
	    Grid.Column: 1;
	    Text: @DynamicResource testString;
    }
}
.listViewItem Label
{
	FontSize: 50;
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

    Button
    {
	    TextColor: Olive;
    }
}
#thebutton
{
	FontSize: 20;
}
.jumbo
{
	FontSize: 50;
	FontAttributes: Italic;
	HorizontalOptions: Center;
}
Grid Grid 
{
    Label:nth-of-type(1)
    {
	    Grid.Row: 1;
	    Grid.Column: 1;
    }

    Label:nth-of-type(2)
    {
	    Grid.Row: 0;
	    Grid.Column: 0;
    }
}
";
        public string currentStyle;
    }
}
