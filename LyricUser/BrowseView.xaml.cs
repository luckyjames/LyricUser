using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;

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
                throw new ApplicationException("itemPath does not exist");
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
            RootPath = @"C:\Documents and Settings\XPMUser\Desktop\GIT\LyricUser\LyricUser.Test\TestData";
        }
    }
}
