using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace CTSConnector
{
    [Serializable]
    public class CTSHeader
    {
        [XmlElement("Field")]
        public List<Field> Fields { get; set; }
    }
}
