using System;

namespace XamlCSS.Linker
{
    /// <summary>
    /// For Xamarin if assemblies are linked into the app
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class
        | AttributeTargets.Struct
        | AttributeTargets.Enum
        | AttributeTargets.Constructor
        | AttributeTargets.Method
        | AttributeTargets.Property
        | AttributeTargets.Field
        | AttributeTargets.Event
        | AttributeTargets.Interface
        | AttributeTargets.Delegate)]
    public sealed class PreserveAttribute : Attribute
    {
        public bool AllMembers;
        public bool Conditional;

        public PreserveAttribute()
        {
        }
    }
}
