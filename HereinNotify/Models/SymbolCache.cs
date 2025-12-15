using System.Collections.Generic;

namespace HereinNotify.Models
{
    internal class SymbolCache
    {
        /// <summary>
        /// string: attribute 名称
        /// </summary>
        private readonly Dictionary<string, AttrInfo> Attrs = new Dictionary<string, AttrInfo>();

        public bool ContainsAttr(string attributeName)
        {
            return Attrs.ContainsKey(attributeName);
        }

        public void AddInfo(AttrInfo symbolAttrInfo)
        {
            var attrName = symbolAttrInfo.AttrName;
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

        public AttrInfo GetAttr(string attrName)
        {
            if (Attrs.TryGetValue(attrName, out var attrInfo))
            {
                return attrInfo;
            }
            else
            {
                return null;
            }
        }


        public override string ToString()
        {
            return $"Attr Cache:{string.Join(",", Attrs.Keys)}";
        }

    }
}
