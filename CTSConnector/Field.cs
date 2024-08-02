using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace CTSConnector
{
    [Serializable]
    public class Field
    {
        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlText]
        public string Value { get; set; }
    }
}
