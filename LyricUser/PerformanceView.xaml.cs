﻿using System;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Documents;

namespace LyricUser
{
    /// <summary>
    /// Interaction logic for PerformanceView.xaml
    /// </summary>
    public partial class PerformanceView : Window
    {
        internal PerformanceView(IPerformableLyrics lyricsPresenter)
        {
            InitializeComponent();

            if (null == lyricsPresenter)
            {
                throw new ApplicationException("Performance view requires lyrics!");
            }
            else
            {
                this.DataContext = lyricsPresenter;
                UpdateTitle();
                PopulateWindow();
            }
        }
        
        private void UpdateTitle()
        {
            IPerformableLyrics lyricsPresenter = LyricsPresenter;
            
            string modifiedIndicator = (lyricsPresenter.IsModified)? "* " : "";

            this.Title = modifiedIndicator + lyricsPresenter.FileName;
        }

        private void ToggleMaxmised()
        {
            switch (this.WindowState)
            {
                case WindowState.Maximized:
                    this.WindowState = System.Windows.WindowState.Normal;
                    this.WindowStyle = System.Windows.WindowStyle.ThreeDBorderWindow;
                    break;
                case WindowState.Minimized:
                    System.Diagnostics.Debug.Print("Window is minimised, leave state unchanged.");
                    break;
                case WindowState.Normal:
                    this.WindowState = System.Windows.WindowState.Maximized;
                    this.WindowStyle = System.Windows.WindowStyle.None;
                    break;
                default:
                    throw new ApplicationException("Unrecognised window state: " + this.WindowState);
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (Key.F11 == e.Key)
            {
                ToggleMaxmised();
            }
        }

        private void PopulateWindow()
        {
            this.lyricsBox.Text = LyricsPresenter.Lyrics;
            this.lyricsBox.TextChanged += new TextChangedEventHandler(lyricsBox_TextChanged);

            Button saveButton = new Button();
            saveButton.Content = "Save";
            saveButton.Click += new RoutedEventHandler(saveButton_Click);
            this.metadataStackPanel.Children.Add(saveButton);

            Button fixButton = new Button();
            fixButton.Content = "Fix";
            fixButton.Click += new RoutedEventHandler(fixButton_Click);
            this.metadataStackPanel.Children.Add(fixButton);

            // For each additional piece of data, push a new TextBlock into the StackPanel
            foreach (KeyValuePair<string, string> entry in LyricsPresenter.Metadata)
            {
                TextBlock newKeyTextBlock = new TextBlock();
                // add some padding above each metadata item
                newKeyTextBlock.Padding = new Thickness(0, 10, 0, 0);
                newKeyTextBlock.Text = entry.Key;
                newKeyTextBlock.FontSize = 14;
                newKeyTextBlock.Foreground = System.Windows.Media.Brushes.Black;
                this.metadataStackPanel.Children.Add(newKeyTextBlock);

                TextBox newValueTextBlock = new TextBox();
                newValueTextBlock.Tag = entry.Key;
                newValueTextBlock.Text = entry.Value;
                newValueTextBlock.FontSize = 14;
                newValueTextBlock.Background = System.Windows.Media.Brushes.White;
                newValueTextBlock.Foreground = System.Windows.Media.Brushes.Black;
                newValueTextBlock.TextChanged += new TextChangedEventHandler(newValueTextBlock_TextChanged);
                this.metadataStackPanel.Children.Add(newValueTextBlock);
            }
        }

        private void fixButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            XmlLyricsFileWritingStrategy.WriteToFile(LyricsPresenter.FileName, LyricsPresenter.AllData);
        }

        private void lyricsBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox changedTextBox = sender as TextBox;
            if (object.ReferenceEquals(null, changedTextBox))
            {
                throw new ApplicationException("Sender was not a textbox");
            }
            else
            {
                LyricsPresenter.Lyrics = changedTextBox.Text;

                UpdateTitle();
            }
        }

        private void newValueTextBlock_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox changedTextBox = sender as TextBox;
            if (object.ReferenceEquals(null, changedTextBox))
            {
                throw new ApplicationException("Sender was not a textbox");
            }
            else
            {
                string attributeName = changedTextBox.Tag as string;
                if (object.ReferenceEquals(null, attributeName))
                {
                    throw new ApplicationException("Sender Tag was not a string");
                }
                else
                {
                    LyricsPresenter.SetMetadata(attributeName, changedTextBox.Text);

                    UpdateTitle();
                }
            }
        }

        private IPerformableLyrics LyricsPresenter
        {
            get
            {
                IPerformableLyrics result = this.DataContext as IPerformableLyrics;
                if (null == result)
                {
                    throw new ApplicationException("PerformanceView: No lyrics to show");
                }
                else
                {
                    return result;
                }
            }
        }

        static bool PromptToSave(string fileName)
        {
            System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show(
                "Save?", fileName, System.Windows.Forms.MessageBoxButtons.YesNo);
            switch (result)
            {
                case System.Windows.Forms.DialogResult.Yes:
                    return true;
                case System.Windows.Forms.DialogResult.Cancel:
                case System.Windows.Forms.DialogResult.No:
                    // Expected navigation cancel
                    return false;
                case System.Windows.Forms.DialogResult.OK:
                case System.Windows.Forms.DialogResult.Abort:
                case System.Windows.Forms.DialogResult.Ignore:
                case System.Windows.Forms.DialogResult.None:
                case System.Windows.Forms.DialogResult.Retry:
                default:
                    throw new NotImplementedException(result.ToString());
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (LyricsPresenter.IsModified && PromptToSave(LyricsPresenter.FileName))
            {
                throw new NotImplementedException("SAVING NOT IMPLEMENTED");
            }
            base.OnClosing(e);
        }
    }
}
