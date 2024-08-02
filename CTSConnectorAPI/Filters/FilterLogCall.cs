using CTSConnector;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CTSConnectorAPI.Filters
{
    public class FilterLogCall : IActionFilter
    {
        public static readonly ILog _log = LogInicializer._log;
        String threadData = "";
        private DateTime startTime;

        public void OnActionExecuting(ActionExecutingContext context)
        {
            startTime = DateTime.Now;
            // Do something before the action executes.
            threadData = context.HttpContext.TraceIdentifier;

            _log.InfoFormat("[{0}]: Llamada Recibida", context.HttpContext.Connection.Id);

            String bodyString = "";
            //Obtiene el BODY de la peticion (el json enviado)

            //context.HttpContext.Request.EnableBuffering();
            context.HttpContext.Request.Body.Position = 0;
            var reader = new StreamReader(context.HttpContext.Request.Body);
            bodyString = reader.ReadToEnd();

            _log.DebugFormat("[{0}]", bodyString);
            context.HttpContext.Request.Body.Seek(0, SeekOrigin.Begin);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Do something after the action executes.

            if (context.Result != null)
            {
                if (context.Result is BadRequestObjectResult)
                {
                    _log.Error("[BadRequestObjectResult]: Verifique el Json en busca de datos incorrectos. Culture: En - US");
                }
                else
                {
                    Microsoft.AspNetCore.Mvc.ObjectResult obj = context.Result as Microsoft.AspNetCore.Mvc.ObjectResult;

                    _log.DebugFormat("[{0} - {1}] Respuesta: {2} - {3}: ", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.ffffff"), context.HttpContext.Connection.Id, obj.StatusCode, JsonConvert.SerializeObject(obj.Value, Formatting.None));
                }

            }
            else
            {
                _log.Error("context.Result = Error");
            }
            DateTime endTime = DateTime.Now;
            _log.Info("Tiempo Peticion: " + (endTime - startTime).TotalMilliseconds + "ms");
        }
    }
}
