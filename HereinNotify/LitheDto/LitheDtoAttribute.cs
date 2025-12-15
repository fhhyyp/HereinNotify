using System;

namespace HereinNotify
{
    /// <summary>
    /// 表示自动生成的属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class LitheDtoAttribute : Attribute
    {
        /// <summary>
        /// 实体类
        /// </summary>
        public Type Source = null;

        /// <summary>
        /// 忽略哪些属性
        /// </summary>
        public string[] Ignore = Array.Empty<string>();

        /// <summary>
        /// 是否生成通知行为
        /// </summary>
        public bool IsUseINPC = false;  

    }

}
