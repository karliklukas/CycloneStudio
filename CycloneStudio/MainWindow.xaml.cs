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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Media.Effects;
using CycloneStudio.structs;
using Microsoft.Win32;
using System.Runtime.Serialization;
using System.Xml;
using System.Text.RegularExpressions;

namespace CycloneStudio
{
    /// <summary>
    /// Interakční logika pro MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int PINSIZE = 20;
        private const int MODULEWIDHT = 120;
        private bool _isRectDragInProg = false;
        private bool _isLineDrag = false;
        private bool _isLineDragDone = false;
        private bool boardChoosen = false;
        private bool isProject = false;
        private bool isBlock = false;
        private string actualProjectName;
        private string choosenBoardName;
        private int moduleId;
        private int wireId;
        private Double zoom = 1;
        private double pinPreviousStroke;

        private Line line;
        private Rectangle rectFrom, rectTo;

        private List<Rectangle> modules;
        private List<Rectangle> deactivated;
        private List<Rectangle> highlighted;        

        private List<MenuItem> unenabledBoards;
        private List<MenuItem> boardItem;
        private List<MenuItem> unenabledItems;

        private FileControler fileControler;

        public MainWindow()
        {
            InitializeComponent();

            fileControler = new FileControler(new RoutedEventHandler(MenuItemGenerateModule), new RoutedEventHandler(MenuItemCustomPin));

            //GenerateMenuItems();
            moduleId = 0;
            wireId = 0;
            actualProjectName = "";
            choosenBoardName = "";
            isProject = true;
            isBlock = false;

            modules = new List<Rectangle>();
            deactivated = new List<Rectangle>();
            highlighted = new List<Rectangle>();

            unenabledBoards = new List<MenuItem>();
            unenabledItems = new List<MenuItem>();
            boardItem = new List<MenuItem>();

            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            canvas.MouseMove += Canvas_MouseMove;
            canvas.MouseLeftButtonUp += Canvas_MouseUp;

            canvas.Height = SystemParameters.PrimaryScreenHeight -94;
            canvas.Width = SystemParameters.PrimaryScreenWidth;            

            ContentRendered += Window_ContentRendered;    
            
        }      

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            EntryWindow entryWindow = new EntryWindow();
            entryWindow.Owner = this;
            if (entryWindow.ShowDialog() == true)
            {
                if (entryWindow.Confirm)
                {
                    ClearAll(entryWindow.isProject, entryWindow.isBlock);
                }
                else
                {
                    ClearAll(entryWindow.isProject, entryWindow.isBlock);
                    LoadSaveAndSetEnviroment(entryWindow.Path, entryWindow.ProjName);
                }
            }
            else
            {
                this.Close();
            }
        }

        private void SetPinEvents(Rectangle rec)
        {
            rec.MouseLeftButtonDown += Pin_MouseLeftButtonDown;
            rec.MouseLeftButtonUp += Pin_MouseLeftButtonUp;
            rec.MouseLeave += EventMouseLeavePin;
            rec.MouseEnter += EventMouseOverpin;
        }

        private void Module_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (deleteToggle.IsChecked == true)
            {
                DeleteModule(sender);

            }
            else if (handToggle.IsChecked == true)
            {
                if (_isLineDrag) return;
                EnableToggleButtons(false);
                Grid el = (Grid)sender;
                Rectangle rec = (Rectangle)el.Tag;
                _isRectDragInProg = true;

                int counter = 300;
                Module module = (Module)rec.Tag;

                Panel.SetZIndex(el, counter++);

                DeletePinsPolylines(module.InPins, ref counter);
                DeletePinsPolylines(module.OutPins, ref counter);
            }

        }

        private void DeletePinsPolylines(List<Pin> data, ref int counter)
        {
            foreach (Pin pin in data)
            {
                foreach (PolylineTagData po in pin.ActiveConnections)
                {
                    if (po != null)
                    {
                        canvas.Children.Remove((Label)po.Polyline.Tag);
                        canvas.Children.Remove(po.Polyline);
                    }
                }
                if (!pin.Hidden)
                    Panel.SetZIndex(pin.Rectangle, counter++);
            }
        }

        private void DeleteModule(object sender)
        {
            MessageBoxResult result = ShowQuestionDialog("Do you want to delete this module?", "Delete module");
            if (result == MessageBoxResult.Yes)
            {
                Grid el = (Grid)sender;
                Rectangle rec = (Rectangle)el.Tag;
                Module module = (Module)rec.Tag;
                ActivateMenuItem(module.Name);

                foreach (string moduleUsedName in module.ModulesUsedInBlock)
                {
                    ActivateMenuItem(moduleUsedName);
                }                

                DeleteModulesPinsAndConnections(module.InPins);
                DeleteModulesPinsAndConnections(module.OutPins);

                if (Int32.TryParse(module.Id.Remove(0, 1), out int numValue))
                {
                    if (numValue == moduleId)
                        moduleId--;
                }

                modules.Remove(rec);
                canvas.Children.Remove(el);
            }
        }

        private void ActivateMenuItem(string name)
        {
            MenuItem it = unenabledItems.Find(item => item.Header as string == name);
            if (it != null)
            {
                unenabledItems.Remove(it);
                it.IsEnabled = true;
            }

            it = boardItem.Find(item => item.Header as string == name);
            if (it != null)
            {
                boardItem.Remove(it);
                it.IsEnabled = true;
                if (boardItem.Count == 0)
                {
                    foreach (var item in unenabledBoards)
                    {
                        item.IsEnabled = true;
                    }
                    unenabledBoards.Clear();
                    boardChoosen = false;
                    choosenBoardName = "";
                }
            }
        }

        private void DeleteModulesPinsAndConnections(List<Pin> pins)
        {
            foreach (Pin pin in pins)
            {
                foreach (PolylineTagData data in pin.ActiveConnections)
                {
                    Pin iopin;
                    if (pin.Type == Types.IN)
                    {
                        iopin = (Pin)data.RecPinOut.Tag;
                    }
                    else
                    {
                        iopin = (Pin)data.RecPinIn.Tag;
                    }

                    System.Windows.Shapes.Path poly = data.Polyline;
                    Label label = (Label)poly.Tag;

                    iopin.ActiveConnections.Remove(data);

                    if (iopin.Type == Types.IN || iopin.ActiveConnections.Count == 0)
                    {
                        iopin.Connected = false;
                        iopin.Name_wire = "";
                    }
                    canvas.Children.Remove(label);
                    canvas.Children.Remove(poly);
                }
                canvas.Children.Remove(pin.Rectangle);
            }
        }

        private void Module_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isLineDrag || handToggle.IsChecked == false) return;
            EnableToggleButtons(true);
            Grid el = (Grid)sender;
            Rectangle rec = (Rectangle)el.Tag;
            _isRectDragInProg = false;

            Module module = (Module)rec.Tag;
            Panel.SetZIndex(el, 1);

            foreach (Pin pin in module.InPins)
            {
                if (!pin.Hidden)
                    Panel.SetZIndex(pin.Rectangle, 1);
            }
            foreach (Pin pin in module.OutPins)
            {
                if (!pin.Hidden)
                    Panel.SetZIndex(pin.Rectangle, 1);
            }

            PinsRestoreLines(module.InPins);
            PinsRestoreLines(module.OutPins);
        }

        private void Module_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isLineDrag || handToggle.IsChecked == false) return;
            
            Grid el = (Grid)sender;
            Rectangle rec = (Rectangle)el.Tag;
            Module module = (Module)rec.Tag;
            PreviewWindow previewWindow = new PreviewWindow(module.BoardInfo.MarginLeft, module.BoardInfo.MarginTop, module.BoardInfo.BoardName);
            previewWindow.ShowDialog();
            
        }

        private void Module_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isRectDragInProg) return;
            Grid el = (Grid)sender;
            Rectangle rec = (Rectangle)el.Tag;

            var mousePos = e.GetPosition(canvas);

            double left = mousePos.X - (el.ActualWidth / 2) - 20;
            double top = mousePos.Y - (el.ActualHeight / 2) - 20;
            Canvas.SetLeft(el, left);
            Canvas.SetTop(el, top);

            Module module = (Module)rec.Tag;
            module.MarginLeft = left;
            module.MarginTop = top;

            MovePinsOnModuleMove(left, top, module.InPins);
            MovePinsOnModuleMove(left, top, module.OutPins);
        }

        private void PinsRestoreLines(List<Pin> data)
        {
            foreach (Pin r in data)
            {
                foreach (PolylineTagData p in r.ActiveConnections)
                {
                    //PolylineTagData p = (PolylineTagData)r?.Tag;
                    if (p != null)
                    {
                        CalculateNewCoordinates(p.RecPinIn, out double xs, out double ys);
                        CalculateNewCoordinates(p.RecPinOut, out double xe, out double ye);

                        p.Polyline = GenerateLine(xs, ys, xe, ye, p.Wirename, p);
                    }
                }
            }
        }

        private void Pin_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (handToggle.IsChecked == false || _isLineDrag) return;
            if (_isLineDragDone)
            {
                _isLineDragDone = false;
                return;
            }

            Rectangle el = (Rectangle)sender;
            rectFrom = el;
            Pin pinInfo = (Pin)el.Tag;
            Types startPinType = pinInfo.Type;

            if (startPinType == Types.IN && pinInfo.Connected)
            {
                return;
            }
            EnableToggleButtons(false);
            _isLineDrag = true;

            HighlightCorrectRectanglesPins(pinInfo, startPinType);

            line = new Line
            {
                Visibility = Visibility.Visible,
                StrokeThickness = 2,
                Stroke = Brushes.DarkBlue,
                Effect = GetShadowEffect()
            };
            canvas.Children.Add(line);

            double x = Canvas.GetLeft(el);
            double y = Canvas.GetTop(el);
            var mousePos = e.GetPosition(canvas);

            line.X1 = x + el.Margin.Left + (el.ActualWidth / 2);
            line.X2 = x + el.Margin.Left + (el.ActualWidth / 2) - 10;
            line.Y1 = y + el.Margin.Top + (el.ActualHeight / 2);
            line.Y2 = y + el.Margin.Top + (el.ActualHeight / 2) - 10;

            Panel.SetZIndex(line, 100);
        }

        private void HighlightCorrectRectanglesPins(Pin pinInfo, Types startPinType)
        {
            deactivated.Clear();
            highlighted.Clear();

            foreach (Rectangle r in modules)
            {
                Module m = (Module)r.Tag;
                bool sameModule = true;

                if (m.InPins.Contains(pinInfo) || m.OutPins.Contains(pinInfo))
                {
                    sameModule = false;
                }

                foreach (Pin p in m.InPins)
                {
                    if (startPinType != Types.IN && !pinInfo.CompareConnections(p.Rectangle) && sameModule && p.ActiveConnections.Count == 0 && (pinInfo.IsBus == p.IsBus))
                    {
                        if (pinInfo.BusType == p.BusType)
                        {
                            p.Rectangle.StrokeThickness = 2;
                            highlighted.Add(p.Rectangle);
                        }
                        else
                        {
                            p.Rectangle.IsHitTestVisible = false;
                            deactivated.Add(p.Rectangle);
                        }
                    }
                    else
                    {
                        p.Rectangle.IsHitTestVisible = false;
                        deactivated.Add(p.Rectangle);
                    }
                    
                }
                foreach (Pin p in m.OutPins)
                {
                    if (startPinType != Types.OUT && !pinInfo.CompareConnections(p.Rectangle) && sameModule && (pinInfo.IsBus == p.IsBus))
                    {
                        if (pinInfo.BusType == p.BusType)
                        {
                            p.Rectangle.StrokeThickness = 2;
                            highlighted.Add(p.Rectangle);
                        }
                        else
                        {
                            p.Rectangle.IsHitTestVisible = false;
                            deactivated.Add(p.Rectangle);
                        }                       
                    }
                    else
                    {
                        p.Rectangle.IsHitTestVisible = false;
                        deactivated.Add(p.Rectangle);
                    }
                }
            }
        }

        private void Pin_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isLineDrag || handToggle.IsChecked == false) return;
            _isLineDragDone = true;
            EnableToggleButtons(true);
            Rectangle el = (Rectangle)sender;

            rectTo = el;
            CalculateNewCoordinates(el, out double x, out double y);
            line.X2 = x;
            line.Y2 = y;
            ChangePinHitVisibility(true);

            _isLineDrag = false;

            Pin pinFrom = (Pin)rectFrom.Tag;
            Pin pinTo = (Pin)rectTo.Tag;
            CreateConnection(pinFrom, pinTo, rectFrom, rectTo);
        }

        private void CreateConnection(Pin pinFrom, Pin pinTo, Rectangle rFrom, Rectangle rTo)
        {
            PolylineTagData data = new PolylineTagData();

            string wireName = FindInOutPinsAndGenName(pinFrom, pinTo, rFrom, rTo, data);

            data.Wirename = wireName;
            data.Polyline = GenerateLine(line.X1, line.Y1, line.X2, line.Y2, wireName, data);

            pinFrom.Connected = true;
            pinFrom.Name_wire = wireName;
            pinFrom.ActiveConnections.Add(data);
            pinTo.Connected = true;
            pinTo.Name_wire = wireName;
            pinTo.ActiveConnections.Add(data);

            pinPreviousStroke = 0;
            Panel.SetZIndex(data.Polyline, 0);
            canvas.Children.Remove(line);
            line = null;
            rectFrom = null;
            rectTo = null;
        }

        private string FindInOutPinsAndGenName(Pin pinFrom, Pin pinTo, Rectangle rFrom, Rectangle rTo, PolylineTagData data)
        {
            string wireName = "e";

            if (pinFrom.Type == Types.IN)
            {
                data.RecPinIn = rFrom;
                data.RecPinOut = rTo;
                if (pinTo.Connected)
                {
                    wireName = pinTo.Name_wire;
                }
            }
            else
            {
                data.RecPinIn = rTo;
                data.RecPinOut = rFrom;
                if (pinFrom.Connected)
                {
                    wireName = pinFrom.Name_wire;
                }
            }

            if (string.Equals(wireName, "e"))
                wireName = "w" + ++wireId;

            if (pinFrom.IsBus)
            {
                wireName = wireName + " " + pinFrom.BusType;
            }
            return wireName;
        }

        private void MovePinsOnModuleMove(double left, double top, List<Pin> data)
        {
            foreach (Pin p in data)
            {
                if (p != null && p.Rectangle != null)
                {
                    Rectangle r = p.Rectangle;
                    Canvas.SetLeft(r, left);
                    Canvas.SetTop(r, top);                   
                }
            }
        }

        private void EventMouseOver(object sender, MouseEventArgs e)
        {
            Rectangle el = (Rectangle)sender;
            el.StrokeThickness = 3;
        }

        private void EventMouseOverGrid(object sender, MouseEventArgs e)
        {
            Grid el = (Grid)sender;
            Rectangle rec = (Rectangle)el.Tag;
            rec.StrokeThickness = 3;
        }

        private void EventMouseOverpin(object sender, MouseEventArgs e)
        {
            if (handToggle.IsChecked == false) return;
            Rectangle el = (Rectangle)sender;
            Pin p = el.Tag as Pin;
            if (p.Connected && p.Type == Types.IN)
            {
                return;
            }
            pinPreviousStroke = el.StrokeThickness;
            el.StrokeThickness = 4;
            if (this.Cursor != Cursors.Hand)
                Mouse.OverrideCursor = Cursors.Hand;

        }

        private void EventMouseLeavePin(object sender, MouseEventArgs e)
        {
            Rectangle el = (Rectangle)sender;
            if (_isLineDrag)
            {
                el.StrokeThickness = pinPreviousStroke;
            }
            else
            {
                el.StrokeThickness = 0;
                pinPreviousStroke = 0;
            }
            if (this.Cursor != Cursors.Arrow)
                Mouse.OverrideCursor = Cursors.Arrow;

        }

        private void EventMouseLeave(object sender, MouseEventArgs e)
        {
            Rectangle el = (Rectangle)sender;
            el.StrokeThickness = 0;
            if (this.Cursor != Cursors.Wait)
                Mouse.OverrideCursor = Cursors.Arrow;

        }

        private void EventMouseLeaveGrid(object sender, MouseEventArgs e)
        {
            Grid el = (Grid)sender;
            Rectangle rec = (Rectangle)el.Tag;
            rec.StrokeThickness = 0;

        }

        private void EventMouseOverLine(object sender, MouseEventArgs e)
        {
            if (deleteToggle.IsChecked == true)
            {
                System.Windows.Shapes.Path poly = (System.Windows.Shapes.Path)sender;
                if (!poly.IsHitTestVisible)
                {
                    return;
                }
                poly.StrokeThickness = 4;
                poly.Stroke = Brushes.Red;
                if (this.Cursor != Cursors.Wait)
                    Mouse.OverrideCursor = Cursors.Cross;
            }
        }

        private void EventMouseLeaveLine(object sender, MouseEventArgs e)
        {
            if (deleteToggle.IsChecked == true)
            {
                System.Windows.Shapes.Path poly = (System.Windows.Shapes.Path)sender;
                poly.StrokeThickness = 2;
                poly.Stroke = Brushes.Black;
                if (this.Cursor != Cursors.Wait)
                    Mouse.OverrideCursor = Cursors.Arrow;
            }
        }

        private void GenerateMenuItems()
        {
            fileControler.GenerateMenuItems(mmMenu);
        }

        private void MenuItemCustomPin(object sender, RoutedEventArgs e)
        {
            MenuItem el = sender as MenuItem;
            //Console.WriteLine(el.Header);
            //DeactivateMenuItem(el);
            
            var dialog = new InputDialog("Create pin", "Enter custom pin name.");
            if (dialog.ShowDialog() == true)
            {
                string pinName = dialog.ResponseText;
                if (Regex.IsMatch(pinName, "(IN_PIN(2[0]|1[0-9]|[1-9])?|OUT_PIN(2[0]|1[0-9]|[1-9])?)$"))
                {
                    MessageBox.Show("Sorry not allowed name.");
                    return;
                }

                MenuData data = el.Tag as MenuData;
                bool success = fileControler.SavaCustomPin(actualProjectName, pinName, data.FilePath, out MenuData newPinData);

                if (success)
                {
                    IEnumerable<string> inPins = newPinData.InPins.Except(newPinData.HiddenPins);
                    IEnumerable<string> outPins = newPinData.OutPins.Except(newPinData.HiddenPins);

                    int pinsCount = Math.Max(inPins.Count(), outPins.Count());
                    CreateModule(newPinData, out Module module, out Grid hlavni, 10 + pinsCount * 30, true);

                    CreatePinsFromList(newPinData.InPins, newPinData.HiddenPins, module, hlavni, 10, 15, Types.IN);
                    CreatePinsFromList(newPinData.OutPins, newPinData.HiddenPins, module, hlavni, 130, 45, Types.OUT);

                    hlavni.Children.Add(CreateTextBlock(30, 0, module.Name, HorizontalAlignment.Center));
                    hlavni.Children.Add(CreateTextBlock(30, (int)hlavni.Height - 15, module.Id, HorizontalAlignment.Left));
                }
                else
                {
                    MessageBox.Show("Error. Pin name propably already exists.");
                }

            }

        }

        private void MenuItemGenerateModule(object sender, RoutedEventArgs e)
        {
            MenuItem el = sender as MenuItem;
            
            DeactivateMenuItem(el);

            MenuData data = el.Tag as MenuData;
            HashSet<string> usedModulesNames = new HashSet<string>();
            HashSet<string> usedModulesPaths = new HashSet<string>();

            if (data.IsBlock)
            {
                bool compatible = CheckBlockCompatibility(data, usedModulesNames, usedModulesPaths);

                if (!compatible)
                {
                    return;
                }
            }

            IEnumerable<string> inPins = data.InPins.Except(data.HiddenPins);
            IEnumerable<string> outPins = data.OutPins.Except(data.HiddenPins);

            int pinsCount = Math.Max(inPins.Count(), outPins.Count());
            if (pinsCount == 0)
            {
                pinsCount = 1;
            }

            CreateModule(data, out Module module, out Grid hlavni, 10 + pinsCount * 30, false);
            module.ModulesUsedInBlock.UnionWith(usedModulesNames);
            module.ModulesPathUsedInBlock.UnionWith(usedModulesPaths);

            CreatePinsFromList(data.InPins, data.HiddenPins, module, hlavni, 10, 15, Types.IN);
            CreatePinsFromList(data.OutPins, data.HiddenPins, module, hlavni, 130, 45, Types.OUT);

            hlavni.Children.Add(CreateTextBlock(30, 0, module.Name, HorizontalAlignment.Center));
            hlavni.Children.Add(CreateTextBlock(30, (int)hlavni.Height - 15, module.Id, HorizontalAlignment.Left));


        }

        private bool CheckBlockCompatibility(MenuData data, HashSet<string> usedModulesNames, HashSet<string> usedModulesPaths)
        {
            int i = data.FilePath.LastIndexOf("\\");
            string path = data.FilePath.Remove(i);

            SaveDataContainer container = fileControler.OpenSaveFile(path, data.Name);

            string blockBoardName = container.Board;

            if (boardChoosen && blockBoardName != choosenBoardName)
            {
                MessageBox.Show("Board used in block is different from board in project.");
                return false;
            }

            foreach (Module item in container.Modules)
            {
                if (unenabledItems.Find(menuitem => menuitem.Header as string == item.Name) != null)
                {
                    MessageBox.Show("Module " + item.Name + " is already used.");
                    return false;
                }
                else if ((boardItem.Find(menuitem => menuitem.Header as string == item.Name) != null))
                {
                    MessageBox.Show("Module " + item.Name + " from board is already used.");
                    return false;
                }
                usedModulesNames.Add(item.Name);
                usedModulesPaths.Add(item.Path);
            }

            if (!boardChoosen)
            {
                choosenBoardName = container.Board;
                boardChoosen = choosenBoardName != "";
            }
            
            DisableMenuItemFromSaveFile(usedModulesNames);
            return true;
        }

        private void CreatePinsFromList(List<string> pinsList, List<string> hiddenPins, Module module, Grid hlavni, int leftMarginPin, int leftMarginText, Types pinType)
        {
            int topMargin = 30;
            int count = 0;

            foreach (string pinName in pinsList)
            {
                Pin createdPin;
                
                if (hiddenPins.Contains(pinName))
                {
                    createdPin = CreateHiddenPin(pinType, pinName, module.Id);
                }
                else
                {
                    createdPin = CreatePin(leftMarginPin, topMargin + topMargin * count, pinType, pinName, module.Id, 50, 0);
                    string[] textSplited = Regex.Split(pinName, "([\\d\\w]*)(\\[\\d{1}:\\d{1}\\])");
                    if (textSplited.Length > 3)
                    {
                        createdPin.IsBus = true;
                        createdPin.BusType = textSplited[2];
                    }

                    HorizontalAlignment alignment = HorizontalAlignment.Left;
                    if (pinType == Types.OUT)
                    {
                        alignment = HorizontalAlignment.Right;
                    }
                    hlavni.Children.Add(CreateTextBlock(leftMarginText, 15 + topMargin * (count++), pinName, alignment));
                }                

                if (pinType == Types.IN)
                {
                    module.InPins.Add(createdPin);
                }
                else if (pinType == Types.OUT)
                {
                    module.OutPins.Add(createdPin);
                }
            }
        }

        private void DeactivateMenuItem(MenuItem el)
        {
            if (el.Parent is MenuItem parent)
            {
                if (parent.Header as string == "io")
                {
                    unenabledItems.Add(el);
                    el.IsEnabled = false;
                }

                if (parent.Parent is MenuItem parentOfParent && (parentOfParent.Header as string) == "board")
                {
                    if (!boardChoosen)
                    {
                        boardChoosen = true;
                        choosenBoardName = parent.Header as string;
                        foreach (MenuItem item in parentOfParent.Items)
                        {
                            if (!item.Equals(parent))
                            {
                                item.IsEnabled = false;
                                unenabledBoards.Add(item);
                            }
                        }
                    }

                    el.IsEnabled = false;
                    boardItem.Add(el);
                }

            }
        }

        private static Label CreateTextBlock(int marginLeft, int marginTop, string text, HorizontalAlignment alignment)
        {
            /*return new TextBlock
            {
                Text = text,
                Foreground = Brushes.Black,
                FontSize = 9,
                Margin = new Thickness(marginLeft, marginTop, 0, 0)
            };*/
            return new Label
            {
                Content = text,
                Foreground = Brushes.Black,
                Width = 60,
                Height = 15,
                Padding = new Thickness(0,0,0,0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalContentAlignment = alignment,
                FontSize = 9,
                Margin = new Thickness(marginLeft, marginTop, 0, 0)
            };
        }

        private void CreateModule(MenuData data, out Module module, out Grid hlavni, int height, bool customPin)
        {
            module = new Module
            {
                Id = "b" + (++moduleId),
                Name = data.Name,
                Path = data.FilePath,
                MarginLeft = 0,
                MarginTop = 0,
                CustomPin = customPin,
                BoardInfo = data.BoardInfo
            };
            Rectangle g = new Rectangle
            {
                Margin = new Thickness(0, 0, 0, 0),
                Width = MODULEWIDHT,
                Height = height,
                RadiusX = 5,
                RadiusY = 5,
                Fill = GetLinearGradientFill(),
                Stroke = Brushes.Red,
                StrokeThickness = 0,
                Tag = module,
                Effect = GetShadowEffect()

            };            

            hlavni = new Grid
            {
                Margin = new Thickness(20, 20, 0, 0),
                Width = MODULEWIDHT,
                Height = height,
                Background = Brushes.Transparent,
                Tag = g,
                Effect = GetShadowEffect()
            };
            Canvas.SetLeft(hlavni, 50);
            Canvas.SetTop(hlavni, 0);

            Panel.SetZIndex(hlavni, 1);

            hlavni.Children.Add(g);
            modules.Add(g);
            if (data.BoardInfo != null)
            {
                Image myImage = CreateQuestionMarkImage(height);
                hlavni.Children.Add(myImage);
                hlavni.MouseRightButtonUp += Module_MouseRightButtonUp;
            }
            

            hlavni.MouseEnter += EventMouseOverGrid;
            hlavni.MouseLeave += EventMouseLeaveGrid;
            hlavni.MouseLeftButtonDown += Module_MouseLeftButtonDown;
            hlavni.MouseLeftButtonUp += Module_MouseLeftButtonUp;
            hlavni.MouseMove += Module_MouseMove;

            canvas.Children.Add(hlavni);
        }

        private static Image CreateQuestionMarkImage(int height)
        {
            Image myImage = new Image
            {
                Height = 20,
                Margin = new Thickness(50, height - 25, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            BitmapImage myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(@"graphics/more.png", UriKind.RelativeOrAbsolute);
            myBitmapImage.DecodePixelHeight = 20;
            myBitmapImage.EndInit();

            myImage.Source = myBitmapImage;
            return myImage;
        }

        private Pin CreatePin(int MarginLeft, int MarginTop, Types pinType, string name, string id, double marginLeft, double marginTop)
        {
            Rectangle rectangle = new Rectangle
            {
                Margin = new Thickness(MarginLeft, MarginTop, 0, 0),
                Width = PINSIZE,
                Height = PINSIZE,
                RadiusX = 2,
                RadiusY = 2,
                Fill = pinType == Types.OUT ? GetLinearGradientFillPinOut() : GetLinearGradientFillPinIn(),
                Stroke = Brushes.OrangeRed,
                StrokeThickness = 0,
                Effect = pinType == Types.IN ? GetShadowEffect() : null
            };

            Pin pin = new Pin
            {
                Connected = false,
                Hidden = false,
                Name = name,
                Type = pinType,
                Rectangle = rectangle,
                ModuleId = id
            };

            Canvas.SetTop(rectangle, marginTop);
            Canvas.SetLeft(rectangle, marginLeft);
            Panel.SetZIndex(rectangle, 2);
            SetPinEvents(rectangle);
            rectangle.Tag = pin;
            canvas.Children.Add(rectangle);

            return pin;
        }

        private Pin CreateHiddenPin(Types pinType, string name, string id)
        {
            Rectangle rectangle = new Rectangle();

            Pin pin = new Pin
            {
                Connected = false,
                Hidden = true,
                Name = name,
                Type = pinType,
                Rectangle = rectangle,
                ModuleId = id
            };
            return pin;
        }

        private void ChangePinHitVisibility(bool change)
        {
            foreach (Rectangle r in deactivated)
            {
                r.IsHitTestVisible = change;
            }
            foreach (Rectangle r in highlighted)
            {
                r.StrokeThickness = 0;
            }
            deactivated.Clear();
            highlighted.Clear();
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isLineDrag) return;

            // get the position of the mouse relative to the Canvas
            var mousePos = e.GetPosition(canvas);

            // center the rect on the mouse
            if (line.X1 < mousePos.X)
            {
                line.X2 = mousePos.X - 4;
                line.Y2 = mousePos.Y;
            }
            else
            {
                line.X2 = mousePos.X + 4;
                line.Y2 = mousePos.Y;
            }
        }

        private void Canvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (!_isLineDrag) return;
            if (!canvas.IsMouseDirectlyOver) return;
            EnableToggleButtons(true);
            _isLineDrag = false;
            canvas.Children.Remove(line);
            ChangePinHitVisibility(true);
            line = null;
            rectFrom = null;
        }

        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control)
                return;

            double zoomMax = 2;
            double zoomMin = 0.9999;
            double zoomSpeed = 0.001;


            zoom += zoomSpeed * e.Delta;
            if (zoom < zoomMin) { zoom = zoomMin; }
            if (zoom > zoomMax) { zoom = zoomMax; }

            Point mousePos = e.GetPosition(canvas);

            if (zoom > 1)
            {
                canvas.LayoutTransform = new ScaleTransform(zoom, zoom, mousePos.X, mousePos.Y);
            }
            else
            {
                canvas.LayoutTransform = new ScaleTransform(zoom, zoom);
            }
            e.Handled = true;
        }

        private System.Windows.Shapes.Path GenerateLine(double startX, double startY, double endX, double endY, string name, PolylineTagData data)
        {
            if (startX >= endX)
            {
                double tmpX = startX, tmpY = startY;
                startX = endX;
                startY = endY;
                endX = tmpX;
                endY = tmpY;
            }

            Polyline polyline = new Polyline
            {
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                Effect = GetShadowEffect()
            };
            polyline.MouseLeftButtonUp += Poly_MouseLeftButtonUp;
            polyline.MouseEnter += EventMouseOverLine;
            polyline.MouseLeave += EventMouseLeaveLine;
            // Create a collection of points for a polyline  
            Point Point1 = new Point(startX + 10, startY);
            double distance = Math.Abs(startX - endX);
            Point Point3 = new Point(startX + (distance / 2), startY);
            Point Point4 = new Point(startX + (distance / 2), endY);
            Point Point5 = new Point(endX - 10, endY);
            
            PointCollection polygonPoints = new PointCollection
            {
                Point1,
                Point3,
                Point4,
                Point5
            };            
            Label text = new Label
            {
                Content = name,
                Tag = data                
            };
            polyline.Tag = text;            

            Canvas.SetLeft(text, startX + 6);
            Canvas.SetTop(text, startY - 25);

            polyline.Points = polygonPoints;

            //canvas.Children.Add(polyline);
            
            ///////////////////////////////////////////
            //Console.WriteLine(distance);
            double dis2 = 0;
            double dis3 = 0;
            if (distance < 100)
            {
                dis2 = distance/3;
            }
            if (distance < 75)
            {
                dis2 = distance / 2;
                dis3 = 5;
            }
            if (distance < 66)
            {
                dis2 = 60;
                dis3 = 10;
            }

            Point[] points = new[] {
            new Point(startX+10, startY),
            new Point(startX+(25-dis3), startY),
            new Point(startX+(70-dis2), startY),
            new Point(endX-(70-dis2), endY),
            new Point(endX-(25-dis3), endY),
            new Point(endX-10, endY)
            };

            PolyLineSegment b = GetBezierApproximation(points, 256);
            PathFigure pf = new PathFigure(b.Points[0], new[] { b }, false);

            PathFigureCollection pfc = new PathFigureCollection();
            pfc.Add(pf);

            PathGeometry pge = new PathGeometry();
            pge.Figures = pfc;

            System.Windows.Shapes.Path p = new System.Windows.Shapes.Path
            {
                Effect = GetShadowEffect(),
                Data = pge,
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 2,
                Tag = text
            };

            p.MouseLeftButtonUp += Poly_MouseLeftButtonUp;
            p.MouseEnter += EventMouseOverLine;
            p.MouseLeave += EventMouseLeaveLine;

            canvas.Children.Add(p);
            canvas.Children.Add(text);
            //return polyline;
            return p;
        }

        PolyLineSegment GetBezierApproximation(Point[] controlPoints, int outputSegmentCount)
        {
            Point[] points = new Point[outputSegmentCount + 1];
            for (int i = 0; i <= outputSegmentCount; i++)
            {
                double t = (double)i / outputSegmentCount;
                points[i] = GetBezierPoint(t, controlPoints, 0, controlPoints.Length);
            }
            return new PolyLineSegment(points, true);
        }

        Point GetBezierPoint(double t, Point[] controlPoints, int index, int count)
        {
            if (count == 1)
                return controlPoints[index];
            var P0 = GetBezierPoint(t, controlPoints, index, count - 1);
            var P1 = GetBezierPoint(t, controlPoints, index + 1, count - 1);
            return new Point((1 - t) * P0.X + t * P1.X, (1 - t) * P0.Y + t * P1.Y);
        }

        private void Poly_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (deleteToggle.IsChecked == true)
            {
                MessageBoxResult result = ShowQuestionDialog("Do you want to delete this connection?", "Delete connection");
                if (result == MessageBoxResult.Yes)
                {
                    System.Windows.Shapes.Path poly = (System.Windows.Shapes.Path)sender;
                    Label label = (Label)poly.Tag;
                    PolylineTagData data = (PolylineTagData)label.Tag;
                    Pin inPin = (Pin)data.RecPinIn.Tag;
                    Pin outPin = (Pin)data.RecPinOut.Tag;

                    if (Int32.TryParse(inPin.Name_wire.Remove(0, 1), out int numValue))
                    {
                        if (numValue == wireId)
                            wireId--;
                    }

                    inPin.ActiveConnections.Remove(data);
                    inPin.Connected = false;
                    inPin.Name_wire = "";

                    outPin.ActiveConnections.Remove(data);
                    if (outPin.ActiveConnections.Count == 0)
                    {
                        outPin.Connected = false;
                        outPin.Name_wire = "";
                    }
                    canvas.Children.Remove(label);
                    canvas.Children.Remove(poly);
                }
                else
                {
                    return;
                }
            }

        }

        private void CalculateNewCoordinates(Rectangle rectangle, out double xOut, out double yOut)
        {
            double x = Canvas.GetLeft(rectangle);
            double y = Canvas.GetTop(rectangle);            
            xOut = x + rectangle.Margin.Left + (PINSIZE / 2);
            yOut = y + rectangle.Margin.Top + (PINSIZE / 2);            
        }

        private void Event_OpenProject(object sender, RoutedEventArgs e)
        {
            ChooseWindow entryWindow = new ChooseWindow(true);
            entryWindow.Owner = this;
            if (entryWindow.ShowDialog() == true)
            {

                if (entryWindow.Confirm)
                {
                    ClearAll(true, false);
                }
                else
                {
                    ClearAll(true, false);
                    LoadSaveAndSetEnviroment(entryWindow.Path, entryWindow.ProjName);
                }
            }
        }

        private void LoadSaveAndSetEnviroment(string path, string name)
        {            
            actualProjectName = name;
            string txt = isProject ? "– PROJECT:" : "– BLOCK:";
            this.Title = "Cyclone Studio " + txt + " " + name;

            SaveDataContainer container = fileControler.OpenSaveFile(path, name);

            if (container != null)
            {
                HashSet<string> usedModules = RenderCanvasFromFile(container);
                DisableMenuItemFromSaveFile(usedModules);
            }
        }

        private void DisableMenuItemFromSaveFile(HashSet<string> usedModules)
        {
            foreach (MenuItem item in mmMenu.Items)
            {
                if (item.Header as string == "board" && boardChoosen)
                {

                    foreach (MenuItem sub in item.Items)
                    {
                        if (sub.Header as string != choosenBoardName)
                        {
                            sub.IsEnabled = false;
                            unenabledBoards.Add(sub);
                        }
                        else
                        {
                            foreach (MenuItem subItem in sub.Items)
                            {
                                if (usedModules.Contains(subItem.Header))
                                {
                                    subItem.IsEnabled = false;
                                    boardItem.Add(subItem);
                                }
                            }
                        }

                    }
                }
                else if (item.Header as string == "io")
                {
                    foreach (MenuItem subItem in item.Items)
                    {
                        if (usedModules.Contains(subItem.Header))
                        {
                            subItem.IsEnabled = false;
                            unenabledItems.Add(subItem);
                        }
                    }
                }
            }
        }

        private void Event_NewProject(object sender, RoutedEventArgs e)
        {            
            MessageBoxResult result = ShowQuestionDialog("Are you sure?", "New project");
            if (result == MessageBoxResult.Yes)
            {
                ClearAll(true, false);
            }
        }

        private void Event_SaveProject(object sender, RoutedEventArgs e)
        {
            if (isProject)
            {
                SaveProjectOrBlock(true);
            }
            else
            {
                MessageBox.Show("Not a project.");
            }
        }

        private void Event_OpenBlock(object sender, RoutedEventArgs e)
        {
            ChooseWindow entryWindow = new ChooseWindow(false);
            entryWindow.Owner = this;
            if (entryWindow.ShowDialog() == true)
            {

                if (entryWindow.Confirm)
                {
                    ClearAll(false, true);
                }
                else
                {
                    ClearAll(false, true);
                    LoadSaveAndSetEnviroment(entryWindow.Path, entryWindow.ProjName);
                }
            }           

        }

        private HashSet<string> RenderCanvasFromFile(SaveDataContainer container)
        {            
            //ClearAll(false, false);
            moduleId = container.ModuleId;
            wireId = container.WireId;
            choosenBoardName = container.Board;
            
            boardChoosen = choosenBoardName != "";
            

            List<Pin> activeOutPins = new List<Pin>();
            HashSet<string> usedModulesNames = new HashSet<string>();

            foreach (Module item in container.Modules)
            {
                int inPins = item.InPins.FindAll(pin => pin.Hidden == false).Count();
                List<Pin> outPinsList = item.OutPins.FindAll(pin => pin.Hidden == false);
                int outPins = outPinsList.Count();
                usedModulesNames.Add(item.Name);

                if (item.ModulesUsedInBlock.Count != 0)
                {
                    usedModulesNames.UnionWith(item.ModulesUsedInBlock);
                }

                activeOutPins.AddRange(outPinsList.FindAll(pin => pin.Connected == true));

                int pinsCount = Math.Max(inPins, outPins);
                CreateModuleFromSave(item, out Module module, out Grid hlavni, 10 + pinsCount * 30);

                CreatePinsFromListSave(item.InPins, module, hlavni, 10, 15, Types.IN);
                CreatePinsFromListSave(item.OutPins, module, hlavni, 130, 45, Types.OUT);

                hlavni.Children.Add(CreateTextBlock(30, 0, module.Name, HorizontalAlignment.Center));
                hlavni.Children.Add(CreateTextBlock(30, (int)hlavni.Height - 15, module.Id, HorizontalAlignment.Left));
            }

            RestoreConnectionsFromSave(activeOutPins);
            return usedModulesNames;
        }

        private void ClearAll(bool proj, bool block)
        {
            canvas.Children.Clear();
            modules.Clear();
            boardChoosen = false;
            isProject = proj;
            isBlock = block;
            unenabledBoards.Clear();
            boardItem.Clear();
            unenabledItems.Clear();
            moduleId = 0;
            wireId = 0;
            actualProjectName = "";
            choosenBoardName = "";
            string txt = proj ? "– PROJECT" : "– BLOCK";
            this.Title = "Cyclone Studio "+txt;

            int count = mmMenu.Items.Count;
            for (int i = 3; i < count; i++)
            {
                mmMenu.Items.RemoveAt(3);
            }
            fileControler.GenerateMenuItems(mmMenu);            
            MenuItem item =  mmMenu.Items[7] as MenuItem;
            ((MenuItem)item.Items[0]).IsEnabled = isBlock;
            ((MenuItem)item.Items[1]).IsEnabled = isBlock;

            item = mmMenu.Items[1] as MenuItem;
            item.IsEnabled = proj;
            item = mmMenu.Items[2] as MenuItem;
            item.IsEnabled = proj;

        }

        private void RestoreConnectionsFromSave(List<Pin> activeOutPins)
        {
            foreach (Pin item in activeOutPins)
            {
                foreach (PolylineTagData oldData in item.ActiveConnections)
                {
                    Rectangle recIn = modules.Find(rec => ((Module)rec.Tag).Id == oldData.ModuleInId);
                    Rectangle recOut = modules.Find(rec => ((Module)rec.Tag).Id == oldData.ModuleOutId);

                    Module moduleIn = recIn.Tag as Module;
                    Module moduleOut = recOut.Tag as Module;

                    Pin pinIn = moduleIn.InPins.Find(pin => pin.Name == oldData.PinInName);
                    Pin pinOut = moduleOut.OutPins.Find(pin => pin.Name == oldData.PinOutName);

                    PolylineTagData data = new PolylineTagData();
                    data.RecPinIn = pinIn.Rectangle;
                    data.RecPinOut = pinOut.Rectangle;

                    CalculateNewCoordinates(pinIn.Rectangle, out double x, out double y);
                    CalculateNewCoordinates(pinOut.Rectangle, out double x1, out double y1);

                    data.Wirename = oldData.Wirename;
                    data.Polyline = GenerateLine(x, y, x1, y1, oldData.Wirename, data);

                    pinOut.Connected = true;
                    pinOut.Name_wire = oldData.Wirename;
                    pinOut.ActiveConnections.Add(data);
                    pinIn.Connected = true;
                    pinIn.Name_wire = oldData.Wirename;
                    pinIn.ActiveConnections.Add(data);

                    Panel.SetZIndex(data.Polyline, 0);
                }
            }
        }

        private void CreateModuleFromSave(Module moduleSaved,out Module module, out Grid hlavni, int height)
        {
            
           module = new Module
            {
                Id = moduleSaved.Id,
                Name = moduleSaved.Name,
                Path = moduleSaved.Path,
                MarginLeft = moduleSaved.MarginLeft,
                MarginTop = moduleSaved.MarginTop,
                CustomPin = moduleSaved.CustomPin,
                ModulesUsedInBlock = moduleSaved.ModulesUsedInBlock,
                ModulesPathUsedInBlock = moduleSaved.ModulesPathUsedInBlock
            };
            Rectangle g = new Rectangle
            {
                Margin = new Thickness(0, 0, 0, 0),
                Width = MODULEWIDHT,
                Height = height,
                RadiusX = 5,
                RadiusY = 5,
                Fill = GetLinearGradientFill(),
                Stroke = Brushes.Red,
                StrokeThickness = 0,
                Tag = module,
                Effect = GetShadowEffect()

            };

            if (moduleSaved.BoardInfo != null)
            {
                BoardInfo binfo = new BoardInfo
                {
                    MarginLeft = moduleSaved.BoardInfo.MarginLeft,
                    MarginTop = moduleSaved.BoardInfo.MarginTop,
                    BoardName = moduleSaved.BoardInfo.BoardName
                };
                module.BoardInfo = binfo;
            }

            hlavni = new Grid
            {
                Margin = new Thickness(20, 20, 0, 0),
                Width = MODULEWIDHT,
                Height = height,
                Background = Brushes.Transparent,
                Tag = g,
                Effect = GetShadowEffect()
            };
            Canvas.SetLeft(hlavni, module.MarginLeft);
            Canvas.SetTop(hlavni, module.MarginTop);

            Panel.SetZIndex(hlavni, 1);
            
            hlavni.Children.Add(g);            
            modules.Add(g);

            if (module.BoardInfo != null)
            {
                Image myImage = CreateQuestionMarkImage(height);
                hlavni.Children.Add(myImage);
                hlavni.MouseRightButtonUp += Module_MouseRightButtonUp;
            }

            hlavni.MouseEnter += EventMouseOverGrid;
            hlavni.MouseLeave += EventMouseLeaveGrid;
            hlavni.MouseLeftButtonDown += Module_MouseLeftButtonDown;
            hlavni.MouseLeftButtonUp += Module_MouseLeftButtonUp;
            hlavni.MouseMove += Module_MouseMove;

            canvas.Children.Add(hlavni);
        }

        private void CreatePinsFromListSave(List<Pin> pinsList, Module module, Grid hlavni, int leftMarginPin, int leftMarginText, Types pinType)
        {
            int topMargin = 30;
            int count = 0;

            foreach (Pin pinName in pinsList)
            {
                Pin createdPin;
                if (pinName.Hidden)
                {
                    createdPin = CreateHiddenPin(pinType, pinName.Name, module.Id);
                }
                else
                {
                    createdPin = CreatePin(leftMarginPin, topMargin + topMargin * count, pinType, pinName.Name, module.Id, module.MarginLeft, module.MarginTop);
                    
                    createdPin.IsBus = pinName.IsBus;
                    createdPin.BusType = pinName.BusType;
                    
                    HorizontalAlignment alignment = HorizontalAlignment.Left;
                    if (pinType == Types.OUT)
                    {
                        alignment = HorizontalAlignment.Right;
                    }
                    hlavni.Children.Add(CreateTextBlock(leftMarginText, 15 + topMargin * (count++), pinName.Name, alignment));
                }

                if (pinType == Types.IN)
                {
                    module.InPins.Add(createdPin);
                }
                else if (pinType == Types.OUT)
                {
                    module.OutPins.Add(createdPin);
                }
            }
        }

        private void Event_SaveBlock(object sender, RoutedEventArgs e)
        {
            if (isBlock)
            {
                SaveProjectOrBlock(false);
                BuildVerilogCode();
                
            } else
            {
                MessageBox.Show("Not a block.");
            }          
            
            //TODO aktualizovat blocks?

        }

        private void SaveProjectOrBlock(bool isProject)
        {
            string dialogTextBig;
            string dialogTextSmall;
            if (isProject)
            {
                dialogTextBig = "Project";
                dialogTextSmall = "project";
            }
            else
            {
                dialogTextBig = "Block";
                dialogTextSmall = "block";
            }
            var dialog = new InputDialog(dialogTextBig +" name", "Enter "+ dialogTextSmall + " name:");
            if (actualProjectName != "")
            {
                dialog.ResponseText = actualProjectName;
            }

            if (dialog.ShowDialog() == true)
            {
                string fileName = dialog.ResponseText;
                string message = "";

                if (fileControler.CheckName(fileName, isProject))
                {
                    string messageBoxText = "Name already exists. Delete and replace "+ dialogTextSmall+"? ";
                    string caption = "Name already exists!";
                    MessageBoxButton button = MessageBoxButton.YesNo;
                    MessageBoxImage icon = MessageBoxImage.Warning;

                    MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);

                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            message += "Old " + dialogTextSmall + " deleted.";
                            fileControler.DeleteFolder(fileName, isProject);
                            break;
                        case MessageBoxResult.No:
                            MessageBox.Show("Choose new name and try again.");
                            return;
                    }
                }
                actualProjectName = fileName;
                string txt = isProject ? "– PROJECT:" : "– BLOCK:";
                this.Title = "Cyclone Studio "+txt+ " " + fileName;
                SaveDataContainer container = new SaveDataContainer();
                container.ModuleId = moduleId;
                container.WireId = wireId;
                container.Board = choosenBoardName;

                List<Module> customPins = new List<Module>();

                foreach (Rectangle item in modules)
                {
                    Module m = item.Tag as Module;
                    if (m.CustomPin)
                    {
                        customPins.Add(m);                        
                    }
                    container.Modules.Add(m);
                }

                bool success = fileControler.SaveProjectOrBlock(fileName, container, isProject, customPins);

                if (success)
                {
                    MessageBox.Show(dialogTextBig+" saved. " + message);
                }
                else
                {
                    MessageBox.Show("Error. " + dialogTextBig + " was not saved. " + message);
                }
            }
        }

        private void Event_NewBlock(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = ShowQuestionDialog("Are you sure?", "New block");
            if (result == MessageBoxResult.Yes)
            {
                fileControler.DeleteBlockTmpFolder();
                ClearAll(false, true);
            }
            
        }

        private void Event_Build(object sender, RoutedEventArgs e)
        {
            BuildVerilogCode();           
        }

        private void BuildVerilogCode()
        {
            if (actualProjectName == "")
            {
                MessageBox.Show("Please save project first.");
                return;
            }

            HashSet<string> usedModulesPath = new HashSet<string>();
            foreach (Rectangle rectangle in modules)
            {
                Module module = rectangle.Tag as Module;
                usedModulesPath.Add(module.Path);

                if (module.ModulesPathUsedInBlock.Count != 0)
                {
                    usedModulesPath.UnionWith(module.ModulesPathUsedInBlock);
                }

                foreach (Pin pin in module.InPins)
                {
                    if (!pin.Connected && !pin.Hidden)
                    {
                        if (isProject)
                        {
                            MessageBox.Show("Module " + module.Id + " (" + module.Name + ") has unconnected pin " + pin.Name);
                        }
                        else if (isBlock)
                        {
                            MessageBox.Show("Module " + module.Id + " (" + module.Name + ") has unconnected pin " + pin.Name + "\n" +
                                "Fix it or you will be unable to use this module in project.");
                        }                        
                        return;
                    }
                }
            }

            if (isProject)
            {
                fileControler.BuildVerilogForProject(modules, actualProjectName, usedModulesPath);
            }
            else if (isBlock)
            {
                fileControler.BuildVerilogForBlock(modules, actualProjectName);
            }


        }

        private void Event_Upload(object sender, RoutedEventArgs e)
        {

        }

        private void Event_SaveAsImage(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.DefaultExt = ".png";
            dlg.Filter = "PNG (*.png)|*.png";
            Nullable<bool> result = dlg.ShowDialog();
            if (result.Value != true) return;

            RenderTargetBitmap rtb = new RenderTargetBitmap((int)this.ActualWidth, (int)this.ActualHeight - 50, 96d, 96d, PixelFormats.Default);
            rtb.Render(canvas);           

            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

            using (var fs = File.OpenWrite(dlg.FileName))
            {
                pngEncoder.Save(fs);
            }
        }

        private void HandToggleChecked(object sender, RoutedEventArgs e)
        {
            deleteToggle.IsChecked = false;
        }

        private void HandToggleUnchecked(object sender, RoutedEventArgs e)
        {
            deleteToggle.IsChecked = true;
        }

        private void DeleteToggleUnchecked(object sender, RoutedEventArgs e)
        {
            handToggle.IsChecked = true;
        }

        private void DeleteToggleChecked(object sender, RoutedEventArgs e)
        {
            handToggle.IsChecked = false;
        }

        private void EnableToggleButtons(bool state)
        {
            handToggle.IsEnabled = state;
            deleteToggle.IsEnabled = state;
        }

        private static MessageBoxResult ShowQuestionDialog(string message, string title)
        {
            MessageBoxImage icon = MessageBoxImage.Question;
            MessageBoxButton buttons = MessageBoxButton.YesNo;
            MessageBoxResult result = MessageBox.Show(message, title, buttons, icon);
            return result;
        }

        private static DropShadowEffect GetShadowEffect()
        {
            return new DropShadowEffect
            {
                Color = Color.FromRgb(100, 100, 100),
                Direction = 225,
                ShadowDepth = 8,
                BlurRadius = 5,
                Opacity = 0.3
            };
        }        

        private static LinearGradientBrush GetLinearGradientFill()
        {
            LinearGradientBrush myVerticalGradient = new LinearGradientBrush();
            myVerticalGradient.StartPoint = new Point(0.5, 0);
            myVerticalGradient.EndPoint = new Point(0.5, 1);
            myVerticalGradient.GradientStops.Add(new GradientStop(Color.FromRgb(0, 170, 255), 0.0));
            myVerticalGradient.GradientStops.Add(new GradientStop(Color.FromRgb(0, 170, 255), 0.2));
            myVerticalGradient.GradientStops.Add(new GradientStop(Color.FromRgb(0, 100, 255), 1.0));
            return myVerticalGradient;
        }

        private static LinearGradientBrush GetLinearGradientFillPinIn()
        {
            LinearGradientBrush myVerticalGradient = new LinearGradientBrush();
            myVerticalGradient.StartPoint = new Point(0, 0.5);
            myVerticalGradient.EndPoint = new Point(1, 0.5);
            myVerticalGradient.GradientStops.Add(new GradientStop(Color.FromRgb(0, 186, 50), 0.0));
            myVerticalGradient.GradientStops.Add(new GradientStop(Color.FromRgb(0, 186, 50), 0.2));
            myVerticalGradient.GradientStops.Add(new GradientStop(Color.FromRgb(0, 131, 35), 1.0));
            return myVerticalGradient;
        }
        
        private static LinearGradientBrush GetLinearGradientFillPinOut()
        {
            LinearGradientBrush myVerticalGradient = new LinearGradientBrush();
            myVerticalGradient.StartPoint = new Point(1, 0.5);
            myVerticalGradient.EndPoint = new Point(0, 0.5);
            myVerticalGradient.GradientStops.Add(new GradientStop(Color.FromRgb(199, 199, 23), 0.0));
            myVerticalGradient.GradientStops.Add(new GradientStop(Color.FromRgb(199, 199, 23), 0.2));
            myVerticalGradient.GradientStops.Add(new GradientStop(Color.FromRgb(179, 179, 24), 1.0));
            return myVerticalGradient;
        }

    }
}
