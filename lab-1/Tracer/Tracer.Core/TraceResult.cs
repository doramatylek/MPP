using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracer.Core
{
    public class MethodInfo
    {

        public string Name { get; set; } = "";

        public string ClassName { get; set; } = "";

        public long Time { get; set; }


        public IList<MethodInfo> ChildMethods { get; private set; }

        private Stopwatch executionTimer = new Stopwatch();

        public void AddChildMethod(MethodInfo rootMethod)
        {

            ChildMethods.Add(rootMethod);

        }

        public void Start()
        {
            executionTimer.Start();
        }
        public void Stop()
        {
            executionTimer.Stop();
            Time = executionTimer.ElapsedMilliseconds;

        }
        public MethodInfo()
        {
            ChildMethods = new List<MethodInfo>();
        }
    }
    public class ThreadInfo
    {
        public int Id { get; set; }

        public long Time { get; set; }

        public IList<MethodInfo> RootMethods { get; private set; }

        public void AddRootMethod( MethodInfo rootMethod) {

            RootMethods.Add(rootMethod);
            Time += rootMethod.Time;

        }
        public ThreadInfo()
        {
            RootMethods = new List<MethodInfo>();
        }

    }

    public class TraceResult
    {
        public IReadOnlyList<ThreadInfo> Threads { get; }
        public TraceResult(IReadOnlyList<ThreadInfo> threads)
        {
            Threads = new List<ThreadInfo>(threads).AsReadOnly();
        }
        public TraceResult()
        {
            Threads = new List<ThreadInfo>();
        }
    }
}
