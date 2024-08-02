using System;
using System.Collections.Generic;
using System.Text;

namespace CtsWrapper.CtsObjects
{
    public class CTSMessage
    {
        private List<CTSHeaderField> _CTSHeader = new List<CTSHeaderField>();

        public List<CTSHeaderField> CTSHeader {
            get
            {
                return _CTSHeader;
            }
        }
        public CTSMessageData Data { get; set; }
    }
}
