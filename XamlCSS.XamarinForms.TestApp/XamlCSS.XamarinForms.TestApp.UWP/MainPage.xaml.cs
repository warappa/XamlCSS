namespace XamlCSS.XamarinForms.TestApp.UWP
{
	public sealed partial class MainPage
	{
		public MainPage()
		{
			this.InitializeComponent();

			LoadApplication(new XamlCSS.XamarinForms.TestApp.App());
			
			Css.Initialize();
		}
	}
}
