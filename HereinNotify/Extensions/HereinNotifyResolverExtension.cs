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
        /// 构建类的特性缓存信息
        /// </summary>
        /// <param name="classCache"></param>
        /// <param name="classSymbol"></param>
        /// <param name="customHandle"></param>
        /// <returns></returns>
        internal static void BuildCacheOfClass(this ClassCache classCache, INamedTypeSymbol classSymbol, 
                                               Action<AttrInfo, AttributeData> customHandle = null)
        {
            if (customHandle is null)
            {
                throw new ArgumentNullException(nameof(customHandle));
            }
            
            var attrs = classSymbol.GetAttributes();
            foreach (var attr in attrs)
            {
                var attributeName = attr.AttributeClass?.Name;
                var info = new AttrInfo(attributeName);

                customHandle?.Invoke(info, attr);

                foreach (var nameArg in attr.NamedArguments)
                {
                    var key = nameArg.Key;
                    if(nameArg.Value.Kind == TypedConstantKind.Array)
                    {
                        var value = nameArg.Value.Values;
                        info.AddMember(key, value);

                    }
                    else
                    {

                        var value = nameArg.Value.Value;
                        info.AddMember(key, value);
                    }

                }

                classCache.Cache.AddInfo(info);
            }

            //return attributesOfClass;

        }

        /// <summary>
        /// <para>构建字段的缓存信息</para>
        /// <para>第1层：字段名称 - 特性集合</para>
        /// <para>第2层：特性名称 - 特性属性集合</para>
        /// <para>第3层：特性属性名称 - 对应的字面量</para>
        /// </summary>
        /// <param name="semanticModel"></param>
        /// <param name="classCache"></param>
        /// <param name="fieldDeclarationSyntaxes"></param>
        /// <returns>关于字段的特性缓存信息</returns>
        internal static void BuildCacheOfField(this ClassCache classCache, SemanticModel semanticModel, IEnumerable<FieldDeclarationSyntax> fieldDeclarationSyntaxes)
        {
            foreach (FieldDeclarationSyntax fieldSyntax in fieldDeclarationSyntaxes)
            {
                if (fieldSyntax.IsReadonly())
                {
                    continue;   
                }

                // 获取类型符号
                var typeSyntax = fieldSyntax.Declaration.Type;
                var typeInfo = semanticModel.GetTypeInfo(typeSyntax);
                var typeSymbol = typeInfo.Type;

                // 如果是如 int 的 简单类型，typeSymbol 也是内置类型 Symbol
                var typeNamespace = typeSymbol?.ContainingNamespace?.ToDisplayString();

                var variable = fieldSyntax.Declaration.Variables.First();
                
                var fieldName = variable.Identifier.Text;
                var fieldType = fieldSyntax.Declaration.Type.ToString();

                var type = fieldType[fieldType.Length -1] == '?' ? $"{typeSymbol}?" : $"{typeSymbol}";

                var fieldCache = classCache.AddField(variable, fieldName, type);

                var attributes = fieldSyntax.AttributeLists;

                if (attributes.Count == 0) { continue; }
                foreach (var attributeList in attributes)
                {
                    if (attributeList.Attributes.Count == 0) { continue; }
                    foreach (var attribute in attributeList.Attributes)
                    {
                        var info = LoadFieldSyntax(attribute, semanticModel);
                        fieldCache.Cache.AddInfo(info);
                    }
                }

            }
        }

        private static AttrInfo LoadFieldSyntax(AttributeSyntax attribute, SemanticModel semanticModel)
        {

            var attributeName = attribute.Name.ToString(); // 特性名称
            AttrInfo info = new AttrInfo(attributeName);

            var arguments = attribute.ArgumentList?.Arguments;
            if (arguments == null || arguments.Value.Count == 0)
            {
                return info;
            }

            // 解析命名属性
            foreach (var argument in arguments)
            {
                var memberName = argument.NameEquals?.Name?.ToString();
                if (string.IsNullOrEmpty(memberName))
                {
                    continue;
                }
                var expr = argument.Expression;

                object value = TryGetValue(expr, semanticModel);
                info.AddMember(memberName, value);
            }
            return info;
        }


        private static object TryGetValue(ExpressionSyntax expr, SemanticModel model)
        {
            // 处理 nameof()
            if (expr is InvocationExpressionSyntax invocation &&
                invocation.Expression is IdentifierNameSyntax id &&
                id.Identifier.Text == "nameof")
            {
                var constant = model.GetConstantValue(expr);
                if (constant.HasValue)
                    return constant.Value;
            }

            // 处理 typeof()
            if (expr is TypeOfExpressionSyntax typeOfExpr)
            {                
                var typeInfo = model.GetTypeInfo(typeOfExpr.Type);
                return typeInfo.Type?.ToDisplayString();
            }

            // 字面量
            if (expr is LiteralExpressionSyntax literal)
            {
                return literal.Token.Value;
            }

            // 其它的常量表达式
            var cv = model.GetConstantValue(expr);
            if (cv.HasValue)
                return cv.Value;

            // fallback
            return expr.ToString();
        }


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
