using HereinNotify.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Xml.Linq;

namespace HereinNotify.Extensions
{
    internal static class ClassCacheExtension
    {


        /// <summary>
        /// 构建类的特性缓存信息
        /// </summary>
        /// <param name="classCache"></param>
        /// <param name="classSymbol"></param>
        /// <param name="context"></param>
        /// <param name="customHandle"></param>
        /// <returns></returns>
        internal static void BuildCacheOfClass(this ClassCache classCache, INamedTypeSymbol classSymbol,
            GeneratorSyntaxContext context, Action<AttrInfo, AttributeData> customHandle = null)
        {
            if (customHandle is null)
            {
                throw new ArgumentNullException(nameof(customHandle));
            }


            var fieldDeclarations = classCache.Syntax.Members.OfType<FieldDeclarationSyntax>();
            var properryDeclarations = classCache.Syntax.Members.OfType<PropertyDeclarationSyntax>();

            classCache.BuildCacheOfField(context.SemanticModel, fieldDeclarations);
            classCache.BuildCacheOfProperty(context.SemanticModel, properryDeclarations);

            var attrs = classSymbol.GetAttributes();
            foreach (var attr in attrs)
            {
                var typeFullname = attr.AttributeClass.ToDisplayString(GeneratorConfig.GlobalFullTypeFormat);
                //var attributeName = attr.AttributeClass?.Name;
                var info = new AttrInfo(typeFullname);

                customHandle?.Invoke(info, attr);

                foreach (var nameArg in attr.NamedArguments)
                {
                    var key = nameArg.Key;
                    if (nameArg.Value.Kind == TypedConstantKind.Array)
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
          
            var classType = classSymbol.ToDisplayString(GeneratorConfig.GlobalFullTypeFormat);
            classCache.ClassFullName = classType;
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
        internal static void BuildCacheOfField(this ClassCache classCache,
            SemanticModel semanticModel,
            IEnumerable<FieldDeclarationSyntax> fieldDeclarationSyntaxes)
        {
            foreach (FieldDeclarationSyntax fieldSyntax in fieldDeclarationSyntaxes)
            {
                if (fieldSyntax.IsReadonly())
                {
                    continue;
                }

                // 获取类型符号
                var typeSyntax = fieldSyntax.Declaration.Type;

                var typeSymbol = semanticModel.GetTypeInfo(fieldSyntax.Declaration.Type).Type;
                if (typeSymbol == null)
                {
                    continue;
                }

                bool isNullableValueType = typeSyntax is INamedTypeSymbol named && named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;

                var variable = fieldSyntax.Declaration.Variables.First();
                var fieldName = variable.Identifier.Text;
                var defaultValue = fieldSyntax.TryGetDefaultValue(out  var dv) ? dv : null;
                var typeName = typeSymbol.ToDisplayString(GeneratorConfig.GlobalFullTypeFormat);

                var memberCacheInfo = new MemberCacheInfo
                {
                    Variable = variable,
                    Name = fieldName,
                    DefaultValue = defaultValue,
                    Kind = MemberKind.Field,
                    Type = typeName,
                    IsNullable = isNullableValueType,
                    IsStatic = semanticModel.GetDeclaredSymbol(variable).IsStatic
                };

                var fieldCache = classCache.AddMember(memberCacheInfo);

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

        /// <summary>
        /// <para>构建属性的缓存信息</para>
        /// <para>第1层：属性名称 - 特性集合</para>
        /// <para>第2层：特性名称 - 特性属性集合</para>
        /// <para>第3层：特性属性名称 - 对应的字面量</para>
        /// </summary>
        /// <param name="semanticModel"></param>
        /// <param name="classCache"></param>
        /// <param name="propertyDeclarationSyntaxes"></param>
        /// <returns>关于属性的特性缓存信息</returns>
        internal static void BuildCacheOfProperty(
                this ClassCache classCache,
                SemanticModel semanticModel,
                IEnumerable<PropertyDeclarationSyntax> propertyDeclarationSyntaxes)
        {
            foreach (PropertyDeclarationSyntax propertySyntax in propertyDeclarationSyntaxes)
            {
                // 忽略只读属性（无 set）
                if (propertySyntax.AccessorList == null ||
                    !propertySyntax.AccessorList.Accessors.Any(a => a.Kind() == SyntaxKind.SetAccessorDeclaration))
                {
                    continue;
                }

                // 获取类型 Symbol（关键）
                var typeSyntax = propertySyntax.Type;
                var typeInfo = semanticModel.GetTypeInfo(typeSyntax);
                var typeSymbol = typeInfo.Type;

                if (typeSymbol == null)
                {
                    continue;
                }

                // 属性名
                var propertyName = propertySyntax.Identifier.Text;

                // 最终类型
                var type = typeSymbol.ToDisplayString(
                    SymbolDisplayFormat.FullyQualifiedFormat
                        .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)
                );

                // 如果语法上是 nullable（?），确保类型字符串包含 ?
                if (typeSyntax is NullableTypeSyntax && !type.EndsWith("?"))
                {
                    type += "?";
                }

                // 5️⃣ 注册到 ClassCache
                /*var memberCacheInfo = new MemberCacheInfo
                {
                    Variable = variable,
                    Name = fieldName,
                    DefaultValue = defaultValue,
                    Kind = MemberKind.Field,
                    Type = typeName,
                    IsNullable = isNullableValueType,
                    IsStatic = semanticModel.GetDeclaredSymbol(variable).IsStatic
                } ;*/


                // 6️⃣ 处理属性特性
                var attributes = propertySyntax.AttributeLists;
                if (attributes.Count == 0)
                {
                    continue;
                }

                foreach (var attributeList in attributes)
                {
                    foreach (var attribute in attributeList.Attributes)
                    {
                        var info = LoadFieldSyntax(attribute, semanticModel);
                    }
                }
            }
        }




        private static AttrInfo LoadFieldSyntax(AttributeSyntax attribute, SemanticModel semanticModel)
        {
            var typeFullname = semanticModel.GetTypeInfo(attribute).Type.ToDisplayString(GeneratorConfig.GlobalFullTypeFormat);
            // var typeFullname = attr.AttributeClass.ToDisplayString(GeneratorConfig.GlobalFullTypeFormat);
            AttrInfo info = new AttrInfo(typeFullname);

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
        /// 生成 using 引用
        /// </summary>
        /// <param name="generator"></param>
        /// <returns></returns>
        internal static GeneratorCache<T> GeneratorUsing<T>(this GeneratorCache<T> generator) where T : ClassCache
        {
            foreach (var ns in GeneratorConfig.DefaultUsings)
            {
                generator.AppendCode($"using {ns};");
            }

            var nns = generator.ClassCache.Cache.GetAttr<HereinUsingAttribute>()?
                                       .GetMenber(nameof(HereinUsingAttribute.Namespace))?
                                       .GetValues() ?? new List<object>();

            foreach (var ns in nns)
            {
                generator.AppendCode($"using {ns.ToString()};");
            }

            return generator;
        }



        /// <summary>
        /// 实现INPC接口
        /// </summary>
        /// <param name="generator"></param>
        /// <returns></returns>
        internal static GeneratorCache<T> GeneratorINPC<T>(this GeneratorCache<T> generator) where T : ClassCache
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
    }
}