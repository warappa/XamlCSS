# XamlCSS
Style Xaml-applications with CSS

Supported platforms
---
- **XamarinForms**  
NuGet: [XamlCSS.XamarinForms](https://www.nuget.org/packages/XamlCSS.XamarinForms)
- **WPF**  
NuGet: [XamlCSS.WPF](https://www.nuget.org/packages/XamlCSS.WPF)
- **Universal Windows Platform**  
NuGet: [XamlCSS.UWP](https://www.nuget.org/packages/XamlCSS.UWP)

Supported Features
---
- **CSS selectors**
- **Remove and reapply** styles
- **Detect new elements** and apply matching styles
- Support **Binding** * (except vanilla UWP)
- Support **StaticResource** *
- Support **DynamicResource** * (except vanilla UWP)
- Set **simple values** like Foreground, FontSize, FontStyle,... by CSS

New in 2.0.0
---
- **Triggers**
- **Multiple** StyleSheets
- **Nested** selectors (like Sass)
- **Css-variables**
- **Import** of other css-files

### Not (yet) supported
- Visual State Manager

[Getting Started](https://github.com/warappa/XamlCSS/wiki/Getting-started)
---

For more information look at the provided test-apps in the solution to see how to initialize and use XamlCSS.

*) **Breaking change** in binding syntax in 2.0.0: instead of `{Binding value}` you now write `#Binding value` or `"{Binding value}"`
