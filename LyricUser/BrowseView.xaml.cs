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
        private static void FilterVisibleNodes(Object stateInfo)
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

        private void BeginFilter()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(FilterVisibleNodes), this);
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

        public BrowseView()
        {
            InitializeComponent();

            // Initialise root path with the last user folder after controls are constructed but
            //  before it is set by the application using this form
            RootPath = LyricUser.Properties.Settings.Default.LastOpenedLyricsFolder;

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
                onlyFavouritesVisible = value;
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
	                BeginFilter();
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
