using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LyricUser
{
    /// <summary>
    /// Interaction logic for BrowseView.xaml
    /// </summary>
    public partial class BrowseView : Window
    {
        private class LyricsTreeViewItem : TreeViewItem
        {
            private readonly string nodePath;

            public LyricsTreeViewItem(string nodePath)
            {
                this.nodePath = nodePath;
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

        void folderTreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;

            if (item.Items.Count == 1 && string.IsNullOrEmpty(((TreeViewItem)item.Items[0]).Tag as string))
            {
                // only one child with no text - this is the first expansion
                item.Items.Clear();

                foreach (string s in Directory.EnumerateFileSystemEntries(item.Tag.ToString()))
                {
                    AddTreeItem(item, CreateItem(s));
                }
            }
        }

        private bool GetLyricsIsFavourite(string lyricsFilePath)
        {
            if (".xml" != Path.GetExtension(lyricsFilePath))
            {
                return false;
            }
            else
            {
                XmlLyricsFileParsingStrategy parser = new XmlLyricsFileParsingStrategy(lyricsFilePath);

                bool result;
                if (parser.TryReadValue<bool>("favourite", out result))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private LyricsTreeViewItem CreateItem(string itemPath)
        {
            LyricsTreeViewItem subitem = new LyricsTreeViewItem(itemPath);
            subitem.Header = itemPath.Substring(itemPath.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            subitem.Tag = itemPath;
            try
            {
                if (Directory.Exists(itemPath))
                {
                    // add a dummy sub-item so it can be expanded
                    subitem.Items.Add(new LyricsTreeViewItem(itemPath));
                    subitem.Expanded += new RoutedEventHandler(folderTreeViewItem_Expanded);

                    // Ensure font weight is set so that it is not inherited
                    subitem.FontWeight = FontWeights.Normal;
                }
                else if (File.Exists(itemPath))
                {
                    subitem.MouseDoubleClick += new MouseButtonEventHandler(fileTreeViewItem_MouseDoubleClick);
                    if (GetLyricsIsFavourite(itemPath))
                    {
                        subitem.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        subitem.FontWeight = FontWeights.Normal;
                    }
                }
                else
                {
                    throw new ApplicationException(
                        "itemPath does not exist - " + itemPath + ". CD = " + Directory.GetCurrentDirectory());
                }
            }
            catch (System.Xml.XmlException xmlException)
            {
                System.Windows.Forms.MessageBox.Show("Bad XML in " + itemPath + ":\n\n" + xmlException);
                subitem.Foreground = System.Windows.Media.Brushes.Red;
            }
            return subitem;
        }

        void fileTreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Set handled to avoid re-selecting old view
            e.Handled = true;

            // This code looks wrong refactoring required..
            App thisApp = Application.Current as App;

            TreeViewItem senderItem = sender as TreeViewItem;
            thisApp.LyricsUrl = senderItem.Tag as string;

            PerformanceView newView = new PerformanceView();
            newView.Show();

            this.Dispatcher.BeginInvoke((Action)(() => { newView.Activate(); }));
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
                AddTreeItem(tree, CreateItem(rootPath));
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
