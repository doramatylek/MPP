using System;
using System.Threading;
using Tracer.Core;
using Tracer.Serialization;

class Program
{
    static void Main()
    {
        ITracer tracer = new ExecutionTracer();

        var shop = new ShopService(tracer);
        var notifier = new NotificationService(tracer);

        var t1 = new Thread(shop.ProcessOrder);
        var t2 = new Thread(notifier.SendEmailNotification);

        t1.Start();
        t2.Start();

        t1.Join();
        t2.Join();

        var result = tracer.GetTraceResult();

        var pluginLoader = new PluginLoader();
        pluginLoader.LoadPlugins("..\\..\\..\\Plugins");

        pluginLoader.SaveResults(result);
        Console.WriteLine("Tracing finished. Results saved.");
    }
}

public class ShopService
{
    private readonly ITracer tracer;

    public ShopService(ITracer tracer)
    {
        this.tracer = tracer; 
    }

    public void ProcessOrder()
    {
        tracer.StartTrace();

        ValidateOrder();
        CalculatePrice();
        SaveOrder();

        tracer.StopTrace();
    }

    private void ValidateOrder()
    {
        tracer.StartTrace();
        Thread.Sleep(50);
        tracer.StopTrace();
    }

    private void CalculatePrice()
    {
        tracer.StartTrace();

        Thread.Sleep(30);
        ApplyDiscount();

        tracer.StopTrace();
    }

    private void ApplyDiscount()
    {
        tracer.StartTrace();
        Thread.Sleep(20);
        tracer.StopTrace();
    }

    private void SaveOrder()
    {
        tracer.StartTrace();
        Thread.Sleep(40);
        tracer.StopTrace();
    }
}

public class NotificationService
{
    private readonly ITracer tracer;

    public NotificationService(ITracer tracer)
    {
        this.tracer = tracer; 
    }

    public void SendEmailNotification()
    {
        tracer.StartTrace();

        BuildEmail();
        SendEmail();

        tracer.StopTrace();
    }

    private void BuildEmail()
    {
        tracer.StartTrace();
        Thread.Sleep(25);
        tracer.StopTrace();
    }

    private void SendEmail()
    {
        tracer.StartTrace();
        Thread.Sleep(35);
        tracer.StopTrace();
    }
}