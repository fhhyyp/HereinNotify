using HereinNotify.ControlToolkits;
using System.Windows;
using System.Windows.Controls;

namespace HereinNotifyDemoWpf
{

    [CustomControl(ProjectType.Wpf)]
    internal partial class Test2UserControl  : Control
    {

        [CustomProperty(BindingType = BindType.TwoWay, IsNew = true)]
        private static int _dataName ;

        [CustomPart]
        private Label PART_123;


    }
}
