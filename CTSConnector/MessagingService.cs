using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using IBM.WMQ;
using PoolUtil;
using log4net;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CTSConnector
{
    public class MessagingService : IDisposable
    {
        private static readonly ILog _log = LogManager.GetLogger("RollingFile");
        private bool _disposed = false;
        private QMPoolManager _qmPoolManager;
        private List<string> _hostNameList;
        private List<int> _portList;
        private int _index;
        private string _channelName;
        private string _queueManagerName;
        private int _waitInterval;
        private int _outMessageExpiry;

        public MessagingService(string hostNames, string ports, string channelName, string queueManagerName, int waitInterval, int outMessageExpiry, bool pooled, int maxPoolSize, TimeSpan poolTimeout)
        {
            if (pooled) _qmPoolManager = new QMPoolManager(maxPoolSize, poolTimeout);

            _hostNameList = GetHostNameList(hostNames);
            _portList = GetPortList(ports, _hostNameList.Count);
            _index = 0;
            _channelName = channelName;
            _queueManagerName = queueManagerName;
            _waitInterval = waitInterval;
            _outMessageExpiry = outMessageExpiry;

            //esta linea es la inicializacion
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }

        private List<string> GetHostNameList(string hostNames)
        {
            List<string> hostNameList = new List<string>();
            string[] hostNameArray = hostNames.Split(',');
            foreach (string hostName in hostNameArray)
            {
                hostNameList.Add(hostName.Trim());
            }
            return hostNameList;
        }

        private List<int> GetPortList(string ports, int totalHostNames)
        {
            List<int> portList = new List<int>();
            string[] portArray = ports.Split(',');
            foreach (string port in portArray)
            {
                portList.Add(int.Parse(port.Trim()));
            }
            if (portList.Count < totalHostNames)
            {
                int diff = totalHostNames - portList.Count;
                for (int i = 0; i < diff; i++)
                {
                    portList.Add(portList[portList.Count - 1]);
                }
            }
            return portList;
        }

        public byte[] PutMessageHA(string queueName, string messageString)
        {
            byte[] messageId = null;

            int index = _index;

            string resourceSource = "unknown";

            try
            {
                messageId = PutMessage(queueName, messageString, _hostNameList[index], _portList[index], resourceSource);
            }
            catch (MQException mqe)
            {
                bool error = true;
                if (IsConnectionError(mqe.ReasonCode))
                {
                    try
                    {
                        messageId = (byte[]) RetryOperation(index, "put", error, mqe, queueName, messageString, resourceSource);
                        error = false;
                    }
                    catch (Exception ex)
                    {
                        error = true;
                    }
                    
                    
                }
                if (error)
                {
                    throw mqe;
                }
            }
            return messageId;
        }

        private bool IsConnectionError(int reasonCode)
        {
            return (reasonCode == MQC.MQRC_HOST_NOT_AVAILABLE ||
                reasonCode == MQC.MQRC_Q_MGR_NOT_AVAILABLE ||
                reasonCode == MQC.MQRC_CONNECTION_BROKEN ||
                reasonCode == MQC.MQRC_HCONN_ERROR) ||
                reasonCode == MQC.MQRC_CONNECTION_NOT_AVAILABLE ||
                reasonCode == MQC.MQRC_CONNECTION_QUIESCING ||
                reasonCode == MQC.MQRC_CONNECTION_STOPPED ||
                reasonCode == MQC.MQRC_CONNECTION_STOPPING ||
                reasonCode == MQC.MQRC_CONNECTION_SUSPENDED;
        }

        private byte[] PutMessage(string queueName, string messageString, string hostName, int port, string resourceSource)
        {
            resourceSource = "unknown";
            byte[] messageId = null;

            try
            {
                var timerInicioPutMessage = DateTime.Now;

                using (IQMResource qmResource = GetQMResource(hostName, port))
                {
                    resourceSource = qmResource.ResourceSource;
                    var timerFinGetQMResource = DateTime.Now;
                    _log.Info("Tiempo GetQMResourcePut() = " + (timerFinGetQMResource - timerInicioPutMessage).TotalMilliseconds + " ms");

                    _log.Debug("Accessing queuePut " + queueName + "..");
                    var timerInicioAccessQueue = DateTime.Now;
                    MQQueue queue = qmResource.AccessQueue(queueName, MQC.MQOO_OUTPUT + MQC.MQOO_FAIL_IF_QUIESCING);
                    var timerFinAccessQueue = DateTime.Now;
                    _log.Info("Tiempo qmResource.AccessQueueGet() = " + (timerFinAccessQueue - timerInicioAccessQueue).TotalMilliseconds + " ms");
                    _log.Debug("Queue accessedPut");

                    MQMessage message = new MQMessage();

                    message.Encoding = MQC.MQENC_NATIVE;
                    message.Format = MQC.MQFMT_STRING; //"MQSTR   ";

                    //El mensaje se envia en UTF-8. Pero la codificacion de caracteres del XML es iso-8859-1.
                    //https://www.ibm.com/docs/en/ibm-mq/9.0?topic=interfaces-character-set-identifiers-net-applications
                    message.CharacterSet = 1208;

                    message.Expiry = _outMessageExpiry;
                    message.WriteString(messageString);

                    _log.Info("Putting message = " + messageString + "..");
                    var timerInicioPut = DateTime.Now;
                    queue.Put(message);
                    var timerFinPut = DateTime.Now;
                    _log.Info("Message put (" + (timerFinPut - timerInicioPut).TotalMilliseconds + " ms)");
                    messageId = message.MessageId;

                    var timerInicioCloseQueue = DateTime.Now;
                    CloseQueue(queue);
                    var timerFinCloseQueue = DateTime.Now;
                    _log.Info("Tiempo CloseQueuePut() = " + (timerFinCloseQueue - timerInicioCloseQueue).TotalMilliseconds + " ms");
                }
            }
            catch (MQException mqe)
            {
                _log.Error(string.Format("MQException caught: {0} - {1}", mqe.ReasonCode, mqe.Message));
                _log.Error(mqe.StackTrace);
                throw mqe;
            }
            return messageId;


        }

        private IQMResource GetQMResource(string hostName, int port)
        {
            IQMResource qmResource;
            _log.Debug("Getting resource..");
            if (_qmPoolManager != null)
                qmResource = _qmPoolManager.GetQMPool(_queueManagerName, hostName, port, _channelName).GetObject();
            else
                qmResource = NonPooledQMResource.Create(_queueManagerName, hostName, port, _channelName);
            _log.Debug("Resource got. Source = " + qmResource.ResourceSource);
            return qmResource;
        }

        private object RetryOperation(int startIndex, string operationType, bool error, MQException mqe, object param1, object param2, string resourceSource)
        {

            object returnValue = null;

            string hostName = _hostNameList[startIndex];
            int port = _portList[startIndex];

            // reset pool
            if (_qmPoolManager != null)
            {
                _log.Info("Resetting pool..");

                _qmPoolManager.ResetQMPool(_queueManagerName, hostName, port, _channelName);
            }

            if (resourceSource == "pool")
            {
                error = false;

                _log.Info("Trying to reconnect..");

                returnValue = Retry(operationType, ref error, ref mqe, param1, param2, hostName, port, resourceSource);

                if (!error) return returnValue;
            }

            int nextIndex = GetNextIndex(startIndex, startIndex);
            while (error && IsConnectionError(mqe.ReasonCode) && nextIndex != -1)
            {
                error = false;

                hostName = _hostNameList[nextIndex];
                port = _portList[nextIndex];

                _log.Info("Trying to connect with server " + hostName + " and port " + port + "..");

                returnValue = Retry(operationType, ref error, ref mqe, param1, param2, hostName, port, resourceSource);

                if (error) nextIndex = GetNextIndex(startIndex, nextIndex);
            }

            if (!error)
            {
                // set new index
                _index = nextIndex;
            }
            else
            {
                throw new Exception("Error [RetryOperation]");
            }

            return returnValue;

        }

        private int GetNextIndex(int startIndex, int lastIndex)
        {
            if (lastIndex + 1 < _hostNameList.Count)
            {
                if (lastIndex + 1 != startIndex) return lastIndex + 1;
                else return -1;
            }
            else
            {
                if (0 != startIndex) return 0;
                else return -1;
            }
        }

        private object Retry(string operationType, ref bool error, ref MQException mqe, object param1, object param2, string hostName, int port, string resourceSource)
        {
            object returnValue = null;

            try
            {
                if (operationType == "put")
                    returnValue = PutMessage((string)param1, (string)param2, hostName, port, resourceSource);
                else
                    returnValue = GetMessage((string)param1, (byte[])param2, hostName, port, resourceSource);
            }
            catch (MQException innerMqe)
            {
                mqe = innerMqe;
                error = true;
            }

            return returnValue;
        }

        public String GetMessageHA(string queueName, byte[] messageId)
        {
            string messageString = null;

            int index = _index;

            string resourceSource = "unknown";

            try
            {
                messageString = GetMessage(queueName, messageId, _hostNameList[_index], _portList[index], resourceSource);
            }
            catch (MQException mqe)
            {
                bool error = true;
                if (IsConnectionError(mqe.ReasonCode))
                {
                    try
                    {
                        messageString = (string)RetryOperation(index, "get", error, mqe, queueName, messageId, resourceSource);
                        error = false;
                    }
                    catch (Exception ex)
                    {
                        error = true;
                    }
                    
                }
                if (error)
                {
                    throw mqe;
                }
            }
            return messageString;
        }

        private String GetMessage(string queueName, byte[] messageId, string hostName, int port, string resourceSource)
        {
            resourceSource = "unknown";
            string messageString = null;

            try
            {
                var timerInicioGetMessage = DateTime.Now;

                using (IQMResource qmResource = GetQMResource(hostName, port))
                {

                    resourceSource = qmResource.ResourceSource;
                    var timerFinGetQMResource = DateTime.Now;
                    _log.Info("Tiempo GetQMResourceGet() = " + (timerFinGetQMResource - timerInicioGetMessage).TotalMilliseconds + " ms");

                    _log.Debug("Accessing queueGet " + queueName + "..");
                    var timerInicioAccessQueue = DateTime.Now;
                    MQQueue queue = qmResource.AccessQueue(queueName, MQC.MQOO_INPUT_AS_Q_DEF + MQC.MQOO_FAIL_IF_QUIESCING);
                    var timerFinAccessQueue = DateTime.Now;
                    _log.Info("Tiempo qmResource.AccessQueueGet() = " + (timerFinAccessQueue - timerInicioAccessQueue).TotalMilliseconds + " ms");
                    _log.Debug("Queue accessed");

                    MQMessage message = new MQMessage();

                    MQGetMessageOptions gmo = new MQGetMessageOptions();
                    gmo.MatchOptions = 2;
                    gmo.Options = MQC.MQGMO_WAIT + MQC.MQOO_INPUT_AS_Q_DEF + MQC.MQOO_FAIL_IF_QUIESCING;
                    message.CorrelationId = messageId;

                    gmo.Options = 1;
                    gmo.WaitInterval = _waitInterval;

                    try
                    {
                        _log.Debug("Getting message..");
                        var timerInigioQueueGet = DateTime.Now;
                        queue.Get(message, gmo);
                        var timerFinQueueGet = DateTime.Now;
                        _log.Info("Tiempo queue.Get() = " + (timerFinQueueGet - timerInigioQueueGet).TotalMilliseconds + " ms");

                        var timerInicioReadString = DateTime.Now;
                        
                        messageString = message.ReadString(message.MessageLength);
                        var timerFinReadString = DateTime.Now;
                        _log.Info("Tiempo message.ReadString() = " + (timerFinReadString - timerInicioReadString).TotalMilliseconds + " ms");

                        _log.Info("Message got = " + messageString);

                        var timerInicioClearMessage = DateTime.Now;
                        message.ClearMessage();
                        var timerFinClearMessage = DateTime.Now;
                        _log.Debug("ClearMessage() = " + (timerFinReadString - timerInicioReadString).TotalMilliseconds + " ms");
                    }
                    catch (MQException mqe)
                    {
                        if (mqe.ReasonCode == 2033)
                        {
                            _log.Error("No message available");
                        }
                        else
                        {
                            _log.Error(string.Format("MQException caught: {0} - {1}", mqe.ReasonCode, mqe.Message));
                            //throw mqe;
                        }

                        var timerFinGetMessageError1 = DateTime.Now;
                        _log.Info("Tiempo GetMessage() = " + (timerFinGetMessageError1 - timerInicioGetMessage).TotalMilliseconds + " ms");

                        throw mqe;
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.ToString());
                        var timerFinGetMessageError2 = DateTime.Now;
                        _log.Info("Tiempo GetMessage() = " + (timerFinGetMessageError2 - timerInicioGetMessage).TotalMilliseconds + " ms");
                    }

                    var timerInicioCloseQueue = DateTime.Now;
                    CloseQueue(queue);
                    var timerFinCloseQueue = DateTime.Now;
                    _log.Info("Tiempo CloseQueueGet() = " + (timerFinCloseQueue - timerInicioCloseQueue).TotalMilliseconds + " ms");
                }

                var timerFinGetMessage = DateTime.Now;
                _log.Info("Tiempo GetMessage() = " + (timerFinGetMessage - timerInicioGetMessage).TotalMilliseconds + " ms");
            }
            catch (MQException mqe)
            {
                _log.Error(string.Format("MQException caught: {0} - {1}", mqe.ReasonCode, mqe.Message));
                _log.Error(mqe.StackTrace);
                throw mqe;
            }
            return messageString;

        }

        private void CloseQueue(MQQueue queue)
        {
            try
            {
                _log.Debug("Closing queue..");
                queue.Close();
                _log.Debug("Queue closed");
            }
            catch (MQException mqe)
            {
                _log.Error(string.Format("MQException caught: {0} - {1}", mqe.ReasonCode, mqe.Message));
                _log.Error(mqe.StackTrace);
            }
        }

        public void Dispose()
        {
            lock (this)
            {
                if (_disposed) return;
                try
                {
                    if (_qmPoolManager != null) _qmPoolManager.Dispose();
                }
                catch (MQException mqe)
                {
                    _log.Error(string.Format("MQException caught: {0} - {1}", mqe.ReasonCode, mqe.Message));
                    _log.Error(mqe.StackTrace);
                }
                finally
                {
                    _disposed = true;
                }
            }
        }
    }
}
