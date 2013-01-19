
using System;
using System.Collections.Generic;
using System.Xml;
using System.Text;

namespace LyricUser
{
    /// <summary>
    /// Stipulates how an XML lyrics file is saved
    /// </summary>

    public class XmlLyricsFileWritingStrategy
    {
        private static XmlWriterSettings MakeSettings()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.CheckCharacters = true;
            // use little endian unicode to support useful characters
            settings.Encoding = System.Text.Encoding.Unicode;
            return settings;
        }

        public static string WriteToString(IDictionary<string, string> data)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            using (var writer = XmlWriter.Create(builder, MakeSettings()))
            {
                Write(writer, data);
            }

            return builder.ToString();
        }

        public static void WriteToFile(string outputFileUrl, IDictionary<string, string> data)
        {
            using (var stream = new System.IO.FileStream(outputFileUrl, System.IO.FileMode.OpenOrCreate))
            using (var writer = XmlWriter.Create(stream, MakeSettings()))
            {
                Write(writer, data);
            }
        }

        public static void Write(XmlWriter writer, IDictionary<string, string> data)
        {
            writer.WriteStartElement("document");
            foreach (string elementName in data.Keys)
            {
                writer.WriteElementString(elementName, data[elementName]);
            }
            writer.WriteEndElement();
        }
    }
}
