using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;

namespace LyricUser
{
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
        private Nullable<bool> isHighlighted;

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

        private LyricsTreeNodePresenter.Filter highlightedFilter;
        private LyricsTreeNodePresenter.Filter HighlightedFilter
        {
            get
            {
                return highlightedFilter;
            }
        }
        private void UpdateWhetherHighlighted(LyricsTreeNodePresenter.Filter filter)
        {
            try
            {
                if (nodePresenter.IsFolder)
                {
                    Nullable<bool> descendentsAreHighlighted = HasDescendentsThatAreHighlighted(filter);
                    this.isHighlighted = (descendentsAreHighlighted.HasValue && descendentsAreHighlighted.Value);
                }
                else
                {
                    this.isHighlighted = filter(nodePresenter);
                }
            }
            catch (System.Exception exception)
            {
                this.Foreground = System.Windows.Media.Brushes.Red;

                XmlLyricsFileParsingStrategy.RecoverBadXml(nodePresenter.nodePath, exception);
            }
        }

        private void LazyUpdateWhetherHighlighted(LyricsTreeNodePresenter.Filter filter)
        {
            if (!this.isHighlighted.HasValue)
            {
                UpdateWhetherHighlighted(filter);
            }
        }

        private void UpdateAppearance(LyricsTreeNodePresenter.Filter highlightedFilter)
        {
            this.Header = nodePresenter.nodeName;
            this.Tag = nodePresenter.nodePath;

            LazyUpdateWhetherHighlighted(highlightedFilter);

            if (isHighlighted.HasValue && isHighlighted.Value)
            {
                this.FontWeight = FontWeights.Bold;
            }
            else
            {
                this.FontWeight = FontWeights.Normal;
            }
        }

        public LyricsTreeViewItem(string nodePath, LyricsTreeNodePresenter.Filter highlightedFilter)
        {
            this.nodePresenter = new LyricsTreeNodePresenter(nodePath);
            this.highlightedFilter = highlightedFilter;

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

            UpdateAppearance(highlightedFilter);
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

        private Nullable<bool> HasDescendentsThatAreHighlighted(LyricsTreeNodePresenter.Filter filter)
        {
            if (this.isHighlighted.HasValue)
            {
                // If current node has determined value, stop recursion
                return this.isHighlighted;
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
                            Nullable<bool> childResult =
                                childLyricsTreeViewItem.HasDescendentsThatAreHighlighted(filter);
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
                    AddTreeItem(this, new LyricsTreeViewItem(s, this.HighlightedFilter));
                }
            }
        }

        public void LazyPopulateFolderNode()
        {
            this.isHighlighted = HasDescendentsThatAreHighlighted(HighlightedFilter);

            UpdateAppearance(HighlightedFilter);
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
