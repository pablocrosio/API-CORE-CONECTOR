using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LogUtil
{
    public class Logger
    {
        private static object _thisLock = new object();
        private static Logger _logger;
        private bool _disposed = false;
        private static IPrinter _printer;
        private StreamWriter _sw;
        private static Queue<LogData> _messages = new Queue<LogData>();

        [ThreadStatic]
        private static string _threadData = "";

        public static void DeleteLogFile()
        {
            string fileName = GetFileName();
            if (File.Exists(fileName)) File.Delete(fileName);
        }

        private static string GetFileName()
        {
            //return "log_" + System.Diagnostics.Process.GetCurrentProcess().Id + ".txt";
            return "d:\\logs\\log_" + System.Diagnostics.Process.GetCurrentProcess().Id + ".txt";
        }

        public static void SetPrinter(IPrinter printer)
        {
            _printer = printer;
        }

        public static Logger GetLogger()
        {
            if (_logger == null) _logger = new Logger();
            return _logger;
        }

        public string ThreadData { get { return _threadData; } set { _threadData = value; } }

        private Logger()
        {
#if NET40
            Encoding encoding1252 = Encoding.GetEncoding(1252);
#else
            Encoding encoding1252 = CodePagesEncodingProvider.Instance.GetEncoding(1252);
#endif
            _sw = new StreamWriter(GetFileName(), true, encoding1252);
            Thread worker = new Thread(WaitForMessages);
            worker.IsBackground = true;
            worker.Start();
        }

        public void Log(string text, params object[] args)
        {
            LogAsync(text, false, args);
        }

        public void LogAndPrint(string text, params object[] args)
        {
            LogAsync(text, true, args);
        }

        private void LogAsync(string text, bool print, params object[] args)
        {
            DateTime eventTime = DateTime.Now;
            string threadName = Thread.CurrentThread.Name == string.Empty ? Thread.CurrentThread.Name : Thread.CurrentThread.ManagedThreadId.ToString();
            //LogDelegate d = new LogDelegate(_Log);
            //d.BeginInvoke(eventTime, _threadData, threadName, text, print, args, null, null);
            //Task.Run(() => _Log(eventTime, _threadData, threadName, text, print, args));
            lock (_messages)
            {
                _messages.Enqueue(new LogData() { EventTime = eventTime, ThreadData = _threadData, ThreadName = threadName, Text = text, Print = print, Args = args });
            }
        }

        //private delegate void LogDelegate(DateTime eventTime, string threaddData, string threadName, string text, bool print, params object[] args);

        private void _Log(DateTime eventTime, string threadData, string threadName, string text, bool print, params object[] args)
        {
            lock (_thisLock)
            {
                if (print && _printer != null) _printer.PrintText(text, args);
#if NET40
                Encoding encoding1252 = Encoding.GetEncoding(1252);
#else
                Encoding encoding1252 = CodePagesEncodingProvider.Instance.GetEncoding(1252);
#endif
                string prefix = DateTime.Now.ToString("yyyyMMdd HHmmss.fff") + "|" + eventTime.ToString("yyyyMMdd HHmmss.fff") + "|" + threadData + "|" + threadName + "|";
                _sw.WriteLine(prefix + string.Format(text, args));
            }
        }

        private void Flush()
        {
            lock (_thisLock)
            {
                _sw.Flush();
            }
        }

        private void WaitForMessages()
        {
            while (!_disposed)
            {
                int messageCount = GetMessageCount();
                while (messageCount > 0)
                {
                    Console.WriteLine(DateTime.Now.ToString("yyyyMMdd HHmmss.fff") + "|" + "Message Count = " + messageCount);
                    try
                    {
                        LogData logData;
                        lock (_messages)
                        {
                            logData = _messages.Dequeue();
                        }
                        _Log(logData.EventTime, logData.ThreadData, logData.ThreadName, logData.Text, logData.Print, logData.Args);
                    }
                    catch (Exception E)
                    {
                        Console.WriteLine("WaitForMessages Exception = " + E.ToString());
                    }
                    messageCount = GetMessageCount();
                }
                Flush();
                Console.WriteLine(DateTime.Now.ToString("yyyyMMdd HHmmss.fff") + "|" + "Start Sleep");
                Thread.Sleep(500);
                Console.WriteLine(DateTime.Now.ToString("yyyyMMdd HHmmss.fff") + "|" + "End Sleep");
            }
            Console.WriteLine("WaitForMessages has exited");
        }

        private int GetMessageCount()
        {
            int messageCount = 0;
            lock (_messages)
            {
                messageCount = _messages.Count;
            }
            return messageCount;
        }

        public void Dispose()
        {
            if (_disposed) return;
            try
            {
                while (GetMessageCount() > 0) Thread.Sleep(500);
                _sw.Close();
            }
            catch { }
            finally { _disposed = true; }
        }

        ~Logger()
        {
            Dispose();
        }
    }
}
