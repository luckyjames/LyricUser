
using System;
using System.Collections.Generic;
using System.Xml;

namespace LyricUser
{
    /// <summary>
    /// Stipulates how an XML lyrics file is parsed
    /// </summary>
    /// <remarks>
    /// This is a simple first iteration of an XML parser that supports my format
    /// Does not perform a full parse on initialisation, but if the requested vaue is not
    ///     found must re-parse the whole XML (avoids keeping file handle/cursor)
    /// It simply looks for key-value pairs (elements with content) for speed, simplicity and extensibility
    /// </remarks>
    public class XmlLyricsFileParsingStrategy
    {
        private readonly string xmlFileUrl;

        private IDictionary<string, string> allDataPairs;

        private class Implementation
        {
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

            public static ValueType ConvertStringToValue<ValueType>(string stringValue)
            {
                return (ValueType)Convert.ChangeType(stringValue, typeof(ValueType));
            }

            /// <summary>
            /// Parse data on construction
            /// </summary>
            /// <param name="xmlFileUrl">The URL of the file to parse</param>
            /// <param name="stopOnKey">The name of a key of interest; once found parsing will stop</param>
            public static IDictionary<string, string> ReadAll(string xmlFileUrl, string stopOnKey)
            {
                IDictionary<string, string> dataPairs = new Dictionary<string, string>(10);

                using (XmlTextReader xmlTextReader = new XmlTextReader(xmlFileUrl))
                {
                    // Read to the document element start node
                    xmlTextReader.ReadToFollowing("document");

                    // Look for elements that represent key-value pair data:
                    //  element name is data name, content is value
                    while (ReadToNextElement(xmlTextReader))
                    {
                        dataPairs[xmlTextReader.Name] = xmlTextReader.ReadElementContentAsString();
                        if (string.Equals(xmlTextReader.Name, stopOnKey))
                        {
                            break;
                        }
                    }
                }

                return dataPairs;
            }
        }

        /// <summary>
        /// Does nothing on construction, ready for subsequent reads
        /// </summary>
        public XmlLyricsFileParsingStrategy(string xmlFileUrl)
        {
            if (null == xmlFileUrl)
            {
                throw new ArgumentNullException("xmlFileUrl");
            }
            else
            {
                this.xmlFileUrl = xmlFileUrl;
            }
        }

        /// <summary>
        /// Finds the value corresponding to the specified key and converts it to the supplied generic type
        /// </summary>
        /// <typeparam name="ValueType"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public ValueType ReadValue<ValueType>(string key)
        {
            ValueType result;
            if (!TryReadValue<ValueType>(key, out result))
            {
                throw new ApplicationException("key " + key + " could not be found");
            }
            else
            {
                return result;
            }
        }

        /// <summary>
        /// Finds the value corresponding to the specified key and converts it to the supplied generic type
        /// </summary>
        /// <typeparam name="ValueType"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool TryReadValue<ValueType>(string key, out ValueType value)
        {
            if (null == key)
            {
                throw new ArgumentNullException("key");
            }
            else
            {
                string result;
                if (object.ReferenceEquals(null, allDataPairs) || !allDataPairs.TryGetValue(key, out result))
                {
                    // Read only until the desired key is found
                    allDataPairs = Implementation.ReadAll(xmlFileUrl, key);
                    if (!allDataPairs.TryGetValue(key, out result))
                    {
                        value = default(ValueType);

                        return false;
                    }
                }

                value = Implementation.ConvertStringToValue<ValueType>(result);

                return true;
            }
        }

        /// <summary>
        /// Parses the whole file, caching all the stored key-value pairs
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, string> ReadAll()
        {
            this.allDataPairs = Implementation.ReadAll(xmlFileUrl, null);

            return this.allDataPairs;
        }
    }
}
