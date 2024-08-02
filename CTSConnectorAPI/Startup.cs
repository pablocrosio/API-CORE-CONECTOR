using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CTSConnector;
using CTSConnectorAPI.Controllers;
using CTSConnectorAPI.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Newtonsoft.Json.Converters;

namespace CTSConnectorAPI
{
    public class Startup
    {
        public static readonly GestorMemoria gestorMemoria = new GestorMemoria();
        public static readonly CTSConnector.MQCleaner cleaner = new CTSConnector.MQCleaner();
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            //Se inicializa la cultura al formato americano (M/d/yy)
            CultureInfo ci = new CultureInfo("en-US");
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;

            //gestorMemoria.StartAsync();
            cleaner.StartAsync();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddResponseCompression(options => {
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
            });

            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest;
            });


            services.AddHealthChecks();

            services.AddControllers(options =>
            {
                options.Filters.Add(typeof(FilterLogCall));
            })
                .AddNewtonsoftJson(jsonOptions => {
                jsonOptions.SerializerSettings.Converters.Add(new StringEnumConverter());
                
            });


            // Registra el generador Swagger, definiendo 1 o nas documentos Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "3.1.0",
                    Title = "Conector - API Core",
                    Description = "Conector para la conexion con el CORE",
                    Contact = new OpenApiContact
                    {
                        Name = "Banco Hipotecario",
                        Email = "core@hipotecario.com.ar",
                        Url = new Uri("http://www.hipotecario.com.ar")
                    }
                });


                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime applicationLifetime)//ILoggerFactory loggerFactory,
        {
            //Esto es para la lectura del BODY. Se indica aqui porque en el FilterLogCallAsync dejo de funcionar en 3.1
            app.Use((context, next) =>
            {
                context.TraceIdentifier = Guid.NewGuid().ToString();

                using (log4net.LogicalThreadContext.Stacks["NDC"].Push(context.TraceIdentifier))
                {
                    context.Request.EnableBuffering();
                    return next();
                }
                
            });

            //Compresion
            //app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //Indica que empiece a utilizar Swagger
            app.UseSwagger();

            //Agrega el endpoint Swagger
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "default");
                c.RoutePrefix = string.Empty;
            });

            
            app.UseRouting();
            //app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });



            var optionsHealthCheck = new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions();
            optionsHealthCheck.ResponseWriter = async (c, r) => {

                c.Response.ContentType = "application/json";

                var result = JsonConvert.SerializeObject(new
                {
                    status = r.Status.ToString(),
                    errors = r.Entries.Select(e => new { key = e.Key, value = e.Value.Status.ToString() }),
                    dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")
                });

                await c.Response.WriteAsync(result);
            };

            //Debe ir luego de UseMvc() porque sino falla.
            app.UseHealthChecks("/health", optionsHealthCheck);


            //Libera el pool de conexiones del conector. Las solicitudes de bloquearan hasta que se complete esta instruccion.
            applicationLifetime.ApplicationStopping.Register(OnShutDown);

            try
            {
                //Inicializo la 1era. sesion
                CTSCaller ctsCaller = CTSCaller.GetCTSCaller(Configuration);
                ctsCaller.IniciarSesion();
            }
            catch (Exception ex)
            {
                Console.WriteLine("La sesion no se pudo inicializarse al cargar la aplicacion. Se inicializará en el primer inicio. Detalle: " + ex.ToString());
            }

        }

        public void OnShutDown()
        {
            CTSCaller ctsCaller = CTSCaller.GetCTSCaller();
            if (ctsCaller != null && ctsCaller.SessionId != "-1") ctsCaller.Dispose();

            //gestorMemoria.StopAsync();
            cleaner.StopAsync();
        }
    }
}
