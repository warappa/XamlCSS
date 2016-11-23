namespace XamlCSS.XamarinForms.TestApp.UWP
{
	public sealed partial class MainPage
	{
		public MainPage()
        {
            Css.Initialize();

            this.InitializeComponent();

            LoadApplication(new TestApp.App());
		}
	}
}
