using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
using Path = System.IO.Path;

namespace CycloneStudio
{
    /// <summary>
    /// Interakční logika pro PreviewWindow.xaml
    /// </summary>
    public partial class PreviewWindow : Window
    {
        public PreviewWindow(int marginLeft, int marginTop, string name)
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            try
            {
                var lastFolder = Path.GetDirectoryName(Environment.CurrentDirectory);
                var pathWithoutLastFolder = Path.GetDirectoryName(lastFolder);                
                string ImagesDirectory = Path.Combine(pathWithoutLastFolder, "graphics"); 
                Uri uri = new Uri(Path.Combine(ImagesDirectory, name + ".jpg"));
                imageView.Source = new BitmapImage(uri);
                           
                someGrid.Height = imageView.Source.Height;
                someGrid.Width = imageView.Source.Width;
                arrowPointer.Margin = new Thickness(marginLeft - 21, marginTop - 25, 0, 0);
            }
            catch (IOException)
            {
                Title = "BAD IMAGE";
            }
            

            
            
                    

        }
    }
}
