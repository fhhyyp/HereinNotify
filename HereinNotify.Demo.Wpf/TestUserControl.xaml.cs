using HereinNotify.ControlToolkits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HereinNotifyDemoWpf
{
    /// <summary>
    /// TestUserControl.xaml 的交互逻辑
    /// </summary>
    [CustomControl(ProjectType.Wpf)]
    public partial class TestUserControl : UserControl
    {

        [CustomProperty(BindingType = BindType.TwoWay, IsAttached = true)]
        private int MyProperty = 99999;

        partial void OnMyPropertyChanged(int oldValue, int newValue)
        {
            
        }


        public TestUserControl()
        {
            InitializeComponent();
            this.Loaded += TestUserControl_Loaded;
        }


        private void TestUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            TestUserControl.SetMyProperty(this, 2334);
        }
    }
}
