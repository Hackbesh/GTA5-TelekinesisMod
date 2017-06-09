using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace TelekinesisMod
{
    public class CoroutineCore
    {
        private readonly object lockObject = new object();

        private LinkedList<IEnumerator<object>> coroutineList = new LinkedList<IEnumerator<object>>();

        private Queue<LinkedListNode<IEnumerator<object>>> removeQueue = new Queue<LinkedListNode<IEnumerator<object>>>();

        public int Count => coroutineList.Count;

        public IDisposable Start(IEnumerable<object> coroutine)
        {
            var enumerator = coroutine.SelectMany(e => e as IEnumerable<object> ?? new[] { e }).GetEnumerator();
            enumerator.MoveNext();

            LinkedListNode<IEnumerator<object>> node;
            lock (lockObject)
            {
                node = coroutineList.AddLast(enumerator);
            }

            return Disposable.Create(() =>
            {
                lock (lockObject)
                {
                    removeQueue.Enqueue(node);
                }
            });
        }

        internal void Run()
        {
            lock (lockObject)
            {
                while (removeQueue.Count > 0)
                {
                    coroutineList.Remove(removeQueue.Dequeue().Value);
                }
            }

            var removeList = new List<LinkedListNode<IEnumerator<object>>>(coroutineList.Count);
            for (var node = coroutineList.First; node != null; node = node.Next)
            {
                if (!node.Value.MoveNext())
                {
                    removeList.Add(node);
                }
            }

            lock (lockObject)
            {
                foreach (var node in removeList)
                {
                    coroutineList.Remove(node);
                }
            }
        }
    }
}