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
using System.ComponentModel;

namespace CycloneStudio.structs
{
    class FileControler
    {
        private readonly RoutedEventHandler eventHandler;

        public FileControler() { }

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

            var nums = "0123456789".ToCharArray();
            var sortedFiles = Files
              .OrderBy(x => TrimModuleName(x).LastIndexOfAny(nums))
              .ThenBy(x => x.Name);

            foreach (FileInfo file in sortedFiles)
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
            MenuData data = new MenuData();
            string text = File.ReadAllText(path);
            string[] textSplited = Regex.Split(text, "module ([\\w\\d]+)\\(\\s*([\\w\\d,_\\n\\s(]+)\\);");
            data.Name = textSplited[1].Remove(0, 1); ;
            data.FilePath = path;


            string[] pins = Regex.Replace(textSplited[2], @"\s+", "").Split(',');

            for (int i = 0; i < pins.Length; i++)
            {
                if (pins[i].Contains("outputwire"))
                {
                    data.OutPins.Add(pins[i].Remove(0, 10));                   
                }
                else if (pins[i].Contains("inputwire"))
                {
                    data.InPins.Add(pins[i].Remove(0, 9));                    
                }
            }

            string[] textSplitedTwo = Regex.Split(textSplited[3], "\\/\\/hidden:\\s([\\w\\d,\\s]+)assign");
            if (textSplitedTwo.Length > 1)
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

        public void GenerateProjectsList(ItemCollection items)
        {
            DirectoryInfo d = new DirectoryInfo(@"../../workspace");
            DirectoryInfo[] subdir = d.GetDirectories();

            foreach (DirectoryInfo sub in subdir)
            {                
                LoadWindowProjects project = new LoadWindowProjects
                {
                    Name = sub.Name,
                    Path = sub.FullName,
                    CreateDate = sub.CreationTime.ToString()
                };
                items.Add(project);
                items.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            }
        }

        public bool BuildVerilogForProject(List<Rectangle> modules, string name)
        {               
            HashSet<string> wires = new HashSet<string>();

            StringBuilder middlePart = new StringBuilder();
            StringBuilder topPart = new StringBuilder();
            StringBuilder hiddenPart = new StringBuilder();

            string inPrefix = "input wire ";
            string outPrefix = "output wire ";
            topPart.Append("module " + name + "(");
            hiddenPart.Append("//hidden ");

            foreach (var rec in modules)
            {
                Module module = rec.Tag as Module;

                middlePart.Append("c" + module.Name + " ");
                middlePart.Append(module.Id + "(");

                ProcessPinsToVerilog(module.InPins, wires, middlePart, topPart, hiddenPart, inPrefix, module);
                ProcessPinsToVerilog(module.OutPins, wires, middlePart, topPart, hiddenPart, outPrefix, module);
               
                middlePart.Remove(middlePart.Length - 1, 1);
                middlePart.AppendLine(");");

            }
            middlePart.AppendLine("\nendmodule");

            topPart.Remove(topPart.Length - 1, 1);
            topPart.AppendLine(");");

            hiddenPart.Remove(hiddenPart.Length - 1, 1);
            hiddenPart.AppendLine("");
            topPart.AppendLine(hiddenPart.ToString());            

            topPart.Append("wire ");

            foreach (string wire in wires)
            {
                topPart.Append(wire + ",");
            }
            topPart.Remove(topPart.Length - 1, 1);            
            topPart.AppendLine(";\n");

            topPart.AppendLine(middlePart.ToString());

            Console.WriteLine(topPart.ToString());
            return false;
        }

        private void ProcessPinsToVerilog(List<Pin> pins, HashSet<string> wires, StringBuilder middlePart, StringBuilder topPart, StringBuilder hiddenPart, string inPrefix, Module module)
        {
            foreach (Pin pin in pins)
            {
                if (pin.Hidden)
                {
                    middlePart.Append("." + pin.Name + "(" + pin.Name + "),");
                    topPart.Append(inPrefix + pin.Name + ",");
                    if (!pin.CustomPin)
                        hiddenPart.Append(pin.Name + ",");
                }
                else
                {
                    middlePart.Append("." + pin.Name + "(" + pin.Name_wire + "),");
                    if (pin.Name_wire != "")
                        wires.Add(pin.Name_wire);
                }

            }
        }
    }
}
