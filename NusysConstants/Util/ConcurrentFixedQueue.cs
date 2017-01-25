using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    /// Util class that will allow you to get a queue with a max size.
    /// This means that it will automatically dequeue any item that is over the max size
    /// </summary>
    public class ConcurrentFixedQueue<T> : IEnumerable<T>
    {
        /// <summary>
        /// the max size of the queue.  Cannot be changed after constructed
        /// </summary>
        public int MaxSize { get; private set; }

        /// <summary>
        /// the private variable holding the concurrent queue
        /// </summary>
        private ConcurrentQueue<T> _queue;

        /// <summary>
        /// the lock object
        /// </summary>
        private readonly object syncObject = new object();

        /// <summary>
        /// the constructor takes in the max size of the queue
        /// </summary>
        /// <param name="maxSize"></param>
        public ConcurrentFixedQueue(int maxSize)
        {
            _queue = new ConcurrentQueue<T>();
            MaxSize = maxSize;
        }

        /// <summary>
        /// returns whether the queue is currently at its max capacity
        /// </summary>
        public bool IsFull
        {
            get
            {
                lock (syncObject)
                {
                    return _queue.Count == MaxSize;
                }
            }
        }

        /// <summary>
        /// The dequeue operation used to get the item most ready to be removed
        /// </summary>
        /// <returns></returns>
        public T Dequeue()
        {
            lock (syncObject)
            {
                T outT;
                _queue.TryDequeue(out outT);
                return outT;
            }
        }

        /// <summary>
        /// the enqueue call
        /// </summary>
        /// <param name="obj"></param>
        public void EnQueue(T obj)
        {
            lock (syncObject)
            {
                _queue.Enqueue(obj);
                if (_queue.Count > MaxSize)
                {
                    Dequeue();
                }
            }
        }

        /// <summary>
        /// interface required method
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            lock (syncObject)
            {
                return _queue.GetEnumerator();
            }
        }

        /// <summary>
        /// interface required method
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (syncObject)
            {
                return _queue.GetEnumerator();
            }
        }
    }
}
