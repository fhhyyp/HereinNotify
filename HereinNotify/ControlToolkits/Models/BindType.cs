namespace HereinNotify.ControlToolkits
{
    /// <summary>
    /// 控制生成什么样的属性
    /// </summary>
    public enum BindType
    {
        /// <summary>
        /// 默认（一般为单向绑定） FrameworkPropertyMetadataOptions.None
        /// </summary>
        Default,
        /// <summary>
        /// 双向绑定
        /// FrameworkPropertyMetadataOptions.BindsTwoWayByDefault
        /// </summary>
        TwoWay,
    }

}
