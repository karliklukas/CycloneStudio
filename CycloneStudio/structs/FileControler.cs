﻿using System;
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
using System.Diagnostics;
using System.Threading;

namespace CycloneStudio.structs
{
    class FileControler
    {
        private LoadingWindow loadingWindow;
        private const string PROJECT_PATH = "..\\..\\workspace";
        private const string BLOCK_PATH = "..\\..\\components\\block";
        private const string SCRIPT_PATH = "..\\..\\scripts";
        private const string QUARTUS_PATH = "..\\..\\scripts\\path.txt";
        private const string COMPONENTS_PATH = "..\\..\\components";
        private readonly RoutedEventHandler eventHandler;
        private readonly RoutedEventHandler eventCustomHandler;
        private string loadingTextTop;
        private string loadingTextBottom;

        public FileControler() { }

        public FileControler(RoutedEventHandler eventHandler, RoutedEventHandler eventCustomHandler)
        {
            this.eventHandler = eventHandler;
            this.eventCustomHandler = eventCustomHandler;
        }

        public void GenerateMenuItems(Menu menu)
        {
            DirectoryInfo d = new DirectoryInfo(COMPONENTS_PATH);
            DirectoryInfo[] subdir = d.GetDirectories().OrderBy(x => x.Name).ToArray();

            ReadAndGenerateItems(menu, subdir);
        }

        private void ReadAndGenerateItems(Menu menu, DirectoryInfo[] directoriesList)
        {
            foreach (DirectoryInfo directory in directoriesList)
            {
                MenuItem newMenuItem = new MenuItem();
                newMenuItem.Header = directory.Name;
                menu.Items.Add(newMenuItem);

                DirectoryInfo[] innerDirList = directory.GetDirectories();
                if (innerDirList.Length != 0)
                {
                    foreach (DirectoryInfo innerDirectory in innerDirList)
                    {
                        MenuItem subItem = new MenuItem();
                        subItem.Header = innerDirectory.Name;


                        if (directory.Name == "block")
                        {
                            if (innerDirectory.Name == "tmp")
                                continue;

                            GenerateBlockItems(innerDirectory, subItem);
                            newMenuItem.Items.Add(subItem);

                        }
                        else
                        {
                            GenerateItems(innerDirectory, subItem);
                            newMenuItem.Items.Add(subItem);
                        }
                    }
                }

                if (directory.Name == "logic")
                {
                    GenerateSubItems(directory, newMenuItem);
                }
                else
                {
                    GenerateItems(directory, newMenuItem);
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

        private void GenerateBlockItems(DirectoryInfo sub, MenuItem newMenuItem)
        {
            FileInfo[] Files = sub.GetFiles("*.v");
            FileInfo mainFile = Files.ToList().Find(file => TrimModuleName(file) == sub.Name);
            if (mainFile == null)
            {
                newMenuItem.IsEnabled = false;
                return;
            }

            MenuData data = ReadAndProcessFile(mainFile.FullName, false);
            newMenuItem.Tag = data;
            newMenuItem.Click += eventHandler;

        }

        private MenuData ReadAndProcessFile(string path, bool isBlock)
        {
            MenuData data = new MenuData();
            string text = File.ReadAllText(path);
            string[] textSplited = Regex.Split(text, "module ([\\w\\d]+[ ]?)\\(\\s*([\\w\\d,_\\]\\[:\\n\\s(]*)\\);");
            data.Name = textSplited[1].Remove(0, 1);

            if (isBlock)
            {
                data.FilePath = path.Replace('/', System.IO.Path.DirectorySeparatorChar);
            }
            else
            {
                MakeRelativePath(path, data, COMPONENTS_PATH);
            }

            if (data.FilePath.Contains("\\block\\"))
            {
                data.IsBlock = true;
            }

            if (textSplited.Length == 4)
            {
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

                ProcessHiddenAndPositions(data, textSplited);
            }

            return data;
        }

        private void ProcessHiddenAndPositions(MenuData data, string[] textSplited)
        {
            string[] textSplitedTwo = Regex.Split(textSplited[3], "\\/\\/hidden:\\s([\\w\\d, ]+)[\\n|\\r\\n](\\/\\/position:\\s([;]?\\d{1,3},\\d{1,3},\\w*)*)?");
            if (textSplitedTwo.Length > 1)
            {
                string[] result = Regex.Replace(textSplitedTwo[1], @"\s+", "").Split(',');
                data.HiddenPins = new List<string>(result);

                if (textSplitedTwo[2].Contains("position"))
                {
                    string[] splitedBoards = Regex.Replace(textSplitedTwo[2], @"\s+", "").Split(';');
                    List<BoardInfo> boardInfos = new List<BoardInfo>();

                    foreach (string oneBoard in splitedBoards)
                    {
                        string[] coordinatesAndName = Regex.Split(oneBoard, "((\\d{1,3}),(\\d{1,3}),(\\w*))");

                        BoardInfo boardInfo = new BoardInfo
                        {
                            MarginLeft = Int32.Parse(coordinatesAndName[2]),
                            MarginTop = Int32.Parse(coordinatesAndName[3]),
                            BoardName = coordinatesAndName[4]
                        };
                        boardInfos.Add(boardInfo);
                    }

                    data.BoardInfo = boardInfos;
                }
            }
        }

        private static void MakeRelativePath(string path, MenuData data, string dirPath)
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

        public void GenerateProjectsList(ItemCollection items, bool isProject)
        {
            string path;
            if (isProject)
            {
                path = PROJECT_PATH;
            }
            else
            {
                path = BLOCK_PATH;
            }
            DirectoryInfo d = new DirectoryInfo(@path);
            DirectoryInfo[] subdir = d.GetDirectories();

            foreach (DirectoryInfo sub in subdir)
            {
                if (sub.Name != "tmp")
                {
                    LoadWindowProjects project = new LoadWindowProjects
                    {
                        Name = sub.Name,
                        Path = sub.FullName,
                        CreateDate = sub.CreationTime.ToString()
                    };
                    items.Add(project);
                }

            }
            items.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
        }

        public bool BuildVerilogForProject(List<Rectangle> modules, string name, HashSet<string> usedModules, string choosenBoardName)
        {
            string nameSrc = name + "\\src";
            string fileName = "c" + name + ".v";
            string dirPathString = System.IO.Path.Combine(PROJECT_PATH, nameSrc);
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
                CopyFile(dirPathString, item);
            }

            if (CopyBuildScriptFiles(dirPathString, choosenBoardName, fileName, usedModules))
            {
                loadingTextTop = "Building...";
                loadingTextBottom = "please wait about 30 sec";
                CreateNewThread();

                int err = CmdCommand(dirPathString, "build.bat");
                CloseWindowSafe();
                if (err == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            else
            {
                return false;
            }
        }

        private void CreateNewThread()
        {
            Thread newWindowThread = new Thread(new ThreadStart(ThreadStartingPoint));
            newWindowThread.SetApartmentState(ApartmentState.STA);
            newWindowThread.IsBackground = true;
            newWindowThread.Start();
        }

        private void ThreadStartingPoint()
        {
            loadingWindow = new LoadingWindow(loadingTextTop, loadingTextBottom);
            loadingWindow.Show();
            System.Windows.Threading.Dispatcher.Run();
        }

        private void CloseWindowSafe()
        {
            if (loadingWindow.Dispatcher.CheckAccess())
            {
                loadingWindow.Close();
            }
            else
            {
                loadingWindow.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new ThreadStart(loadingWindow.Close));
            }
        }

        private int CmdCommand(string path, string command)
        {
            int exitCode;
            ProcessStartInfo processInfo;
            Process process;

            processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.WorkingDirectory = path;

            process = Process.Start(processInfo);
            process.WaitForExit();

            exitCode = process.ExitCode;
            process.Close();
            return exitCode;
        }

        private bool CopyBuildScriptFiles(string targetPath, string boardName, string mainModuleName, HashSet<string> usedModules)
        {
            string sourcePath = System.IO.Path.Combine(SCRIPT_PATH, boardName + "_TEMP");
            string scriptName = "";

            if (boardName == "DE0-Nano")
            {
                scriptName = "DE0Nano";
            }
            else if (boardName == "StormIV-E6")
            {
                scriptName = "StormIV";
            }
            else
            {
                scriptName = boardName;
            }

            if (Directory.Exists(sourcePath))
            {
                string[] files = Directory.GetFiles(sourcePath);

                foreach (string filePath in files)
                {
                    CopyFile(targetPath, filePath);
                }
            }
            else
            {
                Console.WriteLine("Source path does not exist!");
                return false;
            }

            return ChangeScriptsSetting(targetPath, scriptName, mainModuleName, usedModules);
        }

        private bool ChangeScriptsSetting(string targetPath, string scriptName, string mainModuleName, HashSet<string> usedModules)
        {
            string path = System.IO.Path.Combine(targetPath, scriptName + ".qsf");

            string text = File.ReadAllText(path);

            string pureName = mainModuleName.Remove(mainModuleName.Length - 2);

            text = text.Replace("TOP_LEVEL_ENTITY top", "TOP_LEVEL_ENTITY " + pureName);
            text = text.Replace("VERILOG_FILE top.v", "VERILOG_FILE " + mainModuleName);

            foreach (string modulePath in usedModules)
            {
                string module = System.IO.Path.GetFileName(modulePath);
                string addModul = "\nset_global_assignment -name VERILOG_FILE " + module;
                text += addModul;
            }
            File.WriteAllText(path, text);

            path = System.IO.Path.Combine(targetPath, "build.bat");
            ReplaceQuartusPath(path);

            path = System.IO.Path.Combine(targetPath, "upload.bat");
            ReplaceQuartusPath(path);

            return true;
        }

        private static void ReplaceQuartusPath(string path)
        {
            string quartusPath = File.ReadAllText(QUARTUS_PATH);
            string text = File.ReadAllText(path);
            text = text.Replace("QUARTUSpath=", "QUARTUSpath=" + quartusPath);
            File.WriteAllText(path, text);
        }

        private void CopyFile(string targetDirPath, string sourcePath)
        {
            string itenName = System.IO.Path.GetFileName(sourcePath);
            string copyTarget = System.IO.Path.Combine(targetDirPath, itenName);
            File.Copy(sourcePath, copyTarget, true);
        }

        public bool BuildVerilogForBlock(List<Rectangle> modules, string name)
        {
            string fileName = "c" + name + ".v";
            string dirPathString = System.IO.Path.Combine(BLOCK_PATH, name);
            string filePathString = System.IO.Path.Combine(dirPathString, fileName);

            Directory.CreateDirectory(dirPathString);
            string text = CreateVerilogCode(modules, name);

            File.WriteAllText(filePathString, text);

            return true;
        }

        private string CreateVerilogCode(List<Rectangle> modules, string name)
        {
            HashSet<string> wires = new HashSet<string>();
            HashSet<string> wiresBus = new HashSet<string>();

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

                ProcessPinsToVerilog(module.InPins, wires, wiresBus, middlePart, topPart, hiddenPart, inPrefix, module);
                ProcessPinsToVerilog(module.OutPins, wires, wiresBus, middlePart, topPart, hiddenPart, outPrefix, module);

                middlePart.Remove(middlePart.Length - 1, 1);
                middlePart.AppendLine(");");

            }

            middlePart.AppendLine("\nendmodule");
            if (topPart[topPart.Length - 1] != '(')
            {
                topPart.Remove(topPart.Length - 1, 1);
            }

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
            topPart.AppendLine(";");

            foreach (string wire in wiresBus)
            {
                topPart.AppendLine("wire " + wire + ";");
            }
            topPart.AppendLine("");
            topPart.AppendLine(middlePart.ToString());

            return topPart.ToString();
        }

        private void ProcessPinsToVerilog(List<Pin> pins, HashSet<string> wires, HashSet<string> wiresBus, StringBuilder middlePart, StringBuilder topPart, StringBuilder hiddenPart, string prefix, Module module)
        {
            foreach (Pin pin in pins)
            {
                string nameTop = pin.Name;
                string nameMiddle = pin.Name;
                string wireName = pin.Name_wire;
                string wireBusType = "";
                if (pin.IsBus)
                {
                    string[] textSplited = Regex.Split(pin.Name, "([\\d\\w]*)(\\[\\d{1}:\\d{1}\\])");
                    nameTop = textSplited[2] + " " + textSplited[3];
                    nameMiddle = textSplited[3];

                    textSplited = Regex.Split(pin.Name_wire, "([\\d\\w]*) (\\[\\d{1}:\\d{1}\\])");
                    if (textSplited.Length > 1)
                    {
                        wireName = textSplited[1];
                        wireBusType = textSplited[2];
                    }
                }

                if (pin.Hidden)
                {
                    middlePart.Append("." + nameMiddle + "(" + nameMiddle + "),");
                    topPart.Append(prefix + nameTop + ",");
                    if (!module.CustomPin)
                        hiddenPart.Append(nameMiddle + ",");
                }
                else
                {
                    middlePart.Append("." + nameMiddle + "(" + wireName + "),");
                    if (pin.Name_wire != "" && !pin.IsBus)
                    {
                        wires.Add(pin.Name_wire);
                    }
                    else if (pin.Name_wire != "" && pin.IsBus)
                    {
                        wiresBus.Add(wireBusType + " " + wireName);
                    }
                }

            }
        }

        public bool CheckProjectName(string name)
        {
            return Directory.Exists(System.IO.Path.Combine(PROJECT_PATH, name));
        }

        public bool CheckName(string name, bool isProject)
        {
            if (isProject)
            {
                return Directory.Exists(System.IO.Path.Combine(PROJECT_PATH, name));
            }
            else
            {
                return Directory.Exists(System.IO.Path.Combine(BLOCK_PATH, name));
            }
        }

        public void DeleteProjectFolder(string name)
        {
            Directory.Delete(System.IO.Path.Combine(PROJECT_PATH, name), true);
        }

        public void DeleteFolder(string name, bool isProject)
        {
            if (isProject)
            {
                Directory.Delete(System.IO.Path.Combine(PROJECT_PATH, name), true);
            }
            else
            {
               // Directory.Delete(System.IO.Path.Combine(BLOCK_PATH, name), true);
            }

        }

        public void DeleteBlockTmpFolder()
        {
            if (Directory.Exists("..\\..\\components\\block\\tmp"))
            {
                Directory.Delete("..\\..\\components\\block\\tmp", true);
            }
        }

        public void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public bool SaveProjectOrBlock(string name, SaveDataContainer container, bool isProject, List<Module> customPins, bool sameFolder)
        {
            string path;
            if (isProject)
            {
                path = PROJECT_PATH;
            }
            else
            {
                path = BLOCK_PATH;
            }
            string dirPathString = System.IO.Path.Combine(path, name);
            string filePathString = System.IO.Path.Combine(dirPathString, name + ".xml");
            Directory.CreateDirectory(dirPathString);
            if (File.Exists(filePathString))
            {
                File.Delete(filePathString);
            }
            
            if (customPins.Count != 0 && !sameFolder)
            {
                foreach (Module module in customPins)
                {
                    int i = module.Path.LastIndexOf("\\");
                    string itenName = module.Path.Remove(0, i + 1);
                    string copyTarget = System.IO.Path.Combine(dirPathString, itenName);
                    File.Copy(module.Path, copyTarget, true);
                    module.Path = copyTarget;
                }
            }

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

        public SaveDataContainer OpenSaveFile(string path, string name)
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
            string blockPath = System.IO.Path.Combine(BLOCK_PATH, name);
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

        public bool CheckQuartusPathFile()
        {
            return File.Exists(QUARTUS_PATH);
        }

        public void CreateQuartusPathFile(string path)
        {
            File.WriteAllText(QUARTUS_PATH, System.IO.Path.GetDirectoryName(path));
        }

        public bool StartUpload(string name)
        {
            string nameSrc = name + "\\src";
            string dirPathString = System.IO.Path.Combine(PROJECT_PATH, nameSrc);
            string strCmdText = System.IO.Path.Combine(dirPathString, "upload.bat");

            if (File.Exists(strCmdText))
            {
                loadingTextTop = "Uploading...";
                loadingTextBottom = "please wait about 10 sec";
                CreateNewThread();

                int err = CmdCommand(dirPathString, "upload.bat");
                CloseWindowSafe();
                if (err == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        public List<string> GetAllBoards()
        {
            List<string> list = new List<string>();
            string dirPathString = System.IO.Path.Combine(COMPONENTS_PATH, "board");

            DirectoryInfo d = new DirectoryInfo(dirPathString);
            DirectoryInfo[] subdir = d.GetDirectories();

            foreach (DirectoryInfo info in subdir)
            {
                list.Add(info.Name);
            }

            return list;
        }
    }
}
