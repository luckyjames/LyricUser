using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;

namespace LyricUser
{
    public class LyricsPresenter : IPerformableLyrics
    {
        public LyricsPresenter(XmlLyricsFileParser xmlLyricsFileParser)
        {
            if (null == xmlLyricsFileParser)
            {
                throw new ArgumentNullException("xmlLyricsFileParser", "Can't present null");
            }
            else
            {
                // Find lyrics
                lyrics = xmlLyricsFileParser.DataPairs["lyrics"];

                // Now find everything but the lyrics
                Collection<KeyValuePair<string, string>> incomingMetadata = new Collection<KeyValuePair<string, string>>();
                foreach (KeyValuePair<string, string> value in xmlLyricsFileParser.DataPairs)
                {
                    if ("lyrics" != value.Key)
                    {
                        incomingMetadata.Add(value);
                    }
                }

                // USe ReadOnlyCollection to ensure immutability
                metadata = new ReadOnlyCollection<KeyValuePair<string, string>>(incomingMetadata);
            }
        }

        private readonly string lyrics;
        public string Lyrics
        {
            get { return lyrics; }
        }

        private readonly ICollection<KeyValuePair<string, string>> metadata;
        public ICollection<KeyValuePair<string, string>> Metadata
        {
            get { return metadata; }
        }
    }
}
