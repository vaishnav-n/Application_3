using System;

namespace SampleApp3
{
    class Program
    {
        static void Main(string[] args)
        {
            int a, b, sum;
            Console.WriteLine("Enter Value of a and b");
            a = int.Parse(Console.ReadLine());
            b = int.Parse(Console.ReadLine());
            //OR
            //a = Convert.ToInt32(Console.ReadLine());
            //b = Convert.ToInt32(Console.ReadLine());
            sum = a + b;

            Console.WriteLine("The sum of {0} and {1} = {2}", a, b, sum);
            Console.Read();

        }
    }
}
