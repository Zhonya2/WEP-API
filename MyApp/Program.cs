using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Generic інтерфейс
interface IGeneric<T>
{
    T Method(T value);
}

// Звичайний інтерфейс
interface INormal
{
    void SayHello();
}

// Абстрактний клас
abstract class MyBase
{
    public abstract void AbstractMethod();

    public void NormalMethod()
    {
        Console.WriteLine("Звичайний метод з абстрактного класу");
    }
}

// Клас, який наслідує та імплементує інтерфейси
class MyClass : MyBase, IGeneric<int>, INormal
{
    public int Method(int value)
    {
        return value * 2;
    }

    public void SayHello()
    {
        Console.WriteLine("Інтерфейсу I:");
    }

    public override void AbstractMethod()
    {
        Console.WriteLine("Реалізація абстрактного методу");
    }
}

class Program
{
    static async Task RunTasksWaitAll()
    {
        Random rnd = new Random();

        Task t1 = Task.Run(async () =>
        {
            await Task.Delay(rnd.Next(1000, 3000));
            Console.WriteLine("Task 1");
        });

        Task t2 = Task.Run(async () =>
        {
            await Task.Delay(rnd.Next(1000, 3000));
            Console.WriteLine("Task 2");
        });

        Task t3 = Task.Run(async () =>
        {
            await Task.Delay(rnd.Next(1000, 3000));
            Console.WriteLine("Task 3");
        });

        await Task.WhenAll(t1, t2, t3);
    }

    static async Task RunTasksWhenAny()
    {
        Random rnd = new Random();

        Task<string> t1 = Task.Run(async () =>
        {
            await Task.Delay(rnd.Next(1000, 3000));
            return "Task 1";
        });

        Task<string> t2 = Task.Run(async () =>
        {
            await Task.Delay(rnd.Next(1000, 3000));
            return "Task 2";
        });

        Task<string> t3 = Task.Run(async () =>
        {
            await Task.Delay(rnd.Next(1000, 3000));
            return "Task 3";
        });

        Task<string> finished = await Task.WhenAny(t1, t2, t3);
        Console.WriteLine("Першим завершився: " + finished.Result);
    }

    static async Task Main()
    {
        // Перевірка першої частини
        MyClass obj = new MyClass();
        obj.SayHello();
        obj.AbstractMethod();
        obj.NormalMethod();
        Console.WriteLine("Результат generic методу: " + obj.Method(5));

        // Перевірка Task'ів
        Console.WriteLine("\n=== WhenAll ===");
        await RunTasksWaitAll();

        Console.WriteLine("\n=== WhenAny ===");
        await RunTasksWhenAny();
    }
}
