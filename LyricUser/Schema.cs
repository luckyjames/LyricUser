
using System.Collections.Generic;

namespace LyricUser
{
    internal class Schema
    {
        public const string DocumentElementName = "document";

        public const string ArtistElementName = "artist";
        public const string TitleElementName = "title";
        public const string CapoElementName = "capo";
        public const string KeyElementName = "key";
        public const string FavouriteElementName = "favourite";
        public const string SingableElementName = "singable";
        public const string TagsElementName = "tags";
        public const string LyricsElementName = "lyrics";

        public static IList<string> MakeContainerElementList()
        {
            IList<string> elements = new List<string>();
            elements.Add(ArtistElementName);
            elements.Add(TitleElementName);
            elements.Add(CapoElementName);
            elements.Add(KeyElementName);
            elements.Add(FavouriteElementName);
            elements.Add(SingableElementName);
            elements.Add(LyricsElementName);
            elements.Add(TagsElementName);
            return elements;
        }
    }
}
