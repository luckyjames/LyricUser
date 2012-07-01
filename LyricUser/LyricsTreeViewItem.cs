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
        private readonly string nodePath;

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

        public LyricsTreeViewItem(string nodePath)
        {
            this.nodePath = nodePath;

            this.Header = nodePath.Substring(nodePath.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            this.Tag = nodePath;
            try
            {
                if (Directory.Exists(nodePath))
                {
                    // add a dummy sub-item so it can be expanded
                    this.Items.Add(new TreeViewItem());
                    this.Expanded += new RoutedEventHandler(folderTreeViewItem_Expanded);

                    // Ensure font weight is set so that it is not inherited
                    this.FontWeight = FontWeights.Normal;
                }
                else if (File.Exists(nodePath))
                {
                    this.isFavourite = GetLyricsIsFavourite(nodePath);

                    this.MouseDoubleClick += new MouseButtonEventHandler(fileTreeViewItem_MouseDoubleClick);

                    this.IsInvisible = !isFavourite.Value;
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

                System.Xml.XmlException xmlException = exception as System.Xml.XmlException;
                if (object.ReferenceEquals(null, xmlException))
                {
                    System.Diagnostics.Debug.WriteLine("UNRECOVERABLE Non-XML Exception: " + exception);
                    // Unexpected exception; re-throw
                    //throw;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("\nBad XML in " + nodePath + ":\n" + exception);
                    if (xmlException.Message.Contains("Invalid character in the given encoding."))
                    {
                        // Read file trying to auto-detect encoding
                        // Then change file to utf-16 encoding (unicode) and try again
                        const bool detectEncodingFromByteOrderMarks = true;
                        using (var reader = new StreamReader(nodePath, detectEncodingFromByteOrderMarks))
                        {
                            try
                            {
                                string xml = reader.ReadToEnd();
                            }
                            catch (System.Exception unrecoverableException)
                            {
                                System.Diagnostics.Debug.WriteLine("UNRECOVERABLE because" + unrecoverableException);
                                throw;
                            }
                        }
                    }
                    else if (xmlException.Message.Contains("An error occurred while parsing EntityName."))
                    {
                        // This can be cause by ampersands in unescaped text
                        System.Diagnostics.Debug.WriteLine("Unimplemented XML format fix");
                    }
                    else if (xmlException.Message.Contains("Reference to undeclared entity"))
                    {
                        // This can be cause by html literals e.g. &egarve;
                        System.Diagnostics.Debug.WriteLine("Unimplemented XML format fix");
                    }
                    else if (xmlException.Message.Contains("is an invalid character"))
                    {
                        // This can be cause by html literals e.g. &egarve;
                        System.Diagnostics.Debug.WriteLine("Unimplemented XML format fix");
                    }
                    else
                    {
                        // Can't recover from this, rethrow..
                        throw;
                    }
                }
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
                this.IsInvisible = (descendantsAreFavourites.HasValue && !descendantsAreFavourites.Value);
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
