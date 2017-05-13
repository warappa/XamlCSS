using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace XamlCSS.WPF.TestApp
{
    public class ValueExtension : MarkupExtension
    {
        public ValueExtension()
        {
        }

        public ValueExtension(object value)
        {
            this.Value = value;
        }

        [ConstructorArgument("value")]
        public object Value { get; set; }
        
        public IValueConverter Converter { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Converter != null)
            {
                return Converter.Convert(Value, null, null, null);
            }
            return Value;
        }
    }

    public class UrlBrushExtension : MarkupExtension
    {
        public UrlBrushExtension()
        {
        }

        public UrlBrushExtension(object value)
        {
            this.Value = value;
        }

        [ConstructorArgument("value")]
        public object Value { get; set; }

        public IValueConverter Converter { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new StringToImageConverter().Convert(Value, null, null, null);
        }
    }

    public class StringToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() != typeof(string))
            {
                return null;
            }

            return new ImageBrush(new BitmapImage(new Uri((string)value)));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
