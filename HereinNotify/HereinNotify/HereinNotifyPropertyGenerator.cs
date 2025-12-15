using HereinNotify.Extensions;
using HereinNotify.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks.Sources;
using System.Xml.Linq;
namespace HereinNotify
{

    /// <summary>
    /// MVVM通知属性代码生成器
    /// </summary>
    [Generator]
    public class HereinNotifyPropertyGenerator : IIncrementalGenerator
    {
        /// <summary>
        /// 用来 nameof() 的
        /// </summary>
        internal static HereinNotifyPropertyAttribute HereinNotifyProperty = null;
        /// <summary>
        /// 用来 nameof() 的
        /// </summary>
        internal static HereinUsingAttribute HereinUsing = null;
        /// <summary>
        /// 用来 nameof() 的
        /// </summary>
        internal static HereinNotifyObjectAttribute HereinNotifyObject = null;

        /// <summary>
        /// 初始化生成器，定义需要执行的生成逻辑。
        /// </summary>
        /// <param name="context">增量生成器的上下文，用于注册生成逻辑</param>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            //Debugger.Launch(); // 用于调试源生成器

            var classDeclarations = context.SyntaxProvider
                                           .CreateSyntaxProvider(Predicate, Transform)
                                           .Where(x => x != null);


            // 注册一个源生成任务，使用找到的类生成代码
            context.RegisterSourceOutput(classDeclarations, GeneratorCode);
        }

        /// <summary>
        ///  分析这些类
        /// </summary>
        /// <param name="node"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private static bool Predicate(SyntaxNode node, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return false;
            }

            if (node is ClassDeclarationSyntax)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 创建缓存
        /// </summary>
        /// <param name="context"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private HereinNotifyClassCache Transform(GeneratorSyntaxContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return null;
            }
            
            if (context.Node is ClassDeclarationSyntax classDeclaration)
            {
                var semanticModel = context.SemanticModel;
                var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration); // 获取类的符号
                if (classSymbol is null)
                    return null;

                var classCache = new HereinNotifyClassCache(classDeclaration);
                classCache.BuildCacheOfClass(classSymbol, (info, attr) =>
                {
                    var attributeName = attr.AttributeClass?.Name;
                    if (attributeName == nameof(HereinUsingAttribute))
                    {
                        // 记录命名空间
                        foreach (var ctorArg in attr.ConstructorArguments)
                        {
                            var key = nameof(HereinUsingAttribute.Namespace);
                            var value = ctorArg.Value.ToString();
                            info.AddMember(key, value);
                        }
                    }
                });

                bool isInherited = GeneratorHelper.InheritsFromHereinNotifyObject(classSymbol);
                bool isUseAttr = classCache.Cache.ContainsAttr(nameof(HereinNotifyObjectAttribute));

                
                classCache.IsInherited = isInherited;
                classCache.IsUseHereinNotifyObjectAttribute = isUseAttr;

                var fieldDeclarations = classDeclaration.Members.OfType<FieldDeclarationSyntax>();
                classCache.BuildCacheOfField(semanticModel, fieldDeclarations);
                return classCache;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 生成代码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="classCache"></param>
        private static void GeneratorCode(SourceProductionContext context, HereinNotifyClassCache classCache)
        {
            if (context.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (!classCache.IsUseHereinNotifyPropertyAttribute())
            {
                return;
            }

            // 没有使用 [HereinNotifyObject] 特性，也没有继承 HereinNotifyObject
            if (!classCache.IsUseHereinNotifyObjectAttribute && !classCache.IsInherited)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                     DiagnosticDescriptorModel.MissingAttributeDescriptor,
                     location: classCache.Syntax.Identifier.GetLocation(),
                     classCache.ClassName
                 ));
            }


            foreach (var field in classCache.GetFields().OfType<HereinNotifyFieldCache>())
            {
                if (field.Name == field.PropertyName)
                {
                    var desc = new DiagnosticDescriptor(
                                    id: "HN002",
                                    title: "字段命名冲突",
                                    messageFormat: $"字段将会自动创建属性 '{field.PropertyName}' ，当前命名冲突，请更改字段名称",
                                    category: "MemberDefinition",
                                    defaultSeverity: DiagnosticSeverity.Error,
                                    isEnabledByDefault: true
                                );
                    context.ReportDiagnostic(Diagnostic.Create(
                        desc,
                         location: field.Variable.GetLocation(),
                         classCache.ClassName
                     ));
                    field.IsIgnore = true;
                }
            }

            var generatedFileName = $"{classCache.ClassName}.g.cs";
            var generatedCode = classCache.GenerateCode();
            context.AddSource(generatedFileName, SourceText.From(generatedCode, Encoding.UTF8));
        }


    }




    #region 通知属性
    internal class HereinNotifyClassCache : ClassCache
    {
        /// <summary>
        /// 是否使用特性
        /// </summary>
        public bool IsUseHereinNotifyObjectAttribute { get; set; }

        public HereinNotifyClassCache(ClassDeclarationSyntax classDeclaration) : base(classDeclaration)
        {
        }


        public override FieldCache AddField(VariableDeclaratorSyntax variable, string fieldName, string type)
        {
            if (FieldCaches.TryGetValue(fieldName, out var fieldCache))
            {
                return fieldCache;
            }
            else
            {
                var field = new HereinNotifyFieldCache(variable, fieldName, type);
                FieldCaches.Add(field.Name, field);
                return field;
            }
        }


        /// <summary>
        /// 是否需要生成属性
        /// </summary>
        public bool IsUseHereinNotifyPropertyAttribute()
        {
            foreach (var item in FieldCaches.Values)
            {
                var type = item.GetType();
                if(item is HereinNotifyFieldCache hereinNotifyField)
                {
                    if (hereinNotifyField.IsUseHereinNotifyPropertyAttribute())
                    {

                        return true;
                    }
                }
            }
            return false;
        }
    }



    internal class HereinNotifyFieldCache : FieldCache
    {

        /// <summary>
        /// 表示需要验证赋值
        /// </summary>
        public bool IsVerify = false;

        /// <summary>
        /// 表示需要监视属性是否变化
        /// </summary>
        public bool IsChanged = false;

        /// <summary>
        /// 是否使用特性
        /// </summary>
        public bool IsUseHereinNotifyObjectAttribute { get; set; }

        public HereinNotifyFieldCache(VariableDeclaratorSyntax variable, string fieldName, string type) : base(variable, fieldName, type)
        {
            PropertyName = GetPropertyName(fieldName);
        }

        /// <summary>
        /// 转换后的变量名
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// 是否忽略
        /// </summary>
        public bool IsIgnore { get; internal set; }

        /// <summary>
        /// 字段名称转换为属性名称
        /// </summary>
        /// <returns>遵循属性命名规范的新名称</returns>
        private static string GetPropertyName(string fieldName)
        {
            var propertyName = fieldName.StartsWith("_") ? char.ToUpper(fieldName[1]) + fieldName.Substring(2) : char.ToUpper(fieldName[0]) + fieldName.Substring(1); // 创建属性名称
            return propertyName;
        }

        /// <summary>
        /// 是否需要生成属性
        /// </summary>
        public bool IsUseHereinNotifyPropertyAttribute()
        {
            if (this.Cache.ContainsAttr(nameof(HereinNotifyPropertyGenerator.HereinNotifyProperty)))
            {
                return true;
            }
            return false;
        }



    }
    #endregion
}
