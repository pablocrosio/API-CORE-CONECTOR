using CTSConnector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace CTSService.Net40
{
    /// <summary>
    /// Descripción breve de CTSService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // Para permitir que se llame a este servicio web desde un script, usando ASP.NET AJAX, quite la marca de comentario de la línea siguiente. 
    // [System.Web.Script.Services.ScriptService]
    public class CTSService : System.Web.Services.WebService
    {
        private static object _thisLock = new object();
        private static CTSCaller _ctsCaller;

        public static CTSCaller CTSCaller { get { return _ctsCaller; } }

        [WebMethod]
        public string CallCTS(string inMessage)
        {
            //string messageString = "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><CTSMessage><CTSHeader><Field name=\"SPExecutorServiceFactoryFilter\" type=\"S\">(service.impl=object)</Field><Field name=\"supportOffline\" type=\"C\">N</Field><Field name=\"sessionId\" type=\"S\">@@sessionId@@</Field></CTSHeader><Data><ProcedureRequest><SpName>cobis..sp_wst_direccion</SpName><Param name=\"@t_trn\" type=\"56\" io=\"0\" len=\"4\">1386</Param><Param name=\"@i_operacion\" type=\"47\" io=\"0\" len=\"1\">Q</Param><Param name=\"@i_di_direccion\" type=\"52\" io=\"0\" len=\"2\">3</Param><Param name=\"@i_di_ente\" type=\"56\" io=\"0\" len=\"4\">666</Param><Param name=\"@i_sistema_origen\" type=\"39\" io=\"0\" len=\"3\">DEX</Param><Param name=\"@i_usuario_alta\" type=\"39\" io=\"0\" len=\"7\">scoring</Param><Param name=\"@i_di_tipo\" type=\"39\" io=\"0\" len=\"2\">LA</Param><Param name=\"@i_di_descripcion\" type=\"39\" io=\"0\" len=\"7\">FLORIDA</Param><Param name=\"@i_di_numero\" type=\"56\" io=\"0\" len=\"4\">666</Param><Param name=\"@i_di_postal\" type=\"39\" io=\"0\" len=\"4\">1234</Param><Param name=\"@i_di_ciudad\" type=\"52\" io=\"0\" len=\"2\">195</Param><Param name=\"@i_di_provincia\" type=\"52\" io=\"0\" len=\"2\">1</Param><Param name=\"@i_di_pais\" type=\"52\" io=\"0\" len=\"2\">80</Param><Param name=\"@i_componente\" type=\"47\" io=\"0\" len=\"1\">N</Param><Param name=\"@o_di_direccion\" type=\"52\" io=\"1\" len=\"0\">0</Param><Param name=\"@o_di_direccionp\" type=\"52\" io=\"1\" len=\"0\">0</Param></ProcedureRequest></Data></CTSMessage>";

            CTSCaller ctsCaller = GetCTSCaller();

            //return ctsCaller.SendServiceMessage(messageString);
            return ctsCaller.SendServiceMessage(inMessage);
        }

        private CTSCaller GetCTSCaller()
        {
            lock (_thisLock)
            {
                if (_ctsCaller == null)
                {
                    string hostNames = ConfigurationManager.AppSettings["hostNames"];
                    string ports = ConfigurationManager.AppSettings["ports"];
                    string channelName = ConfigurationManager.AppSettings["channelName"];
                    string queueManagerName = ConfigurationManager.AppSettings["queueManagerName"];
                    string inSessionQueueName = ConfigurationManager.AppSettings["inSessionQueueName"];
                    string outSessionQueueName = ConfigurationManager.AppSettings["outSessionQueueName"];
                    string inServiceQueueName = ConfigurationManager.AppSettings["inServiceQueueName"];
                    string outServiceQueueName = ConfigurationManager.AppSettings["outServiceQueueName"];
                    int waitInterval = Convert.ToInt32(ConfigurationManager.AppSettings["waitInterval"]);
                    int outMessageExpiry = Convert.ToInt32(ConfigurationManager.AppSettings["outMessageExpiry"]);
                    bool pooled = ConfigurationManager.AppSettings["pooled"] == "1";
                    int maxPoolSize = Convert.ToInt32(ConfigurationManager.AppSettings["maxPoolSize"]);
                    TimeSpan poolTimeout = TimeSpan.FromMinutes(Convert.ToDouble(ConfigurationManager.AppSettings["poolTimeoutInMin"]));
                    LoginInfo loginInfo = new LoginInfo();
                    loginInfo.ApplicationId = ConfigurationManager.AppSettings["applicationId"];
                    loginInfo.UserId = ConfigurationManager.AppSettings["userId"];
                    loginInfo.Password = ConfigurationManager.AppSettings["password"];
                    int sessionTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["sessionTimeout"]);
                    _ctsCaller = CTSCaller.GetCTSCaller(hostNames, ports, channelName, queueManagerName, inSessionQueueName, outSessionQueueName, inServiceQueueName, outServiceQueueName, waitInterval, outMessageExpiry, pooled, maxPoolSize, poolTimeout, loginInfo, sessionTimeout);
                }
            }
            return _ctsCaller;
        }
    }
}
