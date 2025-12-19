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
        public List<Action<SourceProductionContext>> SendGeneratorError { get; set; } = new List<Action<SourceProductionContext>>();


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
        protected Dictionary<string, MemberCache> MemberCaches { get; } = new Dictionary<string, MemberCache>();
        public string ClassFullName { get; internal set; }

        public virtual MemberCache AddMember(MemberCacheInfo info) 
        {
            if (MemberCaches.TryGetValue(info.Name, out var fieldCache))
            {
                return fieldCache;
            }
            else
            {
                var field = new MemberCache(info);
                MemberCaches.Add(field.Name, field);
                return field;
            }
        }

        public List<MemberCache> GetMembers() => MemberCaches.Values.ToList();

        public override string ToString()
        {
            return $"{Namespace}.{ClassName} : {Cache}";
        }




    }
}
