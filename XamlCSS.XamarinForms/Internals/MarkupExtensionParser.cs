using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.Xaml.Internals;

namespace XamlCSS.XamarinForms.Internals
{
    [XamlCSS.Linker.Preserve(AllMembers = true)]
    public class MarkupExtensionParser : IMarkupExtensionParser
    {
        private IMarkupExtension markupExtension;

        internal static bool MatchMarkup(out string match, string expression, out int end)
        {
            if (expression.Length < 2)
            {
                end = 1;
                match = null;
                return false;
            }
            if (expression[0] != '{')
            {
                end = 2;
                match = null;
                return false;
            }
            bool found = false;
            int i;
            for (i = 1; i < expression.Length; i++)
            {
                if (expression[i] != ' ')
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                end = 3;
                match = null;
                return false;
            }
            int c = 0;
            while (c + i < expression.Length && expression[i + c] != ' ' && expression[i + c] != '}')
            {
                c++;
            }
            if (i + c == expression.Length)
            {
                end = 6;
                match = null;
                return false;
            }
            end = i + c;
            match = expression.Substring(i, c);
            return true;
        }

        private object ParseExpression(ref string expression, IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            if (expression.StartsWith("{}", StringComparison.Ordinal))
            {
                return expression.Substring(2);
            }
            if (expression[expression.Length - 1] != '}')
            {
                throw new Exception("Expression must end with '}'");
            }
            string match;
            int len;
            if (!MatchMarkup(out match, expression, out len))
            {
                return false;
            }
            expression = expression.Substring(len).TrimStart(new char[0]);
            if (expression.Length == 0)
            {
                throw new Exception("Expression did not end in '}'");
            }
            return ((dynamic)Activator.CreateInstance(base.GetType())).Parse(match, ref expression, serviceProvider);
        }
        protected void HandleProperty(string prop, IServiceProvider serviceProvider, ref string remaining, bool isImplicit)
        {
            object value = null;
            if (isImplicit)
            {
                this.SetPropertyValue(null, prop, null, serviceProvider);
                return;
            }
            remaining = remaining.TrimStart(new char[0]);
            string str_value;
            if (remaining.StartsWith("{", StringComparison.Ordinal))
            {
                value = this.ParseExpression(ref remaining, serviceProvider);
                remaining = remaining.TrimStart(new char[0]);
                if (remaining.Length > 0 && remaining[0] == ',')
                {
                    remaining = remaining.Substring(1);
                }
                str_value = (value as string);
            }
            else
            {
                char next;
                str_value = this.GetNextPiece(ref remaining, out next);
            }
            this.SetPropertyValue(prop, str_value, value, serviceProvider);
        }
        protected string GetNextPiece(ref string remaining, out char next)
        {
            bool inString = false;
            int end = 0;
            char stringTerminator = '\0';
            remaining = remaining.TrimStart(new char[0]);
            if (remaining.Length == 0)
            {
                next = '￿';
                return null;
            }
            StringBuilder piece = new StringBuilder();
            while (end < remaining.Length && (inString || (remaining[end] != '}' && remaining[end] != ',' && remaining[end] != '=')))
            {
                if (inString)
                {
                    if (remaining[end] == stringTerminator)
                    {
                        inString = false;
                        end++;
                        break;
                    }
                }
                else if (remaining[end] == '\'' || remaining[end] == '"')
                {
                    inString = true;
                    stringTerminator = remaining[end];
                    end++;
                    continue;
                }
                if (remaining[end] == '\\')
                {
                    end++;
                    if (end == remaining.Length)
                    {
                        break;
                    }
                }
                piece.Append(remaining[end]);
                end++;
            }
            if (inString && end == remaining.Length)
            {
                throw new Exception("Unterminated quoted string");
            }
            if (end == remaining.Length && !remaining.EndsWith("}", StringComparison.Ordinal))
            {
                throw new Exception("Expression did not end with '}'");
            }
            if (end == 0)
            {
                next = '￿';
                return null;
            }
            next = remaining[end];
            remaining = remaining.Substring(end + 1);
            while (piece.Length > 0 && char.IsWhiteSpace(piece[piece.Length - 1]))
            {
                StringBuilder expr_133 = piece;
                int length = expr_133.Length;
                expr_133.Length = length - 1;
            }
            if (piece.Length >= 2)
            {
                char first = piece[0];
                char last = piece[piece.Length - 1];
                if ((first == '\'' && last == '\'') || (first == '"' && last == '"'))
                {
                    piece.Remove(piece.Length - 1, 1);
                    piece.Remove(0, 1);
                }
            }
            return piece.ToString();
        }
        
        private object Parse(string expression)
        {
            expression = expression.Trim().Substring(1);

            var strs = expression.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var remaining = string.Join(" ", strs.Skip(1));

            return Parse(strs[0], ref remaining, new XamlServiceProvider());
        }

        private object Parse(string match, ref string remaining, IServiceProvider serviceProvider)
        {
            IXamlTypeResolver typeResolver = serviceProvider.GetService(typeof(IXamlTypeResolver)) as IXamlTypeResolver;
            if (match == "Binding")
            {
                this.markupExtension = new BindingExtension();
            }
            else if (match == "TemplateBinding")
            {
                this.markupExtension = new TemplateBindingExtension();
            }
            else if (match == "StaticResource")
            {
                this.markupExtension = new StaticResourceExtension();
            }
            else if (match == "DynamicResource")
            {
                this.markupExtension = new DynamicResourceExtension();
            }
            else
            {
                if (typeResolver == null)
                {
                    return null;
                }
                Type type;
                if (!typeResolver.TryResolve(match + "Extension", out type) && !typeResolver.TryResolve(match, out type))
                {
                    IXmlLineInfoProvider lineInfoProvider = serviceProvider.GetService(typeof(IXmlLineInfoProvider)) as IXmlLineInfoProvider;
                    IXmlLineInfo arg_BF_0;
                    if (lineInfoProvider == null)
                    {
                        IXmlLineInfo xmlLineInfo = new XmlLineInfo();
                        arg_BF_0 = xmlLineInfo;
                    }
                    else
                    {
                        arg_BF_0 = lineInfoProvider.XmlLineInfo;
                    }
                    IXmlLineInfo lineInfo = arg_BF_0;
                    throw new XamlParseException(string.Format("MarkupExtension not found for {0}", new object[]
                    {
                        match
                    }), lineInfo);
                }
                this.markupExtension = (Activator.CreateInstance(type) as IMarkupExtension);
            }
            if (this.markupExtension == null)
            {
                IXmlLineInfoProvider lineInfoProvider2 = serviceProvider.GetService(typeof(IXmlLineInfoProvider)) as IXmlLineInfoProvider;
                IXmlLineInfo arg_123_0;
                if (lineInfoProvider2 == null)
                {
                    IXmlLineInfo xmlLineInfo = new XmlLineInfo();
                    arg_123_0 = xmlLineInfo;
                }
                else
                {
                    arg_123_0 = lineInfoProvider2.XmlLineInfo;
                }
                IXmlLineInfo lineInfo2 = arg_123_0;
                throw new XamlParseException(string.Format("Missing public default constructor for MarkupExtension {0}", new object[]
                {
                    match
                }), lineInfo2);
            }
            if (remaining == "}")
            {
                return this.markupExtension;
            }
            char next;
            string piece;
            while ((piece = GetNextPiece(ref remaining, out next)) != null)
            {
                HandleProperty(piece, serviceProvider, ref remaining, next != '=');
            }
            return this.markupExtension;
        }

        internal static string GetContentPropertyName(TypeInfo typeInfo)
        {
            while (typeInfo != null)
            {
                string propName = typeInfo.CustomAttributes
                    .FirstOrDefault(x => x.AttributeType == typeof(ContentPropertyAttribute))
                    ?.ConstructorArguments
                    .Select(x => x.Value as string)
                    .FirstOrDefault();
                if (propName != null)
                {
                    return propName;
                }
                TypeInfo arg_2B_0;
                if (typeInfo == null)
                {
                    arg_2B_0 = null;
                }
                else
                {
                    Type expr_1F = typeInfo.BaseType;
                    arg_2B_0 = ((expr_1F != null) ? expr_1F.GetTypeInfo() : null);
                }
                typeInfo = arg_2B_0;
            }
            return null;
        }

        protected void SetPropertyValue(string prop, string strValue, object value, IServiceProvider serviceProvider)
        {
            MethodInfo setter;
            if (prop == null)
            {
                Type t = this.markupExtension.GetType();
                prop = GetContentPropertyName(t.GetTypeInfo());
                if (prop == null)
                {
                    return;
                }
                setter = t.GetRuntimeProperty(prop).SetMethod;
            }
            else
            {
                setter = this.markupExtension.GetType().GetRuntimeProperty(prop).SetMethod;
            }
            if (value == null && strValue != null)
            {
                value = ConvertTo(strValue, this.markupExtension.GetType().GetRuntimeProperty(prop).PropertyType, null, serviceProvider);
            }
            setter.Invoke(this.markupExtension, new object[]
            {
                value
            });
        }

        internal static object ConvertTo(object value, Type toType, Func<object> getConverter, IServiceProvider serviceProvider)
        {
            if (value == null)
            {
                return null;
            }
            string str = value as string;
            if (str != null)
            {
                object converter = (getConverter != null) ? getConverter() : null;
                TypeConverter xfTypeConverter = converter as TypeConverter;
                IExtendedTypeConverter xfExtendedTypeConverter = xfTypeConverter as IExtendedTypeConverter;
                if (xfExtendedTypeConverter != null)
                {
                    return value = xfExtendedTypeConverter.ConvertFromInvariantString(str, serviceProvider);
                }
                if (xfTypeConverter != null)
                {
                    return value = xfTypeConverter.ConvertFromInvariantString(str);
                }
                Type converterType = (converter != null) ? converter.GetType() : null;
                if (converterType != null)
                {
                    MethodInfo convertFromStringInvariant = converterType.GetRuntimeMethod("ConvertFromInvariantString", new Type[]
                    {
                typeof(string)
                    });
                    if (convertFromStringInvariant != null)
                    {
                        return value = convertFromStringInvariant.Invoke(converter, new object[]
                        {
                    str
                        });
                    }
                }
                if (toType.GetTypeInfo().IsGenericType && toType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    toType = Nullable.GetUnderlyingType(toType);
                }
                if (toType.GetTypeInfo().IsEnum)
                {
                    return Enum.Parse(toType, str);
                }
                if (toType == typeof(int))
                {
                    return int.Parse(str, CultureInfo.InvariantCulture);
                }
                if (toType == typeof(float))
                {
                    return float.Parse(str, CultureInfo.InvariantCulture);
                }
                if (toType == typeof(double))
                {
                    return double.Parse(str, CultureInfo.InvariantCulture);
                }
                if (toType == typeof(bool))
                {
                    return bool.Parse(str);
                }
                if (toType == typeof(TimeSpan))
                {
                    return TimeSpan.Parse(str, CultureInfo.InvariantCulture);
                }
                if (toType == typeof(DateTime))
                {
                    return DateTime.Parse(str, CultureInfo.InvariantCulture);
                }
                if (toType == typeof(string) && str.StartsWith("{}", StringComparison.Ordinal))
                {
                    return str.Substring(2);
                }
                if (toType == typeof(string))
                {
                    return value;
                }
            }
            if (value != null)
            {
                MethodInfo cast = value.GetType().GetRuntimeMethod("op_Implicit", new Type[]
                {
            value.GetType()
                });
                if (cast != null && cast.ReturnType == toType)
                {
                    value = cast.Invoke(null, new object[]
                    {
                value
                    });
                }
            }
            return value;
        }

        public object ProvideValue(string expression, object targetObject, IEnumerable<CssNamespace> namespaces, bool unwrap = true)
        {
            var serviceProvider = new XamlServiceProvider();
            serviceProvider.Add(typeof(IProvideValueTarget), new ProvideValueTarget(targetObject));
            return (Parse(expression) as IMarkupExtension)?.ProvideValue(serviceProvider);
        }
    }
}