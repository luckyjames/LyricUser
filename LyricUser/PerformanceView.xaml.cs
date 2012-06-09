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

            // This code looks wrong refactoring required..
            App thisApp = Application.Current as App;

            if (null == lyricsPresenter)
            {
                throw new ApplicationException("MainWinow property has incorrect type!");
            }
            else
            {
                this.DataContext = lyricsPresenter;

                PopulateWindow();
            }
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

                TextBlock newValueTextBlock = new TextBlock();
                newValueTextBlock.Text = entry.Value;
                newValueTextBlock.FontSize = 14;
                newValueTextBlock.Foreground = System.Windows.Media.Brushes.White;
                this.metadataStackPanel.Children.Add(newValueTextBlock);
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
    }
}
