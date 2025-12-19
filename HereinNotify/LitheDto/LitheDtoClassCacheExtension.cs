using HereinNotify.Extensions;
using HereinNotify.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HereinNotify.LitheDto
{


    internal static class LitheDtoClassCacheExtension
    {
        /// <summary>
        /// 代码生成
        /// </summary>
        /// <param name="classCache"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        internal static string GenerateCode(this LitheDtoClassCache classCache, SourceProductionContext context)
        {
            var sb = new StringBuilder();
            var generator = new GeneratorCache<LitheDtoClassCache>(context, classCache, sb);
            var code = generator.GeneratorUsing() // 添加 using
                                .GeneratorNamespace(() => // 添加命名空间
                                 {
                                     // 添加通知属性的分布类
                                     generator.GeneratorClass(() =>
                                     {
                                         // 生成属性
                                         foreach (var item in generator.ClassCache.DtoSourceInfos)
                                         {
                                             var info = item.Value;
                                             var props = info.GetProps(); //.Where(x => !hasProp.Contains(x.PropName));
                                             foreach (var prop in props)
                                             {
                                                 prop.GeneratorProperty(generator);
                                             }
                                         }

                                         // 生成方法
                                         foreach (var item in generator.ClassCache.DtoSourceInfos)
                                         {
                                             var info = item.Value;
                                             
                                             info.GeneratorInputMethod(generator); 
                                             info.GeneratorOutputMethod(generator);
                                             info.GeneratorWriteMethod(generator);
                                         }
                                     });
                                 }).ToCode();
            return code;

        }

        /// <summary>
        /// 实现INPC接口
        /// </summary>
        /// <param name="generator"></param>
        /// <returns></returns>
        internal static GeneratorCache<LitheDtoClassCache> GeneratorINPC(this GeneratorCache<LitheDtoClassCache> generator)
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

        /// <summary>
        /// 生成命名空间
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="execute"></param>
        /// <returns></returns>
        internal static GeneratorCache<LitheDtoClassCache> GeneratorNamespace(this GeneratorCache<LitheDtoClassCache> generator, Action execute)
        {
            generator.AppendCode($"namespace {generator.ClassCache.Namespace}"); // 命名空间
            generator.AppendCode($"{{");
            generator.IncreaseTab();
            execute.Invoke();
            generator.DecreaseTab();
            generator.AppendCode($"}}");
            return generator;
        }

        /// <summary>
        /// 生成分布类定义
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="execute"></param>
        /// <returns></returns>
        internal static GeneratorCache<LitheDtoClassCache> GeneratorClass(this GeneratorCache<LitheDtoClassCache> generator, Action execute)
        {
            if (generator.ClassCache.IsNeedINPC)
            {
                generator.AppendCode($"partial class {generator.ClassCache.ClassName} : global::System.ComponentModel.INotifyPropertyChanged"); // 命名空间
            }
            else
            {
                generator.AppendCode($"partial class {generator.ClassCache.ClassName}"); // 命名空间
            }
                
            generator.AppendCode($"{{");
            generator.IncreaseTab();
            execute.Invoke();

            if (generator.ClassCache.IsNeedINPC)
            {
                // 生成通知方法
                generator.GeneratorINPC();
            }

            generator.DecreaseTab();
            generator.AppendCode($"}}");
            return generator;
        }

        private static void GeneratorProperty(this DtoPropInfo propInfo, GeneratorCache<LitheDtoClassCache> generator)
        {
            var propName = propInfo.PropName;
            var propType = propInfo.TypeName;
            var otherName = propInfo.OtherName;

            if (!propInfo.ClassSourceInfo.IsUseINPC)
            {
                generator.AppendCode($"/// <inheritdoc cref=\"{propInfo.ClassSourceInfo.TypeName}.{propInfo.PropName}\"/>"); // 继承文档
                if (propInfo.IsHasOtherName)
                {
                    generator.AppendCode($"public {propType} {otherName} {{ get; set; }}");
                }
                else
                {
                    generator.AppendCode($"public {propType} {propName} {{ get; set; }}");
                }
            }
            else
            {
                var oldVarName = "__oldValue";
                var fieldName = $" _{propName}";
                if (propInfo.IsHasOtherName)
                {
                    propName = otherName;
                    fieldName = $" _{otherName}";
                }
                generator.AppendCode($"partial void On{propName}Changed({propType} oldValue, {propType} newValue);");
                generator.AppendCode($"partial void On{propName}Changed({propType} value);");
                generator.AppendCode($"private {propType} {fieldName};");

                generator.AppendCode($"/// <inheritdoc cref=\"{propInfo.ClassSourceInfo.TypeName}.{propInfo.PropName}\"/>"); // 继承文档
                generator.AppendCode($"public {propType} {propName}");
                generator.AppendCode($"{{");
                generator.IncreaseTab();
                generator.AppendCode($"get => {fieldName};");
                generator.AppendCode($"set");
                generator.AppendCode($"{{");
                generator.IncreaseTab();
                generator.AppendCode($"var {oldVarName} = {fieldName};");
                generator.AppendCode($"SetProperty(ref {fieldName}, value);");
                generator.AppendCode($"On{propName}Changed(value);");
                generator.AppendCode($"On{propName}Changed({oldVarName}, value);");
                generator.DecreaseTab();
                generator.AppendCode($"}}");
                generator.DecreaseTab();
                generator.AppendCode($"}}");


            }
        }

        private static void GeneratorInputMethod(this DtoClassSourceInfo classInfo, GeneratorCache<LitheDtoClassCache> generator)
        {
            var className = generator.ClassCache.ClassName;
            var inputTypeFullName = classInfo.TypeName;

            var methodNameSuffix = inputTypeFullName.Split('.').Last();
            var props = classInfo.GetProps(); // .Where(x => !hasProp.Contains(x.PropName));
            string model = nameof(model);
            string input = nameof(input);

            generator.AppendCode($"/// <summary>");
            generator.AppendCode($"/// DTO 实体读取 <see cref=\"{inputTypeFullName}\"/> 对象");
            generator.AppendCode($"/// </summary>");
            generator.AppendCode($"public {className} Input{methodNameSuffix}({inputTypeFullName} {input})");
            generator.AppendCode($"{{");
            generator.IncreaseTab();
            generator.AppendCode($"var {model} = this;");
            foreach (var prop in props)
            {
                var propName = prop.PropName;
                var propType = prop.TypeName;
                var otherName = prop.OtherName;
                if (prop.IsHasOtherName)
                {
                    generator.AppendCode($"{model}.{otherName} = {input}.{propName}; ");
                }
                else
                {
                    generator.AppendCode($"{model}.{propName} = {input}.{propName}; ");
                }
            }

            generator.AppendCode($"return {model};");
            generator.DecreaseTab();
            generator.AppendCode($"}}");
            //hasProp.Clear();
        }

        private static void GeneratorOutputMethod(this DtoClassSourceInfo classInfo, GeneratorCache<LitheDtoClassCache> generator)
        {
            var className = generator.ClassCache.ClassName;
            var outputTypeFullName = classInfo.TypeName;


            var methodNameSuffix = outputTypeFullName.Split('.').Last();
            var props = classInfo.GetProps(); //.Where(x => !hasProp.Contains(x.PropName));


            string output = nameof(output);
            string model = nameof(model);

            generator.AppendCode($"/// <summary>");
            generator.AppendCode($"/// DTO 实体生成 <see cref=\"{outputTypeFullName}\"/> 对象");
            generator.AppendCode($"/// </summary>");
            generator.AppendCode($"public {outputTypeFullName} Output{methodNameSuffix}()");
            generator.AppendCode($"{{");
            generator.IncreaseTab();
            generator.AppendCode($"var {model} = this;");
            generator.AppendCode($"var {output} = new {outputTypeFullName}();");
            foreach (var prop in props)
            {
                var propName = prop.PropName;
                var otherName = prop.OtherName;
                if (prop.IsHasOtherName)
                {
                    generator.AppendCode($"{output}.{propName} = {model}.{otherName}; ");
                }
                else
                {
                    generator.AppendCode($"{output}.{propName} = {model}.{propName}; ");
                }
            }
            generator.AppendCode($"return {output};");
            generator.DecreaseTab();
            generator.AppendCode($"}}");
        }

        private static void GeneratorWriteMethod(this DtoClassSourceInfo classInfo, GeneratorCache<LitheDtoClassCache> generator)
        {

            var className = generator.ClassCache.ClassName;
            var targetTypeFullName = classInfo.TypeName;
            var methodNameSuffix = targetTypeFullName.Split('.').Last();
            var props = classInfo.GetProps(); //.Where(x => !hasProp.Contains(x.PropName));

            string target = nameof(target);
            string model = nameof(model);

            generator.AppendCode($"/// <summary>");
            generator.AppendCode($"/// DTO 实体写入到 <see cref=\"{targetTypeFullName}\"/> 对象");
            generator.AppendCode($"/// </summary>");
            generator.AppendCode($"public void Write{methodNameSuffix}({targetTypeFullName} {target})");
            generator.AppendCode($"{{");
            generator.IncreaseTab();
            generator.AppendCode($"var {model} = this;");
            foreach (var prop in props)
            {
                var propName = prop.PropName;
                var otherName = prop.OtherName;
                if (prop.IsHasOtherName)
                {
                    generator.AppendCode($"{target}.{propName} = {model}.{otherName}; ");
                }
                else
                {
                    generator.AppendCode($"{target}.{propName} = {model}.{propName}; ");
                }
            }
            generator.DecreaseTab();
            generator.AppendCode($"}}");
        }

    }
}