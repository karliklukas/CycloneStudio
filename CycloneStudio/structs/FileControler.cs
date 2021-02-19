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
using System.Runtime.Serialization;
using System.Xml;

namespace CycloneStudio.structs
{    
    class FileControler
    {
        private readonly RoutedEventHandler eventHandler;
        private readonly RoutedEventHandler eventCustomHandler;

        public FileControler() { }

        public FileControler(RoutedEventHandler eventHandler, RoutedEventHandler eventCustomHandler)
        {
            this.eventHandler = eventHandler;
            this.eventCustomHandler = eventCustomHandler;
        }        

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
                        if (sub.Name == "block" && dir.Name == "tmp")
                        {
                            continue;
                        }
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
                MenuData data = ReadAndProcessFile(file.FullName, false);
                newSubMenuItem.Tag = data;

                if (sub.Name == "io" && Regex.IsMatch(file.Name, "(cIN_PIN\\.v|cOUT_PIN\\.v)"))
                {
                    newSubMenuItem.Click += eventCustomHandler;
                    continue;
                }
                
                newSubMenuItem.Click += eventHandler;

                
            }
        }

        private MenuData ReadAndProcessFile(string path, bool isBlock)
        {
            MenuData data = new MenuData();
            string text = File.ReadAllText(path);
            string[] textSplited = Regex.Split(text, "module ([\\w\\d]+)\\(\\s*([\\w\\d,_\\n\\s(]+)\\);");
            data.Name = textSplited[1].Remove(0, 1);
            string dirPath = "../../components";
            if (isBlock)
            {
                data.FilePath = path.Replace('/', System.IO.Path.DirectorySeparatorChar);
            } else
            {
                DirectoryInfo directory = new DirectoryInfo(@dirPath);
                string fullDirectory = directory.FullName;
                string fullFile = path;

                if (!fullFile.StartsWith(fullDirectory))
                {
                    Console.WriteLine("Unable to make relative path");
                }
                else
                {
                    string p = fullFile.Substring(fullDirectory.Length + 1);
                    p = System.IO.Path.Combine(dirPath, p);
                    p = p.Replace('/', System.IO.Path.DirectorySeparatorChar);
                    data.FilePath = p;
                }
            }
            

            //data.FilePath = path;


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

            string[] textSplitedTwo = Regex.Split(textSplited[3], "\\/\\/hidden:\\s([\\w\\d,\\s]+)(assign|wire)");
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
                MenuData data = ReadAndProcessFile(file.FullName, false);
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

        public bool BuildVerilogForProject(List<Rectangle> modules, string name, HashSet<string> usedModules)
        {
            string nameSrc = name + "/src";
            string fileName = "c" + name + ".v";
            string dirPathString = System.IO.Path.Combine("../../workspace", nameSrc);
            string filePathString = System.IO.Path.Combine(dirPathString, fileName);

            if (CheckProjectName(nameSrc))
            {
                DeleteProjectFolder(nameSrc);
            }
            Directory.CreateDirectory(dirPathString);
            string text = CreateVerilogCode(modules, name);

            File.WriteAllText(filePathString, text);

            foreach (string item in usedModules)
            {
                int i = item.LastIndexOf("\\");
                string itenName = item.Remove(0, i+1);
                string copyTarget = System.IO.Path.Combine(dirPathString, itenName);
                File.Copy(item, copyTarget);
            }
            
            return true;
        }

        private string CreateVerilogCode(List<Rectangle> modules, string name)
        {
            HashSet<string> wires = new HashSet<string>();

            StringBuilder middlePart = new StringBuilder();
            StringBuilder topPart = new StringBuilder();
            StringBuilder hiddenPart = new StringBuilder();

            string inPrefix = "input wire ";
            string outPrefix = "output wire ";
            topPart.Append("module c" + name + "(");
            hiddenPart.Append("//hidden: ");

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

            //Console.WriteLine(topPart.ToString());
            return topPart.ToString();
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

        public bool CheckProjectName(string name)
        {
            return Directory.Exists(System.IO.Path.Combine("../../workspace", name));           
        }

        public void DeleteProjectFolder(string name)
        {
            Directory.Delete(System.IO.Path.Combine("../../workspace", name), true);
        }

        public void DeleteBlockTmpFolder()
        {
            if (Directory.Exists("../../components/block/tmp"))
            {
                Directory.Delete("../../components/block/tmp", true);
            }            
        }

        public bool SaveProject(string name, SaveDataContainer container)
        {
            string dirPathString = System.IO.Path.Combine("../../workspace", name);
            string filePathString = System.IO.Path.Combine(dirPathString, name + ".xml");
            Directory.CreateDirectory(dirPathString);

            if (!File.Exists(filePathString))
            {
                using (FileStream writer = new FileStream(filePathString, FileMode.Create))
                {
                    DataContractSerializer ser = new DataContractSerializer(typeof(SaveDataContainer));
                    ser.WriteObject(writer, container);
                    return true;
                }
            }
            return false;
        }

        public SaveDataContainer OpenProject(string path, string name)
        {
            SaveDataContainer container;
            string filePathString = System.IO.Path.Combine(path, name + ".xml");

            if (File.Exists(filePathString))
            {
                using (FileStream fs = new FileStream(filePathString, FileMode.Open))
                {
                    XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
                    DataContractSerializer ser = new DataContractSerializer(typeof(SaveDataContainer));
                    container = (SaveDataContainer)ser.ReadObject(reader, true);
                    reader.Close();
                }
                return container;
            }
            return null;
        }

        public bool SavaCustomPin(string blockName, string pinName, string sourcePinPath, out MenuData data)
        {
            string name = blockName != "" ? blockName : "tmp";
            string blockPath = System.IO.Path.Combine("../../components/block", name);
            Directory.CreateDirectory(blockPath);
            data = null;

            string str = File.ReadAllText(sourcePinPath);
            str = str.Replace("PIN", pinName);
            string targetPath = String.Concat(blockPath, "\\c", pinName, ".v");
            if (File.Exists(targetPath))
            {
                return false;
            }
            File.WriteAllText(targetPath, str);
            data = ReadAndProcessFile(targetPath, true);

            return true;
        }
    }
}
