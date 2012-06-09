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

        private void LyricsViewerDispatcherUnhandledException(
            object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Exception theException = e.Exception;
            string traceFileName = "GeneratorTestbedError.txt";
            string allUsersAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            if (string.IsNullOrEmpty(allUsersAppDataFolder))
            {
                allUsersAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            string theErrorPath = System.IO.Path.Combine(allUsersAppDataFolder, traceFileName);
            using (System.IO.TextWriter theTextWriter = new System.IO.StreamWriter(theErrorPath, true))
            {
                DateTime theNow = DateTime.Now;
                theTextWriter.WriteLine("The error time: " + theNow.ToShortDateString() + " " + theNow.ToShortTimeString());
                while (theException != null)
                {
                    theTextWriter.WriteLine("Exception: " + theException.ToString());
                    theException = theException.InnerException;
                }
            }
            MessageBox.Show("The program crashed. A stack trace can be found at:\n" + theErrorPath + "\n\nClosing the application..");
            e.Handled = true;
            Application.Current.Shutdown();
        }
    }
}
