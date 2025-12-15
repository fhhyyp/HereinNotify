using HereinNotify.Demo.LitheDto;
using System;
namespace HereinNotify.Demo
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


            Console.ReadLine();
        }
    }
}
