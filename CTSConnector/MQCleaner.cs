using IBM.WMQ;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CTSConnector
{
    public class MQCleaner
    {
        private static readonly ILog _log = LogManager.GetLogger("RollingFile");
        private Timer _timer;

        public MQCleaner()
        {
        }

        /// <summary>
        /// Funcion que inicia el trabajo
        /// </summary>
        /// <returns></returns>
        public Task StartAsync()
        {
            _timer = new Timer(DoWork, null, TimeSpan.FromMinutes(0), TimeSpan.FromMinutes(30));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            _log.Info("Inicializando limpieza de la cola MQ()");
            try
            {
                Limpiar();
                _log.Info("Limpieza de la cola MQ finalizada()");
            }
            catch (Exception ex)
            {
                _log.Error("No se pudo limpiar : " + ex.ToString());
            }
            
        }

        public Task StopAsync()
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        public void Limpiar()
        {
            IList<string> Messages = new List<string>();

            // "QM.COBISTS_PRUEBAS", "bhux05d04z06", "1926", "CLIENTES_MF"
            String queueManagerName = Environment.GetEnvironmentVariable("queueManagerName"), hostName = Environment.GetEnvironmentVariable("hostNames") , port = Environment.GetEnvironmentVariable("ports"), channelName = Environment.GetEnvironmentVariable("channelName");

            MQQueueManager queueManager = null;
            //Reason: 2538
            foreach (string hostNameFinal in hostName.Split(','))
            {
                Hashtable properties = new Hashtable();

                properties.Add(MQC.TRANSPORT_PROPERTY, MQC.TRANSPORT_MQSERIES_MANAGED);
                properties.Add(MQC.HOST_NAME_PROPERTY, hostNameFinal);
                properties.Add(MQC.PORT_PROPERTY, port);
                properties.Add(MQC.CHANNEL_PROPERTY, channelName);
                _log.Debug(string.Format("Connecting to queue manager [{0}, {1}, {2}, {3}]..", queueManagerName, hostNameFinal, port, channelName));

                var timerInicioConnectQueueManager = DateTime.Now;

                try
                {
                    queueManager = new MQQueueManager(queueManagerName, properties);
                    var timerFinConnectQueueManager = DateTime.Now;
                    _log.Debug("Tiempo new MQQueueManager-Limpiar() = " + (timerFinConnectQueueManager - timerInicioConnectQueueManager).TotalMilliseconds + " ms");
                    _log.Debug("Connected to queue manager-Limpiar()");
                    break;
                }
                catch (Exception ex)
                {
                    var timerFinConnectQueueManager = DateTime.Now;
                    _log.Error("No se pudo obtener MQQueueManager-Limpiar() (" + (timerFinConnectQueueManager - timerInicioConnectQueueManager).TotalMilliseconds + ") : " + ex.ToString());
                }
                
            }

            


            if (queueManager.IsConnected)
            {
                //Se inicializa la cola para browse
                var _queue = queueManager.AccessQueue("WRH_RESP_MF", MQC.MQOO_INPUT_SHARED | MQC.MQOO_BROWSE | MQC.MQOO_FAIL_IF_QUIESCING | MQC.MQOO_INQUIRE, null, null, null);

                //Opciones para el primer mensaje
                MQGetMessageOptions mqGetMsgOpts = new MQGetMessageOptions();
                mqGetMsgOpts.Options = MQC.MQGMO_ALL_MSGS_AVAILABLE | MQC.MQGMO_WAIT | MQC.MQGMO_PROPERTIES_AS_Q_DEF | MQC.MQGMO_FAIL_IF_QUIESCING | MQC.MQGMO_BROWSE_NEXT ;//MQC.MQGMO_BROWSE_FIRST; 
                mqGetMsgOpts.MatchOptions = MQC.MQMO_MATCH_CORREL_ID;
                mqGetMsgOpts.WaitInterval = 5000;

                MQMessage msg = new MQMessage();
                               
                try
                {
                    //Se leen los suguientes
                    while (true)
                    {

                        _queue.Get(msg, mqGetMsgOpts);

                        DateTime fechaIngreso = msg.PutDateTime;

                        if (DateTime.Now.ToUniversalTime().Subtract(msg.PutDateTime).TotalMinutes > 5)
                        {
                            //Borra el mensaje del curso actual
                            MQGetMessageOptions gmo2 = new MQGetMessageOptions();
                            gmo2.Options = MQC.MQGMO_MSG_UNDER_CURSOR | MQC.MQGMO_FAIL_IF_QUIESCING | MQC.MQGMO_SYNCPOINT;

                            msg = new MQMessage(); 
                            _queue.Get(msg, gmo2);

                            msg.MessageId = MQC.MQMI_NONE;
                            msg.CorrelationId = MQC.MQCI_NONE;

                            // Debemos star preparados para manejar el codigo 2033
                            queueManager.Commit();

                            msg.ClearMessage();
                        }

                    }
                }
                catch (MQException ex)
                {
                    if (ex.CompCode == 2 && ex.Reason == 2033)
                    {
                        // Se llego al final de la cola
                    }
                }

                
                queueManager.Disconnect();
            }

            queueManager.Close();




        }
    }
}
