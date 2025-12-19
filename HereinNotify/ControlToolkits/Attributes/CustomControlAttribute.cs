using System;

namespace HereinNotify.ControlToolkits
{
    /// <summary>
    /// 自定义控件类型
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class CustomControlAttribute : Attribute
    {
        /// <summary>
        /// 控制生成的代码
        /// </summary>
        public ProjectType ProjectType { get; }

        /// <summary>
        /// 生成的代码类型
        /// </summary>
        /// <param name="projectType"></param>
        public CustomControlAttribute(ProjectType projectType)
        {
            ProjectType = projectType;
        }

        /// <summary>
        /// <para>用来控制在重写的 OnApplyTemplate 方法中是否调用  base.OnApplyTemplate(); </para>
        /// <para>因为当前控件类有字段/属性存在 <seealso cref="CustomPartAttribute"/> 特性时，将会自动重写 OnApplyTemplate 方法。</para>
        /// <para>如果设置为 true ，将不会调用父类的 OnApplyTemplate 方法，其控件加载行为需要你控制。</para>
        /// <para>推荐配合 OnPartLoaded 分部方法使用。</para>
        /// </summary>
        public bool IsCallBaseOnApplyTemplate = true;
    }

}
