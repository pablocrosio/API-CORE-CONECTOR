using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace CTSConnector.Sesiones
{
    public static class MensajesCTS
    {
        public static String ApplicationId { get; set; } = "";
        public static String UserId { get; set; } = "";
        public static String Password { get; set; } = "";

        /// <summary>
        /// Se obtiene un mensaje CTS para iniciar la sesion
        /// </summary>
        /// <returns></returns>
        public static String GetLoginMessage()
        {
            List<Field> fields = new List<Field>() { new Field { Type = "S", Name = "servicio", Value = "8" } };
            CTSHeader ctsHeader = new CTSHeader() { Fields = fields };

            List<Param> parameters = new List<Param> { new Param { Type = "39", Name = "@i_servicio", Len = "1", IO = "0", Value = "8" },
                                                       new Param { Type = "38", Name = "@i_id_aplicacion", Len = "0", IO = "0", Value = ApplicationId },
                                                       new Param { Type = "39", Name = "@i_login", Len = "0", IO = "0", Value = UserId },
                                                       new Param { Type = "39", Name = "@i_clave", Len = "0", IO = "0", Value = Password }
                                                     };

            ProcedureRequest procedureRequest = new ProcedureRequest() { SpName = "master..sp_reclogin_rc",
                                                                         Params = parameters };

            CTSInMessage ctsMessage = new CTSInMessage() { CTSHeader = ctsHeader,
                                                           Data = new InData() { ProcedureRequest = procedureRequest }
                                                         };

            return ctsMessage.GetXml();
        }


        /// <summary>
        /// A partir del mensaje CTS recibido se obtiene la sesion. El mensaje debe ser la respuesta a un inicio de sesion
        /// </summary>
        /// <param name="xmlOut"></param>
        /// <returns></returns>
        public static String GetIdSesionDesdeMensajeCTS(String xmlOut)
        {
            String resultado = "-1";

            //<?xml version="1.0" encoding="ISO-8859-1" ?><CTSMessage><CTSHeader><Field name="fromServer" type="S">server1</Field><Field name="servicio" type="S">8</Field><Field name="sesn" type="N">30193</Field><Field name="dbms" type="S">SYBCTS</Field></CTSHeader><Data><ProcedureResponse><ResultSet><Header><col name="reclogin" type="39" len="16"/></Header><rw><cd>00:00:0023:59:59</cd></rw></ResultSet><ResultSet><Header><col name="respuesta" type="39" len="18"/></Header><rw><cd>Bienvenido a COBIS</cd></rw></ResultSet><OutputParams><param name="sessionId" type="39" len="51">ID:9b84a29fcb53416984e07a0277b8dfdc0000000000000000</param></OutputParams><return>0</return></ProcedureResponse></Data></CTSMessage>
            XmlDocument document = new XmlDocument();

            document.LoadXml(xmlOut);
            XmlNode paramSessionId = document.SelectSingleNode("/CTSMessage/Data/ProcedureResponse/OutputParams/param[@name='sessionId']");

            if (paramSessionId == null)
            {
                //_log.Error("No se pudo obtener sessionId");
                throw new ApplicationException("No se pudo obtener sessionId");
            }
            else
            {
                resultado = paramSessionId.InnerText;
            }
            

            return resultado;
        }


        /// <summary>
        /// Se obtiene un mensaje CTS para cerrar la sesion
        /// </summary>
        /// <returns></returns>
        public static string GetLogOutMessage(String SessionId)
        {
            List<Field> fields = new List<Field>() { new Field { Type = "S", Name = "externalUser", Value = UserId },
                                                     new Field { Type = "S", Name = "servicio", Value = "8" },
                                                     new Field { Type = "S", Name = "externalApplicationId", Value = ApplicationId },
                                                     new Field { Type = "S", Name = "sessionId", Value = SessionId } };

            CTSHeader ctsHeader = new CTSHeader() { Fields = fields };
            List<Param> parameters = new List<Param> { new Param { Type = "39", Name = "@i_servicio", Len = "1", IO = "0", Value = "8" },
                                                       new Param { Type = "38", Name = "@i_id_aplicacion", Len = "0", IO = "0", Value = ApplicationId },
                                                       new Param { Type = "39", Name = "@i_login", Len = "0", IO = "0", Value = UserId } };

            ProcedureRequest procedureRequest = new ProcedureRequest() { SpName = "master..sp_endlogin_rc",
                                                                         Params = parameters };

            CTSInMessage ctsMessage = new CTSInMessage() { CTSHeader = ctsHeader,
                                                           Data = new InData() { ProcedureRequest = procedureRequest }
                                                         };

            return ctsMessage.GetXml();
        }
    }
}
