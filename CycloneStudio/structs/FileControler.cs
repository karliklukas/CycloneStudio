using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace AppSpace.structs
{
    class FileControler
    {
        public FileControler() {

        }        

        public static void GenerateMenuItems(Menu mmMenu)
        {
            DirectoryInfo d = new DirectoryInfo(@"../../components");
            DirectoryInfo[] subdir = d.GetDirectories();

            ReadAndGenerateItems(mmMenu, subdir);
        }

        private static void ReadAndGenerateItems(Menu mmMenu, DirectoryInfo[] subdir)
        {
            foreach (DirectoryInfo sub in subdir)
            {   
                MenuItem newMenuItem = new MenuItem();
                newMenuItem.Header = sub.Name;
                mmMenu.Items.Add(newMenuItem);

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

        private static void GenerateItems(DirectoryInfo sub, MenuItem newMenuItem)
        {
            FileInfo[] Files = sub.GetFiles("*.v");
            foreach (FileInfo file in Files)
            {
                MenuItem newSubMenuItem = new MenuItem();
                string name = file.Name.Remove(0, 1);
                name = name.Remove(name.Length - 2);
                newSubMenuItem.Header = name;
                newMenuItem.Items.Add(newSubMenuItem);
            }
        }

        private static void GenerateSubItems(DirectoryInfo sub, MenuItem newMenuItem)
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
                string name = file.Name.Remove(0, 1);
                name = name.Remove(name.Length - 2);
                if (Regex.IsMatch(name, "^AND\\d"))
                {
                    MenuItem newSubMenuItem = new MenuItem();
                    newSubMenuItem.Header = name;
                    andItems.Items.Add(newSubMenuItem);
                }
                else if (Regex.IsMatch(name, "^NAND\\d"))
                {
                    MenuItem newSubMenuItem = new MenuItem();
                    newSubMenuItem.Header = name;
                    nandItems.Items.Add(newSubMenuItem);
                }
                else if (Regex.IsMatch(name, "^OR\\d"))
                {
                    MenuItem newSubMenuItem = new MenuItem();
                    newSubMenuItem.Header = name;
                    orItems.Items.Add(newSubMenuItem);
                }
                else if (Regex.IsMatch(name, "^NOR\\d"))
                {
                    MenuItem newSubMenuItem = new MenuItem();
                    newSubMenuItem.Header = name;
                    norItems.Items.Add(newSubMenuItem);
                }
                else
                {
                    MenuItem newSubMenuItem = new MenuItem();                
                    newSubMenuItem.Header = name;
                    newMenuItem.Items.Add(newSubMenuItem);
                }
                

            }
        }
    }
}
