using System.Windows;
using System.Windows.Controls;

namespace LyricUser
{
    /// <summary>
    /// Interaction logic for AutoScalingTextBox.xaml
    /// </summary>
    public partial class AutoScalingTextBox : UserControl
    {
        public AutoScalingTextBox()
        {
            InitializeComponent();
        }

        public string Text
        {
            get
            {
                return (string)GetValue(InfoTextProperty);
            }
            set
            {
                SetValue(InfoTextProperty, value);
            }
        }

        public event TextChangedEventHandler TextChanged
        {
            add
            {
                this.lyricsTextBlock.TextChanged += value;
            }
            remove
            {
                this.lyricsTextBlock.TextChanged -= value;
            }
        }

        public static readonly DependencyProperty InfoTextProperty =
           DependencyProperty.Register(
              "InfoText",
              typeof(string),
              typeof(AutoScalingTextBox),
              new FrameworkPropertyMetadata(
                 new PropertyChangedCallback(ChangeText)));

        private static void ChangeText(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            (source as AutoScalingTextBox).UpdateText(e.NewValue.ToString());
        }

        private void UpdateText(string newText)
        {
            this.lyricsTextBlock.Text = newText;
        }
    }
}
