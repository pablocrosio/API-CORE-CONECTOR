using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogUtil
{
    public class LoggingObject
    {
        private Logger _logger;
        private bool _fullLog;

        public LoggingObject()
        {
            _logger = Logger.GetLogger();
            _fullLog = false;
        }

        public void SetFullLog(bool fullLog)
        {
            _fullLog = fullLog;
        }

        public void Log(string text, params object[] args)
        {
            if (_fullLog) _logger.LogAndPrint(text, args);
            else _logger.Log(text, args);
        }

        public void LogAndPrint(string text, params object[] args)
        {
            _logger.LogAndPrint(text, args);
        }

        public string ThreadData { get { return _logger.ThreadData; } set { _logger.ThreadData = value; } }

        public Logger GetLogger()
        {
            return _logger;
        }
    }
}
