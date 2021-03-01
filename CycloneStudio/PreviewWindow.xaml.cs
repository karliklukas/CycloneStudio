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
    /// Interakční logika pro PreviewWindow.xaml
    /// </summary>
    public partial class PreviewWindow : Window
    {
        public PreviewWindow(int marginLeft, int marginTop, string name)
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
                       
            imageView.Source = new BitmapImage(new Uri(@"graphics/"+name+".jpg", UriKind.RelativeOrAbsolute));
            //Console.WriteLine(imageView.Source.Width+" "+ imageView.Source.Height);            
            someGrid.Height = imageView.Source.Height;
            someGrid.Width = imageView.Source.Width;

            
            arrowPointer.Margin = new Thickness(marginLeft - 21, marginTop - 25, 0, 0);
                    

        }
    }
}