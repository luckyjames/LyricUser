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

    class LyricsTreeViewItem : TreeViewItem
    {
        static private bool GetLyricsIsFavourite(string lyricsFilePath)
        {
            if (".xml" != Path.GetExtension(lyricsFilePath))
            {
                return false;
            }
            else
            {
                XmlLyricsFileParsingStrategy parser = new XmlLyricsFileParsingStrategy(lyricsFilePath);
                
                const bool defaultIsFavourite = false;

                return parser.ReadToFirstValue(Schema.FavouriteElementName, defaultIsFavourite );
            }
        }

        private readonly LyricsTreeNodePresenter nodePresenter;
        public LyricsTreeNodePresenter NodePresenter
        {
            get
            {
                return this.nodePresenter;
            }
        }

        /// <summary>
        /// Indicates that a file is a favourite, or an artist has favourite files
        /// </summary>
        private Nullable<bool> isFavourite;

        private bool IsInvisible
        {
            get
            {
                return (System.Windows.Visibility.Visible != this.Visibility);
            }
            set
            {
                if (value != this.IsInvisible)
                {
                    if (value)
                    {
                        this.Visibility = System.Windows.Visibility.Collapsed;
                    }
                    else
                    {
                        this.Visibility = System.Windows.Visibility.Visible;
                    }
                }
            }
        }

        private void UpdateIsFavourite()
        {
            try
            {
                if (nodePresenter.IsFolder)
                {
                    Nullable<bool> descendantsAreFavourites = HasDescendentsThatAreFavourites();
                    this.isFavourite = (descendantsAreFavourites.HasValue && descendantsAreFavourites.Value);
                }
                else
                {
                    this.isFavourite = GetLyricsIsFavourite(nodePresenter.nodePath);
                }
            }
            catch (System.Exception exception)
            {
                this.Foreground = System.Windows.Media.Brushes.Red;

                XmlLyricsFileParsingStrategy.RecoverBadXml(nodePresenter.nodePath, exception);
            }
        }

        private void LazyUpdateIsFavourite()
        {
            if (!this.isFavourite.HasValue)
            {
                UpdateIsFavourite();
            }
        }

        private void UpdateAppearance()
        {
            this.Header = nodePresenter.nodeName;
            this.Tag = nodePresenter.nodePath;

            LazyUpdateIsFavourite();

            if (isFavourite.HasValue && isFavourite.Value)
            {
                this.FontWeight = FontWeights.Bold;
            }
            else
            {
                this.FontWeight = FontWeights.Normal;
            }
        }

        public LyricsTreeViewItem(string nodePath)
        {
            this.nodePresenter = new LyricsTreeNodePresenter(nodePath);

            if (nodePresenter.IsFolder)
            {
                // add a dummy sub-item so it can be expanded
                this.Expanded += new RoutedEventHandler(folderTreeViewItem_Expanded);
            }
            else if (nodePresenter.IsFile)
            {
                this.MouseDoubleClick += new MouseButtonEventHandler(fileTreeViewItem_MouseDoubleClick);
            }
            else
            {
                throw new ApplicationException(
                    "itemPath does not exist - " + nodePresenter.nodePath + ". CD = " + Directory.GetCurrentDirectory());
            }

            RepopulateFolderNode();

            UpdateAppearance();
        }

        private static void AddTreeItem(ItemsControl itemsControl, TreeViewItem newItem)
        {
            itemsControl.Items.Add(newItem);
            if (FontWeights.Normal != newItem.FontWeight)
            {
                // ensure all parent directories indicate favourites
                TreeViewItem pointer = newItem.Parent as TreeViewItem;
                while (null != pointer)
                {
                    pointer.FontWeight = newItem.FontWeight;
                    pointer = pointer.Parent as TreeViewItem;
                }
            }
        }

        public Nullable<bool> HasDescendentsThatAreFavourites()
        {
            if (this.isFavourite.HasValue)
            {
                // If current node has determined value, stop recursion
                return this.isFavourite;
            }
            else
            {
                // must be a folder, and not populated yet, look in all descendents
                if (1 > this.Items.Count)
                {
                    // not populated or empty
                    return null;
                }
                else
                {
                    foreach (TreeViewItem child in this.Items)
                    {
                        LyricsTreeViewItem childLyricsTreeViewItem = child as LyricsTreeViewItem;
                        if (object.ReferenceEquals(null, childLyricsTreeViewItem))
                        {
                            // child is not a lyrics tree item, ssume it is a dummy..
                            return null;
                        }
                        else
                        {
                            Nullable<bool> childResult = childLyricsTreeViewItem.HasDescendentsThatAreFavourites();
                            if (!childResult.HasValue)
                            {
                                // we don't know about something
                                return null;
                            }
                            else
                            {
                                // the current child knows..
                                if (childResult.Value)
                                    return true;
                            }
                        }
                    }

                    // all children know whether they are favourites, and none were..
                    return false;
                }
            }
        }

        public void RepopulateFolderNode()
        {
            // only one child with no text - this is the first expansion
            this.Items.Clear();

            if (nodePresenter.IsFolder)
            {
                foreach (string s in Directory.EnumerateFileSystemEntries(this.nodePresenter.nodePath))
                {
                    AddTreeItem(this, new LyricsTreeViewItem(s));
                }
            }
        }

        public void LazyPopulateFolderNode()
        {
            Nullable<bool> descendantsAreFavourites = HasDescendentsThatAreFavourites();
            this.isFavourite = (descendantsAreFavourites.HasValue && descendantsAreFavourites.Value);

            UpdateAppearance();
        }

        public LyricsTreeViewItem GetArtistNode()
        {
            switch (NodePresenter.type)
            {
                case NodeType.Song:
                    return this.Parent as LyricsTreeViewItem;
                case NodeType.Artist:
                    return this;
                case NodeType.Folder:
                    return null;
                default:
                    throw new Exception("Invalid value for NodeType");
            }
        }

        private void folderTreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;

            LazyPopulateFolderNode();
        }

        private void fileTreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Set handled to avoid re-selecting old view
            e.Handled = true;

            TreeViewItem senderItem = sender as TreeViewItem;
            string filePathSelected = senderItem.Tag as string;
            if (string.IsNullOrEmpty(filePathSelected))
            {
                throw new ApplicationException("BrowseView: no file selected");
            }
            else
            {
                PerformanceView newView = new PerformanceView(
                    new LyricsPresenter(new XmlLyricsFileParsingStrategy(filePathSelected)));
                newView.Show();

                this.Dispatcher.BeginInvoke((Action)(() => { newView.Activate(); }));
            }
        }
    }
}
