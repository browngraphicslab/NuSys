using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    /// public class for a thread-safe hashset.  Built on top of Concurrent Dictionary
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConcurrentHashSet<T> : IEnumerable<T>
    {
        /// <summary>
        /// the private instance variable for the concurrent dictionary we use to maintain concurrency
        /// </summary>
        private ConcurrentDictionary<T, byte> _dict;

        /// <summary>
        /// the number of items in the hashset
        /// </summary>
        public int Count
        {
            get { return _dict.Count; }
        }

        /// <summary>
        /// empty constrcutor
        /// </summary>
        public ConcurrentHashSet()
        {
            _dict = new ConcurrentDictionary<T, byte>();
        }

        /// <summary>
        /// this constructor allows you to pass in already-present T values
        /// </summary>
        /// <param name="startingSet"></param>
        public ConcurrentHashSet(IEnumerable<T> startingSet)
        {
            _dict = new ConcurrentDictionary<T, byte>(startingSet.ToDictionary(k => k, v=> new byte()));
        }


        /// <summary>
        /// clears the entire hashset of all values
        /// </summary>
        public void Clear()
        {
            _dict.Clear();
        }

        /// <summary>
        /// Removes the value from the hash set.  Returns true if successful
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Remove(T value)
        {
            byte outByte;
            return _dict.TryRemove(value, out outByte);
        }

        /// <summary>
        /// add the value to the hashset. Returns true if successful, false otherwise.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Add(T value)
        {
            return _dict.TryAdd(value, 0);
        }

        /// <summary>
        /// Returns a bool representing if the value is in the hash set or not
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(T value)
        {
            return _dict.ContainsKey(value);
        }

        /// <summary>
        /// Interface-required.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return _dict.Select(kvp => kvp.Key).GetEnumerator();
        }

        /// <summary>
        /// Interface-required.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dict.Select(kvp => kvp.Key).GetEnumerator();
        }
    }
}
