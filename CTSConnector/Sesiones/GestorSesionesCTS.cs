using CTSConnector.MQ;
using log4net;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CTSConnector.Sesiones
{
    public class GestorSesionesCTS
    {
        private static readonly ILog log = LogManager.GetLogger("RollingFile");

        public ServicioMQ ServicioMQ { get; set; } = new ServicioMQ();

        public Int32 SessionTimeout { get; set; }
        public String InSessionQueueName { get; set; }
        public String OutSessionQueueName { get; set; }
        

        /// <summary>
        /// El ticket de sesion para el CTS
        /// </summary>
        private static readonly SesionCTS TicketAccesoCTS = new SesionCTS();


        public SesionCTS Ticket { get => TicketAccesoCTS; }

        /// <summary>
        /// Inicia la sesion en CTS
        /// </summary>
        /// <returns></returns>
        public void IniciarSesion()
        {
            lock (TicketAccesoCTS)
            {
                if (!TicketAccesoCTS.SesionIniciada)
                {
                    NuevoInicioDeSesion();
                }
                else
                {
                    ValidarSesion();
                }
            }
            
        }

        

        /// <summary>
        /// Valida que la sesion sea correcta
        /// </summary>
        /// <returns></returns>
        private void ValidarSesion()
        {
            lock (TicketAccesoCTS)
            {
                if (TicketAccesoCTS.FechaExpiracion < DateTime.Now)
                {
                    TicketAccesoCTS.SesionIniciada = false;
                    FinalizarSesionCTS();
                    NuevoInicioDeSesion();
                }
            }
        }



        /// <summary>
        /// Inicia la sesion en CTS invocando a IBM MQ
        /// </summary>
        private void NuevoInicioDeSesion()
        {
            //string inMessage = "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><CTSMessage><CTSHeader><Field name=\"servicio\" type=\"S\">8</Field></CTSHeader><Data><ProcedureRequest><SpName>master..sp_reclogin_rc</SpName><Param name=\"@i_servicio\" type=\"39\" io=\"0\" len=\"1\">8</Param><Param name=\"@i_id_aplicacion\" type=\"38\" io=\"0\" len=\"0\">46372</Param><Param name=\"@i_login\" type=\"39\" io=\"0\" len=\"0\">airraid</Param></ProcedureRequest></Data></CTSMessage>";
            string inMessage = MensajesCTS.GetLoginMessage();

            DateTime startTime = DateTime.Now;

            log.Info("Initializing session..");


            
            string outMessage = ServicioMQ.SendMessageSession(InSessionQueueName, OutSessionQueueName, inMessage);

            String _sessionId = "";

            try
            {
                _sessionId = MensajesCTS.GetIdSesionDesdeMensajeCTS(outMessage);
            }
            catch (ApplicationException ex)
            {
                log.Error(ex.Message);
                Console.WriteLine(ex.Message);
                TicketAccesoCTS.SesionIniciada = false;
                throw ex;
            }

            var timerFinNuevoInicioSesion = DateTime.Now;
            log.Info("Session initialized. Elapsed time = " + (timerFinNuevoInicioSesion - startTime).TotalMilliseconds);
            log.Info("Session id = " + _sessionId);

            TicketAccesoCTS.Id = _sessionId;
            TicketAccesoCTS.FechaCreacion = DateTime.Now;
            TicketAccesoCTS.FechaExpiracion = DateTime.Now + new TimeSpan(SessionTimeout, 0, 0);
            TicketAccesoCTS.SesionIniciada = true;

            log.Info("Session expiration = " + TicketAccesoCTS.FechaExpiracion.ToString("yyyy-MM-dd HH:mm:ss"));

        }


        /// <summary>
        /// Finaliza la sesion en CTS invocando a IBM MQ
        /// </summary>
        private void FinalizarSesionCTS()
        {

            //string inMessage = string.Format("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><CTSMessage><CTSHeader><Field name=\"externalUser\" type=\"S\">airraid</Field><Field name=\"servicio\" type=\"S\">8</Field><Field name=\"externalApplicationId\" type=\"S\">46372</Field><Field name=\"sessionId\" type=\"S\">{0}</Field></CTSHeader><Data><ProcedureRequest><SpName>master..sp_endlogin_rc</SpName><Param name=\"@i_servicio\" type=\"39\" io=\"0\" len=\"1\">8</Param><Param name=\"@i_id_aplicacion\" type=\"38\" io=\"0\" len=\"0\">46372</Param><Param name=\"@i_login\" type=\"39\" io=\"0\" len=\"0\">airraid</Param></ProcedureRequest></Data></CTSMessage>", _sessionId);
            string inMessage = MensajesCTS.GetLogOutMessage(TicketAccesoCTS.Id);

            DateTime startTime = DateTime.Now;

            log.Info("Finalizing session " + TicketAccesoCTS.Id + "..");


            string outMessage = ServicioMQ.SendMessageSession(InSessionQueueName, OutSessionQueueName, inMessage);

            var timerFinFinalizeSesionCTS = DateTime.Now;
            log.Info("Session finalized. Elapsed time = " + (timerFinFinalizeSesionCTS - startTime).TotalMilliseconds);

            TicketAccesoCTS.Id = "";
            TicketAccesoCTS.FechaCreacion = new DateTime();
            TicketAccesoCTS.FechaExpiracion = new DateTime();
            TicketAccesoCTS.SesionIniciada = false;

        }



    }
}
