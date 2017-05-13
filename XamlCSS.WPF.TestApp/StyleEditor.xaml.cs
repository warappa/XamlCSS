using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Reflection;
using System.Windows;
using System.Xml;

namespace XamlCSS.WPF.TestApp
{
    /// <summary>
    /// Interaktionslogik für StyleEditor.xaml
    /// </summary>
    public partial class StyleEditor : Window
    {
        public StyleEditor()
        {
            InitializeComponent();

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("XamlCSS.WPF.TestApp.Resources.css.xshd"))
            {
                using (var reader = new XmlTextReader(stream))
                {
                    styleTextBox.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }

            styleTextBox.Text = (Application.Current.FindResource("InternalStyle") as StyleSheet).Content;
        }

        private void styleTextBox_TextChanged(object sender, EventArgs e)
        {
            (Application.Current.FindResource("InternalStyle") as StyleSheet).Content = styleTextBox.Text;
        }
    }
}
