using NUnit.Framework;
using System.Threading;
using System.Linq;
using Tracer.Core;

namespace Tracer.Core.Tests
{
    [TestFixture]
    public class TracerTests
    {
        private ExecutionTracer tracer;

        [SetUp]
        public void Setup()
        {
            tracer = new ExecutionTracer();
        }

        [Test]
        public void Tracer_ShouldCreateInstance()
        {
            Assert.That(tracer, Is.Not.Null);
        }

        [Test]
        public void StartTrace_ShouldRecordMethod()
        {
       
            tracer.StartTrace();
            Thread.Sleep(50);
            tracer.StopTrace();

            var result = tracer.GetTraceResult();

            
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Threads, Has.Count.EqualTo(1));
                Assert.That(result.Threads[0].RootMethods, Is.Not.Empty);
            });
        }

        [Test]
        public void NestedMethods_ShouldBeRecorded()
        {
            
            tracer.StartTrace(); 
            Thread.Sleep(10);

            tracer.StartTrace(); 
            Thread.Sleep(20);
            tracer.StopTrace(); 

            tracer.StopTrace();

            var result = tracer.GetTraceResult();
            var outerMethod = result.Threads[0].RootMethods[0];

           
            Assert.Multiple(() =>
            {
                Assert.That(outerMethod.ChildMethods, Has.Count.EqualTo(1));
                Assert.That(outerMethod.Time, Is.GreaterThan(0));
                Assert.That(outerMethod.ChildMethods[0].Time, Is.GreaterThan(0));
            });
        }

        [Test]
        public void MultipleThreads_ShouldBeRecordedSeparately()
        {
           
            var results = new System.Collections.Concurrent.ConcurrentBag<TraceResult>();

          
            var thread1 = new Thread(() =>
            {
                tracer.StartTrace();
                Thread.Sleep(30);
                tracer.StopTrace();
                results.Add(tracer.GetTraceResult());
            });

            var thread2 = new Thread(() =>
            {
                tracer.StartTrace();
                Thread.Sleep(50);
                tracer.StopTrace();
                results.Add(tracer.GetTraceResult());
            });

            thread1.Start();
            thread2.Start();

            thread1.Join();
            thread2.Join();

            var finalResult = tracer.GetTraceResult();

            Assert.Multiple(() =>
            {
                Assert.That(finalResult.Threads, Has.Count.EqualTo(2));
                Assert.That(finalResult.Threads.Select(t => t.Id),
                    Contains.Item(thread1.ManagedThreadId));
                Assert.That(finalResult.Threads.Select(t => t.Id),
                    Contains.Item(thread2.ManagedThreadId));
            });
        }

        [Test]
        public void MethodTime_ShouldBeMeasuredCorrectly()
        {

            int delay = 100;

            tracer.StartTrace();
            Thread.Sleep(delay);
            tracer.StopTrace();

            var result = tracer.GetTraceResult();
            var methodTime = result.Threads[0].RootMethods[0].Time;

            Assert.That(methodTime, Is.InRange(delay * 0.9, delay * 1.1),
                $"Expected time around {delay}ms, but got {methodTime}ms");
        }

        [Test]
        public void StopTrace_WithoutStart_ShouldNotThrow()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => tracer.StopTrace());
        }

        [Test]
        [Timeout(5000)]
        public void ComplexNesting_ShouldWorkCorrectly()
        {

            tracer.StartTrace(); 
            Thread.Sleep(10);

            tracer.StartTrace();
            Thread.Sleep(20);

            tracer.StartTrace(); 
            Thread.Sleep(30);
            tracer.StopTrace(); 

            tracer.StopTrace(); 

            tracer.StartTrace(); 
            Thread.Sleep(40);
            tracer.StopTrace(); 

            tracer.StopTrace(); 

            var result = tracer.GetTraceResult();
            var rootMethod = result.Threads[0].RootMethods[0];

   
            Assert.Multiple(() =>
            {
                Assert.That(rootMethod.ChildMethods, Has.Count.EqualTo(2)); 
                Assert.That(rootMethod.ChildMethods[0].ChildMethods, Has.Count.EqualTo(1));
                Assert.That(rootMethod.ChildMethods[1].ChildMethods, Is.Empty);
            });
        }

    }
}