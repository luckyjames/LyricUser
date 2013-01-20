using System.Windows;
using System;

namespace LyricUser
{
    /// <summary>
    /// Interaction logic for LyricUserApplication.xaml
    /// </summary>
    public partial class LyricUserApplication : Application
    {
        private StartupEventArgs startupArguments;

        /// <summary>
        /// The arguments used to start this application
        /// </summary>
        public StartupEventArgs StartupArguments
        {
            get
            {
                return startupArguments;
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            startupArguments = e;
        }

        private void LyricsViewerDispatcherUnhandledException(
            object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {

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
                    Exception innerExceptionReference = e.Exception;
                    while (innerExceptionReference != null)
                    {
                        theTextWriter.WriteLine("Exception: " + e.Exception.ToString());
                        innerExceptionReference = e.Exception.InnerException;
                    }
                }
                MessageBox.Show(string.Format("The program crashed. A stack trace can be found at:\n{0}\n\nException:\n{1}\n\nClosing the application..", theErrorPath, e.Exception.ToString()));
            }
            catch (Exception exception)
            {
                MessageBox.Show(string.Format("The program crashed. A stack trace couldn't be written because of an exception in LyricsViewerDispatcherUnhandledException :\n{0}\n\nClosing the application..", exception.ToString()));
            }

            Application.Current.Shutdown();
        }
    }
}
