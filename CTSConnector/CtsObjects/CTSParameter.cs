using System;
using System.Collections.Generic;
using System.Text;

namespace CtsWrapper.CtsObjects
{
    public class CTSParameter
    {
        public String name { get; set; }
        public String type { get; set; }
        public String io { get; set; }
        public String len { get; set; }
        public Object value { get; set; }
    }
}
