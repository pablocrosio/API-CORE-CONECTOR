using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CTSConnector;
using CTSConnectorAPI.Dto;
using CtsWrapper.CtsObjects;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CTSConnectorAPI.Controllers
{
    /// <summary>
    /// Permite realizar ejecuciones a la DB por medio de CTS
    /// </summary>
    [Route("v1/[controller]")]
    [ApiController]
    public class CTSController : ControllerBase
    {
        private static readonly ILog _log = LogInicializer._log;
        static JsonConverter[] converters = { new FooConverter() };
        CTSSerializer serializer = new CTSSerializer();


        public CTSController(IConfiguration configuration)
        {
            this.Configuration = configuration;

        }


        public IConfiguration Configuration { get; set; }

        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(200, Type = typeof(RespuestaDTO))]
        [ProducesResponseType(500, Type = typeof(String))]
        public IActionResult Post([FromBody] ConsultaDTO dto)
        {
            
            
                ObjectResult objRespuesta = null;
                CTSCaller ctsCaller = CTSCaller.GetCTSCaller(Configuration);



                try
                {
                    RespuestaDTO respuesta = new RespuestaDTO();
                    //dto.XmlIn = "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><CTSMessage><CTSHeader><Field name=\"SPExecutorServiceFactoryFilter\" type=\"S\">(service.impl=object)</Field><Field name=\"supportOffline\" type=\"C\">N</Field><Field name=\"sessionId\" type=\"S\">@@sessionId@@</Field></CTSHeader><Data><ProcedureRequest><SpName>cobis..sp_wst_direccion</SpName><Param name=\"@t_trn\" type=\"56\" io=\"0\" len=\"4\">1386</Param><Param name=\"@i_operacion\" type=\"47\" io=\"0\" len=\"1\">Q</Param><Param name=\"@i_di_direccion\" type=\"52\" io=\"0\" len=\"2\">3</Param><Param name=\"@i_di_ente\" type=\"56\" io=\"0\" len=\"4\">666</Param><Param name=\"@i_sistema_origen\" type=\"39\" io=\"0\" len=\"3\">DEX</Param><Param name=\"@i_usuario_alta\" type=\"39\" io=\"0\" len=\"7\">scoring</Param><Param name=\"@i_di_tipo\" type=\"39\" io=\"0\" len=\"2\">LA</Param><Param name=\"@i_di_descripcion\" type=\"39\" io=\"0\" len=\"7\">FLORIDA</Param><Param name=\"@i_di_numero\" type=\"56\" io=\"0\" len=\"4\">666</Param><Param name=\"@i_di_postal\" type=\"39\" io=\"0\" len=\"4\">1234</Param><Param name=\"@i_di_ciudad\" type=\"52\" io=\"0\" len=\"2\">195</Param><Param name=\"@i_di_provincia\" type=\"52\" io=\"0\" len=\"2\">1</Param><Param name=\"@i_di_pais\" type=\"52\" io=\"0\" len=\"2\">80</Param><Param name=\"@i_componente\" type=\"47\" io=\"0\" len=\"1\">N</Param><Param name=\"@o_di_direccion\" type=\"52\" io=\"1\" len=\"0\">0</Param><Param name=\"@o_di_direccionp\" type=\"52\" io=\"1\" len=\"0\">0</Param></ProcedureRequest></Data></CTSMessage>";

                    String xml_in = ObtenerXmlIn(dto);

                    CTSMessage obj = ProcesarMensaje(ctsCaller, xml_in);

                    //Se evaluan los parametros de salida porque los numericos son removidos. 
                    //Esto se hace para evitar actualizar en todas las api el paquete
                    ProcesarOutputParams(dto, obj);

                    respuesta.XmlOut = JsonConvert.SerializeObject(obj, new JsonSerializerSettings() { Formatting = Formatting.None });

                    objRespuesta = StatusCode(200, respuesta);

                    _log.Debug("Respuesta entregada satisfactoria");
                }
                catch (Exception ex)
                {
                    objRespuesta = StatusCode(500, ex.ToString());

                    _log.Error(ex.ToString());
                }

                return objRespuesta;
            
                
        }

        [HttpPost]
        [Route("/sesiones")]
        [Produces("application/json")]
        [ProducesResponseType(200, Type = typeof(RespuestaDTO))]
        [ProducesResponseType(500, Type = typeof(String))]
        public IActionResult Login([FromBody] ConsultaDTO dto)
        {
            ObjectResult objRespuesta = null;
            CTSCaller ctsCaller = CTSCaller.GetCTSCaller(Configuration);

            try
            {
                RespuestaDTO respuesta = new RespuestaDTO();
                //dto.XmlIn = "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><CTSMessage><CTSHeader><Field name=\"SPExecutorServiceFactoryFilter\" type=\"S\">(service.impl=object)</Field><Field name=\"supportOffline\" type=\"C\">N</Field><Field name=\"sessionId\" type=\"S\">@@sessionId@@</Field></CTSHeader><Data><ProcedureRequest><SpName>cobis..sp_wst_direccion</SpName><Param name=\"@t_trn\" type=\"56\" io=\"0\" len=\"4\">1386</Param><Param name=\"@i_operacion\" type=\"47\" io=\"0\" len=\"1\">Q</Param><Param name=\"@i_di_direccion\" type=\"52\" io=\"0\" len=\"2\">3</Param><Param name=\"@i_di_ente\" type=\"56\" io=\"0\" len=\"4\">666</Param><Param name=\"@i_sistema_origen\" type=\"39\" io=\"0\" len=\"3\">DEX</Param><Param name=\"@i_usuario_alta\" type=\"39\" io=\"0\" len=\"7\">scoring</Param><Param name=\"@i_di_tipo\" type=\"39\" io=\"0\" len=\"2\">LA</Param><Param name=\"@i_di_descripcion\" type=\"39\" io=\"0\" len=\"7\">FLORIDA</Param><Param name=\"@i_di_numero\" type=\"56\" io=\"0\" len=\"4\">666</Param><Param name=\"@i_di_postal\" type=\"39\" io=\"0\" len=\"4\">1234</Param><Param name=\"@i_di_ciudad\" type=\"52\" io=\"0\" len=\"2\">195</Param><Param name=\"@i_di_provincia\" type=\"52\" io=\"0\" len=\"2\">1</Param><Param name=\"@i_di_pais\" type=\"52\" io=\"0\" len=\"2\">80</Param><Param name=\"@i_componente\" type=\"47\" io=\"0\" len=\"1\">N</Param><Param name=\"@o_di_direccion\" type=\"52\" io=\"1\" len=\"0\">0</Param><Param name=\"@o_di_direccionp\" type=\"52\" io=\"1\" len=\"0\">0</Param></ProcedureRequest></Data></CTSMessage>";

                String xml_in = ObtenerXmlIn(dto);

                CTSMessage obj = ProcesarLogin(ctsCaller, xml_in);

                //Se evaluan los parametros de salida porque los numericos son removidos. 
                //Esto se hace para evitar actualizar en todas las api el paquete
                ProcesarOutputParams(dto, obj);

                respuesta.XmlOut = JsonConvert.SerializeObject(obj, new JsonSerializerSettings() { Formatting = Formatting.None });

                objRespuesta = StatusCode(200, respuesta);

                _log.Debug("Respuesta entregada satisfactoria");
            }
            catch (Exception ex)
            {
                objRespuesta = StatusCode(500, ex.ToString());

                _log.Error(ex.ToString());
            }
            return objRespuesta;
        }


        private void ProcesarOutputParams(ConsultaDTO dto, CTSMessage objRespuestaCTS)
        {
            //Convirte la llamada en XML
            CTSMessage objConsultaCTS = JsonConvert.DeserializeObject<CTSMessage>(dto.XmlIn, new JsonSerializerSettings() { Converters = converters });

            foreach (CTSParameter param in ((CTSProcedureRequest)(objConsultaCTS.Data)).Parametros.FindAll(x => x.io == "1"))
            {
                if (((CTSProcedureResponse)objRespuestaCTS.Data).OutputParams.Find(x => x.name == param.name) == null)
                {
                    CTSParameter objParamNuevo = new CTSParameter();
                    objParamNuevo.name = param.name;
                    objParamNuevo.type = param.type;
                    objParamNuevo.value = param.value;

                    ((CTSProcedureResponse)objRespuestaCTS.Data).OutputParams.Add(objParamNuevo);
                }
            }
        }

        private String ObtenerXmlIn(ConsultaDTO dto)
        {
            //Convirte la llamada en XML
            
            CTSMessage objDeserializado = JsonConvert.DeserializeObject<CTSMessage>(dto.XmlIn, new JsonSerializerSettings() { Converters = converters });

            //Convierte el XML de CTS a json
            String resultado = serializer.ToXML(objDeserializado);

            return resultado;
        }

        private CTSMessage ProcesarMensaje(CTSCaller ctsCaller, String xml_in)
        {
            _log.Info("Executing service..");
            ctsCaller.IniciarSesion();

            String xml_out = ctsCaller.SendServiceMessage(xml_in);
            CTSMessage resultado = serializer.FromXML(xml_out);


            return resultado;
        }

        private CTSMessage ProcesarLogin(CTSCaller ctsCaller, String xml_in)
        {
            _log.Info("Executing service..");

            String xml_out = ctsCaller.GestorSesionesCobis().IniciarSesion(xml_in);
            CTSMessage resultado = serializer.FromXML(xml_out);
            return resultado;
        }


    }


    public class FooConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(CTSMessageData));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            if (!String.IsNullOrEmpty(jo["SpName"].Value<string>()))
            {
                return jo.ToObject<CTSProcedureRequest>(serializer);
            }
            else if (!String.IsNullOrEmpty(jo["Return"].Value<string>()))
            {
                return jo.ToObject<CTSProcedureResponse>(serializer);
            }
            else
            {
                throw new Exception("No se pudo identificar el tipo de objeto CTSMessageData");
            }

            return null;
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }



    
}



