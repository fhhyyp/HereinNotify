using HereinNotify.Extensions;
using HereinNotify.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HereinNotify.ControlToolkits
{
    internal static class ControlToolkitsExtension
    {

        private static string DependencyObject = $"global::System.Windows.{nameof(DependencyObject)}";
        private static string DependencyProperty = $"global::System.Windows.{nameof(DependencyProperty)}";
        private static string FrameworkPropertyMetadata = $"global::System.Windows.{nameof(FrameworkPropertyMetadata)}";
        private static string FrameworkPropertyMetadataOptions = $"global::System.Windows.{nameof(FrameworkPropertyMetadataOptions)}";
        private static string DependencyPropertyChangedEventArgs = $"global::System.Windows.{nameof(DependencyPropertyChangedEventArgs)}";
        private static string TemplatePartAttribute = $"global::System.Windows.{nameof(TemplatePartAttribute)}";

        /// <summary>
        /// 代码生成
        /// </summary>
        /// <param name="classCache"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        internal static string GenerateCode(this CurrentControlClassCache classCache, SourceProductionContext context)
        {
            var sb = new StringBuilder();
            var generator = new GeneratorCache<CurrentControlClassCache>(context, classCache, sb);
            var code = generator.GeneratorUsing() // 添加 using
                                .GeneratorNamespace(() => // 添加命名空间
                                {
                                    // 添加通知属性的分布类
                                    generator.GeneratorClass(() =>
                                    {
                                        var projectType = classCache.ProjectType;
                                        //生成附加属性
                                        #region 生成附加属性
                                        var propertys = generator.ClassCache.GetMembers().Where(x => x.Cache.ContainsAttr<CustomPropertyAttribute>()).ToList();
                                        var partControls = generator.ClassCache.GetMembers().Where(x => x.Cache.ContainsAttr<CustomPartAttribute>()).ToList();
                                        if (projectType == ProjectType.Wpf)
                                        {

                                            if(partControls.Count > 0)
                                            {
                                                generator.GeneratorWpfPartLoaded(partControls);
                                            }

                                            foreach (var prop in propertys)
                                                prop.GeneratorWpfProperty(generator);
                                        }
                                        else
                                        {

                                            if (partControls.Count > 0)
                                            {
                                                generator.GeneratorAvaloniaPartLoaded(partControls);
                                            }

                                            foreach (var prop in propertys)
                                                prop.GeneratorAvaloniaProperty(generator);

                                        }
                                        #endregion


                                        #region 生成Part控件


                                        #endregion
                                    });
                                }).ToCode();
            return code;

        }

        /// <summary>
        /// 实现INPC接口
        /// </summary>
        /// <param name="generator"></param>
        /// <returns></returns>
        internal static GeneratorCache<CurrentControlClassCache> GeneratorINPC(this GeneratorCache<CurrentControlClassCache> generator)
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
        internal static GeneratorCache<CurrentControlClassCache> GeneratorNamespace(this GeneratorCache<CurrentControlClassCache> generator, Action execute)
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
        internal static GeneratorCache<CurrentControlClassCache> GeneratorClass(this GeneratorCache<CurrentControlClassCache> generator, Action execute)
        {
            var projectType = generator.ClassCache.ProjectType;

            var partControls = generator.ClassCache.GetMembers().Where(x => x.Cache.ContainsAttr<CustomPartAttribute>()).ToList();
            if (partControls.Count > 0) 
            {
                if(projectType == ProjectType.Wpf)
                {
                    foreach (var part in partControls)
                    {
                        generator.AppendCode($"[{TemplatePartAttribute}(Name = nameof({part.Name}), Type = typeof({part.Type}))]");
                    }
                }
            }

            //[TemplatePart(Name = PART_Btn_GetCurrentTime, Type = typeof(Label))]
            generator.AppendCode($"partial class {generator.ClassCache.ClassName}"); // 命名空间
            generator.AppendCode($"{{");
            generator.IncreaseTab();
            execute.Invoke();
            generator.DecreaseTab();
            generator.AppendCode($"}}");
            return generator;
        }

        #region wpf 属性
        private static void GeneratorWpfProperty(this MemberCache propInfo, GeneratorCache<CurrentControlClassCache> generator)
        {

            var propName = propInfo.Name;
            var propType = propInfo.Type;
            var defaultValue = propInfo.DefaultValue;
            var propertyName = GeneratorHelper.GetPropertyName(propInfo.Name);
            var bindType = propInfo.Cache.GetAttr<CustomPropertyAttribute, BindType>(x => x.BindingType);
            var isNew = propInfo.Cache.GetAttr<CustomPropertyAttribute, bool>(x => x.IsNew);
            var isAttached = propInfo.Cache.GetAttr<CustomPropertyAttribute, bool>(x => x.IsAttached);

            var controlType = generator.ClassCache.ClassFullName;
            var changedEventName = $"On{propertyName}Changed";
            var propertyProperty = $"{propertyName}Property";
            var fpmOption = bindType == BindType.Default ? $"{FrameworkPropertyMetadataOptions}.None" : $"{FrameworkPropertyMetadataOptions}.BindsTwoWayByDefault";
            var registerMethod = isAttached ? "RegisterAttached" : "Register";

            if (isAttached)
            {
               
                generator.AppendCode($"public static {propType} Get{propertyName}({DependencyObject} obj)");
                generator.AppendCode($"{{");
                generator.IncreaseTab();
                generator.AppendCode($"return ({propType})obj.GetValue({propertyProperty});");
                generator.DecreaseTab();
                generator.AppendCode($"}}");

                generator.AppendCode($"public static void Set{propertyName}({DependencyObject} obj, {propType} value)");
                generator.AppendCode($"{{");
                generator.IncreaseTab();
                if (propInfo.IsStatic)
                {
                    generator.AppendCode($"{propName} = value;");
                }
                else
                {
                    generator.AppendCode($"(({controlType})obj).{propName} = value;");
                }
                generator.AppendCode($"obj.SetValue({propertyProperty}, value);");
                generator.DecreaseTab();
                generator.AppendCode($"}}");

            }
            else
            {
               
                if (propertyName == propInfo.Name)
                {
                    var desc = new DiagnosticDescriptor(
                                  id: "HN002",
                                  title: "字段命名冲突",
                                  messageFormat: $"标记成员将会自动创建附加属性 '{propertyName}' ，当前命名冲突，请更改字段名称",
                                  category: "MemberDefinition",
                                  defaultSeverity: DiagnosticSeverity.Error,
                                  isEnabledByDefault: true
                              );
                    generator.Context.ReportDiagnostic(Diagnostic.Create(
                        desc,
                         location: propInfo.Variable.GetLocation(),
                         generator.ClassCache.ClassName
                     ));
                    return;
                }

                generator.AppendCode($"/// <inheritdoc cref=\"{propName}\"/>"); // 继承文档
                generator.AppendCode($"public {propType} {propertyName}");
                generator.AppendCode($"{{");
                generator.IncreaseTab();
                generator.AppendCode($"get {{ return ({propType})GetValue({propertyProperty}); }}"); // getter方法
                generator.AppendCode($"set {{ {propName} = value; SetValue({propertyProperty}, value); }}");
                generator.DecreaseTab();
                generator.AppendCode($"}}");

            }

            generator.AppendCode($"/// <inheritdoc cref=\"{propName}\"/>"); // 继承文档
            generator.AppendCode($"public static readonly {DependencyProperty} {propertyProperty} =");
            generator.AppendCode($"    {DependencyProperty}.{registerMethod}(nameof({propertyName}), typeof({propType}), typeof({controlType}), ");
            generator.AppendCode($"        new {FrameworkPropertyMetadata}({defaultValue}, {fpmOption}, {changedEventName}_g));");

            generator.AppendCode($"partial void {changedEventName}({propType} value);");
            generator.AppendCode($"partial void {changedEventName}({propType} oldValue, {propType} newValue);");

            generator.AppendCode($"private static void {changedEventName}_g({DependencyObject} d, {DependencyPropertyChangedEventArgs} e)");
            generator.AppendCode($"{{");
            generator.IncreaseTab();
            generator.AppendCode($"if(d is {controlType} control && e.OldValue is {propType} oldValue && e.NewValue is {propType} newValue)");
            generator.AppendCode($"{{");
            generator.IncreaseTab();
            generator.AppendCode($"control.{changedEventName}(newValue);");
            generator.AppendCode($"control.{changedEventName}(oldValue, newValue);");
            generator.DecreaseTab();
            generator.AppendCode($"}}");
            generator.DecreaseTab();
            generator.AppendCode($"}}");
        }


        private static void GeneratorWpfPartLoaded(this GeneratorCache<CurrentControlClassCache> generator, List<MemberCache> partControls)
        {
            string OnPartLoaded = nameof(OnPartLoaded);
            generator.AppendCode($"partial void {OnPartLoaded}();");
            generator.AppendCode($"public override void OnApplyTemplate()");
            generator.AppendCode($"{{");
            generator.IncreaseTab();
            generator.AppendCode($"base.OnApplyTemplate();");
            foreach(var part in partControls)
            {
                var partName = part.Name;
                var partType = part.Type;

                if (!partName.StartsWith("PART_"))
                {
                    var desc = new DiagnosticDescriptor(
                                 id: "HN003",
                                 title: "模板部件命名格式错误",
                                 messageFormat: $"请检查模板控件 '{partName}' 的名称， 更改为 PART_XXX 的格式",
                                 category: "MemberDefinition",
                                 defaultSeverity: DiagnosticSeverity.Error,
                                 isEnabledByDefault: true
                             );
                    generator.Context.ReportDiagnostic(Diagnostic.Create(
                        desc,
                         location: part.Variable.GetLocation(),
                         generator.ClassCache.ClassName
                     ));
                    continue;
                }
                generator.AppendCode($"{partName} = GetTemplateChild(nameof({partName})) as {partType}");
                generator.AppendCode($"     ?? throw new global::System.ArgumentNullException($\"无法找到控件 '{{nameof({partName})}}'\");");
            }

            generator.AppendCode($"{OnPartLoaded}();");
            generator.DecreaseTab();
            generator.AppendCode($"}}");

        }


        #endregion


        #region Avalonia 属性
        private static void GeneratorAvaloniaProperty(this MemberCache propInfo, GeneratorCache<CurrentControlClassCache> generator)
        {
        }

        private static void GeneratorAvaloniaPartLoaded(this GeneratorCache<CurrentControlClassCache> generator, List<MemberCache> partControls)
        {
        }

        private static void GeneratorAvaloniaPartProperty(this MemberCache propInfo, GeneratorCache<CurrentControlClassCache> generator)
        {
        } 
        #endregion
    }
    
}
