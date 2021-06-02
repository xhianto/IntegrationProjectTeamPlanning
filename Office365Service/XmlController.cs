using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

namespace Office365Service
{
    public class XmlController
    {
        /// <summary>
        /// Method for converting any type of object to an xml string
        /// </summary>
        /// <typeparam name="T">Generic type</typeparam>
        /// <param name="obj">Generic object</param>
        /// <returns></returns>
        public string ConvertObjectToXML<T>(T obj)
        {
            string xml = "";
            try
            {
                MemoryStream mStream = new MemoryStream();
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                XmlTextWriter writer = new XmlTextWriter(mStream, null);
                writer.Formatting = System.Xml.Formatting.Indented;
                serializer.Serialize(writer, obj);
                xml = Encoding.UTF8.GetString(mStream.ToArray());

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return xml;
        }

        public T ConvertXMLtoObject<T>(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            StringReader reader = new StringReader(xml);
            return (T)serializer.Deserialize(reader);
        }

        public bool XSDValidatie(string xml, string xsd) //geef xml string en welk xsd bestand je wilt gebruiken bvb "event.xsd"
        {
            XmlSchemaSet xmlSchema = new XmlSchemaSet();
            xmlSchema.Add("", Environment.CurrentDirectory + "XMLvalidation/" + xsd + ".xsd");
            bool xmlValidation = true;

            XDocument doc = XDocument.Parse(xml);

            doc.Validate(xmlSchema, (sender, args) =>
            {
                Console.WriteLine("Error Message: " + args.Message);
                xmlValidation = false;
            });
            Console.WriteLine("XmlValidatie voor " + xsd + ": " + xmlValidation);
            return xmlValidation;
        }
    }
}
