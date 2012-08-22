
using System;
using System.Collections.Generic;
using System.Xml;

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
