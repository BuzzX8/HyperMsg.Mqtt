using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt
{
    internal class RequestStorage
    {
        private readonly ConcurrentDictionary<ushort, object> storage = new();

        public void AddOrReplace<T>(ushort requestId, T request)
        { }

        public bool Contains<T>(ushort requestId)
        {
            throw new NotImplementedException();
        }

        public T Get<T>(ushort requestId)
        {
            throw new NotImplementedException();
        }

        public bool TryGet<T>(ushort requestId, out T request)
        {
            if (storage.TryGetValue(requestId, out var value) && value is T val)
            {
                request = val;
                return true;
            }

            request = default;
            return false;
        }

        public void Remove<T>(ushort requestId)
        { }
    }
}
