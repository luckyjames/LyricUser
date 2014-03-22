using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Deployment.Application;

namespace LyricUser
{
    /// <summary>
    /// Interaction logic for BrowseView.xaml
    /// </summary>
    public partial class BrowseView : Window
    {
        private static void PopulateTree(Object stateInfo)
        {
            BrowseView view = ((BrowseView)stateInfo);

            LyricsTreeViewItem rootItem = view.fileTree.Items[0] as LyricsTreeViewItem;

            // This code is expected to be called on the threadpool, invoke using the dispatcher and
            //  wait for the result
            System.Windows.Threading.DispatcherOperation token = view.Dispatcher.BeginInvoke(
                (Action)(() => { rootItem.LazyPopulateFolderNode(); }));
            token.Wait();

            foreach (TreeViewItem descendent in rootItem.Items)
            {
                LyricsTreeViewItem artistItem = descendent as LyricsTreeViewItem;
                if (!object.ReferenceEquals(null, artistItem))
                {
                    // Use background priority to try and give priority to user interactions
                    System.Windows.Threading.DispatcherOperation artistNodeToken = view.Dispatcher.BeginInvoke(
                        (Action)(() => { artistItem.LazyPopulateFolderNode(); }),
                        System.Windows.Threading.DispatcherPriority.Background);
                    // Wait for the artist node population to finish before queueing the next one, otherwise
                    //  the GUI becomes very unresponsive
                    artistNodeToken.Wait();
                }
            }
        }

        private void StartUpdatingAppearanceOfFavourites()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(PopulateTree), this);
        }

        private static string GetProductName()
        {
            // Place in local code
            Assembly ass = Assembly.GetExecutingAssembly();

            if (null == ass)
            {
                throw new ApplicationException("Couldn't get executing assembly!");
            }
            else
            {
                return FileVersionInfo.GetVersionInfo(ass.Location).ProductName;
            }
        }

        private static string GetAssemblyVersionString()
        {
            // Place in local code
            Assembly ass = Assembly.GetExecutingAssembly();

            if (null == ass)
            {
                throw new ApplicationException("Couldn't get executing assembly!");
            }
            else
            {
                return FileVersionInfo.GetVersionInfo(ass.Location).FileVersion;
            }
        }

        private static string GetApplicationVersionString()
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                return ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            }
            else
            {
                return "Debug Version";
            }
        }

        private static string DetermineInitialFolder()
        {
            LyricUserApplication lyricsApp = Application.Current as LyricUserApplication;
            if (null == lyricsApp)
            {
                throw new ApplicationException("Bad type of application");
            }
            else
            {
                if (lyricsApp.StartupArguments.Args.Length > 0)
                {
                    return lyricsApp.StartupArguments.Args[0];
                }
                else
                {
                    return LyricUser.Properties.Settings.Default.LastOpenedLyricsFolder;
                }
            }
        }
        public BrowseView()
        {
            InitializeComponent();

            // Initialise root path before it is set by the application using this form
            this.RootPath = DetermineInitialFolder();

            this.Title = String.Concat(GetProductName(), " ", GetApplicationVersionString());
            
            this.KeyDown += new KeyEventHandler(BrowseView_KeyDown);  
            this.TextInput += new TextCompositionEventHandler(BrowseView_TextInput);
        }

        void BrowseView_TextInput(object sender, TextCompositionEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Text))
            {
                // ignore empty input
            }
            else
            {
                SelectNextItem(e.Text);
            }
        }

        LyricsTreeViewItem RootItem
        {
            get
            {
                LyricsTreeViewItem root = this.fileTree.Items[0] as LyricsTreeViewItem;
                if (null == root)
                {
                    throw new ApplicationException("There must be a root item in the tree");
                }
                else
                {
                    return root;
                }
            }
        }
        /// <summary>
        /// We want to find the artist names not lyrics or root
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        private TreeViewItem FindFirstArtistWithPrefix(string prefix)
        {
            foreach (LyricsTreeViewItem lyricsItem in this.RootItem.Items)
            {
                string nodeName = lyricsItem.NodePresenter.artistFolderName;
                if (!string.IsNullOrEmpty(nodeName)
                    && nodeName.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
                {
                    return lyricsItem;
                }
            }
            return null;
        }

        private void SelectNextItem(string prefix)
        {
            TreeViewItem treeViewItem = FindFirstArtistWithPrefix(prefix);
            if (null != treeViewItem)
            {
                treeViewItem.IsSelected = true;
            }
        }

        void BrowseView_KeyDown(object sender, KeyEventArgs e)
        {
            if (Key.A > e.Key || Key.Z < e.Key)
            {
                // Ignored
            }
            else
            {
            }
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

                ClearAndInitialiseTree(this.fileTree, this.rootPath);
            }
        }

        private bool onlyFavouritesVisible = false;
        private bool OnlyFavouritesVisible
        {
            get
            {
                return onlyFavouritesVisible;
            }
            set
            {
                if (onlyFavouritesVisible != value)
                {
                    onlyFavouritesVisible = value;

                    StartUpdatingAppearanceOfFavourites();
                }
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            LyricUser.Properties.Settings.Default.LastOpenedLyricsFolder = RootPath;
            LyricUser.Properties.Settings.Default.Save();
            base.OnClosing(e);
        }

        /// <summary>
        /// Called to reset the tree to its initial state, e.g. when a new root folder is selected
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="rootPath"></param>
        private void ClearAndInitialiseTree(TreeView tree, string rootPath)
        {
            tree.Items.Clear();
            if (!Directory.Exists(rootPath))
            {
                tree.Items.Add("File not found - " + rootPath);
            }
            else
            {
                tree.Items.Add(new LyricsTreeViewItem(rootPath));

                if (this.favouritesCheckBox.IsChecked.HasValue && this.favouritesCheckBox.IsChecked.Value)
                {
                    // Once tree populated, start background thread to find favourites
                    StartUpdatingAppearanceOfFavourites();
                }
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

        private LyricsTreeViewItem RootNode
        {
            get
            {
                LyricsTreeViewItem root = fileTree.Items[0] as LyricsTreeViewItem;
                if (null == root)
                {
                    throw new ApplicationException("Bad root");
                }
                else
                {
                    return root;
                }
            }
        }

        public static string InputBox(string title, string promptText)
        {
            return Microsoft.VisualBasic.Interaction.InputBox(promptText, title, "Default", 0, 0);
        }

        private LyricsTreeViewItem AddArtist(string artistName)
        {
            string artistFolderPath = Path.Combine(this.RootPath, artistName);
            if (Directory.Exists(artistFolderPath))
            {
                throw new ApplicationException("Folder already exists " + artistFolderPath);
            }
            else
            {
                Directory.CreateDirectory(artistFolderPath);
                LyricsTreeViewItem newNode = new LyricsTreeViewItem(artistFolderPath);
                RootNode.Items.Add(newNode);
                return newNode;
            }
        }

        private void newButton_Click(object sender, RoutedEventArgs e)
        {
            LyricsTreeViewItem currentItem = this.fileTree.SelectedItem as LyricsTreeViewItem;
            if (null == currentItem)
            {
                throw new ApplicationException("No node selected to insert new lyrics!");
            }
            else
            {
                string songName = InputBox("New Song..", "Choose new song name..");
                if (string.IsNullOrEmpty(songName))
                {
                    return;
                }
                LyricsTreeViewItem artistNode = currentItem.GetArtistNode();
                if (null == artistNode)
                {
                    string newArtistName = InputBox("New Artist..", "Choose new artist name..");
                    if (string.IsNullOrEmpty(newArtistName))
                    {
                        return;
                    }
                    artistNode = AddArtist(newArtistName);
                }
                string artistName = artistNode.NodePresenter.nodeName;

                IDictionary<string, string> values = new Dictionary<string, string>();
                values.Add(Schema.ArtistElementName, artistName);
                values.Add(Schema.TitleElementName, songName);
                values.Add(Schema.CapoElementName, "");
                values.Add(Schema.KeyElementName, "");
                values.Add(Schema.FavouriteElementName, "true");
                values.Add(Schema.SingableElementName, "true");
                values.Add(Schema.LyricsElementName, "");

                string newFilePath = Path.Combine(artistNode.NodePresenter.nodePath, songName + ".xml");
                XmlLyricsFileWritingStrategy.WriteToFile(newFilePath, values);
                
                switch (currentItem.NodePresenter.type)
                {
                    case NodeType.Song:
                        LyricsTreeViewItem parent = currentItem.Parent as LyricsTreeViewItem;
                        if (null != parent)
                        {
                            parent.RepopulateFolderNode();
                        }
                        break;
                    case NodeType.Artist:
                    case NodeType.Folder:
                        currentItem.RepopulateFolderNode();
                        break;
                    default:
                        throw new Exception("Invalid value for NodeType");
                }
            }
        }

        private void renameButton_Click(object sender, RoutedEventArgs e)
        {
            LyricsTreeViewItem currentItem = this.fileTree.SelectedItem as LyricsTreeViewItem;
            if (null == currentItem)
            {
                throw new ApplicationException("No node selected is not a LyricsTreeViewItem!");
            }
            else
            {
                if (currentItem.NodePresenter.IsFolder)
                {
                    // Remove current folder node
                    // Update file system
                    // Update lyrics file contents
                }
                else
                {
                    // Remove current file node
                    // Update file system
                    // Update lyrics file contents
                }
            }
        }

        private void favouritesCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            OnlyFavouritesVisible = true;
        }

        private void favouritesCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            OnlyFavouritesVisible = false;
        }
    }
}
