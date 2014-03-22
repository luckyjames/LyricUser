using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;

namespace LyricUser
{
    enum NodeType
    {
        Song,
        Artist,
        Folder
    }

    // This class represents the lyrics file in the tree data structure
    struct LyricsTreeNodePresenter
    {
        public delegate bool Filter(LyricsTreeNodePresenter presenter);

        public static bool NodeIsFavourite(LyricsTreeNodePresenter presenter)
        {
            if (".xml" != Path.GetExtension(presenter.nodePath))
            {
                return false;
            }
            else
            {
                XmlLyricsFileParsingStrategy parser = new XmlLyricsFileParsingStrategy(presenter.nodePath);
                
                const bool defaultIsFavourite = false;

                return parser.ReadToFirstValue(Schema.FavouriteElementName, defaultIsFavourite );
            }
        }

        public static bool NodeIsSingable(LyricsTreeNodePresenter presenter)
        {
            if (".xml" != Path.GetExtension(presenter.nodePath))
            {
                return false;
            }
            else
            {
                XmlLyricsFileParsingStrategy parser = new XmlLyricsFileParsingStrategy(presenter.nodePath);
                
                const bool defaultIsSingable = false;

                return parser.ReadToFirstValue(Schema.SingableElementName, defaultIsSingable);
            }
        }

        static bool FolderContainsLyrics(string folderPath)
        {
            foreach (var file in Directory.EnumerateFiles(folderPath))
            {
                return true;
            }
            return false;
        }

        static NodeType DetermineNodeType(string nodePath)
        {
            if (File.Exists(nodePath))
            {
                return NodeType.Song;
            }
            else if (FolderContainsLyrics(nodePath))
            {
                // there is one or more files in the folder
                return NodeType.Artist;
            }
            else
            {
                return NodeType.Folder;
            }
        }

        public readonly string nodePath;
        public readonly string nodeName;
        public readonly string artistFolderName;
        public readonly string artistFolderPath;
        public readonly NodeType type;

        public bool IsFile
        {
            get
            {
                return type == NodeType.Song;
            }
        }

        public bool IsFolder
        {
            get
            {
                return !IsFile;
            }
        }
        
        public LyricsTreeNodePresenter(string path)
        {
            this.nodePath = path;
            this.nodeName = Path.GetFileName(path);
            this.type = DetermineNodeType(path);
            this.artistFolderPath = (type != NodeType.Song) ? nodePath : Path.GetDirectoryName(nodePath);
            this.artistFolderName = Path.GetFileName(artistFolderPath);
        }
    }
}
