using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
namespace HereinNotify
{
    /// <summary>
    /// 工具类
    /// </summary>
    internal static class GeneratorHelper
    {
        /// <summary>
        /// 获取类所在的命名空间。
        /// </summary>
        /// <param name="classSyntax">类的语法节点。</param>
        /// <returns>命名空间的名称，或者 "GlobalNamespace" 如果没有命名空间声明。</returns>
        internal static string GetNamespace(SyntaxNode classSyntax)
        {
            // 查找最近的命名空间声明
            var namespaceDeclaration = classSyntax.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            return namespaceDeclaration?.Name.ToString() ?? "GlobalNamespace";
        }

        /// <summary>
        /// 检查是否继承了某个类
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        internal static bool InheritsFromHereinNotifyObject(INamedTypeSymbol symbol)
        {
            while (symbol != null)
            {
                if (symbol.Name == nameof(HereinNotifyObject)) // 如果继承了该类
                    return true;

                symbol = symbol.BaseType;
            }
            return false;
        }
    }

}
