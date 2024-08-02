using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IBM.WMQ;
using log4net;

namespace PoolUtil
{
    public class NonPooledQMResource : IQMResource
    {
        private static readonly ILog _log = LogManager.GetLogger("RollingFile");
        private MQQueueManager _queueManager;

        public static NonPooledQMResource Create(string queueManagerName, string hostName, int port, string channelName)
        {
            return new NonPooledQMResource(QMCreator.CreateQueueManager(queueManagerName, hostName, port, channelName));
        }

        private NonPooledQMResource(MQQueueManager queueManager)
        {
            _queueManager = queueManager;
        }

        public string ResourceSource => "new";

        public MQQueue AccessQueue(string queueName, int openOptions)
        {
            return _queueManager.AccessQueue(queueName, openOptions);
        }

        public void Dispose()
        {
            DisconnectQueueManager();
        }

        private void DisconnectQueueManager()
        {
            try
            {
                _log.Info("Disconnecting queue manager..");
                _queueManager.Disconnect();
                _log.Info("Queue manager disconnected");
            }
            catch (MQException mqe)
            {
                _log.Error(string.Format("MQException caught: {0} - {1}", mqe.ReasonCode, mqe.Message));
                _log.Error(mqe.StackTrace);
            }
        }
    }
}
