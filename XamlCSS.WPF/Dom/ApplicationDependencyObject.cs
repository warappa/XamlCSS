using System.ComponentModel;
using System.Windows;

namespace XamlCSS.WPF.Dom
{
    public class ApplicationDependencyObject : FrameworkElement
    {
        private readonly Application application;
        
        public ApplicationDependencyObject(Application application)
        {
            this.application = application;

            if (application is IStyleSheetHolder app)
            {
                Css.SetStyleSheet(this, app.AttachedStyleSheet);

                if (application is INotifyPropertyChanged npc)
                {
                    npc.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(IStyleSheetHolder.AttachedStyleSheet))
                        {
                            Css.SetStyleSheet(this, app.AttachedStyleSheet);
                        }
                    };
                }
            }
        }
    }
}
