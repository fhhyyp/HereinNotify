using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace HereinNotify.Models
{
    internal class SymbolCache
    {
        /// <summary>
        /// string: attribute 名称
        /// </summary>
        private readonly Dictionary<string, AttrInfo> Attrs = new Dictionary<string, AttrInfo>();

        public bool ContainsAttr<TAttribute>() where TAttribute : Attribute
        {
            var type = typeof(TAttribute);
            var fullName = $"global::{type.FullName}";
            return Attrs.ContainsKey(fullName);
        }

        public void AddInfo(AttrInfo symbolAttrInfo)
        {
            var attrName = symbolAttrInfo.AttrTypeName;
            if (Attrs.TryGetValue(attrName, out var attrInfo))
            {
                foreach (var member in symbolAttrInfo.Members)
                {
                    var memberName = member.Key;
                    foreach(var value in member.Value.Values)
                    {
                        attrInfo.AddMember(memberName, value);
                    }
                }
            }
            else
            {
                Attrs.Add(attrName, symbolAttrInfo);
            }
        }

        public AttrInfo GetAttr<TAttribute>() where TAttribute : Attribute
        {
            var type = typeof(TAttribute);
            var fullName = $"global::{type.FullName}";

            if (Attrs.TryGetValue(fullName, out var attrInfo))
            {
                return attrInfo;
            }
            else
            {
                return null;
            }
        }

        public TMember GetAttr<TAttribute, TMember>(Expression<Func<TAttribute, TMember>> expression) where TAttribute : Attribute
        {
            var type = typeof(TAttribute);
            var fullName = $"global::{type.FullName}";

            if (!Attrs.TryGetValue(fullName, out var attrInfo))
            {
                return default;
            }
            var memberName = GetPropName(expression);
            if (string.IsNullOrEmpty(memberName))
            {
                return default;
            }
            var memberInfo = attrInfo.GetMenber(memberName);
            if (memberInfo is null)
            {
                return default;
            }
            return memberInfo.GetFirstValue<TMember>();
        }
        private static string GetPropName<T, TResult>(Expression<Func<T, TResult>> expr)
        {
            if (expr.Body is MemberExpression memberExpr)
                return memberExpr.Member.Name;

            if (expr.Body is UnaryExpression unary && unary.Operand is MemberExpression innerMember)
                return innerMember.Member.Name;

            return null;
        }

        public override string ToString()
        {
            return $"Attr Cache:{string.Join(",", Attrs.Keys)}";
        }

    }
}
