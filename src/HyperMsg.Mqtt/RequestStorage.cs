using System;
using System.Collections.Concurrent;

namespace HyperMsg.Mqtt
{
    public class RequestStorage
    {
        private readonly ConcurrentDictionary<(ushort, Type), object> storage = new();

        public void AddOrReplace<T>(ushort requestId, T request)
        { 
            storage[(requestId, typeof(T))] = request;
        }

        public bool Contains<T>(ushort requestId) => storage.ContainsKey((requestId, typeof(T)));

        public T Get<T>(ushort requestId)
        {
            if (Contains<T>(requestId))
            {
                return (T)storage[(requestId, typeof(T))];
            }

            throw new InvalidOperationException();
        }

        public bool TryGet<T>(ushort requestId, out T request)
        {
            if (storage.TryGetValue((requestId, typeof(T)), out var value))
            {
                request = (T)value;
                return true;
            }

            request = default;
            return false;
        }

        public void Remove<T>(ushort requestId)
        {
            if (!Contains<T>(requestId))
                return;

            storage.TryRemove((requestId, typeof(T)), out _);
        }
    }
}
