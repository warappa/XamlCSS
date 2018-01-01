using System;

namespace XamlCSS
{
    public class DependencyPropertyInfo<TDependencyProperty>
        where TDependencyProperty : class
    {
        public DependencyPropertyInfo(TDependencyProperty property, Type declaringType, string name)
        {
            Property = property;
            DeclaringType = declaringType;
            Name = name;
            ShortName = name.Substring(0, name.Length - 8);
        }

        public TDependencyProperty Property { get; }
        public string Name { get; }
        public Type DeclaringType { get; }
        public string ShortName { get; internal set; }
    }
}
