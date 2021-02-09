using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;

namespace AppSpace.structs
{
    class FileControler
    {
        private readonly RoutedEventHandler eventHandler;

        public FileControler(RoutedEventHandler eventHandler) => this.eventHandler = eventHandler;

        public void GenerateMenuItems(Menu menu)
        {
            DirectoryInfo d = new DirectoryInfo(@"../../components");
            DirectoryInfo[] subdir = d.GetDirectories();

            ReadAndGenerateItems(menu, subdir);
        }

        private void ReadAndGenerateItems(Menu menu, DirectoryInfo[] subdir)
        {
            foreach (DirectoryInfo sub in subdir)
            {   
                MenuItem newMenuItem = new MenuItem();
                newMenuItem.Header = sub.Name;
                menu.Items.Add(newMenuItem);

                DirectoryInfo[] innerDir = sub.GetDirectories();
                if (innerDir.Length != 0)
                {
                    foreach (DirectoryInfo dir in innerDir)
                    {                       
                        MenuItem subItem = new MenuItem();
                        subItem.Header = dir.Name;
                        newMenuItem.Items.Add(subItem);                       

                        GenerateItems(dir, subItem);
                    }
                }
                if (sub.Name == "logic")
                {
                    GenerateSubItems(sub, newMenuItem);
                }
                else
                {
                    GenerateItems(sub, newMenuItem);
                }                
            }
        }

        private void GenerateItems(DirectoryInfo sub, MenuItem newMenuItem)
        {
            FileInfo[] Files = sub.GetFiles("*.v");
            foreach (FileInfo file in Files)
            {
                MenuItem newSubMenuItem = new MenuItem();
                newSubMenuItem.Header = TrimModuleName(file);
                newMenuItem.Items.Add(newSubMenuItem);
                newSubMenuItem.Click += eventHandler;

                MenuData data = ReadAndProcessFile(file.FullName);
                newSubMenuItem.Tag = data;
            }
        }

        private MenuData ReadAndProcessFile(string path)
        {
            MenuData data = new MenuData();//
            string text = File.ReadAllText(path);
            string[] textSplited = Regex.Split(text, "module ([\\w\\d]+)\\(\\s*([\\w\\d,_\\n\\s(]+)\\);");
            data.Name = textSplited[1];
            data.FilePath = path;
            

            string[] pins = Regex.Replace(textSplited[2], @"\s+", "").Split(',');
            
            for (int i = 0; i < pins.Length; i++)
            {
                if (pins[i].Contains("outputwire"))
                {
                    data.OutPins.Add(pins[i].Remove(0,10));
                    //Console.WriteLine("out "+pins[i].Remove(0, 10));
                } else if (pins[i].Contains("inputwire"))
                {
                    data.InPins.Add(pins[i].Remove(0, 9));
                    //Console.WriteLine("in " + pins[i].Remove(0, 9));
                }
            }

            string[] textSplitedTwo = Regex.Split(textSplited[3], "\\/\\/hidden:\\s([\\w\\d,\\s]+)assign");
            if (textSplitedTwo.Length >1)
            {
                string[] result = Regex.Replace(textSplitedTwo[1], @"\s+", "").Split(',');
                data.HiddenPins = new List<string>(result);                
            }            
            return data;
        }

        private void GenerateSubItems(DirectoryInfo sub, MenuItem newMenuItem)
        {
            FileInfo[] Files = sub.GetFiles("*.v");
            MenuItem andItems = new MenuItem();
            andItems.Header = "AND";
            newMenuItem.Items.Add(andItems);
            MenuItem nandItems = new MenuItem();
            nandItems.Header = "NAND";
            newMenuItem.Items.Add(nandItems);
            MenuItem norItems = new MenuItem();
            norItems.Header = "NOR";
            newMenuItem.Items.Add(norItems);
            MenuItem orItems = new MenuItem();
            orItems.Header = "OR";
            newMenuItem.Items.Add(orItems);

            foreach (FileInfo file in Files)
            {
                MenuData data = ReadAndProcessFile(file.FullName);
                string name = TrimModuleName(file);
                MenuItem newSubMenuItem = new MenuItem();
                newSubMenuItem.Header = name;
                newSubMenuItem.Tag = data;
                newSubMenuItem.Click += eventHandler;

                if (Regex.IsMatch(name, "^AND\\d"))
                {
                    andItems.Items.Add(newSubMenuItem);
                }
                else if (Regex.IsMatch(name, "^NAND\\d"))
                {
                    nandItems.Items.Add(newSubMenuItem);
                }
                else if (Regex.IsMatch(name, "^OR\\d"))
                {
                    orItems.Items.Add(newSubMenuItem);
                }
                else if (Regex.IsMatch(name, "^NOR\\d"))
                {
                    norItems.Items.Add(newSubMenuItem);
                }
                else
                {
                    newMenuItem.Items.Add(newSubMenuItem);
                }


            }
        }

        private string TrimModuleName(FileInfo file)
        {
            string name = file.Name.Remove(0, 1);
            name = name.Remove(name.Length - 2);
            return name;
        }
        
    }
}
