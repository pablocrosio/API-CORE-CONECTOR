using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace CTSConnector
{
    [Serializable]
    public class Param
    {
        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("len")]
        public string Len { get; set; }

        [XmlAttribute("io")]
        public string IO { get; set; }

        [XmlText]
        public string Value { get; set; }
    }
}
