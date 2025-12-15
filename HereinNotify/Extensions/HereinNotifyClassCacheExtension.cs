using HereinNotify.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HereinNotify.Extensions
{


    internal static class HereinNotifyClassCacheExtension
    {

        /// <summary>
        /// 代码生成
        /// </summary>
        /// <param name="classCache"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        internal static string GenerateCode(this HereinNotifyClassCache classCache, SourceProductionContext context)
        {
            var sb = new StringBuilder();
            var generator = new GeneratorCache<HereinNotifyClassCache>(context, classCache, sb);
            var code = generator.GeneratorUsing() // 添加 using
                                .GeneratorNamespace(() => // 添加命名空间
                                 {
                                     generator.GeneratorClass(() =>
                                     {
                                         generator.GeneratorMembers();
                                     }); // 添加通知属性的分布类
                                 }).ToCode();
            return code;

        }



        /// <summary>
        /// 生成命名空间
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="execute"></param>
        /// <returns></returns>
        private static GeneratorCache<HereinNotifyClassCache> GeneratorNamespace(this GeneratorCache<HereinNotifyClassCache> generator, Action execute)
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
        private static GeneratorCache<HereinNotifyClassCache> GeneratorClass(this GeneratorCache<HereinNotifyClassCache> generator, Action execute)
        {
            generator.AppendCode($"partial class {generator.ClassCache.ClassName}"); // 命名空间
            generator.AppendCode($"{{");
            generator.IncreaseTab();
            
            execute.Invoke();
            generator.DecreaseTab();
            generator.AppendCode($"}}");
            return generator;
        }


        private class GeneratorMemberContext
        {
            public GeneratorMemberContext(GeneratorCache<HereinNotifyClassCache> generator)
            {
                Generator = generator;
            }

            public List<HereinNotifyFieldCache> FieldToProps { get; } = new List<HereinNotifyFieldCache>();
            public List<HereinNotifyFieldCache> Changeds { get; } = new List<HereinNotifyFieldCache>();
            public List<HereinNotifyFieldCache> VerifyFails { get; } = new List<HereinNotifyFieldCache>();

            public GeneratorCache<HereinNotifyClassCache> Generator { get; set; }
        } 

        /// <summary>
        /// 生成所有属性
        /// </summary>
        /// <param name="generator"></param>
        private static void GeneratorMembers(this GeneratorCache<HereinNotifyClassCache> generator)
        {
            var fields = generator.ClassCache.GetFields().OfType<HereinNotifyFieldCache>();
            // 需要优先生成状态属性
            GeneratorMemberContext context = new GeneratorMemberContext(generator);

            foreach (var field in fields)
            {
                if (field.Cache.GetAttr(nameof(HereinNotifyPropertyGenerator.HereinVerifyFailState)) is AttrInfo verifyAttrInfo)
                {
                    context.VerifyFails.Add(field);
                }
                else if (field.Cache.GetAttr(nameof(HereinNotifyPropertyGenerator.HereinChangedState)) is AttrInfo changedAttrInfo)
                {
                    context.Changeds.Add(field);
                }
                

                if(field.IsUseHereinNotifyPropertyAttribute())
                {
                    context.FieldToProps.Add(field);
                }
            }



            // 通知属性
            foreach (var field in context.FieldToProps)
            {
                generator.AppendCode($"");
                if (field.IsUseHereinNotifyPropertyAttribute())
                {
                    if (field.IsIgnore)
                    {
                        continue;
                    }
                    field.InitAttribute(); // 初始化字段特性
                    field.GeneratorPropertys(context);
                    field.GeneratorPartialMethod(generator);
                    if (field.IsChanged)
                    {
                        field.GeneratorMonitorChanged(generator);
                    }
                    generator.AppendCode($"");
                }
                else
                {
                    // 
                }
            }


            if (!generator.ClassCache.IsInherited
               && generator.ClassCache.IsUseHereinNotifyObjectAttribute)
            {
                generator.GeneratorINPC(); // 生成INPC接口
            }
        }

        /// <summary>
        /// 初始化字段特性
        /// </summary>
        /// <param name="field"></param>
        private static void InitAttribute(this HereinNotifyFieldCache field)
        {
            var hnp = field.Cache.GetAttr(nameof(HereinNotifyPropertyGenerator.HereinNotifyProperty));

            var isVerifySetter = hnp?.GetMenber(nameof(HereinNotifyPropertyGenerator.HereinNotifyProperty.IsVerify))?
                                     .GetFirstValue<bool>() ?? false;
            var isMonitorChanged = hnp?.GetMenber(nameof(HereinNotifyPropertyGenerator.HereinNotifyProperty.IsChanged))?
                                      .GetFirstValue<bool>() ?? false;

            field.IsVerify = isVerifySetter;
            field.IsChanged = isMonitorChanged;
        }


        /// <summary>
        /// 生成分布方法
        /// </summary>
        /// <param name="field"></param>
        /// <param name="generator"></param>
        /// <returns></returns>
        private static void GeneratorMonitorChanged(this HereinNotifyFieldCache field, GeneratorCache<HereinNotifyClassCache> generator)
        {
            var propertyName = field.PropertyName;
            var monitorfieldName = $"{field.Name}IsChanged";
            var monitorPropertyName = $"{propertyName}IsChanged";
            var boolType = $"bool";
            //var boolType = $"using::{typeof(bool).FullName}";

            generator.AppendCode($"partial void On{monitorPropertyName}Changed({boolType} oldValue, {boolType} newValue);");
            generator.AppendCode($"partial void On{monitorPropertyName}Changed({boolType} value);");

            generator.AppendCode($"/// <summary>");
            generator.AppendCode($"/// 监视属性 <see cref=\"{propertyName}\"/> 的更改");
            generator.AppendCode($"/// </summary>");
            generator.AppendCode($"private {boolType} {monitorfieldName};"); // 私有属性

            generator.AppendCode($"/// <summary>");
            generator.AppendCode($"/// 监视属性 <see cref=\"{propertyName}\"/> 的更改");
            generator.AppendCode($"/// </summary>");
            generator.AppendCode($"public {boolType} {monitorPropertyName}");
            generator.AppendCode($"{{");
            generator.IncreaseTab();

            generator.AppendCode($"get => {monitorfieldName};"); // getter方法
            generator.AppendCode($"set");
            generator.AppendCode($"{{");
            generator.IncreaseTab();

            generator.AppendCode($"var __oldValue = {monitorfieldName};");
            generator.AppendCode($"SetProperty(ref {monitorfieldName}, value);");
            generator.AppendCode($"On{monitorPropertyName}Changed(value);");
            generator.AppendCode($"On{monitorPropertyName}Changed(__oldValue, value);");

            generator.DecreaseTab();
            generator.AppendCode($"}}");

            generator.DecreaseTab();
            generator.AppendCode($"}}"); // 属性的结尾大括号 
        }

        /// <summary>
        /// 生成分布方法
        /// </summary>
        /// <param name="field"></param>
        /// <param name="generator"></param>
        /// <returns></returns>
        private static void GeneratorPartialMethod(this HereinNotifyFieldCache field, GeneratorCache<HereinNotifyClassCache> generator)
        {
            var fieldType = field.Type; // 获取字段类型
            var propertyName = field.PropertyName; // 合适的属性名称
            generator.AppendCode($"partial void On{propertyName}Changed({fieldType} oldValue, {fieldType} newValue);");
            generator.AppendCode($"partial void On{propertyName}Changed({fieldType} value);");

            if (field.IsVerify)
            {
                generator.AppendCode($"/// <summary>");
                generator.AppendCode($"/// 属性<see cref=\"{propertyName}\"/>被赋值时的验证方法。");
                generator.AppendCode($"/// 如果 isAllow 被设置为 false， 此次赋值将不会成功。");
                generator.AppendCode($"/// </summary>");
                generator.AppendCode($"partial void Verify{propertyName}Setter(ref bool isAllow, {fieldType} newValue);");
                                
                generator.AppendCode($"/// <summary>");
                generator.AppendCode($"/// 属性<see cref=\"{propertyName}\"/>赋值失败时的方法。");
                generator.AppendCode($"/// 入参 value 是被拦截的值。");
                generator.AppendCode($"/// </summary>");
                generator.AppendCode($"partial void On{propertyName}VerifyFail({fieldType} value);");
            }

        }

        /// <summary>
        /// 生成通知属性
        /// </summary>
        /// <param name="field"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private static void GeneratorPropertys(this HereinNotifyFieldCache field, GeneratorMemberContext context)
        {
            var generator = context.Generator;

            generator.AppendCode($"/// <inheritdoc cref=\"{field.Name}\"/>"); // 继承文档
            field.GeneratorPropertyCustomAttribute(generator); // 生成自定义特性
            generator.AppendCode($"public {field.Type} {field.PropertyName}");
            generator.AppendCode($"{{");
            generator.IncreaseTab();
            field.GeneratorPropertyGetter(context); // 生成 getter
            field.GeneratorPropertySetter(context); // 生成 setter
            generator.DecreaseTab();
            generator.AppendCode($"}}");
        }


        /// <summary>
        /// 生成属性的特性
        /// </summary>
        /// <param name="field"></param>
        /// <param name="generator"></param>
        private static void GeneratorPropertyCustomAttribute(this HereinNotifyFieldCache field, GeneratorCache<HereinNotifyClassCache> generator)
        {
            var hnp = field.Cache.GetAttr(nameof(HereinNotifyPropertyGenerator.HereinNotifyProperty));


            var attrs = hnp?.GetMenber(nameof(HereinNotifyPropertyGenerator.HereinNotifyProperty.Attr))?
                           .GetValues() ?? new List<object>();
            var attrParmass = hnp?.GetMenber(nameof(HereinNotifyPropertyGenerator.HereinNotifyProperty.AttrParmas))?
                              .GetValues() ?? new List<object>();

            var len = Math.Min(attrs.Count, attrParmass.Count);
            for (var i = 0; i < len; i++) 
            { 
                var attr = attrs[i].ToString();
                var attrParmas = attrParmass[i].ToString();

                if (!string.IsNullOrWhiteSpace(attr))
                {
                    if (!string.IsNullOrEmpty(attrParmas))
                    {
                        generator.AppendCode($"[{attr}({attrParmas})]");
                    }
                    else
                    {
                        generator.AppendCode($"[{attr}]");
                    }
                }
            }

            generator.AppendCode($"[{nameof(HereinNotify)}.{nameof(HereinAutoPropertyAttribute)}]"); // 表示自动生成的属性
            


        }

        /// <summary>
        /// 生成属性的Getter
        /// </summary>
        /// <param name="field"></param>
        /// <param name="context"></param>
        private static void GeneratorPropertyGetter(this HereinNotifyFieldCache field, GeneratorMemberContext context)
        {
            context.Generator.AppendCode($"get => {field.Name};"); // 生成 getter
        }


        /// <summary>
        /// 生成属性的Setter
        /// </summary>
        /// <param name="field"></param>
        /// <param name="context"></param>
        private static void GeneratorPropertySetter(this HereinNotifyFieldCache field, GeneratorMemberContext context)
        {
            GeneratorCache<HereinNotifyClassCache> generator = context.Generator;
            var fieldName = field.Name; // 获取字段名称
            var fieldType = field.Type; // 获取字段类型
            var propertyName = field.PropertyName; // 合适的属性名称

            var monitorfieldName = $"{fieldName}IsChanged";
            var monitorPropertyName = $"{propertyName}IsChanged";



            generator.AppendCode($"set");
            generator.AppendCode($"{{");
            generator.IncreaseTab();
            if (field.IsVerify)
            {
                generator.AppendCode($"bool isAllow = true;");
                generator.AppendCode($"Verify{propertyName}Setter(ref isAllow, value);");
                generator.AppendCode($"if(!isAllow)");
                generator.AppendCode($"{{");
                generator.IncreaseTab();
                generator.AppendCode($"On{propertyName}VerifyFail(value);");
                foreach(var vierfyFailField in context.VerifyFails)
                {
                    var vierfyFailMemberName = vierfyFailField.IsUseHereinNotifyPropertyAttribute() ? vierfyFailField.PropertyName : vierfyFailField.Name;
                    generator.AppendCode($"{vierfyFailMemberName} = true;");
                }
                generator.AppendCode($"return;");
                generator.DecreaseTab();
                generator.AppendCode($"}}");
            }

            generator.AppendCode($"var __oldValue = {fieldName};");

            if (field.IsChanged)
            {
                generator.AppendCode($"bool isChanged = SetProperty(ref {fieldName}, value);");
                generator.AppendCode($"if(isChanged)");
                generator.AppendCode($"{{");
                generator.IncreaseTab();
                generator.AppendCode($"{monitorPropertyName} = true;");
                foreach (var changedField in context.Changeds)
                {
                    var changedMemberName = changedField.IsUseHereinNotifyPropertyAttribute() ? changedField.PropertyName : changedField.Name;
                    generator.AppendCode($"{changedMemberName} = true;");
                }
                generator.DecreaseTab();
                generator.AppendCode($"}}");

            }
            else
            {
                generator.AppendCode($"SetProperty(ref {fieldName}, value);");
            }


            generator.AppendCode($"On{propertyName}Changed(value);");
            generator.AppendCode($"On{propertyName}Changed(__oldValue, value);");

            // 通知其它属性
            var notifyOtherPropertNames = field.Cache.GetAttr(nameof(HereinNotifyPropertyGenerator.HereinNotifyProperty))?
                                                     .GetMenber(nameof(HereinNotifyPropertyGenerator.HereinNotifyProperty.Notify))?
                                                     .GetValues() ?? new List<object>();
            foreach (var nopn in notifyOtherPropertNames)
            {
                var name = nopn.ToString();
                generator.AppendCode($"OnPropertyChanged(\"{name}\");");
            }

            generator.DecreaseTab();
            generator.AppendCode($"}}");
        }

    }
}