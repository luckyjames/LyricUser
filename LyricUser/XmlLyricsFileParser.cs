using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace LyricUser
{
    /// <summary>
    /// Parses some lyrics from a file
    /// </summary>
    public class XmlLyricsFileParser
    {
        private readonly string lyrics;
        public string Lyrics
        {
            get
            {
                return lyrics;
            }
        }

        public XmlLyricsFileParser(string xmlFileUrl)
        {
            const string lyricsElementName = "lyrics";
            using (XmlTextReader xmlTextReader = new XmlTextReader(xmlFileUrl))
            {
                if (!xmlTextReader.ReadToDescendant(lyricsElementName))
                {
                    throw new ApplicationException("couldn't find element - " + lyricsElementName);
                }
                else
                {
                    this.lyrics = xmlTextReader.ReadElementContentAsString();
                }
            }
        }
    }
}
