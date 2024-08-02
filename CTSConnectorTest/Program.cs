using System;
using System.IO;
using System.Reflection;
using CTSConnector;
using log4net;
using log4net.Config;
using Microsoft.Extensions.Configuration;

namespace CTSConnectorTest
{
    class Program
    {
        static void Main(string[] args)
        {
            CTSCaller ctsCaller = null;
            try
            {
                var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
                XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

                IConfiguration config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", true, true)
                    .Build();

                string hostNames = config["hostNames"];
                string ports = config["ports"];
                string channelName = config["channelName"];
                string queueManagerName = config["queueManagerName"];
                string inSessionQueueName = config["inSessionQueueName"];
                string outSessionQueueName = config["outSessionQueueName"];
                string inServiceQueueName = config["inServiceQueueName"];
                string outServiceQueueName = config["outServiceQueueName"];
                int waitInterval = Convert.ToInt32(config["waitInterval"]);
                int outMessageExpiry = Convert.ToInt32(config["outMessageExpiry"]);
                bool pooled = config["pooled"] == "1";
                int maxPoolSize = Convert.ToInt32(config["maxPoolSize"]);
                TimeSpan poolTimeout = TimeSpan.FromMinutes(Convert.ToDouble(config["poolTimeoutInMin"]));
                LoginInfo loginInfo = new LoginInfo();
                loginInfo.ApplicationId = config["applicationId"];
                loginInfo.UserId = config["userId"];
                int sessionTimeout = Convert.ToInt32(config["sessionTimeout"]);

                ctsCaller = CTSCaller.GetCTSCaller(hostNames, ports, channelName, queueManagerName, inSessionQueueName, outSessionQueueName, inServiceQueueName, outServiceQueueName, waitInterval, outMessageExpiry, pooled, maxPoolSize, poolTimeout, loginInfo, sessionTimeout);

                string inMessage = "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><CTSMessage><CTSHeader><Field name=\"SPExecutorServiceFactoryFilter\" type=\"S\">(service.impl=object)</Field><Field name=\"supportOffline\" type=\"C\">N</Field><Field name=\"sessionId\" type=\"S\">@@sessionId@@</Field></CTSHeader><Data><ProcedureRequest><SpName>cobis..sp_wst_direccion</SpName><Param name=\"@t_trn\" type=\"56\" io=\"0\" len=\"4\">1386</Param><Param name=\"@i_operacion\" type=\"47\" io=\"0\" len=\"1\">Q</Param><Param name=\"@i_di_direccion\" type=\"52\" io=\"0\" len=\"2\">3</Param><Param name=\"@i_di_ente\" type=\"56\" io=\"0\" len=\"4\">666</Param><Param name=\"@i_sistema_origen\" type=\"39\" io=\"0\" len=\"3\">DEX</Param><Param name=\"@i_usuario_alta\" type=\"39\" io=\"0\" len=\"7\">scoring</Param><Param name=\"@i_di_tipo\" type=\"39\" io=\"0\" len=\"2\">LA</Param><Param name=\"@i_di_descripcion\" type=\"39\" io=\"0\" len=\"7\">FLORIDA</Param><Param name=\"@i_di_numero\" type=\"56\" io=\"0\" len=\"4\">666</Param><Param name=\"@i_di_postal\" type=\"39\" io=\"0\" len=\"4\">1234</Param><Param name=\"@i_di_ciudad\" type=\"52\" io=\"0\" len=\"2\">195</Param><Param name=\"@i_di_provincia\" type=\"52\" io=\"0\" len=\"2\">1</Param><Param name=\"@i_di_pais\" type=\"52\" io=\"0\" len=\"2\">80</Param><Param name=\"@i_componente\" type=\"47\" io=\"0\" len=\"1\">N</Param><Param name=\"@o_di_direccion\" type=\"52\" io=\"1\" len=\"0\">0</Param><Param name=\"@o_di_direccionp\" type=\"52\" io=\"1\" len=\"0\">0</Param></ProcedureRequest></Data></CTSMessage>";
                for (int i = 0; i < 10; i++)
                {
                    ctsCaller.SendServiceMessage(inMessage);
                }

                Console.WriteLine("End.");
            }
            catch (Exception E)
            {
                Console.WriteLine("Exception = " + E.ToString());
            }
            finally
            {
                if (ctsCaller != null) ctsCaller.Dispose();
            }

            Console.WriteLine("Press key to continue...");
            Console.ReadKey();
        }
    }
}
