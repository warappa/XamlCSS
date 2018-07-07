using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace XamlCSS.WPF.Dom
{
    public class ApplicationDependencyObject : FrameworkElement, IStyleSheetHolder
    {
        private readonly Application application;
        private readonly DispatcherTimer dispatcherTimer;

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

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(100);
            dispatcherTimer.Tick += (s, e) =>
            {
                CheckForSharedStyleSheet(application);
            };

            var isInDesigner = DesignerProperties.GetIsInDesignMode(this) || Debugger.IsAttached;

            if (isInDesigner)
            {
                dispatcherTimer.Start();
            }
            else
            {
                dispatcherTimer.Stop();

                application.Startup += (s, e) =>
                {
                    CheckForSharedStyleSheet(application);
                };
                application.Navigated += (s, e) =>
                {
                    CheckForSharedStyleSheet(application);
                };
                application.FragmentNavigation += (s, e) =>
                {
                    CheckForSharedStyleSheet(application);
                };
                application.LoadCompleted += (s, e) =>
                {
                    CheckForSharedStyleSheet(application);
                };
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == DesignerProperties.IsInDesignModeProperty)
            {
                if ((bool)e.NewValue)
                {
                    dispatcherTimer.Start();
                }
                else
                {
                    dispatcherTimer.Stop();
                }
            }
        }

        private void CheckForSharedStyleSheet(Application application, bool force = false)
        {
            var allStyleSheets = application.Resources.Values.OfType<StyleSheet>()
                                    .Concat(application.Resources.MergedDictionaries.SelectMany(x => x.Values.OfType<StyleSheet>()))
                                    .ToList();

            StyleSheet found = null;
            foreach (var styleSheet in allStyleSheets)
            {
                if (styleSheet.IsSharedApplicationStyleSheet)
                {
                    found = styleSheet;
                    break;
                }
            }

            if (!force)
            {
                if ((found == AttachedStyleSheet && found?.Version == lastVersion))
                {
                    return;
                }

                if (found == null &&
                    found == AttachedStyleSheet)
                {
                    return;
                }
            }

            AttachedStyleSheet = found;
            
            if (found != null && AttachedStyleSheet == null)
            {
                var content = found.Content;
                found.Content = "";
                found.Content = content;
            }

            lastVersion = AttachedStyleSheet?.Version ?? -1;

            Css.SetStyleSheet(this, null);
            Css.SetStyleSheet(this, AttachedStyleSheet);
        }

        public StyleSheet AttachedStyleSheet { get; private set; }

        private int lastVersion;
    }
}
