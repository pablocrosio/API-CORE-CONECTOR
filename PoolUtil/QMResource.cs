using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeProject.ObjectPool;
using IBM.WMQ;
using log4net;

namespace PoolUtil
{
    public class QMResource : PooledObject, IQMResource
    {
        private static readonly ILog _log = LogManager.GetLogger("RollingFile");
        private static readonly ConcurrentDictionary<string, int> _resourcesCreated = new ConcurrentDictionary<string, int>();
        private MQQueueManager _queueManager;
        private string _resourceSource = "new";

        public QMResource(string queueManagerName, string hostName, int port, string channelName)
        {
            string key = queueManagerName + "@@" + hostName + "@@" + port + "@@" + channelName;

            _queueManager = QMCreator.CreateQueueManager(queueManagerName, hostName, port, channelName);


                if (_resourcesCreated.ContainsKey(key))
                    _resourcesCreated[key]++;
                else
                    _resourcesCreated.TryAdd(key, 1);
                _log.Info("Resource created in pool " + key + ". Count = " + _resourcesCreated[key]);
            

            if (_queueManager != null)
            {
                OnReleaseResources = () =>
                {
                    // Called if the resource needs to be manually cleaned before the memory is reclaimed.
                    DisconnectQueueManager(_queueManager, key);
                };

                OnResetState = () =>
                {
                    // Called if the resource needs resetting before it is getting back into the pool.
                    _resourceSource = "pool";
                };
            }

            
        }

        public string ResourceSource => _resourceSource;

        public MQQueue AccessQueue(string queueName, int openOptions)
        {
            return _queueManager.AccessQueue(queueName, openOptions);
        }

        private void DisconnectQueueManager(MQQueueManager queueManager, string key)
        {
            try
            {
                _log.Info("Disconnecting pooled queue manager from pool " + key + "..");
                _queueManager.Disconnect();
                _log.Info("Queue manager disconnected");
            }
            catch (MQException mqe)
            {
                _log.Error(string.Format("MQException caught: {0} - {1}", mqe.ReasonCode, mqe.Message));
                _log.Error(mqe.StackTrace);
            }
            finally
            {

                    _resourcesCreated[key]--;
                    _log.Info("Resource released from pool " + key + ". Count = " + _resourcesCreated[key]);
                
            }
        }
    }
}
