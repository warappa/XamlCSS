using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using XamlCSS.WPF;

namespace XamlCSS.WPF.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class CssClassExtensionsTests
    {
        private DependencyObject dp;

        
        [SetUp]
        public void Initialize()
        {
            
            dp = new Button();
            Css.SetClass(dp, "main light");
        }

        [Test]
        [STAThread]
        public void Can_toggle_existing_class()
        {
            dp.ToggleClass("light");

            Css.GetClass(dp).Should().Be("main");
        }

        [Test]
        [STAThread]
        public void Can_toggle_new_class()
        {
            dp.ToggleClass("dark");

            Css.GetClass(dp).Should().Be("main light dark");
        }

        [Test]
        [STAThread]
        public void Add_can_handle_existing_class()
        {
            dp.AddClass("light");

            Css.GetClass(dp).Should().Be("main light");
        }

        [Test]
        [STAThread]
        public void Can_add_new_class()
        {
            dp.AddClass("dark");

            Css.GetClass(dp).Should().Be("main light dark");
        }

        [Test]
        [STAThread]
        public void Can_remove_existing_class()
        {
            dp.RemoveClass("light");

            Css.GetClass(dp).Should().Be("main");
        }

        [Test]
        [STAThread]
        public void Remove_can_handle_notset_class()
        {
            dp.RemoveClass("dark");

            Css.GetClass(dp).Should().Be("main light");
        }
    }
}
