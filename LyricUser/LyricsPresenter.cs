using System.Collections.Generic;
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
                this.fileName = xmlLyricsFileParser.XmlFileUrl;

                IDictionary<string, string> dataPairs = xmlLyricsFileParser.ReadAll();

                // Find lyrics
                lyrics = dataPairs["lyrics"];

                metadata = new Collection<KeyValuePair<string, string>>();

                // Now find everything but the lyrics
                foreach (KeyValuePair<string, string> value in dataPairs)
                {
                    if ("lyrics" != value.Key)
                    {
                        metadata.Add(value);
                    }
                }
            }
        }

        private string lyrics;
        public string Lyrics
        {
            get { return lyrics; }
            set
            {
                if (value != lyrics)
                {
                    lyrics = value;

                    isModified = true;
                }
            }
        }

        private bool isModified;
        public bool IsModified
        {
            get { return isModified; }
        }

        private readonly string fileName;
        public string FileName
        {
            get { return fileName; }
        }

        private readonly Collection<KeyValuePair<string, string>> metadata;
        public ICollection<KeyValuePair<string, string>> Metadata
        {
            get { return new ReadOnlyCollection<KeyValuePair<string, string>>(metadata); }
        }

        private Nullable<KeyValuePair<string, string>> RemoveValue(string key)
        {
            // Now find everything but the lyrics
            foreach (KeyValuePair<string, string> value in metadata)
            {
                if (key == value.Key)
                {
                    metadata.Remove(value);
                    return value;
                }
            }

            return null;
        }

        public void SetMetadata(string metadataName, string newValue)
        {
            Nullable<KeyValuePair<string, string>> removeResult = RemoveValue(metadataName);
            KeyValuePair<string, string> newKeyValuePair = new KeyValuePair<string, string>(metadataName, newValue);
            metadata.Add(newKeyValuePair);
            if (!removeResult.HasValue
                || (removeResult.Value.Key != newKeyValuePair.Key)
                || (removeResult.Value.Value != newKeyValuePair.Value))
            {
                isModified = true;
            }
        }
    }
}
