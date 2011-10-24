using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
    }
}
