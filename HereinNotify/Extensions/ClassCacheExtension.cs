using HereinNotify.Models;
using System.Collections.Generic;

namespace HereinNotify.Extensions
{
    internal static class ClassCacheExtension
    {

        /// <summary>
        /// 生成 using 引用
        /// </summary>
        /// <param name="generator"></param>
        /// <returns></returns>
        internal static GeneratorCache<T> GeneratorUsing<T>(this GeneratorCache<T> generator) where T : ClassCache
        {
            foreach (var ns in HereinNotifyGeneratorConfig.DefaultUsings)
            {
                generator.AppendCode($"using {ns};");
            }

            var nns = generator.ClassCache.Cache.GetAttr(nameof(HereinUsingAttribute))?
                                       .GetMenber(nameof(HereinUsingAttribute.Namespace))?
                                       .GetValues() ?? new List<object>();

            foreach (var ns in nns)
            {
                generator.AppendCode($"using {ns.ToString()};");
            }

            return generator;
        }




        /// <summary>
        /// 实现INPC接口
        /// </summary>
        /// <param name="generator"></param>
        /// <returns></returns>
        internal static GeneratorCache<T> GeneratorINPC<T>(this GeneratorCache<T> generator) where T : ClassCache
        {
            generator.AppendCode("public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;");
            generator.AppendCode("protected bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)");
            generator.AppendCode("{");
            generator.IncreaseTab();
            generator.AppendCode("if (EqualityComparer<T>.Default.Equals(storage, value)) return false;");
            generator.AppendCode("storage = value;");
            generator.AppendCode("OnPropertyChanged(propertyName);");
            generator.AppendCode("return true;");
            generator.DecreaseTab();
            generator.AppendCode("}");
            generator.AppendCode("public void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));");
            generator.AppendCode("");
            return generator;
        }
    }
}