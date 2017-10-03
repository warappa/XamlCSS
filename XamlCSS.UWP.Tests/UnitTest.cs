using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using NUnit.Framework;
using System;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace XamlCSS.UWP.Tests
{
    [TestClass]
    public class CssClassExtensionsTests
    {
        private DependencyObject dp;

        [TestInitialize]
        public void Initialize()
        {
            Css.Initialize(new Assembly[0]);
            dp = new Button();
            Css.SetClass(dp, "main light");
        }

        [UITestMethod]
        public void Can_toggle_existing_class()
        {
            dp.ToggleClass("light");

            Css.GetClass(dp).Should().Be("main");
        }

        [UITestMethod]
        public void Can_toggle_new_class()
        {
            dp.ToggleClass("dark");

            Css.GetClass(dp).Should().Be("main light dark");
        }

        [UITestMethod]
        public void Add_can_handle_existing_class()
        {
            dp.AddClass("light");

            Css.GetClass(dp).Should().Be("main light");
        }

        [UITestMethod]
        public void Can_add_new_class()
        {
            dp.AddClass("dark");

            Css.GetClass(dp).Should().Be("main light dark");
        }

        [UITestMethod]
        public void Can_remove_existing_class()
        {
            dp.RemoveClass("light");

            Css.GetClass(dp).Should().Be("main");
        }

        [UITestMethod]
        public void Remove_can_handle_notset_class()
        {
            dp.RemoveClass("dark");

            Css.GetClass(dp).Should().Be("main light");
        }
    }
}
