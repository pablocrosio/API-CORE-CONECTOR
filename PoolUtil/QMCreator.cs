using IBM.WMQ;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PoolUtil
{
    public class QMCreator
    {
        private static readonly ILog _log = LogManager.GetLogger("RollingFile");

        public static MQQueueManager CreateQueueManager(string queueManagerName, string hostName, int port, string channelName)
        {
            MQQueueManager queueManager = null;
            
                try
                {
                    Hashtable properties = new Hashtable();
                    properties.Add(MQC.TRANSPORT_PROPERTY, MQC.TRANSPORT_MQSERIES_MANAGED);
                    properties.Add(MQC.HOST_NAME_PROPERTY, hostName);
                    properties.Add(MQC.PORT_PROPERTY, port);
                    properties.Add(MQC.CHANNEL_PROPERTY, channelName);
                    _log.Info(string.Format("Connecting to queue manager [{0}, {1}, {2}, {3}]..", queueManagerName, hostName, port, channelName));

                    var timerInicioNewQueueManager = DateTime.Now;

                    queueManager = new MQQueueManager(queueManagerName, properties);

                    var timerFinNewQueueManager = DateTime.Now;
                    _log.Info("Tiempo new MQQueueManager() = " + (timerFinNewQueueManager - timerInicioNewQueueManager).TotalMilliseconds + " ms");
                    _log.Info("Connected to queue manager");
                }
                catch (Exception ex)
                {
                    _log.Error("NOT Connected to queue manager. Mensaje Error: " + ex.ToString());
                    throw ex;
                }

            
            
            
            return queueManager;
        }
    }
}
