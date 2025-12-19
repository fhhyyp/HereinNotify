using HereinNotify.ControlToolkits;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Xml.Linq;

namespace HereinNotify.Models
{

    internal enum MemberKind
    {
        Field,
        Property
    }

    internal class MemberCacheInfo
    {
        public VariableDeclaratorSyntax Variable { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public MemberKind Kind { get; set; }
        public string DefaultValue { get; set; }

        public bool IsStatic { get; set; }
        public bool IsNullable { get;  set; }
    }

    internal class MemberCache
    {
        public MemberCache(MemberCacheInfo info)
        {
            Variable = info.Variable;
            // 获取字段名称和类型
            Name = info.Name;
            Type = info.Type;
            Kind = info.Kind;
            DefaultValue = info.DefaultValue;
            IsStatic = info.IsStatic;
            IsNullable = info.IsNullable;
        }

        /// <summary>
        /// 成员特性缓存
        /// </summary>
        public SymbolCache Cache { get; } = new SymbolCache();

        /// <summary>
        /// 变量符号
        /// </summary>
        public VariableDeclaratorSyntax Variable { get; }

        /// <summary>
        /// 成员命名
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 成员类型
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// 类型
        /// </summary>
        public MemberKind Kind { get; }

        /// <summary>
        /// 默认值
        /// </summary>
        public string DefaultValue { get; }
        public bool IsStatic { get; }
        public bool IsNullable { get; }

        public override string ToString()
        {
            return $"{Name}[{Type}] -> {Cache}";
        }

    }
}
