
using System;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;

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
        public string XmlFileUrl
        {
            get
            {
                return this.xmlFileUrl;
            }
        }

        private IDictionary<string, string> allDataPairs;

        /// <summary>
        /// Finds the contents of an element (assuming there are no duplicates)
        /// </summary>
        /// <returns></returns>
        private static string FindElementContents(string input, string elementName)
        {
            string startTag = string.Concat("<", elementName, ">");
            string endTag = string.Concat("</", elementName, ">");

            int startTagIndex = input.IndexOf(startTag);

            if (-1 == startTagIndex)
            {
                // value not set
                return null;
            }
            else
            {
                int contentsStart = startTagIndex + startTag.Length;
                if (-1 != input.IndexOf(startTag, contentsStart))
                {
                    throw new ApplicationException("Duplicate tag - " + startTag);
                }
                else
                {
                    int firstIndexAfterContents = input.IndexOf(endTag, contentsStart);
                    if (-1 == firstIndexAfterContents)
                    {
                        throw new ApplicationException("No end tag - " + endTag);
                    }
                    else
                    {
                        int contentsLength = firstIndexAfterContents - contentsStart;

                        return input.Substring(contentsStart, contentsLength);
                    }
                }
            }
        }

        public static IDictionary<string, string> BruteForceFromString(string fileContents)
        {
            IDictionary<string, string> dataPairs = new Dictionary<string, string>(10);
            foreach (String elementName in Schema.MakeContainerElementList())
            {
                string elementContents = FindElementContents(fileContents, elementName);
                if (!object.ReferenceEquals(null, elementContents))
                {
                    dataPairs[elementName] = elementContents;
                }
            }

            return dataPairs;
        }

        public static Encoding DetectEncoding(String fileName)
        {
            // open the file with the stream-reader:
            using (StreamReader reader = new StreamReader(fileName, true))
            {
                // read the contents of the file into a string
                reader.ReadToEnd();

                // return the encoding.
                return reader.CurrentEncoding;
            }
        }

        private static string ReadWithEncodingDetection(string xmlFileUrl)
        {
            // My bet is the file is encoded in Windows-1252. This is almost the same as ISO 8859-1. 
            // The difference is Windows-1252 uses "displayable characters rather than control characters
            // in the 0x80 to 0x9F range". (Which is where the slanted apostrophe is located. i.e. 0x92)

            // ReadAllText: "This method attempts to automatically detect the encoding of a file based on the presence
            //  of byte order marks. Encoding formats UTF-8 and UTF-32 (both big-endian and little-endian)
            //  can be detected."

            // Use set to avoid duplicate encodings
            ISet<Encoding> encodingsToTry = new HashSet<Encoding>();
            encodingsToTry.Add(Encoding.GetEncoding(1252)); // Try Windows-1252

            foreach (Encoding encoding in encodingsToTry)
            {
                string incoming = System.IO.File.ReadAllText(xmlFileUrl, encoding);
                // seachr fo 65533 of 0xFFFD question mark character for bad encoding..
                if (-1 != incoming.LastIndexOf('\uFFFD'))
                {
                    // Found bad character, try next encoding
                }
                else
                {
                    return incoming;
                }
            }

            throw new ApplicationException("None of the encodings worked");
        }

        public static IDictionary<string, string> BruteForce(string xmlFileUrl)
        {
            return BruteForceFromString(ReadWithEncodingDetection(xmlFileUrl));
        }

        public static IDictionary<string, string> RecoverBadXml(string xmlFileUrl, Exception exception)
        {
            System.Xml.XmlException xmlException = exception as System.Xml.XmlException;
            if (object.ReferenceEquals(null, xmlException))
            {
                System.Diagnostics.Debug.WriteLine("UNRECOVERABLE Non-XML Exception: " + exception);
                // Unexpected exception; re-throw
                throw new ApplicationException(string.Format("Failed to parse {0}", xmlFileUrl), exception);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("\nBad XML in " + xmlFileUrl + ":\n" + exception);
                if (xmlException.Message.Contains("Invalid character in the given encoding."))
                {
                    // Read file trying to auto-detect encoding
                    // Then change file to utf-16 encoding (unicode) and try again
                    const bool detectEncodingFromByteOrderMarks = true;
                    using (var reader = new System.IO.StreamReader(xmlFileUrl, detectEncodingFromByteOrderMarks))
                    {
                        try
                        {
                            string xml = reader.ReadToEnd();

                            System.Diagnostics.Debug.WriteLine("XML:\n" + xml);
                            return BruteForceFromString(xml);
                        }
                        catch (System.Exception unrecoverableException)
                        {
                            System.Diagnostics.Debug.WriteLine("UNRECOVERABLE because" + unrecoverableException);
                            throw;
                        }
                    }
                }
                else if (xmlException.Message.Contains("An error occurred while parsing EntityName."))
                {
                    // This can be cause by ampersands in unescaped text
                    return BruteForce(xmlFileUrl);
                }
                else if (xmlException.Message.Contains("Reference to undeclared entity"))
                {
                    // This can be cause by html literals e.g. &egarve;
                    return BruteForce(xmlFileUrl);
                }
                else if (xmlException.Message.Contains("is an invalid character"))
                {
                    // This can be cause by html literals e.g. &egarve;
                    return BruteForce(xmlFileUrl);
                }
                else if (xmlException.Message.Contains("Invalid syntax for a decimal numeric entity reference"))
                {
                    return BruteForce(xmlFileUrl);
                }
                else
                {
                    // Unknown issue, rethrow..
                    throw exception;
                }
            }
        }

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
                    xmlTextReader.ReadToFollowing(Schema.DocumentElementName);

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
        public ValueType ReadToFirstValue<ValueType>(string key)
        {
            ValueType result;
            if (!TryReadToFirstValue<ValueType>(key, out result))
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
        public ValueType ReadToFirstValue<ValueType>(string key, ValueType defaultValue)
        {
            ValueType result;
            if (!TryReadToFirstValue<ValueType>(key, out result))
            {
                return defaultValue;
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
        public bool TryReadToFirstValue<ValueType>(string key, out ValueType value)
        {
            if (null == key)
            {
                throw new ArgumentNullException("key");
            }
            else
            {
                string stringValueFound;
                if (object.ReferenceEquals(null, allDataPairs) || !allDataPairs.TryGetValue(key, out stringValueFound))
                {
                    // Passing key to ReadAll will halt when the desired key is found
                    allDataPairs = Implementation.ReadAll(xmlFileUrl, key);

                    if (!allDataPairs.TryGetValue(key, out stringValueFound))
                    {
                        value = default(ValueType);

                        return false;
                    }
                }

                try
                {
                    value = XmlValueConverter.ConvertStringToValue<ValueType>(stringValueFound);

                    return true;
                }
                catch (System.FormatException formatException)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "Bad value '" + stringValueFound + "' for " + key + " in " + xmlFileUrl + ":\n\n" + formatException);

                    throw formatException;
                }
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
