using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XamlCSS.Utils;

namespace XamlCSS.Tests.Utils
{
    [TestFixture]
    public class FastReflectionTests
    {
        private class TestClass
        {
            public string Value { get; set; }
        }
        
        [Test]
        public void Test_getter_performance()
        {
            var test = new TestClass()
            {
                Value = "Hallo"
            };

            var stopwatch = new Stopwatch();
            object a;

            stopwatch.Restart();
            for (var i = 0; i < 100000; i++)
            {
                var accessor = (typeof(TestClass).GetProperty("Value"));

                a = accessor.GetValue(test);//.Should().Be("Hallo");
            }

            stopwatch.Stop();
            Debug.WriteLine($"{stopwatch.ElapsedMilliseconds}ms - propertyinfo");

            stopwatch.Restart();
            for (var i = 0; i < 100000; i++)
            {
                var accessor = PropertyInfoHelper.GetAccessor(typeof(TestClass).GetProperty("Value"));

                a = accessor.GetValue(test);//.Should().Be("Hallo");
            }

            stopwatch.Stop();
            Debug.WriteLine($"{stopwatch.ElapsedMilliseconds}ms - accessor");

            var cachedAccessor = PropertyInfoHelper.GetAccessor(typeof(TestClass).GetProperty("Value"));
            var accessorCache = new Dictionary<string, IPropertyAccessor>() { { "Value", cachedAccessor} };
            stopwatch.Restart();
            for (var i = 0; i < 100000; i++)
            {
                var accessor = accessorCache["Value"];

                a = accessor.GetValue(test);//.Should().Be("Hallo");
            }

            stopwatch.Stop();
            Debug.WriteLine($"{stopwatch.ElapsedMilliseconds}ms - cached accessor");

            stopwatch.Restart();
            for (var i = 0; i < 100000; i++)
            {
                var accessor = TypeHelpers.GetPropertyAccessor(typeof(TestClass), "Value");

                a = accessor.GetValue(test);//.Should().Be("Hallo");
            }

            stopwatch.Stop();
            Debug.WriteLine($"{stopwatch.ElapsedMilliseconds}ms - typehelper");


            stopwatch.Restart();
            for (var i = 0; i < 100000; i++)
            {
                a = test.Value;//.Should().Be("Hallo");
            }

            stopwatch.Stop();
            Debug.WriteLine($"{stopwatch.ElapsedMilliseconds}ms - direct");

            stopwatch.Restart();
            for (var i = 0; i < 100000; i++)
            {
                var accessor = FastMember.TypeAccessor.Create(typeof(TestClass));
                
                a = accessor[test, "Value"];//.Should().Be("Hallo");
            }

            stopwatch.Stop();
            Debug.WriteLine($"{stopwatch.ElapsedMilliseconds}ms - fastmember");

        }

        [Test]
        public void Test_setter_performance()
        {
            var test = new TestClass()
            {
                Value = "Hallo"
            };

            var stopwatch = new Stopwatch();

            stopwatch.Restart();
            for (var i = 0; i < 100000; i++)
            {
                var accessor = typeof(TestClass).GetProperty("Value");

                accessor.SetValue(test, "uuu");//.Should().Be("Hallo");
            }

            stopwatch.Stop();
            Debug.WriteLine($"{stopwatch.ElapsedMilliseconds}ms - propertyinfo");

            stopwatch.Restart();

            var cachedPropertyInfo = typeof(TestClass).GetProperty("Value");
            Dictionary<string, PropertyInfo> cache = new Dictionary<string, PropertyInfo>();
            cache["Value"] = cachedPropertyInfo;
            for (var i = 0; i < 100000; i++)
            {
                var accessor = cache["Value"];
                accessor.SetValue(test, "uuu");//.Should().Be("Hallo");
            }

            stopwatch.Stop();
            Debug.WriteLine($"{stopwatch.ElapsedMilliseconds}ms - propertyinfo cached");

            stopwatch.Restart();
            for (var i = 0; i < 100000; i++)
            {
                var accessor = PropertyInfoHelper.GetAccessor(typeof(TestClass).GetProperty("Value"));

                accessor.SetValue(test, "uuu");
            }

            stopwatch.Stop();
            Debug.WriteLine($"{stopwatch.ElapsedMilliseconds}ms - accessor");

            var cachedAccessor = PropertyInfoHelper.GetAccessor(typeof(TestClass).GetProperty("Value"));
            var accessorCache = new Dictionary<string, IPropertyAccessor>() { { "Value", cachedAccessor } };
            stopwatch.Restart();
            for (var i = 0; i < 100000; i++)
            {
                var accessor = accessorCache["Value"];

                accessor.SetValue(test, "uuu");//.Should().Be("Hallo");
            }

            stopwatch.Stop();
            Debug.WriteLine($"{stopwatch.ElapsedMilliseconds}ms - cached accessor");

            stopwatch.Restart();
            for (var i = 0; i < 100000; i++)
            {
                var accessor = TypeHelpers.GetPropertyAccessor(typeof(TestClass), "Value");

                accessor.SetValue(test, "uuu");
            }

            stopwatch.Stop();
            Debug.WriteLine($"{stopwatch.ElapsedMilliseconds}ms - typehelper");


            stopwatch.Restart();
            for (var i = 0; i < 100000; i++)
            {
                test.Value = "uuu";//.Should().Be("Hallo");
            }

            stopwatch.Stop();
            Debug.WriteLine($"{stopwatch.ElapsedMilliseconds}ms - direct");

            stopwatch.Restart();
            for (var i = 0; i < 100000; i++)
            {
                var accessor = FastMember.TypeAccessor.Create(typeof(TestClass));

                accessor[test, "Value"] = "uuu";//.Should().Be("Hallo");
            }

            stopwatch.Stop();
            Debug.WriteLine($"{stopwatch.ElapsedMilliseconds}ms - fastmember");

        }
    }

}
