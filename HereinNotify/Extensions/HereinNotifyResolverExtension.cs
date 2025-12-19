using HereinNotify.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace HereinNotify.Extensions
{
    /// <summary>
    /// MyAttributeResolver
    /// </summary>

    internal static class HereinNotifyResolverExtension
    {

        /// <summary>
        /// 判断字段是否有默认值
        /// </summary>
        /// <param name="field"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static bool TryGetDefaultValue(this FieldDeclarationSyntax field, out string defaultValue)
        {
            if (field.Declaration.Variables.First().Initializer != null)
            {
                defaultValue = field.Declaration.Variables.First().Initializer.Value.ToString();
                return true;
            }
            else
            {
                defaultValue = null;
                return false;
            }
        }


        /// <summary>
        /// 判断字段是否为只读
        /// </summary>
        /// <param name="fieldDeclaration">字段的语法节点</param>
        /// <returns>如果字段是只读的，返回 true；否则返回 false</returns>
        public static bool IsReadonly(this FieldDeclarationSyntax fieldDeclaration)
        {
            // 判断字段是否有 readonly 修饰符
            return fieldDeclaration.Modifiers.Any(SyntaxKind.ReadOnlyKeyword);
        }

    }
}
