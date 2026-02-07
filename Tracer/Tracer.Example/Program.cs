// See https://aka.ms/new-console-template for more information
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

        
        var pluginLoader = new  PluginLoader();
        pluginLoader.LoadPlugins("..\\..\\..\\Plugins");
    
       
     


        pluginLoader.SaveResults(result);
        Console.WriteLine("Tracing finished. Results saved.");
    }
}

public class ShopService
{
    private readonly ITracer _tracer;

    public ShopService(ITracer tracer)
    {
        _tracer = tracer;
    }

    public void ProcessOrder()
    {
        _tracer.StartTrace();

        ValidateOrder();
        CalculatePrice();
        SaveOrder();

        _tracer.StopTrace();
    }

    private void ValidateOrder()
    {
        _tracer.StartTrace();
        Thread.Sleep(50);
        _tracer.StopTrace();
    }

    private void CalculatePrice()
    {
        _tracer.StartTrace();

        Thread.Sleep(30);
        ApplyDiscount();

        _tracer.StopTrace();
    }

    private void ApplyDiscount()
    {
        _tracer.StartTrace();
        Thread.Sleep(20);
        _tracer.StopTrace();
    }

    private void SaveOrder()
    {
        _tracer.StartTrace();
        Thread.Sleep(40);
        _tracer.StopTrace();
    }
}

public class NotificationService
{
    private readonly ITracer _tracer;

    public NotificationService(ITracer tracer)
    {
        _tracer = tracer;
    }

    public void SendEmailNotification()
    {
        _tracer.StartTrace();

        BuildEmail();
        SendEmail();

        _tracer.StopTrace();
    }

    private void BuildEmail()
    {
        _tracer.StartTrace();
        Thread.Sleep(25);
        _tracer.StopTrace();
    }

    private void SendEmail()
    {
        _tracer.StartTrace();
        Thread.Sleep(35);
        _tracer.StopTrace();
    }
}

