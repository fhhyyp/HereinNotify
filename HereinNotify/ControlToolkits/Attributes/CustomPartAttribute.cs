using System;

namespace HereinNotify.ControlToolkits
{
    /// <summary>
    /// 自动获取对应的模板控件，要求当前类必须继承自 DependencyObject 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class CustomPartAttribute : Attribute
    {
        /// <summary>
        /// 是否必须
        /// </summary>
        public bool IsRequired = true;
    }

}
