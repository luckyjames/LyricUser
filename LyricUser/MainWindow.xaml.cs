using System;
using System.Windows;
using System.Windows.Input;

namespace LyricUser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // This code looks wrong refactoring required..
            App thisApp = Application.Current as App;

            if (null == thisApp)
            {
                throw new ApplicationException("MainWinow property has incorrect type!");
            }
            else
            {
                this.LyricsPresenter = new LyricsPresenter(new XmlLyricsFileParser(thisApp.LyricsUrl));
            }
        }

        private void ToggleMaxmised()
        {
            switch (this.WindowState)
            {
                case WindowState.Maximized:
                    this.WindowState = System.Windows.WindowState.Normal;
                    break;
                case WindowState.Minimized:
                    System.Diagnostics.Debug.Print("Window is minimised, leave state unchanged.");
                    break;
                case WindowState.Normal:
                    this.WindowState = System.Windows.WindowState.Maximized;
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
            this.lyricsBox.Text = lyricsPresenter.Lyrics;

            System.Diagnostics.Debug.WriteLine("POpulating form with metadata not implemented yet.");
        }

        private IPerformableLyrics lyricsPresenter;
        internal IPerformableLyrics LyricsPresenter
        {
            get
            {
                return lyricsPresenter;
            }
            set
            {
                lyricsPresenter = value;

                PopulateWindow();
            }
        }
    }
}
