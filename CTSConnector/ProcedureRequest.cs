using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace CTSConnector
{
    [Serializable]
    public class ProcedureRequest
    {
        public string SpName { get; set; }

        [XmlElement("Param")]
        public List<Param> Params { get; set; }
    }
}
