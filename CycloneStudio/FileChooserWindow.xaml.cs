using Microsoft.Win32;
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
    /// Interakční logika pro FileChooserWindow.xaml
    /// </summary>
    public partial class FileChooserWindow : Window
    {
        public string ResponseText
        {
            get { return pathBox.Text; }
            set { pathBox.Text = value; }
        }

        public FileChooserWindow()
        {
            InitializeComponent();
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            
           OpenFileDialog openFileDlg = new OpenFileDialog();
            openFileDlg.Filter = "EXE (.exe)|*.exe";

            Nullable<bool> result = openFileDlg.ShowDialog();           
            if (result == true)
            {
                ResponseText = openFileDlg.FileName;                
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void TextChangedEventHandler(object sender, TextChangedEventArgs e)
        {
            if (pathBox.Text.Length < 5)
            {
                okBtn.IsEnabled = false;
            }
            else
            {
                okBtn.IsEnabled = true;
            }
        }
    }
}
