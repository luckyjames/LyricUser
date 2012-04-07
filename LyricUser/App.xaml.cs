using System.Windows;
using System;

namespace LyricUser
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private string lyricsUrl;
        public string LyricsUrl
        {
            get
            {
                return lyricsUrl;
            }
            set
            {
                lyricsUrl = value;
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (e.Args.Length < 1)
            {
                System.Diagnostics.Debug.WriteLine("No arguments available.");
            }
            else
            {
                lyricsUrl = e.Args[0];
            }
        }
    }
}
