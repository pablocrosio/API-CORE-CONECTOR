using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogUtil
{
    public interface IPrinter
    {
        void PrintText(string text, params object[] args);
    }
}
