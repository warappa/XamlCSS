using System;
using System.Collections.Generic;
using System.Xml;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace XamlCSS.XamarinForms.Internals
{
    /// <summary>For internal use by the XAML infrastructure.</summary>
    /// <remarks>To be added.</remarks>
    [XamlCSS.Linker.Preserve(AllMembers = true)]
    [ContentProperty("Key")]
	public sealed class StaticResourceExtension : IMarkupExtension
	{
		/// <summary>For internal use by the XAML infrastructure.</summary>
		/// <value>To be added.</value>
		/// <remarks>To be added.</remarks>
		public string Key
		{
			get;
			set;
		}

		/// <param name="serviceProvider">To be added.</param>
		/// <summary>For internal use by the XAML infrastructure.</summary>
		/// <returns>To be added.</returns>
		/// <remarks>To be added.</remarks>
		public object ProvideValue(IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
			{
				throw new ArgumentNullException("serviceProvider");
			}
			if (this.Key == null)
			{
				IXmlLineInfoProvider lineInfoProvider = serviceProvider.GetService(typeof(IXmlLineInfoProvider)) as IXmlLineInfoProvider;
				IXmlLineInfo arg_40_0;
				if (lineInfoProvider == null)
				{
					IXmlLineInfo xmlLineInfo2 = new XmlLineInfo();
					arg_40_0 = xmlLineInfo2;
				}
				else
				{
					arg_40_0 = lineInfoProvider.XmlLineInfo;
				}
				IXmlLineInfo lineInfo = arg_40_0;
				throw new XamlParseException("you must specify a key in {StaticResource}", lineInfo);
			}
			IProvideParentValuesPublic expr_62 = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideParentValuesPublic;
			if (expr_62 == null)
			{
				throw new ArgumentException();
			}
			IXmlLineInfoProvider xmlLineInfoProvider = serviceProvider.GetService(typeof(IXmlLineInfoProvider)) as IXmlLineInfoProvider;
			IXmlLineInfo xmlLineInfo = (xmlLineInfoProvider != null) ? xmlLineInfoProvider.XmlLineInfo : null;
			using (IEnumerator<object> enumerator = expr_62.ParentObjects.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					VisualElement ve = enumerator.Current as VisualElement;
					object res;
					if (ve != null && ve.Resources != null && ve.Resources.TryGetValue(this.Key, out res))
					{
						object result = res;
						return result;
					}
				}
			}
			if (Application.Current != null && Application.Current.Resources != null && Application.Current.Resources.ContainsKey(this.Key))
			{
				return Application.Current.Resources[this.Key];
			}

            return null;
		}
	}
}