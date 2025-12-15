using System;
using System.Collections.Generic;
using System.Text;

namespace HereinNotify.Demo.LitheDto
{
    [LitheDto(Source = typeof(TestObject1))]
    [LitheDto(Source = typeof(TestObject2), IsUseINPC = true)] // 标记了IsUseINPC为true时，就会自动实现 INotifyPropertyChanged
    internal partial class TestDto
    {

        partial void OnIdChanged(int oldValue, int newValue)
        {
        }
    }

    internal class TestObject1
    {
        /// <summary>
        /// 测试1
        /// </summary>
        public int ValueX { get; set; }

        /// <summary>
        /// 测试2
        /// </summary>
        public int ValueY { get; set; }

        /// <summary>
        /// 测试3
        /// </summary>
        public int ValueZ { get; set; }
    }
    internal class TestObject2
    {
        public int Id { get; set; }
        public DateTime AtCreate { get; set; }
    }

    public static class TestDtoGenerator
    {
        public static void Run()
        {
            Console.WriteLine("\r\n===============");
            var obj1 = new TestObject1
            {
                ValueX = 999,
                ValueY = 666,
                ValueZ = 886,
            };

            var obj2 = new TestObject2
            {
                Id = 114514,
                AtCreate = DateTime.Now,
            };
            var dto = new TestDto().InputTestObject1(obj1).InputTestObject2(obj2);
            Console.WriteLine(dto.ValueX);


            var dto2 = new TestDto
            {
                Id = 10101,
                ValueX = 4321,
                ValueY = 1234,
                AtCreate = new DateTime(1999, 5, 1, 12, 12, 12)
            };
            var oobj1 = dto2.OutputTestObject1();
            var oobj2 = dto2.OutputTestObject2();
        }
    }


}
