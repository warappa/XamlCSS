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
        }

        public TDependencyProperty Property { get; }
        public string Name { get; }
        public Type DeclaringType { get; }
    }
}
