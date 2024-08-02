using CTSConnector.MQ;
using log4net;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CTSConnector.Sesiones
{
    public class GestorSesionesCobis
    {
        private static readonly ILog log = LogManager.GetLogger("RollingFile");

        public ServicioMQ ServicioMQ { get; set; } = new ServicioMQ();

        public Int32 SessionTimeout { get; set; }
        public String InSessionQueueName { get; set; }
        public String OutSessionQueueName { get; set; }
        

        /// <summary>
        /// Inicia la sesion en CTS
        /// </summary>
        /// <returns></returns>
        public string IniciarSesion(String xml_in)
        {
            return NuevoInicioDeSesion(xml_in);           
        }

        /// <summary>
        /// Inicia la sesion en CTS invocando a IBM MQ
        /// </summary>
        private string NuevoInicioDeSesion(String xml_in)
        {
            //string inMessage = "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><CTSMessage><CTSHeader><Field name=\"servicio\" type=\"S\">8</Field></CTSHeader><Data><ProcedureRequest><SpName>master..sp_reclogin_rc</SpName><Param name=\"@i_servicio\" type=\"39\" io=\"0\" len=\"1\">8</Param><Param name=\"@i_id_aplicacion\" type=\"38\" io=\"0\" len=\"0\">46372</Param><Param name=\"@i_login\" type=\"39\" io=\"0\" len=\"0\">airraid</Param></ProcedureRequest></Data></CTSMessage>";
            string inMessage = xml_in;

            DateTime startTime = DateTime.Now;

            log.Info("Initializing session..");

            string outMessage = "";
            outMessage = ServicioMQ.SendMessageSession(InSessionQueueName, OutSessionQueueName, inMessage);

            //String _sessionId = "";
            return outMessage;
        }
    }
}
