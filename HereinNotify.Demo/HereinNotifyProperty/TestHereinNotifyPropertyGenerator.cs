using System;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
namespace HereinNotify.Demo
{
    [HereinUsing("System.Text.Json.Serialization")]
    internal partial class TestModel : HereinNotifyObject
    {
        [HereinNotifyProperty(Attr = typeof(TestAttribute), AttrParmas="\"test_A\"")]
        [HereinNotifyProperty(Attr = typeof(TestAttribute), AttrParmas="\"test_B\"")]
        [HereinNotifyProperty(Attr = typeof(TestAttribute), AttrParmas="\"test_C\"")]
        [HereinNotifyProperty(IsVerify = true)]
        [HereinNotifyProperty(IsChanged = true)]
        //[HereinNotifyProperty(Notify = nameof(Name))]
        private int _id = 666;

        [HereinNotifyProperty(Attr = typeof(TestAttribute), AttrParmas = "\"test_1\"")]
        [HereinNotifyProperty(Attr = typeof(TestAttribute), AttrParmas = "\"test_2\"")]
        [HereinNotifyProperty(Attr = typeof(TestAttribute), AttrParmas = "\"test_3\"")]
        [HereinNotifyProperty(Attr = typeof(JsonPropertyNameAttribute), AttrParmas = "\"JSON_KEY\"")]
        private string _name = string.Empty;

        partial void VerifyIdSetter(ref bool isAllow, int newValue) => isAllow = newValue > 0;
        partial void OnIdVerifyFail(int value) => Console.WriteLine($"赋值被拦截：{value}");
        partial void OnIdChanged(int oldValue, int newValue) => Console.WriteLine($"old : {oldValue},     new :{newValue}");
    }


    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public sealed class TestAttribute : Attribute
    {
        public TestAttribute(string value)
        {

        }
    }


    internal static class TestHereinNotifyPropertyGenerator
    {
        internal static void Run()
        {
            Console.WriteLine("\r\n===============");
            var model = new TestModel();
            model.Id = -999;
            Console.WriteLine($"Id改变：{model.IdIsChanged}");
            model.Id = 23313;
            Console.WriteLine($"Id改变：{model.IdIsChanged}");

            Console.WriteLine("");

            typeof(TestModel).GetProperties().Where(x => x.GetCustomAttribute<HereinAutoPropertyAttribute>() is not null).ToList().ForEach(p => Console.WriteLine($"自动生成属性：{p.Name}"));
            Console.WriteLine("");

            typeof(TestModel).GetProperties().ToList().ForEach(p => p.GetCustomAttributes().ToList().ForEach(a => Console.WriteLine($"为属性{p.Name}自动生成特性： {a}")));


        }


    }

}
