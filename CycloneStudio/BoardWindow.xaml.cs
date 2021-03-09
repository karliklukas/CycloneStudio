using CycloneStudio.structs;
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
    /// Interakční logika pro BoardWindow.xaml
    /// </summary>
    public partial class BoardWindow : Window
    {
        private string choosenBoardName;
        private FileControler fileControler;
        public string ChoosenBoardName
        {
            get { return choosenBoardName; }
            set { choosenBoardName = value; }
        }

        public BoardWindow()
        {
            InitializeComponent();
            fileControler = new FileControler();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            boardSelect.ItemsSource = fileControler.GetAllBoards();
        }        

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }
            if (boardSelect.SelectedItem != null)
            {               
                choosenBoardName = boardSelect.SelectedItem as string;
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

    }
}
