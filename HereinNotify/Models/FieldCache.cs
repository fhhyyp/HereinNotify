using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace HereinNotify.Models
{
    internal class FieldCache
    {

        public FieldCache(VariableDeclaratorSyntax variable, string fieldName, string type)
        {
            Variable = variable;
            // 获取字段名称和类型
            Name = fieldName;
            Type = type;
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



        public override string ToString()
        {
            return $"{Name}[{Type}] -> {Cache}";
        }
    }
}
