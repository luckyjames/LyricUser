﻿using System;
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
        private static void FindAllFavourites(Object stateInfo)
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

        private void BeginFindAllFavourites()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(FindAllFavourites), this);
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
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(ass.Location);
                return fileVersionInfo.ProductName;
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
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(ass.Location);
                return String.Concat(fileVersionInfo.ProductName, " ", fileVersionInfo.FileVersion);
            }
        }

        private static string GetApplicationVersionString()
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                return String.Concat(GetProductName(), " ", ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString());
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

            this.Title = GetApplicationVersionString();
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

        private static void TidyFile(string filePath)
        {
            XmlLyricsFileParsingStrategy reader = new XmlLyricsFileParsingStrategy(filePath);
            try
            {
                reader.ReadAll();
            }
            catch
            {
                // Something went wrong; only then do I try and correct the file
                System.Diagnostics.Debug.Print("Correcting {0}..", filePath);

                // Read using brute force
                IDictionary<string, string> data = XmlLyricsFileParsingStrategy.BruteForce(filePath);

                // Write again using normal XML writing to create file
                string newPath = filePath + ".correct.xml";
                XmlLyricsFileWritingStrategy.WriteToFile(newPath, data);

                // Check subsequent read now works
                XmlLyricsFileParsingStrategy resultantReader = new XmlLyricsFileParsingStrategy(newPath);
                try
                {
                    resultantReader.ReadAll();
                }
                catch
                {
                    // Delete the new file and bomb out
                    File.Delete(newPath);

                    throw;
                }

                // ReadAll will throw if the problem is still not fixed
                // so now we know it's fixed, swap the old file with the new one..
                File.Delete(filePath);
                File.Move(newPath, filePath);
            }
        }
        
        private static void TidyLyricsInTree(string rootFolderPath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(rootFolderPath);
            FileInfo[] allFiles = dirInfo.GetFiles("*.xml", SearchOption.AllDirectories);
            foreach (FileInfo fileInfo in allFiles)
            {
                TidyFile(fileInfo.FullName);
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
                MessageBoxResult answer = MessageBox.Show("Tidy XML?", "Caption", MessageBoxButton.YesNo);
                if (MessageBoxResult.Yes == answer)
                {
                    TidyLyricsInTree(rootPath);
                }

                tree.Items.Add(new LyricsTreeViewItem(rootPath));

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
