using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CTSConnectorAPI
{
    public class GestorMemoria
    {
        private Timer _timer;

        public GestorMemoria()
        {
        }

        public Task StartAsync()
        {
            _timer = new Timer(DoWork, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(30));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            GC.Collect();
            Console.WriteLine("Aplicando GC.Collect()");
        }

        public Task StopAsync()
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
