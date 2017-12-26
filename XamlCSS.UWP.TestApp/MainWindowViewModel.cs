using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Data;

namespace XamlCSS.UWP.TestApp
{
    public class MainWindowViewModel
    {
        public string Message { get; set; } = "Hello World from DataContext!";
    }

    public class ListToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (targetType != typeof(string))
                throw new InvalidOperationException("The target must be a String");

            return String.Join("\n", ((IEnumerable<string>)value).ToArray());
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}