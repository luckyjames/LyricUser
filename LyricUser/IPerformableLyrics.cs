using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LyricUser
{
    /// <summary>
    /// Provide access to lyrics and associated data relevant to a performance
    /// </summary>
    internal interface IPerformableLyrics
    {
        string Lyrics { get; }

        ICollection<KeyValuePair<string, string>> Metadata { get; }
    }
}
