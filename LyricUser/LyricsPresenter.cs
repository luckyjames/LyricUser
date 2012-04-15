﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;

namespace LyricUser
{
    /// <summary>
    /// A readonly presenter class for performance
    /// </summary>
    public class LyricsPresenter : IPerformableLyrics
    {
        public LyricsPresenter(XmlLyricsFileParsingStrategy xmlLyricsFileParser)
        {
            if (null == xmlLyricsFileParser)
            {
                throw new ArgumentNullException("xmlLyricsFileParser", "Can't present null");
            }
            else
            {
                IDictionary<string, string> dataPairs = xmlLyricsFileParser.ReadAll();

                // Find lyrics
                lyrics = dataPairs["lyrics"];

                // Now find everything but the lyrics
                Collection<KeyValuePair<string, string>> incomingMetadata = new Collection<KeyValuePair<string, string>>();
                foreach (KeyValuePair<string, string> value in dataPairs)
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
