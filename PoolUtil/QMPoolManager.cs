using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeProject.ObjectPool;
using IBM.WMQ;

namespace PoolUtil
{
    public class QMPoolManager : IDisposable
    {
        private static object _thisLock = new object();
        private ConcurrentDictionary<string, TimedObjectPool<QMResource>> _qmPools = new ConcurrentDictionary<string, TimedObjectPool<QMResource>>();
        private int _maxPoolSize;
        private TimeSpan _timeout;

        public QMPoolManager(int maxPoolSize, TimeSpan timeout)
        {
            _maxPoolSize = maxPoolSize;
            _timeout = timeout;
        }

        public TimedObjectPool<QMResource> GetQMPool(string queueManagerName, string hostName, int port, string channelName)
        {

                string key = queueManagerName + "@@" + hostName + "@@" + port + "@@" + channelName;
                if (_qmPools.ContainsKey(key))
                    return _qmPools[key];
                else
                {
                    var qmPool = new TimedObjectPool<QMResource>(_maxPoolSize, () => new QMResource(queueManagerName, hostName, port, channelName), _timeout);
                    _qmPools.TryAdd(key, qmPool);
                    return qmPool;
                }
            
        }

        public void ResetQMPool(string queueManagerName, string hostName, int port, string channelName)
        {

                string key = queueManagerName + "@@" + hostName + "@@" + port + "@@" + channelName;
                if (_qmPools.ContainsKey(key))
                {
                    _qmPools[key].Clear();
                    // TO_DO: check how to release pool from memory
                    //_qmPools.Remove(key);
                }
            
        }

        public void Dispose()
        {
            foreach (var qmPool in _qmPools.Values)
            {
                qmPool.Clear();
            }
        }
    }
}
