using HereinNotifyDemo.LitheDtoTest;
using System;
namespace HereinNotifyDemo
{


    internal class Program
    {
        private int myField;

        public int MyProperty
        {
            get => myField;
            set
            {
                myField = value;
            }
        }

        static void Main(string[] args)
        {
            TestHereinNotifyPropertyGenerator.Run();
            TestDtoGenerator.Run();
            UserAndRoleDemo.Run();

            Console.ReadLine();
        }
    }
}
