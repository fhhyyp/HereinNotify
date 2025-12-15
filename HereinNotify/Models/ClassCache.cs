using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HereinNotify.Models
{
    internal abstract class ClassCache
    {
        public ClassCache(ClassDeclarationSyntax classDeclaration)
        {
            var className = classDeclaration.Identifier.Text;
            var @namespace = GeneratorHelper.GetNamespace(classDeclaration);

            ClassName = className;
            Namespace = @namespace;
            Syntax = classDeclaration;
        }

        public ClassCache(string @namespace, string name)
        {
        }

        /// <summary>
        /// 是否继承
        /// </summary>
        public bool IsInherited { get; set; }

        /// <summary>
        /// 类名
        /// </summary>
        public string ClassName { get; }

        /// <summary>
        /// 类名
        /// </summary>
        public string Namespace { get; }

        /// <summary>
        /// 类符号
        /// </summary>
        public ClassDeclarationSyntax Syntax { get; }

        /// <summary>
        /// 类特性缓存
        /// </summary>
        public SymbolCache Cache { get; } = new SymbolCache();


        /// <summary>
        /// 字段符号缓存
        /// </summary>
        protected Dictionary<string, FieldCache> FieldCaches { get; } = new Dictionary<string, FieldCache>();


        public virtual FieldCache AddField(VariableDeclaratorSyntax variable, string fieldName, string type)
        {
            if (FieldCaches.TryGetValue(fieldName, out var fieldCache))
            {
                return fieldCache;
            }
            else
            {
                var field = new FieldCache(variable, fieldName, type);
                FieldCaches.Add(field.Name, field);
                return field;
            }
        }

        public List<FieldCache> GetFields() => FieldCaches.Values.ToList();

        public override string ToString()
        {
            return $"{Namespace}.{ClassName} : {Cache}";
        }




    }
}
