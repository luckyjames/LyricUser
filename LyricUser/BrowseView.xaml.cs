using System;
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
        public BrowseView()
        {
            InitializeComponent();
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

        void folderTreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;

            if (item.Items.Count == 1 && string.IsNullOrEmpty(((TreeViewItem)item.Items[0]).Tag as string))
            {
                // only one child with no text - this is the first expansion
                item.Items.Clear();

                foreach (string s in Directory.EnumerateFileSystemEntries(item.Tag.ToString()))
                {
                    item.Items.Add(CreateItem(s));
                }
            }
        }

        private TreeViewItem CreateItem(string itemPath)
        {
            TreeViewItem subitem = new TreeViewItem();
            subitem.Header = itemPath.Substring(itemPath.LastIndexOf("\\") + 1);
            subitem.Tag = itemPath;
            subitem.FontWeight = FontWeights.Normal;
            if (Directory.Exists(itemPath))
            {
                // add a dummy sub-item so it can be expanded
                subitem.Items.Add(new TreeViewItem());
                subitem.Expanded += new RoutedEventHandler(folderTreeViewItem_Expanded);
            }
            else if (File.Exists(itemPath))
            {
                subitem.MouseDoubleClick += new MouseButtonEventHandler(fileTreeViewItem_MouseDoubleClick);
            }
            else
            {
                throw new ApplicationException(
                    "itemPath does not exist - " + itemPath + ". CD = " + Directory.GetCurrentDirectory());
            }
            return subitem;
        }

        void fileTreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // This code looks wrong refactoring required..
            App thisApp = Application.Current as App;

            TreeViewItem senderItem = sender as TreeViewItem;
            thisApp.LyricsUrl = senderItem.Tag as string;

            PerformanceView newView = new PerformanceView();
            newView.Show();
        }

        private void RepopulateTree(TreeView tree, string rootPath)
        {
            tree.Items.Clear();

            tree.Items.Add(CreateItem(rootPath));
        }

        private void fileTree_Initialized(object sender, EventArgs e)
        {
            // Debug run directory
            RootPath = @"..\..\..\LyricUser.Test\TestData";
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
    }
}
