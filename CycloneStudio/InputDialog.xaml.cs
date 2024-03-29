﻿using System;
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
    /// Interakční logika pro InputDialog.xaml
    /// </summary>
    public partial class InputDialog : Window
    {
        public string ResponseText
        {
            get { return textBoxName.Text; }
            set { textBoxName.Text = value; }
        }

        public InputDialog()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            textBoxName.Focus();
        }

        public InputDialog(string newTitle,string newText)
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            textBoxName.Focus();
            this.Title = newTitle;
            textLabel.Content = newText;
        }        

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {           
            DialogResult = true;                      
        }        

        private void textChangedEventHandler(object sender, TextChangedEventArgs e)
        {
            if (textBoxName.Text.Length < 4)
            {
                okButton.IsEnabled = false;
            }
            else
            {
                okButton.IsEnabled = true;
            }
        }
    }
}
