using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CycloneStudio
{
    /// <summary>
    /// Interakční logika pro Loading.xaml
    /// </summary>
    public partial class LoadingWindow : Window
    {
        public LoadingWindow(string textTop, string textBottom)
        {
            InitializeComponent();
            labelTop.Content = textTop;
            if (textBottom != "")
            {
                labelBottom.Content = textBottom;
            }
            Uri uri = new Uri("..\\..\\graphics\\bb.gif", UriKind.RelativeOrAbsolute);
            media.Source = uri;
            media.Play();
        }

        private void Ended(object sender, RoutedEventArgs e)
        {
            media.Position = new TimeSpan(0, 0, 1);
            media.Play();
        }


    }
}
