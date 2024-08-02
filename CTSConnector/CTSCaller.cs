using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CTSConnector.MQ;
using CTSConnector.Sesiones;
using log4net;
using Microsoft.Extensions.Configuration;

namespace CTSConnector
{
    public class CTSCaller : IDisposable
    {
        private static readonly ILog _log = LogInicializer._log;
        private static readonly object _thisLock = new object();
        private static readonly CTSCaller _instancia = new CTSCaller();
        private bool _disposed = false;
        LoginInfo _loginInfo;
        private int _sessionTimeout = 0;
        private string _sessionId = "-1";
        private DateTime _sessionExpiration = new DateTime();
        private MessagingService _messagingServices;
        private string _inSessionQueueName;
        private string _outSessionQueueName;
        private string _inServiceQueueName;
        private string _outServiceQueueName;
        private static readonly GestorSesionesCTS gestorSesionesCTS = new GestorSesionesCTS();
        private static readonly GestorSesionesCobis gestorSesionesCobis = new GestorSesionesCobis();

        public Boolean Inicializado { get; internal set; } = false;

        public GestorSesionesCobis GestorSesionesCobis()
        {
            return gestorSesionesCobis;
        }
        

        private CTSCaller()
        {
        }

        public static CTSCaller GetCTSCaller(IConfiguration Configuration)
        {
            if (!_instancia.Inicializado)
            {
                lock (_thisLock)
                {
                    if (_instancia.Inicializado == false)
                    {
                        //La prioridad la tienen las variables de entorno
                        string hostNames = Configuration.GetSection("hostNames").Value;
                        if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("hostNames")))
                        {
                            hostNames = Environment.GetEnvironmentVariable("hostNames");
                        }

                        string ports = Configuration.GetSection("ports").Value;
                        if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("ports")))
                        {
                            ports = Environment.GetEnvironmentVariable("ports");
                        }

                        string channelName = Configuration.GetSection("channelName").Value;
                        if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("channelName")))
                        {
                            channelName = Environment.GetEnvironmentVariable("channelName");
                        }

                        string queueManagerName = Configuration.GetSection("queueManagerName").Value;
                        if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("queueManagerName")))
                        {
                            queueManagerName = Environment.GetEnvironmentVariable("queueManagerName");
                        }

                        string inSessionQueueName = Configuration.GetSection("inSessionQueueName").Value;
                        if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("inSessionQueueName")))
                        {
                            inSessionQueueName = Environment.GetEnvironmentVariable("inSessionQueueName");
                        }

                        string outSessionQueueName = Configuration.GetSection("outSessionQueueName").Value;
                        if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("outSessionQueueName")))
                        {
                            outSessionQueueName = Environment.GetEnvironmentVariable("outSessionQueueName");
                        }

                        string inServiceQueueName = Configuration.GetSection("inServiceQueueName").Value;
                        if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("inServiceQueueName")))
                        {
                            inServiceQueueName = Environment.GetEnvironmentVariable("inServiceQueueName");
                        }

                        string outServiceQueueName = Configuration.GetSection("outServiceQueueName").Value;
                        if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("outServiceQueueName")))
                        {
                            outServiceQueueName = Environment.GetEnvironmentVariable("outServiceQueueName");
                        }

                        int waitInterval = Convert.ToInt32(Configuration.GetSection("waitInterval").Value);
                        if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("waitInterval")))
                        {
                            waitInterval = Convert.ToInt32(Environment.GetEnvironmentVariable("waitInterval"));
                        }

                        int outMessageExpiry = Convert.ToInt32(Configuration.GetSection("outMessageExpiry").Value);
                        if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("outMessageExpiry")))
                        {
                            outMessageExpiry = Convert.ToInt32(Environment.GetEnvironmentVariable("outMessageExpiry"));
                        }

                        bool pooled = Configuration.GetSection("pooled").Value == "1";
                        if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("pooled")))
                        {
                            pooled = Environment.GetEnvironmentVariable("pooled") == "1";
                        }

                        int maxPoolSize = Convert.ToInt32(Configuration.GetSection("maxPoolSize").Value);
                        if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("maxPoolSize")))
                        {
                            maxPoolSize = Convert.ToInt32(Environment.GetEnvironmentVariable("maxPoolSize"));
                        }

                        TimeSpan poolTimeout = TimeSpan.FromMinutes(Convert.ToDouble(Configuration.GetSection("poolTimeoutInMin").Value));
                        if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("poolTimeoutInMin")))
                        {
                            poolTimeout = TimeSpan.FromMinutes(Convert.ToDouble(Environment.GetEnvironmentVariable("poolTimeoutInMin")));
                        }

                        LoginInfo loginInfo = new LoginInfo();
                        loginInfo.ApplicationId = Configuration.GetSection("applicationId").Value;
                        if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("applicationId")))
                        {
                            loginInfo.ApplicationId = Environment.GetEnvironmentVariable("applicationId");
                        }

                        loginInfo.UserId = Configuration.GetSection("userId").Value;
                        if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("userId")))
                        {
                            loginInfo.UserId = Environment.GetEnvironmentVariable("userId");
                        }

                        loginInfo.Password = Configuration.GetSection("password").Value;
                        if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("password")))
                        {
                            loginInfo.Password = Environment.GetEnvironmentVariable("password");
                        }

                        int sessionTimeout = Convert.ToInt32(Configuration.GetSection("sessionTimeout").Value);
                        if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("sessionTimeout")))
                        {
                            sessionTimeout = Convert.ToInt32(Environment.GetEnvironmentVariable("sessionTimeout"));
                        }


                        _instancia.SetCtsCaller(hostNames, ports, channelName, queueManagerName, inSessionQueueName, outSessionQueueName, inServiceQueueName, outServiceQueueName, waitInterval, outMessageExpiry, pooled, maxPoolSize, poolTimeout, loginInfo, sessionTimeout);

                        _instancia.Inicializado = true;


                        return _instancia;

                    }
                    else
                    {
                        return _instancia;
                    }
                }
            }
            else
            {
                return _instancia;
            }

        }

        public void IniciarSesion()
        {
            var timerInicioSesion = DateTime.Now;

            gestorSesionesCTS.IniciarSesion();

            var timerFinSesion = DateTime.Now;

            _log.Debug("Tiempo InicioSesion() = " + (timerFinSesion - timerInicioSesion).TotalMilliseconds + " ms");
        }

        public static CTSCaller GetCTSCaller()
        {
            if (_instancia == null)
            {
                throw new NullReferenceException();
            }
            else
            {
                return _instancia;
            }
        }

        private void SetCtsCaller(string hostNames, string ports, string channelName, string queueManagerName, string inSessionQueueName, string outSessionQueueName, string inServiceQueueName, string outServiceQueueName, int waitInterval, int outMessageExpiry, bool pooled, int maxPoolSize, TimeSpan poolTimeout, LoginInfo loginInfo, int sessionTimeout)
        {
            _loginInfo = loginInfo;
            _sessionTimeout = sessionTimeout;
            _messagingServices = new MessagingService(hostNames, ports, channelName, queueManagerName, waitInterval, outMessageExpiry, pooled, maxPoolSize, poolTimeout);

            //Indica el messageServices
            gestorSesionesCTS.ServicioMQ.MessagingServices = _messagingServices;
            gestorSesionesCTS.SessionTimeout = _sessionTimeout;
            gestorSesionesCTS.InSessionQueueName = inSessionQueueName;
            gestorSesionesCTS.OutSessionQueueName = outSessionQueueName;

            //Indica el gestorSessionCobis
            gestorSesionesCobis.ServicioMQ.MessagingServices = _messagingServices;
            gestorSesionesCobis.SessionTimeout = _sessionTimeout;
            gestorSesionesCobis.InSessionQueueName = inSessionQueueName;
            gestorSesionesCobis.OutSessionQueueName = outSessionQueueName;

            _inSessionQueueName = inSessionQueueName;
            _outSessionQueueName = outSessionQueueName;
            _inServiceQueueName = inServiceQueueName;
            _outServiceQueueName = outServiceQueueName;

            //Se cargan los datos para los mensajes
            MensajesCTS.ApplicationId = _loginInfo.ApplicationId;
            MensajesCTS.UserId = _loginInfo.UserId;
            MensajesCTS.Password = _loginInfo.Password;
        }

        

        public String InitializeSession()
        {

            if (_sessionId != "-1")
                try { FinalizeSession(); }
                catch { }

            //string inMessage = "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><CTSMessage><CTSHeader><Field name=\"servicio\" type=\"S\">8</Field></CTSHeader><Data><ProcedureRequest><SpName>master..sp_reclogin_rc</SpName><Param name=\"@i_servicio\" type=\"39\" io=\"0\" len=\"1\">8</Param><Param name=\"@i_id_aplicacion\" type=\"38\" io=\"0\" len=\"0\">46372</Param><Param name=\"@i_login\" type=\"39\" io=\"0\" len=\"0\">airraid</Param></ProcedureRequest></Data></CTSMessage>";
            string inMessage = GetLoginXml();

            DateTime startTime = DateTime.Now;

            _log.Info("Initializing session..");
            string outMessage = SendMessage(_inSessionQueueName, _outSessionQueueName, inMessage);

            _sessionId = GetSessionId(outMessage);
            var timerFinSessionId = DateTime.Now;
            _log.Info("Session initialized. Elapsed time = " + (timerFinSessionId - startTime).TotalMilliseconds);
            _log.Info("Session id = " + _sessionId);

            _sessionExpiration = DateTime.Now + new TimeSpan(_sessionTimeout, 0, 0);

            _log.Info("Session expiration = " + _sessionExpiration.ToString("yyyy-MM-dd HH:mm:ss"));
            return _sessionId;
        }

        private string GetLoginXml()
        {
            List<Field> fields = new List<Field>() { new Field { Type = "S", Name = "servicio", Value = "8" } };
            CTSHeader ctsHeader = new CTSHeader() { Fields = fields };
            List<Param> parameters = new List<Param> { new Param { Type = "39", Name = "@i_servicio", Len = "1", IO = "0", Value = "8" }, new Param { Type = "38", Name = "@i_id_aplicacion", Len = "0", IO = "0", Value = _loginInfo.ApplicationId }, new Param { Type = "39", Name = "@i_login", Len = "0", IO = "0", Value = _loginInfo.UserId }, new Param { Type = "39", Name = "@i_clave", Len = "0", IO = "0", Value = _loginInfo.Password } };
            ProcedureRequest procedureRequest = new ProcedureRequest() { SpName = "master..sp_reclogin_rc", Params = parameters };
            CTSInMessage ctsMessage = new CTSInMessage() { CTSHeader = ctsHeader, Data = new InData() { ProcedureRequest = procedureRequest } };
            return ctsMessage.GetXml();
        }

        public string SessionId { get { return _sessionId; } }

        private String SendMessage(string inQueueName, string outQueueName, string messageString)
        {
            //string messageString = "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><CTSMessage><CTSHeader><Field name=\"servicio\" type=\"S\">8</Field></CTSHeader><Data><ProcedureRequest><SpName>master..sp_reclogin_rc</SpName><Param name=\"@i_servicio\" type=\"39\" io=\"0\" len=\"1\">8</Param><Param name=\"@i_id_aplicacion\" type=\"38\" io=\"0\" len=\"0\">46372</Param><Param name=\"@i_login\" type=\"39\" io=\"0\" len=\"0\">airraid</Param></ProcedureRequest></Data></CTSMessage>";
            //ID:414d5120514d2e434f4249535453202059234abc20319202
            //string messageString = "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><CTSMessage><CTSHeader><Field name=\"SPExecutorServiceFactoryFilter\" type=\"S\">(service.impl=object)</Field><Field name=\"supportOffline\" type=\"C\">N</Field><Field name=\"sessionId\" type=\"S\">ID:414d5120514d2e434f4249535453202059234abc20319202</Field></CTSHeader><Data><ProcedureRequest><SpName>cobis..sp_wst_direccion</SpName><Param name=\"@t_trn\" type=\"56\" io=\"0\" len=\"4\">1386</Param><Param name=\"@i_operacion\" type=\"47\" io=\"0\" len=\"1\">Q</Param><Param name=\"@i_di_direccion\" type=\"52\" io=\"0\" len=\"2\">3</Param><Param name=\"@i_di_ente\" type=\"56\" io=\"0\" len=\"4\">666</Param><Param name=\"@i_sistema_origen\" type=\"39\" io=\"0\" len=\"3\">DEX</Param><Param name=\"@i_usuario_alta\" type=\"39\" io=\"0\" len=\"7\">scoring</Param><Param name=\"@i_di_tipo\" type=\"39\" io=\"0\" len=\"2\">LA</Param><Param name=\"@i_di_descripcion\" type=\"39\" io=\"0\" len=\"7\">FLORIDA</Param><Param name=\"@i_di_numero\" type=\"56\" io=\"0\" len=\"4\">666</Param><Param name=\"@i_di_postal\" type=\"39\" io=\"0\" len=\"4\">1234</Param><Param name=\"@i_di_ciudad\" type=\"52\" io=\"0\" len=\"2\">195</Param><Param name=\"@i_di_provincia\" type=\"52\" io=\"0\" len=\"2\">1</Param><Param name=\"@i_di_pais\" type=\"52\" io=\"0\" len=\"2\">80</Param><Param name=\"@i_componente\" type=\"47\" io=\"0\" len=\"1\">N</Param><Param name=\"@o_di_direccion\" type=\"52\" io=\"1\" len=\"0\">0</Param><Param name=\"@o_di_direccionp\" type=\"52\" io=\"1\" len=\"0\">0</Param></ProcedureRequest></Data></CTSMessage>";
            //string queueName = "SESSION_REQ_MF";

            byte[] messageId = _messagingServices.PutMessageHA(inQueueName, messageString);

            //string queueName = "SESSION_RESP_MF";

            messageString = _messagingServices.GetMessageHA(outQueueName, messageId);

            return messageString;
        }

        private string GetSessionId(string outMessage)
        {
            //<?xml version="1.0" encoding="ISO-8859-1" ?><CTSMessage><CTSHeader><Field name="fromServer" type="S">server1</Field><Field name="servicio" type="S">8</Field><Field name="sesn" type="N">30193</Field><Field name="dbms" type="S">SYBCTS</Field></CTSHeader><Data><ProcedureResponse><ResultSet><Header><col name="reclogin" type="39" len="16"/></Header><rw><cd>00:00:0023:59:59</cd></rw></ResultSet><ResultSet><Header><col name="respuesta" type="39" len="18"/></Header><rw><cd>Bienvenido a COBIS</cd></rw></ResultSet><OutputParams><param name="sessionId" type="39" len="51">ID:9b84a29fcb53416984e07a0277b8dfdc0000000000000000</param></OutputParams><return>0</return></ProcedureResponse></Data></CTSMessage>
            string sessionId = "-1";
            XmlDocument document = new XmlDocument();
            document.LoadXml(outMessage);
            XmlNode paramNode = document.SelectSingleNode("/CTSMessage/Data/ProcedureResponse/OutputParams/param[@name='sessionId']");
            if (paramNode == null)
            {
                string errorMessage = "Could not get session id";
                _log.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }
            else sessionId = paramNode.InnerText;
            return sessionId;
        }

        public String FinalizeSession()
        {
            string outMessage = "";
            if (_sessionId != "-1")
            {
                //string inMessage = string.Format("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><CTSMessage><CTSHeader><Field name=\"externalUser\" type=\"S\">airraid</Field><Field name=\"servicio\" type=\"S\">8</Field><Field name=\"externalApplicationId\" type=\"S\">46372</Field><Field name=\"sessionId\" type=\"S\">{0}</Field></CTSHeader><Data><ProcedureRequest><SpName>master..sp_endlogin_rc</SpName><Param name=\"@i_servicio\" type=\"39\" io=\"0\" len=\"1\">8</Param><Param name=\"@i_id_aplicacion\" type=\"38\" io=\"0\" len=\"0\">46372</Param><Param name=\"@i_login\" type=\"39\" io=\"0\" len=\"0\">airraid</Param></ProcedureRequest></Data></CTSMessage>", _sessionId);
                string inMessage = GetLogoutXml();
                DateTime startTime = DateTime.Now;
                _log.Info("Finalizing session " + _sessionId + "..");
                outMessage = SendMessage(_inSessionQueueName, _outSessionQueueName, inMessage);
                var timerFinSendMessageSession = DateTime.Now;
                _log.Info("Session finalized. Elapsed time = " + (timerFinSendMessageSession - startTime).TotalMilliseconds);
                _sessionId = "-1";
            }
            return outMessage;
        }

        private string GetLogoutXml()
        {
            List<Field> fields = new List<Field>() { new Field { Type = "S", Name = "externalUser", Value = _loginInfo.UserId }, new Field { Type = "S", Name = "servicio", Value = "8" }, new Field { Type = "S", Name = "externalApplicationId", Value = _loginInfo.ApplicationId }, new Field { Type = "S", Name = "sessionId", Value = _sessionId } };
            CTSHeader ctsHeader = new CTSHeader() { Fields = fields };
            List<Param> parameters = new List<Param> { new Param { Type = "39", Name = "@i_servicio", Len = "1", IO = "0", Value = "8" }, new Param { Type = "38", Name = "@i_id_aplicacion", Len = "0", IO = "0", Value = _loginInfo.ApplicationId }, new Param { Type = "39", Name = "@i_login", Len = "0", IO = "0", Value = _loginInfo.UserId } };
            ProcedureRequest procedureRequest = new ProcedureRequest() { SpName = "master..sp_endlogin_rc", Params = parameters };
            CTSInMessage ctsMessage = new CTSInMessage() { CTSHeader = ctsHeader, Data = new InData() { ProcedureRequest = procedureRequest } };
            return ctsMessage.GetXml();
        }

        public String SendServiceMessage(String inMessage)
        {
            DateTime startTime = DateTime.Now;

            String outMessage = "";

            //string threadData = Guid.NewGuid().ToString();

            //using (log4net.LogicalThreadContext.Stacks["NDC"].Push(threadData))
            //{
            //_log.Info("Executing service..");

            //var t1 = System.Diagnostics.Stopwatch.StartNew();
            //await ValidateSession().ConfigureAwait(false);
            _sessionId = gestorSesionesCTS.Ticket.Id;
            //t1.Stop();
            //_log.Info("Tiempo ValidateSession() = " + t1.ElapsedMilliseconds + " ms");

            inMessage = inMessage.Replace("@@sessionId@@", _sessionId);

            var timerInicioSendMessage = DateTime.Now;

            byte[] idMensaje = gestorSesionesCTS.ServicioMQ.SendMessageWork(_inServiceQueueName, _outServiceQueueName, inMessage);

            var timerFinSendMessage = DateTime.Now;
            _log.Debug("Tiempo SendMessageWork() = " + (timerFinSendMessage - timerInicioSendMessage).TotalMilliseconds + " ms");

            var timerInicioGetMessage = DateTime.Now;
            if (idMensaje != null)
            {
                
                outMessage = gestorSesionesCTS.ServicioMQ.GetMessageWork(_inServiceQueueName, _outServiceQueueName, idMensaje);
                var timerFinGetMessage = DateTime.Now;
                _log.Debug("GetMessageWork(). Elapsed time = " + (timerFinGetMessage - timerInicioGetMessage).TotalMilliseconds);
            }
            else
            {
                var timerFinGetMessageError = DateTime.Now;
                _log.Info("No se obtuvo idMensaje luego de poner en cola MQ " + (timerFinGetMessageError - timerInicioGetMessage).TotalMilliseconds);
            }


            var timerFinSendServiceMessage = DateTime.Now;
            _log.Info("Service executed. Elapsed time = " + (timerFinSendServiceMessage - startTime).TotalMilliseconds);
            //}


            return outMessage;




        }

        public Task ValidateSession()
        {
            return Task.Run(() =>
            {
                lock (_sessionId)
                {
                    if ((_sessionId == "-1") || (DateTime.Now > _sessionExpiration))
                    {
                        InitializeSession();
                    }

                }

            });
            
        }

        public void Dispose()
        {
            lock (this)
            {
                if (_disposed) return;
                try
                {
                    FinalizeSession();
                    _messagingServices.Dispose();
                }
                catch (Exception E)
                {
                    _log.Error(string.Format("Exception caught in CTSConnector.Dispose(): {0}", E.ToString()));
                }
                finally
                {
                    _disposed = true;
                }
            }
        }

        ~CTSCaller()
        {
            Dispose();
        }
    }
}
