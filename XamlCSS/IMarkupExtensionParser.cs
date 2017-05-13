namespace XamlCSS
{
    public interface IMarkupExtensionParser
    {
        object ProvideValue(string expression, object targetObject);
    }
}
