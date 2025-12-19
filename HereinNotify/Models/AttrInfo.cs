using System.Collections.Generic;

namespace HereinNotify.Models
{
    internal class AttrInfo
    {
        public string AttrTypeName { get; }

        /// <summary>
        /// 一个符号可能存在多个特性
        /// </summary>
        public Dictionary<string, AttrMemberInfo> Members { get; } = new Dictionary<string, AttrMemberInfo>();

        internal AttrInfo(string attrTypeName)
        {
            AttrTypeName = attrTypeName;
        }

        public bool ContainsMember(string memberName)
        {
            return Members.ContainsKey(memberName);
        }

        public void AddMember(string memberName, object value)
        {
            var key = memberName;
            if (!Members.TryGetValue(key, out var member))
            {
                member = new AttrMemberInfo(memberName);
                Members.Add(key, member);
            }
            member.AddValue(value);
        }

        public AttrMemberInfo GetMenber(string attrName)
        {
            if (Members.TryGetValue(attrName, out var item))
            {
                return item;
            }
            else
            {
                return null;
            }
        }

        public override string ToString()
        {
            return $"Attr:{AttrTypeName} -> {string.Join(",", Members.Values)}";
        }
    }
}
