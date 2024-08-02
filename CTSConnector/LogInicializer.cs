using log4net;
using System;
using System.Collections.Generic;
using System.Text;

namespace CTSConnector
{
    public static class LogInicializer
    {

        private static readonly ILog logAPP = LogManager.GetLogger("RollingFile");

        public static ILog _log { get => logAPP; }
    }
}
