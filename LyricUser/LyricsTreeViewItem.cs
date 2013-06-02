using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;

namespace LyricUser
{
    struct LyricsTreeNodePresenter
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

                return parser.GetLyricsIsFavourite();
            }
        }

        public string nodePath;
        public string nodeName;
        public string artistFolderPath;
        public bool isFile;
        public bool isFolder;
        public bool isFavourite;

        public LyricsTreeNodePresenter(string path)
        {
            this.nodePath = path;
            this.nodeName = path.Substring(path.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            this.isFile = File.Exists(nodePath);
            this.isFolder = Directory.Exists(nodePath);
            this.isFavourite = GetLyricsIsFavourite(nodePath);
            this.artistFolderPath = this.isFolder ? nodePath : Path.GetDirectoryName(nodePath);
        }
    }

    class LyricsTreeViewItem : TreeViewItem
    {
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

        public LyricsTreeViewItem(string nodePath)
        {
            this.nodePresenter = new LyricsTreeNodePresenter(nodePath);

            this.Header = nodePresenter.nodeName;
            this.Tag = nodePresenter.nodePath;
            try
            {
                if (nodePresenter.isFolder)
                {
                    // add a dummy sub-item so it can be expanded
                    this.Items.Add(new TreeViewItem());
                    this.Expanded += new RoutedEventHandler(folderTreeViewItem_Expanded);

                    // Ensure font weight is set so that it is not inherited
                    this.FontWeight = FontWeights.Normal;
                }
                else if (nodePresenter.isFile)
                {
                    this.isFavourite = nodePresenter.isFavourite;

                    this.MouseDoubleClick += new MouseButtonEventHandler(fileTreeViewItem_MouseDoubleClick);

                    if (isFavourite.Value)
                    {
                        this.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        this.FontWeight = FontWeights.Normal;
                    }
                }
                else
                {
                    throw new ApplicationException(
                        "itemPath does not exist - " + nodePath + ". CD = " + Directory.GetCurrentDirectory());
                }
            }
            catch (System.Exception exception)
            {
                this.Foreground = System.Windows.Media.Brushes.Red;

                XmlLyricsFileParsingStrategy.RecoverBadXml(nodePath, exception);
            }
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

        public void PopulateFolderNode()
        {
            if (this.Items.Count == 1 && string.IsNullOrEmpty(((TreeViewItem)this.Items[0]).Tag as string))
            {
                // only one child with no text - this is the first expansion
                this.Items.Clear();

                foreach (string s in Directory.EnumerateFileSystemEntries(this.Tag.ToString()))
                {
                    AddTreeItem(this, new LyricsTreeViewItem(s));
                }

                Nullable<bool> descendantsAreFavourites = HasDescendentsThatAreFavourites();
                this.isFavourite = (descendantsAreFavourites.HasValue && !descendantsAreFavourites.Value);
            }
        }

        private void folderTreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;

            PopulateFolderNode();
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
