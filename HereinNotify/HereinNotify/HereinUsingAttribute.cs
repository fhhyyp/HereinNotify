using System;

namespace HereinNotify
{
    /// <summary>
    /// 用于引用命名空间
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class HereinUsingAttribute : Attribute
    {
        /// <summary>
        /// 需要引入的命名空间
        /// </summary>
        public string Namespace { get; }

        /// <summary>
        /// 使用命名空间(不需要using)
        /// </summary>
        /// <param name="namespace"></param>
        public HereinUsingAttribute(string @namespace)
        {
            Namespace = @namespace;
        }
    }

}
