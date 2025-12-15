using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace HereinNotify
{


    /// <summary>
    /// 标记一个字段，为其自动创建属性属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public sealed class HereinNotifyPropertyAttribute : Attribute
    {

        /// <summary>
        /// 表示需要验证赋值，赋值前将调用 Verify{XXX}Setter 分部方法，如果验证失败，将会调用 On{XXX}VerifyFail 分部方法。
        /// </summary>
        public bool IsVerify = false;

        /// <summary>
        /// 表示需要监视属性是否变化，将会自动生成命名为 {XXX}IsChanged 的布尔（bool）变量（具备通知功能），完成赋值后对应的监视变量将会置为 true。
        /// </summary>
        public bool IsChanged = false;

        /// <summary>
        /// 表示当对应属性被赋值后，还需要通知哪些属性
        /// </summary>
        public string Notify = string.Empty;

        /// <summary>
        /// <para>表示需要为自动生成的属性声明特性，需要使用 typeof() 语法糖。</para>
        /// <para>如果特性来自其它命名空间，需要在当前实体类声明 <see cref="HereinUsingAttribute"/> 特性（传入命名空间）</para>
        /// <para>例如: [HereinUsing("System.Text.Json.Serialization")] </para>
        /// </summary>
        public Type Attr = null;

        /// <summary>
        /// <para>表示需要为自动生成的属性声明特性的入参，内容由你自定义（可以为空）。</para>
        /// </summary>
        public string AttrParmas = string.Empty;


    }
}
