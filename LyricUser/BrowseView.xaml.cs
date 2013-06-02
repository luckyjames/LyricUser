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
                (Action)(() => { rootItem.PopulateFolderNode(); }));
            token.Wait();

            foreach (TreeViewItem descendent in rootItem.Items)
            {
                LyricsTreeViewItem artistItem = descendent as LyricsTreeViewItem;
                if (!object.ReferenceEquals(null, artistItem))
                {
                    // Use background priority to try and give priority to user interactions
                    System.Windows.Threading.DispatcherOperation artistNodeToken = view.Dispatcher.BeginInvoke(
                        (Action)(() => { artistItem.PopulateFolderNode(); }),
                        System.Windows.Threading.DispatcherPriority.Background);
                    // Wait for the artist node population to finish before queueing the next one, otherwise
                    //  the GUI becomes very unresponsive
                    artistNodeToken.Wait();
                }
            }
        }

        private void StartUpdatingVisibleFilter()
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

                    StartUpdatingVisibleFilter();
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
                    StartUpdatingVisibleFilter();
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

        private void newButton_Click(object sender, RoutedEventArgs e)
        {
            LyricsTreeViewItem currentItem = this.fileTree.SelectedItem as LyricsTreeViewItem;
            if (null == currentItem)
            {
                throw new ApplicationException("No node selected to insert new lyrics!");
            }
            else
            {
                bool isKnownArtist = currentItem.NodePresenter.isFolder && null != currentItem.Parent;
                string artistName = isKnownArtist ? currentItem.NodePresenter.nodeName : "_NEWARTIST_";
                string songName = "_NEWSONG_";

                IDictionary<string, string> values = new Dictionary<string, string>();
                values.Add(Schema.ArtistElementName, artistName);
                values.Add(Schema.TitleElementName, songName);
                values.Add(Schema.CapoElementName, "");
                values.Add(Schema.KeyElementName, "");
                values.Add(Schema.FavouriteElementName, "true");
                values.Add(Schema.SingableElementName, "true");
                values.Add(Schema.LyricsElementName, "");

                string newFilePath = Path.Combine(this.RootPath, artistName, songName + ".xml");
                XmlLyricsFileWritingStrategy.WriteToFile(newFilePath, values);
                
                if (currentItem.NodePresenter.isFolder)
                {
                    currentItem.PopulateFolderNode();
                }
            }
        }

        private void renameButton_Click(object sender, RoutedEventArgs e)
        {
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
