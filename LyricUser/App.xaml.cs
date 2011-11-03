using System.Windows;

namespace LyricUser
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private XmlLyricsFileParser xmlLyricsFileParser;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (e.Args.Length < 1)
            {
                System.Diagnostics.Debug.WriteLine("No arguments available.");
            }
            else
            {
                xmlLyricsFileParser = new XmlLyricsFileParser(e.Args[0]);
            }
        }
    }
}
