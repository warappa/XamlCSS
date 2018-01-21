# XamlCSS
Style Xaml-applications with CSS - get it for [WPF](https://www.nuget.org/packages/XamlCSS.WPF), [Xamarin.Forms](https://www.nuget.org/packages/XamlCSS.XamarinForms) or [UWP](https://www.nuget.org/packages/XamlCSS.UWP) and start [right here](https://github.com/warappa/XamlCSS/wiki/Getting-started)!

## Why XamlCSS For XAML?
### Concise
Styling with (s)css gives a **more concise declaration of your styles**. XamlCSS even supports a **subset of SCSS** features like [selector nesting](https://github.com/warappa/XamlCSS/wiki/Nested-Css-Selectors), [css-variables](https://github.com/warappa/XamlCSS/wiki/Css-Variables) and [mixins](https://github.com/warappa/XamlCSS/wiki/Mixins). This enables you to make your declarations **even _more_ concise**.

### Freely Combine Styles
Other than vanilla Xaml-styles, css allows you to **freely combine styles** - _no_ `BasedOn` restriction. Even better, this is done for you behind [the curtains](https://github.com/warappa/XamlCSS/wiki/How-does-it-work%3F/#generating-styles)!  
And if you really want to combine styles yourself use [@extend](https://github.com/warappa/XamlCSS/wiki/Extend).

### Semantic Meaning
**Semantic meaning** can be conveyed, i.e. is your ui-element `important`, a `warning`, a `header` or a `sub-header`? This is achieved by using [css-classes](https://github.com/warappa/XamlCSS/wiki/Css-Selectors-Quick-Reference#by-role-class).

### Based On View-Hierarchy
Css takes into account **where inside your view-hierarchy** your element gets added. No need to manually assigning a style.  
It also **detects that an element was added or removed**.  
In combination with semantic selectors you can style a button differently just because it is in a `warning` dialog. And if you want to create a `dark`, a `light` and a `custom` **theme**, just **switch the css-class-name** on your root view-element and all elements **update themselves automatically**.

### Support For Xaml-Features
You can use [markup-extensions](https://github.com/warappa/XamlCSS/wiki/Markup-Extensions-in-Css) and [triggers](https://github.com/warappa/XamlCSS/wiki/Xaml-Triggers-in-Css) in your (s)css.

In css you **_cannot_ declare** an **instance of an object** as you can do in xaml. A `Storyboard` for example must be declared as usual in a `ResourceDictionary` but then can be referenced in css with a markup-extension.

### Designer Support
XamlCSS builds on top of the native Xaml-Style implementations, so **it works** with the **WPF and UWP designer**. For **Xamarin.Forms** there is **[LiveXAML](https://www.livexaml.com)**.

# Supported platforms
- **XamarinForms**  
NuGet: [XamlCSS.XamarinForms](https://www.nuget.org/packages/XamlCSS.XamarinForms) [![NuGet](https://img.shields.io/nuget/v/XamlCSS.XamarinForms.svg)]()
- **WPF**  
NuGet: [XamlCSS.WPF](https://www.nuget.org/packages/XamlCSS.WPF) [![NuGet](https://img.shields.io/nuget/v/XamlCSS.WPF.svg)]()
- **Universal Windows Platform**  
NuGet: [XamlCSS.UWP](https://www.nuget.org/packages/XamlCSS.UWP) [![NuGet](https://img.shields.io/nuget/v/XamlCSS.UWP.svg)]()

# Supported Features
- **CSS selectors**
- **Remove and reapply** styles
- **Detect new elements** and apply matching styles
- Support **Binding** * (except vanilla UWP)
- Support **StaticResource** *
- Support **DynamicResource** * (except vanilla UWP)
- Set **simple values** like Foreground, FontSize, FontStyle,... by CSS
- **Triggers**
- **Multiple** StyleSheets
- **Nested** selectors (like Sass)
- **Css-variables**
- **Import** of other css-files
- **Mixins**

## Not (yet) supported
- Visual State Manager


For more information look at the provided test-apps in the solution to see how to initialize and use XamlCSS.

*) **Breaking change** in binding syntax in 2.0.0: instead of `{Binding value}` you now write `#Binding value` or `"{Binding value}"`
