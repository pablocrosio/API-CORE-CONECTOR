using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IBM.WMQ;

namespace PoolUtil
{
    public interface IQMResource : IDisposable
    {
        string ResourceSource { get; }
        MQQueue AccessQueue(string queueName, int openOptions);
    }
}
