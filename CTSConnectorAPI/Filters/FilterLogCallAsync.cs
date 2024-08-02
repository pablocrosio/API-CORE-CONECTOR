using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Newtonsoft.Json;
using log4net;

namespace CTSConnectorAPI.Filters
{
    public class FilterLogCallAsync : IAsyncActionFilter, IAsyncResultFilter
    {
        private static readonly ILog _log = LogManager.GetLogger("RollingFile");
        String threadData = "";

        //Filtr de accion (antes del controlador y despues el mismo)
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //Nuevo GUID
            threadData = Guid.NewGuid().ToString();

            using (log4net.LogicalThreadContext.Stacks["NDC"].Push(threadData))
            {
                _log.DebugFormat("[{0}]: Llamada Recibida", context.HttpContext.Connection.Id);

                String bodyString = "";
                //Obtiene el BODY de la peticion (el json enviado)

                //context.HttpContext.Request.EnableBuffering();
                context.HttpContext.Request.Body.Position = 0;
                var reader = new StreamReader(context.HttpContext.Request.Body);
                var taskBody = reader.ReadToEndAsync();
                Task.WaitAll(taskBody);
                bodyString = taskBody.Result;

                _log.DebugFormat("[{0}]", bodyString);
                context.HttpContext.Request.Body.Seek(0, SeekOrigin.Begin);

                // next() calls the action method.
                await next();
            }

                
        }


        /// <summary>
        /// Filtro Final
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            using (log4net.LogicalThreadContext.Stacks["NDC"].Push(threadData))
            {
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


                // next() calls the action method.
                await next();
            }

                
        }
    }
}
