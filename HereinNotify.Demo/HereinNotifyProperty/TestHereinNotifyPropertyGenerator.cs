using HereinNotify;
using System;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HereinNotifyDemo
{
    [HereinUsing("System.Text.Json.Serialization")]
    internal partial class TestModel : HereinNotifyObject
    {
        [HereinNotifyProperty(IsVerify = true)]
        [HereinNotifyProperty(IsChanged = true)]
        [HereinNotifyProperty(Notify = nameof(Name))]
        private int _id = 666;

        [HereinNotifyProperty(Attr = typeof(JsonPropertyNameAttribute), AttrParmas = "\"UserName\"")]
        private string _name = string.Empty;

        [HereinNotifyProperty]
        [HereinVerifyFailState(IsNotify = true)]
        private bool _isVerifyFail;

        [HereinChangedState]
        [HereinNotifyProperty]
        private bool _isChanged;

        partial void VerifyIdSetter(ref bool isAllow, int newValue) => isAllow = newValue > 0;
        partial void OnIdVerifyFail(int value) => Console.WriteLine($"赋值被拦截：{value}");
        partial void OnIdChanged(int oldValue, int newValue) => Console.WriteLine($"old : {oldValue},     new :{newValue}");


    }







    internal static class TestHereinNotifyPropertyGenerator
    {

#if true


        internal static void Run()
        {
            Console.WriteLine("\r\n===============");
            var model = new TestModel();
            model.Name = "HereinNotify";

            Console.WriteLine($"验证失败状态变量：{model.IsVerifyFail}");
            Console.WriteLine($"改变状态变量：{model.IsChanged}");
            Console.WriteLine($"---");
            model.Id = -999;
            Console.WriteLine($"验证失败状态变量：{model.IsVerifyFail}");
            Console.WriteLine($"改变状态变量：{model.IsChanged}");
            Console.WriteLine($"---");
            Console.WriteLine($"Id改变：{model.IdIsChanged}");
            model.Id = 23313;

            Console.WriteLine($"验证失败状态变量：{model.IsVerifyFail}");
            Console.WriteLine($"改变状态变量：{model.IsChanged}");
            Console.WriteLine($"---");
            Console.WriteLine($"Id改变：{model.IdIsChanged}");

            Console.WriteLine("");

            typeof(TestModel).GetProperties().Where(x => x.GetCustomAttribute<HereinAutoPropertyAttribute>() is not null).ToList().ForEach(p => Console.WriteLine($"自动生成属性：{p.Name}"));
            Console.WriteLine("");

            typeof(TestModel).GetProperties().ToList().ForEach(p => p.GetCustomAttributes().ToList().ForEach(a => Console.WriteLine($"为属性{p.Name}自动生成特性： {a}")));

        }


#endif
    }

}
