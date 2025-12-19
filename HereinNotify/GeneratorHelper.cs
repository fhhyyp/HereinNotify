using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
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
        /// 字段名称转换为属性名称
        /// </summary>
        /// <returns>遵循属性命名规范的新名称</returns>
        public static string GetPropertyName(string fieldName)
        {
            var propertyName = fieldName.StartsWith("_") ? char.ToUpper(fieldName[1]) + fieldName.Substring(2) : char.ToUpper(fieldName[0]) + fieldName.Substring(1); // 创建属性名称
            return propertyName;
        }

        /// <summary>
        /// 检查是否继承了某个类
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        internal static bool Inherits(this INamedTypeSymbol symbol, Func<INamedTypeSymbol, bool> func)
        {
            while (symbol != null)
            {
                if (func.Invoke(symbol)) // 如果继承了该类
                    return true;

                symbol = symbol.BaseType;
            }
            return false;
        }
    }

}
