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
    /// Interakční logika pro EntryWindow.xaml
    /// </summary>
    public partial class EntryWindow : Window
    {       
        public bool Confirm { get; set; }
        public string Path { get; set; }
        public string ProjName { get; set; }
        public bool isProject { get; set; }
        public bool isBlock { get; set; }
        private FileControler fileControler;

        public EntryWindow()
        {
            InitializeComponent();
            Confirm = false;
            isProject = true;
            isBlock = false;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            fileControler = new FileControler();                       

            
            radioProject.IsChecked = true;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            if (null != Owner)
            {
                Owner.Activate();
            }
        }

        private void Event_Close(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Event_OpenProject(object sender, RoutedEventArgs e)
        {
            if (listViewProjects.SelectedItem != null)
            {
                LoadWindowProjects selectedItem = listViewProjects.SelectedItem as LoadWindowProjects;
                Path = selectedItem.Path;
                ProjName = selectedItem.Name;
                DialogResult = true;
            }

            //this.Close();
        }

        private void Event_NewProject(object sender, RoutedEventArgs e)
        {
            Confirm = true;
            isBlock = false;
            isProject = true;
            DialogResult = true;
        }

        private void Event_ProjChecked(object sender, RoutedEventArgs e)
        {
            listViewProjects.Items.Clear();
            fileControler.GenerateProjectsList(listViewProjects.Items, true);
            HeaderLabel.Content = "Projects";
            OpenBtn.Content = "Open project";
            isBlock = false;
            isProject = true;
            if (listViewProjects.Items.Count == 0)
            {
                OpenBtn.IsEnabled = false;
            }
        }

        private void Event_BlockChecked(object sender, RoutedEventArgs e)
        {
            listViewProjects.Items.Clear();
            fileControler.GenerateProjectsList(listViewProjects.Items, false);
            HeaderLabel.Content = "Blocks";
            OpenBtn.Content = "Open block";
            isBlock = true;
            isProject = false;
            if (listViewProjects.Items.Count == 0)
            {
                OpenBtn.IsEnabled = false;
            }
        }

        private void Event_NewBlock(object sender, RoutedEventArgs e)
        {
            Confirm = true;
            isBlock = true;
            isProject = false;
            DialogResult = true;
        }
    }
}
