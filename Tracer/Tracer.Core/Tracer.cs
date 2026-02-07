using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Tracer.Core
{
    public class ExecutionTracer : ITracer
    {
      
        private Dictionary<int, ThreadInfo> Threads { get; set; }

       
        private ThreadLocal<Stack<MethodInfo>> Methods { get; set; }

        public ExecutionTracer()
        {
            Threads = new Dictionary<int, ThreadInfo>();
            Methods = new ThreadLocal<Stack<MethodInfo>>(() => new Stack<MethodInfo>());
        }

        public void StartTrace()
        {
            
            StackFrame frame = new StackFrame(1, false);
            var method = frame.GetMethod();

            MethodInfo methodInfo = new MethodInfo
            {
                Name = method?.Name ?? "Unknown",
                ClassName = method?.DeclaringType?.Name ?? "Unknown"
            };

            Methods.Value.Push(methodInfo);

            methodInfo.Start();
        }

        public void StopTrace()
        {
            var stack = Methods.Value;
            int threadId = Environment.CurrentManagedThreadId;

            if (stack.Count == 0)
            {
                throw new InvalidOperationException($"StopTrace called without StartTrace in thread {threadId}");
            }

            MethodInfo methodInfo = stack.Pop();
            methodInfo.Stop();

            if (stack.Count > 0)
            {
                stack.Peek().AddChildMethod(methodInfo);
            }
            else
            {
                if (!Threads.TryGetValue(threadId, out ThreadInfo threadInfo))
                {
                    threadInfo = new ThreadInfo { Id = threadId };
                    Threads[threadId] = threadInfo;
                }

                threadInfo.AddRootMethod(methodInfo);
            }
        }

        public TraceResult GetTraceResult()
        {
            return new TraceResult(Threads.Values.ToList());
        }
    }
}
