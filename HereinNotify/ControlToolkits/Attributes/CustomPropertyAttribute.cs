using System;

namespace HereinNotify.ControlToolkits
{
    /// <summary>
    /// 标记在字段或属性上，自动生成附加属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class CustomPropertyAttribute : Attribute
    {
        /// <summary>
        /// 是否为独立的附加属性
        /// </summary>
        public bool IsAttached = false;

        /// <summary>
        /// 是否为新属性，即隐藏父类的附加属性（名称相同）
        /// </summary>
        public bool IsNew = false;

        /// <summary>
        /// 绑定类型
        /// </summary>
        public BindType BindingType = BindType.Default;
    }

}
