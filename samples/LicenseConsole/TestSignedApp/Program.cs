using System;
using TestComponent;
using DevZest.Licensing;

namespace TestSignedApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Calling TestComponent.MyComponent from SIGNED app:");
            Console.WriteLine("--------------------------------------------------");
            MyComponent.Feature1();
            MyComponent.Feature2();
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
