using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CTSConnector
{
    [Serializable]
    [XmlRoot(ElementName = "CTSMessage")]
    public class CTSInMessage
    {
        public CTSHeader CTSHeader { get; set; }
        public InData Data { get; set; }

        public string GetXml()
        {
            string xmlMessage;

            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
            {
                /*Indent = true,*/
                OmitXmlDeclaration = false,
#if NET40
                Encoding = Encoding.GetEncoding(28591)
#else
                //Encoding = CodePagesEncodingProvider.Instance.GetEncoding(28591)
                Encoding = Encoding.UTF8
#endif
            };

            MemoryStream memoryStream = new MemoryStream();

            using (XmlWriter xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings))
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");
                XmlSerializer serializer = new XmlSerializer(GetType());
                serializer.Serialize(xmlWriter, this, ns);
            }

            memoryStream.Position = 0;
            using (StreamReader sr = new StreamReader(memoryStream))
            {
                xmlMessage = sr.ReadToEnd();
            }

            return xmlMessage;
        }
    }
}
