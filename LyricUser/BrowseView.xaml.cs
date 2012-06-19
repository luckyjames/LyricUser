using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;

namespace LyricUser
{
    /// <summary>
    /// Interaction logic for BrowseView.xaml
    /// </summary>
    public partial class BrowseView : Window
    {
        private static void FindAllFavourites(Object stateInfo)
        {
            BrowseView view = ((BrowseView)stateInfo);

            LyricsTreeViewItem rootItem = view.fileTree.Items[0] as LyricsTreeViewItem;

            System.Windows.Threading.DispatcherOperation token
                = view.Dispatcher.BeginInvoke((Action)(() => { rootItem.PopulateArtistTreeNode(); }));
            token.Wait();

            foreach (TreeViewItem artistItem in rootItem.Items)
            {
                if (artistItem is LyricsTreeViewItem)
                {
                    System.Windows.Threading.DispatcherOperation artistNodeToken
                       = view.Dispatcher.BeginInvoke((Action)(() => { ((LyricsTreeViewItem)artistItem).PopulateArtistTreeNode(); }));
                    artistNodeToken.Wait();
                }
            }
        }

        private void BeginFindAllFavourites()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(FindAllFavourites), this);
        }

        private class LyricsTreeViewItem : TreeViewItem
        {
            private readonly string nodePath;

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
                        this.MouseDoubleClick += new MouseButtonEventHandler(fileTreeViewItem_MouseDoubleClick);
                        if (GetLyricsIsFavourite(nodePath))
                        {
                            System.Diagnostics.Debug.WriteLine("Favourite found: " + nodePath);
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
                    System.Diagnostics.Debug.WriteLine("Bad XML in " + nodePath + ":\n\n" + exception);
                    this.Foreground = System.Windows.Media.Brushes.Red;

                    System.Xml.XmlException xmlException = exception as System.Xml.XmlException;
                    if (!object.ReferenceEquals(null, xmlException)
                        && xmlException.Message.Contains("Invalid character in the given encoding."))
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
                                System.Diagnostics.Debug.WriteLine("UNRECOVERABLE XML " + nodePath + ":\n\n" + unrecoverableException);
                            }
                        }
                    }
                }
            }

            public void PopulateArtistTreeNode()
            {
                if (this.Items.Count == 1 && string.IsNullOrEmpty(((TreeViewItem)this.Items[0]).Tag as string))
                {
                    // only one child with no text - this is the first expansion
                    this.Items.Clear();

                    foreach (string s in Directory.EnumerateFileSystemEntries(this.Tag.ToString()))
                    {
                        AddTreeItem(this, new LyricsTreeViewItem(s));
                    }
                }
            }

            private void folderTreeViewItem_Expanded(object sender, RoutedEventArgs e)
            {
                TreeViewItem item = sender as TreeViewItem;

                PopulateArtistTreeNode();
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

        public BrowseView()
        {
            InitializeComponent();

            // Initialise root path with the last user folder after controls are constructed but
            //  before it is set by the application using this form
            RootPath = LyricUser.Properties.Settings.Default.LastOpenedLyricsFolder;
        }

        private string rootPath;
        public string RootPath
        {
            get
            {
                return rootPath;
            }
            set
            {
                rootPath = value;

                RepopulateTree(this.fileTree, this.rootPath);
            }
        }

        private bool favouritesVisible;
        public bool FavouritesVisible
        {
            get
            {
                return favouritesVisible;
            }
            set
            {
                favouritesVisible = value;
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            LyricUser.Properties.Settings.Default.LastOpenedLyricsFolder = RootPath;
            LyricUser.Properties.Settings.Default.Save();
            base.OnClosing(e);
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

        private void RepopulateTree(TreeView tree, string rootPath)
        {
            tree.Items.Clear();
            if (!Directory.Exists(rootPath))
            {
                tree.Items.Add("File not found - " + rootPath);
            }
            else
            {
                AddTreeItem(tree, new LyricsTreeViewItem(rootPath));

                // Once tree populated, start background thread to find favourites
                BeginFindAllFavourites();
            }
        }

        private void browseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    RootPath = dialog.SelectedPath;
                    break;
                case System.Windows.Forms.DialogResult.Cancel:
                    // Expected navigation cancel
                    break;
                case System.Windows.Forms.DialogResult.Yes:
                case System.Windows.Forms.DialogResult.Abort:
                case System.Windows.Forms.DialogResult.Ignore:
                case System.Windows.Forms.DialogResult.No:
                case System.Windows.Forms.DialogResult.None:
                case System.Windows.Forms.DialogResult.Retry:
                default:
                    throw new NotImplementedException(result.ToString());
            }
        }

        private void favouritesCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            FavouritesVisible = true;
        }

        private void favouritesCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            FavouritesVisible = false;
        }
    }
}
