using HereinNotify.Extensions;
using HereinNotify.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
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
    public class LitheDtoGenerator : IIncrementalGenerator
    {
        /// <summary>
        /// 用来 nameof() 的
        /// </summary>
        internal static LitheDtoAttribute LitheDto = null;


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

            if (node is ClassDeclarationSyntax cds && cds.AttributeLists.Count > 0)
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
        private LitheDtoClassCache Transform(GeneratorSyntaxContext context, CancellationToken token)
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

                var classCache = new LitheDtoClassCache(classDeclaration);
                classCache.BuildCacheOfClass(classSymbol, (info, attr) =>
                {
                    var attributeName = attr.AttributeClass?.Name;
                    if (attributeName == nameof(LitheDtoAttribute))
                    {
                        var anargs = attr.NamedArguments.ToList();
                        var argType = anargs.FirstOrDefault(x => x.Key == nameof(LitheDtoAttribute.Source) && x.Value.Kind == TypedConstantKind.Type);
                        var isUseINPCConstant  = anargs.FirstOrDefault(x => x.Key == nameof(LitheDtoAttribute.IsUseINPC) && x.Value.Kind == TypedConstantKind.Primitive);
                        var isUseINPC = isUseINPCConstant.Key is null ? false : isUseINPCConstant.Value.Value is bool uinpc ? uinpc : false;

                        DtoClassSourceInfo dto = null;
                        if (argType.Value.Value is ITypeSymbol sourceType)
                        {
                            var sourceTypeName = sourceType.ToString();
                            var properties = sourceType.GetMembers()
                                                       .OfType<IPropertySymbol>()
                                                       .Where(p => p.DeclaredAccessibility == Accessibility.Public).ToList();
                            if (properties.Count > 0)
                            {
                                dto = new DtoClassSourceInfo(sourceTypeName);
                                dto.IsUseINPC = isUseINPC;
                                foreach (var p in properties)
                                {
                                    var propName = p.Name; // 属性名称
                                    var propType = p.Type.ToDisplayString(); // 完整类型
                                    dto.AddProp(propName, propType);
                                }
                                classCache.AddDtoSoucre(dto);

                                var argIgnore = anargs.FirstOrDefault(x => x.Key == nameof(LitheDtoAttribute.Ignore) && x.Value.Kind == TypedConstantKind.Array);
                                if (argIgnore.Key is null)
                                {
                                    return;
                                }
                                var arrayValues = argIgnore.Value.Values;
                                var ignoredMembers = arrayValues.Select(x => x.Value?.ToString())
                                                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                                                .ToImmutableArray();
                                foreach (var member in ignoredMembers)
                                {
                                    dto.AddIgnored(member);
                                }
                            }
                        }

                       
                    }

                });



                /*bool isInherited = GeneratorHelper.InheritsFromHereinNotifyObject(classSymbol);
                bool isUseAttr = classCache.Cache.ContainsAttr(nameof(HereinNotifyObjectAttribute));*/

                
                /*classCache.IsInherited = isInherited;
                classCache.IsUseHereinNotifyObjectAttribute = isUseAttr;
                var fieldDeclarations = classDeclaration.Members.OfType<FieldDeclarationSyntax>();
                classCache.BuildCacheOfField(semanticModel, fieldDeclarations);*/


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
        private static void GeneratorCode(SourceProductionContext context, LitheDtoClassCache classCache)
        {
            if (context.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (!classCache.HasSource)
            {
                return;
            }

            var generatedFileName = $"{classCache.ClassName}.g.cs";
            var generatedCode = classCache.GenerateCode();
            context.AddSource(generatedFileName, SourceText.From(generatedCode, Encoding.UTF8));
        }


    }



    #region DTO
    internal class LitheDtoClassCache : ClassCache
    {
        /// <summary>
        /// 变量名称
        /// </summary>
        public Dictionary<string, DtoClassSourceInfo> DtoSourceInfos { get; } = new Dictionary<string, DtoClassSourceInfo>();

        public bool HasSource => DtoSourceInfos.Count > 0;

        public bool IsNeedINPC => DtoSourceInfos.Values.Any(x => x.IsUseINPC);

        public LitheDtoClassCache(ClassDeclarationSyntax classDeclaration) : base(classDeclaration)
        {
        }

        public void AddDtoSoucre(DtoClassSourceInfo dtoClassSource)
        {
            var key = dtoClassSource.TypeName;
            DtoSourceInfos[key] = dtoClassSource;

            /*if (DtoSourceInfo.TryGetValue(key, out var info))
            {
                return;
            }
            else
            {
                DtoSourceInfo[key] = dtoClassSource;
            }*/
        }

    }


    internal class DtoClassSourceInfo
    {
        public bool IsUseINPC { get; set; }
        public string TypeName { get;}

        public Dictionary<string, DtoPropInfo> Props { get; } = new Dictionary<string, DtoPropInfo>();

        public HashSet<string> IgnoredMenber { get; } = new HashSet<string>();

        public DtoClassSourceInfo(string typeName)
        {
            TypeName = typeName;
        }


        public void AddProp(string name, string type)
        {
            if (Props.ContainsKey(name))
            {
                return;
            }
            Props[name] = new DtoPropInfo(this, name, type);
        }

        internal void AddIgnored(string member)
        {
            IgnoredMenber.Add(member);
        }

        internal IEnumerable<DtoPropInfo> GetProps()
        {
           return Props.Values.Where(x => !IgnoredMenber.Contains(x.PropName));
        }
    }


    internal class DtoPropInfo
    {
        internal DtoClassSourceInfo ClassSourceInfo { get;  }

        /// <summary>
        /// 属性名称
        /// </summary>
        internal string PropName { get; }

        /// <summary>
        /// 类型名称
        /// </summary>
        internal string TypeName { get;}

        internal DtoPropInfo(DtoClassSourceInfo info, string propName, string typeName)
        {
            ClassSourceInfo = info;
            PropName = propName;
            TypeName = typeName;
        }
    }


    internal class LitheDtoFieldCache : FieldCache
    {
        public LitheDtoFieldCache(VariableDeclaratorSyntax variable, string fieldName, string type) : base(variable, fieldName, type)
        {
        }

        /// <summary>
        /// 转换后的变量名
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// 是否忽略
        /// </summary>
        public bool IsIgnore { get; internal set; }

    }
    #endregion
}
