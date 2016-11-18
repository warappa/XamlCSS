using System.Threading.Tasks;

namespace XamlCSS.XamarinForms.TestApp
{
    public class XamarinFormsStyler 
    {

        public XamarinFormsStyler()
        {
        }

        public async Task ApplyStyleAsync(string css)
        {
            if (string.IsNullOrWhiteSpace(css))
            {
                Css.SetStyleSheet(App.Current, null);
                return;
            }

            try
            {
                var styleSheet = XamlCSS.CssParsing.CssParser.Parse(css);

                Css.SetStyleSheet(App.Current, styleSheet);
            }
            catch
            {
                var styleSheet = XamlCSS.CssParsing.CssParser.Parse("Editor { TextColor: Red }");
                Css.SetStyleSheet(App.Current, styleSheet);
            }
        }
    }
}