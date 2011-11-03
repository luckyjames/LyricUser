using System.Collections.Generic;
using System.Xml;

namespace LyricUser
{
    /// <summary>
    /// Parses some lyrics and associated data from a file
    /// </summary>
    /// <remarks>
    /// This is a simple first iteration of an XML parser that supports my format
    /// It simply looks for key-value pairs (elements with content)
    /// This is quite quick as it avoids looking for values by name
    /// This is also extensible
    /// </remarks>
    public class XmlLyricsFileParser
    {
        private readonly IDictionary<string, string> dataPairs;

        /// <summary>
        /// Vulnerability here: IDictionary is not immutable - consider storing a ReadOnlyCollection of pairs
        /// </summary>
        public IDictionary<string, string> DataPairs
        {
            get
            {
                return dataPairs;
            }
        }

        /// <summary>
        /// Keeps reading looking for an element or EOF
        /// </summary>
        /// <param name="xmlTextReader"></param>
        /// <returns>boolean; true if current node is an element, false if EOF</returns>
        private static bool ReadToNextElement(XmlTextReader xmlTextReader)
        {
            do
            {
                xmlTextReader.Read();
            }
            // Keep reading if not end of file or not on an element
            while (!xmlTextReader.EOF && XmlNodeType.Element != xmlTextReader.NodeType);

            return (XmlNodeType.Element == xmlTextReader.NodeType);
        }

        /// <summary>
        /// Parse data on construction
        /// </summary>
        /// <param name="xmlFileUrl"></param>
        public XmlLyricsFileParser(string xmlFileUrl)
        {
            dataPairs = new Dictionary<string, string>(10);

            using (XmlTextReader xmlTextReader = new XmlTextReader(xmlFileUrl))
            {
                // Read to the document element start node
                xmlTextReader.ReadToFollowing("document");

                // Look for elements that represent key-value pair data:
                //  element name is data name, content is value
                while (ReadToNextElement(xmlTextReader))
                {
                    dataPairs[xmlTextReader.Name] = xmlTextReader.ReadElementContentAsString();
                }
            }
        }
   }
}
