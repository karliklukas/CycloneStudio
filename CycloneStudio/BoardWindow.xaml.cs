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
        public string ChoosenBoardName
        {
            get { return choosenBoardName; }
            set { choosenBoardName = value; }
        }

        public BoardWindow()
        {
            InitializeComponent();
            choosenBoardName = "DE0-Nano";
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }
            if (boardSelect.SelectedItem != null)
            {
                ComboBoxItem typeItem = (ComboBoxItem)boardSelect.SelectedItem;
                choosenBoardName = typeItem.Content.ToString();
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

    }
}
