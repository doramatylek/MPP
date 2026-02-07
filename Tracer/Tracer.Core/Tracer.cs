using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Tracer.Core
{
    public class ExecutionTracer : ITracer
    {
        // Словарь потоков с результатами
        private Dictionary<int, ThreadInfo> Threads { get; set; }

        // Отдельный стек методов для каждого потока
        private ThreadLocal<Stack<MethodInfo>> Methods { get; set; }

        public ExecutionTracer()
        {
            Threads = new Dictionary<int, ThreadInfo>();
            Methods = new ThreadLocal<Stack<MethodInfo>>(() => new Stack<MethodInfo>());
        }

        public void StartTrace()
        {
            // Определяем текущий метод
            StackFrame frame = new StackFrame(1, false);
            var method = frame.GetMethod();

            MethodInfo methodInfo = new MethodInfo
            {
                Name = method?.Name ?? "Unknown",
                ClassName = method?.DeclaringType?.Name ?? "Unknown"
            };

            // Пушим метод в стек текущего потока
            Methods.Value.Push(methodInfo);

            // Запускаем таймер
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

            // Достаём метод из стека
            MethodInfo methodInfo = stack.Pop();
            methodInfo.Stop();

            if (stack.Count > 0)
            {
                // Есть родительский метод в стеке — добавляем как дочерний
                stack.Peek().AddChildMethod(methodInfo);
            }
            else
            {
                // Нет родителя — это корневой метод
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
