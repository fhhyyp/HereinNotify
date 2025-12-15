using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HereinNotify
{

    /// <summary>
    /// 生成对象的实体
    /// </summary>
    public class HereinNotifyObject : global::System.ComponentModel.INotifyPropertyChanged
    {
        /// <inheritdoc/> 
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        /// <summary>
        /// 验证是否相等，不相等时赋值并通知
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="storage">字段值</param>
        /// <param name="value">新的值</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns></returns>
        protected bool SetProperty<T>(ref T storage,
                                          T value, 
                                          [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// 通知某个属性发生改变
        /// </summary>
        /// <param name="propertyName"></param>
        public void OnPropertyChanged(string propertyName) =>  PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }
}
