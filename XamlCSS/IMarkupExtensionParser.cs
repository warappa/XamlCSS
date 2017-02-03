namespace XamlCSS
{
    public interface IMarkupExtensionParser
    {
        object Parse(string expression);
        object ProvideValue(string expression, object targetObject);
    }
}
