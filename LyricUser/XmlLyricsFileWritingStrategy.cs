
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
            settings.CheckCharacters = false;
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
            try
            {
                using (var stream = new System.IO.FileStream(outputFileUrl, System.IO.FileMode.OpenOrCreate))
                using (var writer = XmlWriter.Create(stream, MakeSettings()))
                {
                    // As the file was opened with OpenOrCreate, we may have opened an existing file and
                    //  are about to overwrite from the start. This means if the new file is smaller than the
                    //  old, spurious text will remain at the end. Hence set length to zero to clear..
                    stream.SetLength(0);

                    Write(writer, data);
                }
            }
            catch (Exception exception)
            {
                throw new ApplicationException(string.Concat("Couldn't write file ", outputFileUrl), exception);
            }
        }

        public static void Write(XmlWriter writer, IDictionary<string, string> data)
        {
            writer.WriteStartElement("document");
            foreach (string elementName in data.Keys)
            {
                try
                {
                    writer.WriteElementString(elementName, data[elementName]);
                }
                catch (Exception exception)
                {
                    throw new ApplicationException(
                        string.Format("couldn't write {0} - {1}", elementName, data[elementName]),
                        exception);
                }
            }
            writer.WriteEndElement();
        }
    }
}
