using CtsWrapper.CtsObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace CTSConnector
{
    public class CTSSerializer
    {
        public String ToXML(CTSMessage obj)
        {
            //El CTS responde siempre en "iso-8859-1" por lo tanto el XML a enviar tiene definida en su declaracion iso-8859-1

            XmlDocument xmlDoc = new XmlDocument();
            XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "iso-8859-1", null);

            //Elemento Root
            XmlNode cts_message = xmlDoc.CreateNode(XmlNodeType.Element, "CTSMessage", "");

            //El header
            XmlNode cts_header = xmlDoc.CreateNode(XmlNodeType.Element, "CTSHeader", "");

            foreach (CTSHeaderField hf in obj.CTSHeader)
            {
                //Elemento Field
                XmlNode cts_field = xmlDoc.CreateNode(XmlNodeType.Element, "Field", "");
                cts_field.InnerText = hf.value;

                //Atributo name
                XmlAttribute attr_name = xmlDoc.CreateAttribute("name");
                attr_name.InnerText = hf.name;
                cts_field.Attributes.Append(attr_name);

                //Atributo type
                XmlAttribute attr_type = xmlDoc.CreateAttribute("type");
                attr_type.InnerText = hf.type;
                cts_field.Attributes.Append(attr_type);


                cts_header.AppendChild(cts_field);
            }

            cts_message.AppendChild(cts_header);

            //Elemento Data
            XmlNode cts_data = xmlDoc.CreateElement("Data");

            //Elemento ProcedureRequest (dado que se esta llamando a consultar un SP)
            XmlNode cts_procedure_request = xmlDoc.CreateElement("ProcedureRequest");
            CTSProcedureRequest sp = (CTSProcedureRequest)obj.Data;

            //El procedimiento que va dentro del data
            XmlNode sp_name = xmlDoc.CreateElement("SpName");
            sp_name.InnerText = sp.SpName;
            cts_procedure_request.AppendChild(sp_name);

            cts_data.AppendChild(cts_procedure_request);

            //Elementos Param del Procedure Request
            foreach (CTSParameter param in sp.Parametros)
            {
                //Elemento Param
                XmlNode cts_param = xmlDoc.CreateNode(XmlNodeType.Element, "Param", "");

                //El valor del parametro a escribir (Si es una fecha o no)
                if (param.type == "61") //Es una fecha
                {

                    if ((param.value == null) || (param.value == DBNull.Value))
                    {
                        if (param.io == "1") //parametros de salida
                        {
                            cts_param.InnerText = "";
                        }
                        else
                        {
                            cts_param.InnerText = "NULL";
                        }
                    }
                    else
                    {
                        DateTime fecha = DateTime.Parse(param.value.ToString(), new System.Globalization.CultureInfo("en-US", false));
                        cts_param.InnerText = fecha.ToString("MM-dd-yyyy") + " " + fecha.ToLongTimeString();
                    }

                }
                else
                {
                    if ((param.value == DBNull.Value) || (param.value == null))
                    {
                        if (param.io == "1")//parametros de salida
                        {
                            switch (param.type)
                            {
                                case "44": //TinyInt
                                case "48": //Int16
                                case "52": //Int32
                                case "56": //Int64
                                case "60": //Single
                                case "62": //Double y Float
                                    param.value = "NULL";
                                    //continue;
                                    break;
                                default:
                                    cts_param.InnerText = "NULL";
                                    break;
                            }

                        }
                        else
                        {
                            cts_param.InnerText = "NULL";
                        }
                    }
                    else
                    {
                        cts_param.InnerText = param.value.ToString();
                    }

                }

                //Atributo name
                XmlAttribute attr_name = xmlDoc.CreateAttribute("name");
                attr_name.InnerText = param.name;
                cts_param.Attributes.Append(attr_name);

                //Atributo type
                XmlAttribute attr_type = xmlDoc.CreateAttribute("type");
                attr_type.InnerText = param.type;
                cts_param.Attributes.Append(attr_type);

                //Atributo io
                XmlAttribute attr_io = xmlDoc.CreateAttribute("io");
                attr_io.InnerText = param.io;
                cts_param.Attributes.Append(attr_io);

                //Atributo len
                XmlAttribute attr_len = xmlDoc.CreateAttribute("len");
                attr_len.InnerText = param.len;
                cts_param.Attributes.Append(attr_len);

                cts_procedure_request.AppendChild(cts_param);

            }

            cts_message.AppendChild(cts_data);

            xmlDoc.AppendChild(xmlDeclaration);
            xmlDoc.AppendChild(cts_message);

            var xml = xmlDoc.OuterXml;

            return xml;
        }

        public CTSMessage FromXML(String xml)
        {
            CTSMessage cts_message = new CTSMessage();

            XmlDocument doc = new XmlDocument();
            // Se preservan los espacios vacios. (compatibilidad WOCI)
            doc.PreserveWhitespace = true;

            try
            {
                doc.LoadXml(xml);
            }
            catch (XmlException ex)
            {
                //Valores unicode no imprimibles
                xml = Regex.Replace(xml, @"[^\u0000-\u007F]+", string.Empty);

                //Valores Hex
                string re = @"[^\x09\x0A\x0D\x20-\xD7FF\xE000-\xFFFD\x10000-x10FFFF]";
                xml = Regex.Replace(xml, re, "");


                try
                {
                    doc.LoadXml(xml);

                }
                catch (Exception ex2)
                {
                    throw ex;
                }

            }


            //El nodo CTSMessage
            XmlElement root = doc.DocumentElement;

            //El Nodo Header
            XmlNode cts_header = root.FirstChild;

            foreach (XmlElement elemento in cts_header.ChildNodes)
            {
                CTSHeaderField field = new CTSHeaderField();

                field.name = elemento.Attributes["name"].InnerText;
                field.type = elemento.Attributes["type"].InnerText;
                field.value = elemento.InnerText;

                cts_message.CTSHeader.Add(field);
            }

            //El Nodo ProcedureResponse dentro del Nodo DATA
            XmlNode cts_procedure_response = cts_header.NextSibling.FirstChild;

            CTSProcedureResponse respuesta_sp = new CTSProcedureResponse();

            XmlNodeList listado_mensajes_mq = cts_procedure_response.SelectNodes("Message");
            foreach (XmlElement elemento in listado_mensajes_mq)
            {
                MensajeMQ msg_mq = new MensajeMQ();
                msg_mq.msgNo = elemento.Attributes["msgNo"].InnerText;
                msg_mq.type = elemento.Attributes["type"].InnerText;
                msg_mq.value = elemento.InnerText;

                respuesta_sp.MensajesMQ.Add(msg_mq);
            }

            respuesta_sp.Return = cts_procedure_response.SelectSingleNode("return").InnerText;

            //El resultset
            foreach (XmlElement element_rs in cts_procedure_response.SelectNodes("ResultSet"))
            {
                CTSResultSet cts_resultset = new CTSResultSet();
                foreach (XmlElement elemento in element_rs.SelectNodes("Header/col"))
                {
                    CTSColumn col = new CTSColumn();
                    col.name = elemento.Attributes["name"].InnerText;
                    col.type = elemento.Attributes["type"].InnerText;
                    col.len = elemento.Attributes["len"].InnerText;

                    cts_resultset.Header.Add(col);
                }


                foreach (XmlElement elemento in element_rs.SelectNodes("rw"))
                {
                    Object[] fila = new string[cts_resultset.Header.Count];

                    XmlNodeList datos = elemento.SelectNodes("cd");
                    for (int i = 0; i <= cts_resultset.Header.Count - 1; i++)
                    {

                        if (datos[i].InnerText != "null")
                        {
                            if (cts_resultset.Header[i].type == "60") //(Money, Single) => 60 
                            {
                                fila[i] = Double.Parse(datos[i].InnerText).ToString();
                            }
                            else if (cts_resultset.Header[i].type == "62") // (Double, Float) => 62
                            {
                                fila[i] = Double.Parse(datos[i].InnerText).ToString();
                            }
                            else
                            {
                                fila[i] = datos[i].InnerText;
                            }


                        }
                        else
                        {
                            if (cts_resultset.Header[i].type == "39") //(Varchar) => 39 
                            {
                                fila[i] = "";
                            }
                            else if (cts_resultset.Header[i].type == "47") //(Char) => 47 
                            {
                                fila[i] = "";
                            }
                            else
                            {
                                fila[i] = null;
                            }
                            
                        }

                    }

                    cts_resultset.ItemArray.Add(fila);
                }


                respuesta_sp.ResultSet.Add(cts_resultset);
            }


            //El ProcedureResponse
            cts_message.Data = respuesta_sp;


            //El OutputParams
            XmlNodeList listado_output_params = cts_procedure_response.SelectNodes("OutputParams/param");
            foreach (XmlElement elemento in listado_output_params)
            {
                CTSParameter param = new CTSParameter();
                param.name = elemento.Attributes["name"].InnerText;
                param.type = elemento.Attributes["type"].InnerText;
                param.len = elemento.Attributes["len"].InnerText;
                param.value = elemento.InnerText;


                respuesta_sp.OutputParams.Add(param);
            }

            return cts_message;
        }
    }
}
