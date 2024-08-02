using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CTSConnector.MQ
{
    public class ServicioMQ
    {
        public MessagingService MessagingServices { get;  set; }


        public String SendMessageSession(string inQueueName, string outQueueName, string messageString)
        {
            byte[] messageId = MessagingServices.PutMessageHA(inQueueName, messageString);


            messageString = MessagingServices.GetMessageHA(outQueueName, messageId);



            return messageString;
        }


        public byte[] SendMessageWork(string inQueueName, string outQueueName, string messageString)
        {
            byte[] messageId = MessagingServices.PutMessageHA(inQueueName, messageString);

            return messageId;
        }

        public String GetMessageWork(string inQueueName, string outQueueName, byte[] messageId)
        {
            String messageString = MessagingServices.GetMessageHA(outQueueName, messageId);


            return messageString;
        }
    }
}
