using CTSConnector;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CTSConnectorTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Program program = new Program();
            program.TestA(args);
            //program.TestB();
        }

        public void TestA(string[] args)
        {
            CTSCaller ctsCaller = null;
            try
            {
                XmlConfigurator.Configure(new FileInfo("log4net.config"));

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
                int sessionTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["sessionTimeout"]);

                ctsCaller = CTSCaller.GetCTSCaller(hostNames, ports, channelName, queueManagerName, inSessionQueueName, outSessionQueueName, inServiceQueueName, outServiceQueueName, waitInterval, outMessageExpiry, pooled, maxPoolSize, poolTimeout, loginInfo, sessionTimeout);

                string inMessage = "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><CTSMessage><CTSHeader><Field name=\"SPExecutorServiceFactoryFilter\" type=\"S\">(service.impl=object)</Field><Field name=\"supportOffline\" type=\"C\">N</Field><Field name=\"sessionId\" type=\"S\">@@sessionId@@</Field></CTSHeader><Data><ProcedureRequest><SpName>cobis..sp_wst_direccion</SpName><Param name=\"@t_trn\" type=\"56\" io=\"0\" len=\"4\">1386</Param><Param name=\"@i_operacion\" type=\"47\" io=\"0\" len=\"1\">Q</Param><Param name=\"@i_di_direccion\" type=\"52\" io=\"0\" len=\"2\">3</Param><Param name=\"@i_di_ente\" type=\"56\" io=\"0\" len=\"4\">666</Param><Param name=\"@i_sistema_origen\" type=\"39\" io=\"0\" len=\"3\">DEX</Param><Param name=\"@i_usuario_alta\" type=\"39\" io=\"0\" len=\"7\">scoring</Param><Param name=\"@i_di_tipo\" type=\"39\" io=\"0\" len=\"2\">LA</Param><Param name=\"@i_di_descripcion\" type=\"39\" io=\"0\" len=\"7\">FLORIDA</Param><Param name=\"@i_di_numero\" type=\"56\" io=\"0\" len=\"4\">666</Param><Param name=\"@i_di_postal\" type=\"39\" io=\"0\" len=\"4\">1234</Param><Param name=\"@i_di_ciudad\" type=\"52\" io=\"0\" len=\"2\">195</Param><Param name=\"@i_di_provincia\" type=\"52\" io=\"0\" len=\"2\">1</Param><Param name=\"@i_di_pais\" type=\"52\" io=\"0\" len=\"2\">80</Param><Param name=\"@i_componente\" type=\"47\" io=\"0\" len=\"1\">N</Param><Param name=\"@o_di_direccion\" type=\"52\" io=\"1\" len=\"0\">0</Param><Param name=\"@o_di_direccionp\" type=\"52\" io=\"1\" len=\"0\">0</Param></ProcedureRequest></Data></CTSMessage>";
                int total = 10;
                if (args.Length > 0) int.TryParse(args[0], out total);
                for (int i = 0; i < total; i++)
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

        public void TestB()
        {
            try
            {
                List<Field> fields = new List<Field>() { new Field { Type = "C", Name = "pepe", Value = "xxxx" } };
                CTSHeader ctsHeader = new CTSHeader() { Fields = fields };
                List<Param> parameters = new List<Param> { new Param { Type = "56", Name = "@t_trn", Len = "4", IO = "0", Value = "1386" } };
                ProcedureRequest procedureRequest = new ProcedureRequest() { SpName = "cobis..sp_wst_direccion", Params = parameters };
                CTSInMessage ctsMessage = new CTSInMessage() { CTSHeader = ctsHeader, Data = new InData() { ProcedureRequest = procedureRequest } };
                Console.WriteLine(ctsMessage.GetXml());
            }
            catch (Exception E)
            {
                Console.WriteLine("Exception = " + E.ToString());
            }
            Console.WriteLine("Press key to continue...");
            Console.ReadKey();
        }
    }
}
