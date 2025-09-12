using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        // Арифметика
        Console.WriteLine(Add(1, 2.5)); 
        Console.WriteLine(Multiply(2, 3)); 

        // Логічні та умовні оператори
        int a = 5, b = 10;
        if (a < b)
        {
            Console.WriteLine("a < b");
        }
        else if (a == b)
        {
            Console.WriteLine("a == b");
        }
        else
        {
            Console.WriteLine("a > b");
        }

        bool logic = (a < b) || (b < 0);
        Console.WriteLine("Логіка: " + logic);

        // Масив і цикли
        int[] arr = { 1, 2, 3 };
        List<string> list = new List<string> { "A", "B" };

        for (int i = 0; i < arr.Length; i++)
        {
            Console.WriteLine(arr[i]);
        }

        int j = 0;
        while (j < arr.Length)
        {
            Console.WriteLine(arr[j++]);
        }

        int k = 0;
        do
        {
            Console.WriteLine(arr[k++]);
        } while (k < arr.Length);

        foreach (var item in list)
        {
            Console.WriteLine(item);
        }

        MyClass obj = new MyClass("Test");
        obj.PublicMethod();
    }

    static double Add(int a, double b)
    {
        return a + b;
    }

    static int Multiply(int a, int b)
    {
        return a * b;
    }
}

class MyClass
{
    private string name;

    public MyClass(string name)
    {
        this.name = name;
    }

    public void PublicMethod()
    {
        Console.WriteLine("Hello from " + name);
    }
}
