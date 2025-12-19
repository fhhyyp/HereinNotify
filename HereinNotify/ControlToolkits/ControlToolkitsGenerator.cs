using HereinNotify.Extensions;
using HereinNotify.LitheDto;
using HereinNotify.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
namespace HereinNotify.ControlToolkits
{
    // ControlToolkitsGenerator

    
    /// <summary>
    /// MVVM通知属性代码生成器
    /// </summary>
    [Generator]
    public class ControlToolkitsGenerator : IIncrementalGenerator
    {
        /// <summary>
        /// 用来 nameof() 的
        /// </summary>
        internal static LitheDtoAttribute LitheDto = null;


        /// <summary>
        /// 用来 nameof() 的
        /// </summary>
        internal static LitheDtoNameAttribute LitheDtoName = null;


        /// <summary>
        /// 初始化生成器，定义需要执行的生成逻辑。
        /// </summary>
        /// <param name="context">增量生成器的上下文，用于注册生成逻辑</param>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {

            if (GeneratorConfig.IsDebugControlToolkits)
                Debugger.Launch(); // 用于调试源生成器

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
        private CurrentControlClassCache Transform(GeneratorSyntaxContext context, CancellationToken token)
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

                if (classSymbol.ToString().Contains("UserModel"))
                {

                }
                Dictionary<string, ITypeSymbol> propHas = new Dictionary<string, ITypeSymbol>();
                //Dictionary<string, string> inheritDtos = new Dictionary<string, string>();

                var classCache = new CurrentControlClassCache(classDeclaration);
                classCache.BuildCacheOfClass(classSymbol, context, (AttrInfo info, AttributeData attr) => { });

                if (!classCache.Cache.ContainsAttr<CustomControlAttribute>())
                {
                    return null;
                }

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
        private static void GeneratorCode(SourceProductionContext context, CurrentControlClassCache classCache)
        {
            if (context.CancellationToken.IsCancellationRequested)
            {
                return;
            }

           
            classCache.SendGeneratorError.ForEach(x => x.Invoke(context));

            var generatedFileName = $"{classCache.ClassName}.g.cs";
            var generatedCode = classCache.GenerateCode(context);
            context.AddSource(generatedFileName, SourceText.From(generatedCode, Encoding.UTF8));
        }


    }



    #region DTO
    internal class CurrentControlClassCache : ClassCache
    {
        /// <summary>
        /// 变量名称(类型主键索引)
        /// </summary>
        public Dictionary<string, PartControlInfo> PartControlInfos { get; } = new Dictionary<string, PartControlInfo>();

        public CurrentControlClassCache(ClassDeclarationSyntax classDeclaration) : base(classDeclaration)
        {

        }

        public ProjectType ProjectType => Cache.GetAttr<CustomControlAttribute, ProjectType>(x => x.ProjectType);

        public void AddPartControl(PartControlInfo info)
        {
            var key = info.PartName;
            PartControlInfos[key] = info;

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


    internal class PartControlInfo
    {
        public string TypeName { get; }
        public string PartName { get; }

        public PartControlInfo(string typeName, string partName)
        {
            TypeName = typeName;
            PartName = partName;
        }
    }



    internal class CurrentPropertyFieldCache : MemberCache
    {
        public CurrentPropertyFieldCache(MemberCacheInfo info) : base(info)
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
