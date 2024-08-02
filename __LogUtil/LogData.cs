using System;
using System.Collections.Generic;
using System.Text;

namespace LogUtil
{
    public class LogData
    {
        public DateTime EventTime;
        public string ThreadData;
        public string ThreadName;
        public string Text;
        public bool Print;
        public object[] Args;
    }
}
