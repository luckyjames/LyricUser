
using System.Collections.Generic;

namespace LyricUser
{
    /// <summary>
    /// Provide access to lyrics and associated data relevant to a performance
    /// </summary>
    internal interface IPerformableLyrics
    {
        bool IsModified { get; }

        string FileName { get; }

        string Lyrics { get; set; }

        ICollection<KeyValuePair<string, string>> Metadata { get; }

        void SetMetadata(string metadataName, string newValue);
    }
}
