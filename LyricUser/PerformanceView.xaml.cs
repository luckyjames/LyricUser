using System;
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

            Load(lyricsPresenter);
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

        private void AddAttributeEditor(string elementName, string currentValue)
        {
            TextBlock newKeyTextBlock = new TextBlock();
            // add some padding above each metadata item
            newKeyTextBlock.Padding = new Thickness(0, 10, 0, 0);
            newKeyTextBlock.Text = elementName;
            newKeyTextBlock.FontSize = 14;
            newKeyTextBlock.Foreground = System.Windows.Media.Brushes.Black;
            this.metadataStackPanel.Children.Add(newKeyTextBlock);

            TextBox newValueTextBlock = new TextBox();
            newValueTextBlock.Tag = elementName;
            newValueTextBlock.Text = currentValue;
            newValueTextBlock.FontSize = 14;
            newValueTextBlock.Background = System.Windows.Media.Brushes.White;
            newValueTextBlock.Foreground = System.Windows.Media.Brushes.Black;
            newValueTextBlock.TextChanged += new TextChangedEventHandler(newValueTextBlock_TextChanged);
            this.metadataStackPanel.Children.Add(newValueTextBlock);
        }

        private void RepopulateWindow()
        {
            this.lyricsBox.Text = LyricsPresenter.Lyrics;
            this.lyricsBox.TextChanged += new TextChangedEventHandler(lyricsBox_TextChanged);

            // Clear the existing children so we can save reload as many times as required
            this.metadataStackPanel.Children.Clear();

            // Now populate with automatically generated user interface
            Button saveButton = new Button();
            saveButton.Content = "Save";
            saveButton.Click += new RoutedEventHandler(saveButton_Click);
            saveButton.Padding = new Thickness(0, 10, 0, 0);
            this.metadataStackPanel.Children.Add(saveButton);

            // Fix functionality is currently disabled as there is nothing to do.
            Button fixButton = new Button();
            fixButton.Content = "Fix";
            fixButton.Click += new RoutedEventHandler(fixButton_Click);
            fixButton.IsEnabled = false;
            fixButton.Padding = new Thickness(0, 10, 0, 0);
            this.metadataStackPanel.Children.Add(fixButton);

            SortedSet<string> attributesAdded = new SortedSet<string>();

            // For each additional piece of data, push a new TextBlock into the StackPanel
            foreach (KeyValuePair<string, string> entry in LyricsPresenter.Metadata)
            {
                attributesAdded.Add(entry.Key);
                AddAttributeEditor(entry.Key, entry.Value);
            }

            // Ensure all possible elements are edited
            foreach (string elementName in Schema.MakeContainerElementList())
            {
                if (!attributesAdded.Contains(elementName) && elementName != Schema.LyricsElementName)
                {
                    AddAttributeEditor(elementName, "");
                }
            }
        }

        private void fixButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("No fix policy; not doing anything");
        }

        /// <summary>
        /// Simply save the current data file to its current location
        /// Doesn't reload so this can be called when saving
        /// </summary>
        private void Save()
        {
            XmlLyricsFileWritingStrategy.WriteToFile(LyricsPresenter.FileName, LyricsPresenter.AllData);
        }

        /// <summary>
        /// Loads a given lyrics presnter into the view
        /// </summary>
        /// <param name="lyricsPresenter"></param>
        private void Load(IPerformableLyrics lyricsPresenter)
        {
            if (null == lyricsPresenter)
            {
                throw new ApplicationException("Performance view requires lyrics!");
            }
            else
            {
                this.DataContext = lyricsPresenter;
                UpdateTitle();
                RepopulateWindow();
            }
        }

        /// <summary>
        /// Saves the current data, then reloads the file to continue in a consistent manner
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            Save();

            // Reload everything from disc
            Load(new LyricsPresenter(new XmlLyricsFileParsingStrategy(LyricsPresenter.FileName)));
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
                Save();
            }
            base.OnClosing(e);
        }
    }
}
