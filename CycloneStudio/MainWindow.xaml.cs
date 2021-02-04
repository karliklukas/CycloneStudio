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
using AppSpace.structs;
using System.Windows.Media.Effects;

namespace AppSpace
{
    /// <summary>
    /// Interakční logika pro MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isRectDragInProg = false;
        private bool _isLineDrag = false;
        private bool _isLineDragDone = false;        
        private int moduleId;
        private int wireId;

        private Line line = new Line();
        Polyline polyline;            
        private Rectangle rectFrom, rectTo;

        private List<Rectangle> modules;
        private List<Rectangle> deactivated;



        public MainWindow()
        {
            InitializeComponent();
            generateMenuItems();            
            moduleId = 0;
            wireId = 0;

            modules = new List<Rectangle>();
            deactivated = new List<Rectangle>();

            /*//Add to main menu
            MenuItem newMenuItem1 = new MenuItem();
            newMenuItem1.Header = "Test 123";
            this.mmMenu.Items.Add(newMenuItem1);*/

            //Add to a sub item
            MenuItem newMenuItem2 = new MenuItem();
            MenuItem newExistMenuItem = (MenuItem)this.mmMenu.Items[0];

            MenuItem newMenuItem3 = new MenuItem();
            newMenuItem3.Header = "Generuj";
            newExistMenuItem.Items.Add(newMenuItem3);
            
            newMenuItem3.Click += new RoutedEventHandler(menuItemClick);
        }

        


        private void GenerujBlok()
        {

            Label popis = new Label
            {
                Height = 100 * 2,
                Width = 100 * 2,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Content = "text",
                Margin = new Thickness(50, 50, 0, 0),
                Foreground = Brushes.Red,
                FontWeight = FontWeights.Bold,
                IsHitTestVisible = false
            };
            Rectangle vedHorni = new Rectangle
            {
                Margin = new Thickness(10, 30, 0, 0),
                Width = 10 * 2,
                Height = 10 * 2,
                Fill = Brushes.Green,
                Stroke = Brushes.Gray,
                StrokeThickness = 0
            };
            Rectangle vedDolni = new Rectangle
            {
                Margin = new Thickness(110, 80, 0, 0),
                Width = 10 * 2,
                Height = 10 * 2,
                Fill = Brushes.Yellow,
                Stroke = Brushes.OrangeRed,
                StrokeThickness = 0
            };

            Pin inpin1 = new Pin
            {
                Connected = false,
                Hidden = false,
                Name = "A",
                Type = Types.IN,
                Rectangle = vedHorni
            };

            Pin outpin1 = new Pin
            {
                Connected = false,
                Hidden = false,
                Name = "B",
                Type = Types.OUT,
                Rectangle = vedDolni
            };


            Module module = new Module
            {
                Name = "Mod " + (++moduleId)
            };
            module.InPins.Add(inpin1);
            module.OutPins.Add(outpin1);            

            Rectangle g = new Rectangle
            {
                Margin = new Thickness(0, 0, 0, 0),
                Width = 50 * 2,
                Height = 50 * 2,
                RadiusX = 5,
                RadiusY = 5,
                Fill = Brushes.Black,
                Stroke = Brushes.Gray,
                StrokeThickness = 0,
                Tag = module,
                Effect = GetShadowEffect()

        };
            Grid hlavni = new Grid
            {
                Margin = new Thickness(20, 20, 0, 0),
                Width = 50 * 2,
                Height = 50 * 2,
                Background = Brushes.Transparent,
                Tag = g,
                Effect = GetShadowEffect()
            };



            TextBlock someText = new TextBlock();
            someText.Text = module.Name;
            someText.Foreground = Brushes.White;
            FontSizeConverter fSizeConverter = new FontSizeConverter();
            someText.FontSize = (double)fSizeConverter.ConvertFromString("7pt");
            someText.Margin = new Thickness(25, 5, 0, 0);

            TextBlock someText1 = new TextBlock();
            someText1.Text = "Cau cau";
            someText1.Foreground = Brushes.White;
            FontSizeConverter fSizeConverter1 = new FontSizeConverter();
            someText1.FontSize = (double)fSizeConverter1.ConvertFromString("7pt");
            someText1.Margin = new Thickness(5, 70, 0, 0);

            hlavni.Children.Add(g);
            hlavni.Children.Add(someText);
            hlavni.Children.Add(someText1);

            Canvas.SetLeft(hlavni, 0);
            Canvas.SetTop(hlavni, 0);
            Canvas.SetLeft(vedDolni, 0);
            Canvas.SetTop(vedDolni, 0);
            Canvas.SetTop(vedHorni, 0);
            Canvas.SetLeft(vedHorni, 0);


            Panel.SetZIndex(hlavni, 1);
            Panel.SetZIndex(vedHorni, 1);
            Panel.SetZIndex(vedDolni, 1);

            hlavni.MouseEnter += EventMouseOverGrid;
            hlavni.MouseLeave += EventMouseLeaveGrid;
            hlavni.MouseLeftButtonDown += rect_MouseLeftButtonDown;
            hlavni.MouseLeftButtonUp += rect_MouseLeftButtonUp;
            hlavni.MouseMove += rect_MouseMove;

            SetPinEvents(vedDolni);
            SetPinEvents(vedHorni);

            vedHorni.Tag = inpin1;// new List<PolylineTagData>();
            vedDolni.Tag = outpin1;// new List<PolylineTagData>();

            canvas.Children.Add(hlavni);
            canvas.Children.Add(vedHorni);
            canvas.Children.Add(vedDolni);
            //canvas.Children.Add(popis);

            canvas.MouseMove += canvas_MouseMove;
            canvas.MouseLeftButtonUp += canvas_MouseUp;

            modules.Add(g);

        }

        

        private void SetPinEvents(Rectangle rec)
        {
            rec.MouseLeftButtonDown += ved_MouseLeftButtonDown;
            rec.MouseLeftButtonUp += ved_MouseLeftButtonUp;
            rec.MouseLeave += EventMouseLeave;
            rec.MouseEnter += EventMouseOverpin;
        }

        private void rect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isLineDrag) return;
            Grid el = (Grid)sender;
            Rectangle rec = (Rectangle)el.Tag;
            _isRectDragInProg = true;
           
            Module pol = (Module)rec.Tag;
            Panel.SetZIndex(el, 100);
            Panel.SetZIndex(pol.InPins[0].Rectangle, 101);
            Panel.SetZIndex(pol.OutPins[0].Rectangle, 102);           
        }

        private void rect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isLineDrag) return;

            Grid el = (Grid)sender;
            Rectangle rec = (Rectangle)el.Tag;
            _isRectDragInProg = false;

            Module module = (Module)rec.Tag;
            Panel.SetZIndex(el, 1);
            Panel.SetZIndex(module.InPins[0].Rectangle, 1);
            Panel.SetZIndex(module.OutPins[0].Rectangle, 1);
            PinsRestoreLines(module.InPins);
            PinsRestoreLines(module.OutPins);
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

                        p.Polyline = GenerateLine(xs, ys, xe, ye, p.Id, p);
                    }
                }
            }
        }

        private void ved_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isLineDragDone)
            {
                _isLineDragDone = false;
                return;
            }
            
            Rectangle el = (Rectangle)sender;
            rectFrom = el;
            _isLineDrag = true;           

            Pin pinInfo = (Pin)el.Tag;
            Types startPinType = pinInfo.Type;
            deactivated.Clear();
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
                    if (startPinType != Types.IN && !pinInfo.CompareConnections(p.Rectangle) && sameModule && p.ActiveConnections.Count == 0)
                    {
                        continue;
                    }
                    p.Rectangle.IsHitTestVisible = false;
                    deactivated.Add(p.Rectangle);
                }
                foreach (Pin p in m.OutPins)
                {
                    if (startPinType != Types.OUT && !pinInfo.CompareConnections(p.Rectangle) && sameModule)
                    {
                        continue;
                    }
                    p.Rectangle.IsHitTestVisible = false;
                    deactivated.Add(p.Rectangle);
                }                
            }

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

        private void rect_MouseMove(object sender, MouseEventArgs e)
        {           
            if (!_isRectDragInProg) return;
            Grid el = (Grid)sender;
            Rectangle rec = (Rectangle)el.Tag;
            // get the position of the mouse relative to the Canvas
            var mousePos = e.GetPosition(canvas);

            // center the rect on the mouse
            double left = mousePos.X - (el.ActualWidth / 2);
            double top = mousePos.Y - (el.ActualHeight / 2);
            Canvas.SetLeft(el, left);
            Canvas.SetTop(el, top);

            Module module = (Module)rec.Tag;

            MovePinsOnMouseMove(left, top, module.InPins);
            MovePinsOnMouseMove(left, top, module.OutPins);
        }

        private void MovePinsOnMouseMove(double left, double top, List<Pin> data)
        {
            foreach (Pin p in data)
            {
                if (p != null)
                {
                    Rectangle r = p.Rectangle;
                    Canvas.SetLeft(r, left);
                    Canvas.SetTop(r, top);
                    foreach (PolylineTagData po in p.ActiveConnections)
                    {
                        if (po != null)
                        {
                            canvas.Children.Remove((Label)po.Polyline.Tag);
                            canvas.Children.Remove(po.Polyline);
                        }
                    }
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
            Rectangle el = (Rectangle)sender;
            el.StrokeThickness = 3;
            if (this.Cursor != Cursors.Wait)
                Mouse.OverrideCursor = Cursors.Hand;

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

        private void menuItemClick(object sender, RoutedEventArgs e)
        {
            GenerujBlok();
        }

        private void generateMenuItems()
        {
            DirectoryInfo d = new DirectoryInfo(@"../../components");
            DirectoryInfo[] subdir = d.GetDirectories();
            /*string[] lines = System.IO.File.ReadAllLines(@"../../components/bit/cONE.v");
            
            System.Console.WriteLine("Contents of WriteLines2.txt = ");
            foreach (string line in lines)
            {
                // Use a tab to indent each line of the file.
                Console.WriteLine("\t" + line);
            }*/
            foreach (DirectoryInfo sub in subdir)
            {
                Console.WriteLine(sub.Name);
                //Add to menu
                MenuItem newMenuItem = new MenuItem();
                newMenuItem.Header = sub.Name;
                this.mmMenu.Items.Add(newMenuItem);

                FileInfo[] Files = sub.GetFiles("*.v");
                foreach (FileInfo file in Files)
                {
                    MenuItem newSubMenuItem = new MenuItem();   
                    string name = file.Name.Remove(0, 1);
                    name = name.Remove(name.Length -2);
                    newSubMenuItem.Header = name;
                    newMenuItem.Items.Add(newSubMenuItem);
                    
                }
            }
        }

        private void ved_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isLineDrag) return;
            _isLineDragDone = true;
            Rectangle el = (Rectangle)sender;
            rectTo = el;
            CalculateNewCoordinates(el, out double x, out double y);
            line.X2 = x;
            line.Y2 = y;
            ChangePinHitVisibility(true);

            _isLineDrag = false;
           

            Pin pinFrom = (Pin)rectFrom.Tag;
            Pin pinTo = (Pin)rectTo.Tag;
            PolylineTagData data = new PolylineTagData();
           
            string wireName= "e";
            
            if (pinFrom.Type == Types.IN)
            {
                data.RecPinIn = rectFrom;
                data.RecPinOut = rectTo;
                if (pinTo.Connected)
                {
                    wireName = pinTo.Name_wire;                    
                }
            } else
            {
                data.RecPinIn = rectTo;
                data.RecPinOut = rectFrom;                
                if (pinFrom.Connected)
                {
                    wireName = pinFrom.Name_wire;                    
                }
            }

            if (string.Equals(wireName, "e"))
                wireName = "w" + ++wireId;

            data.Id = wireName;
            data.Polyline = GenerateLine(line.X1, line.Y1, line.X2, line.Y2, wireName, data);

            pinFrom.Connected = true;
            pinFrom.Name_wire = wireName;
            pinFrom.ActiveConnections.Add(data);
            pinTo.Connected = true;
            pinTo.Name_wire = wireName;
            pinTo.ActiveConnections.Add(data);

            canvas.Children.Remove(line);
            line = null;
            rectFrom = null;
            rectTo = null;
            Console.WriteLine("asd");
        }

        private void ChangePinHitVisibility(bool change)
        {
            foreach (Rectangle r in deactivated)
            {               
                r.IsHitTestVisible = change;
            }
            deactivated.Clear();
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {           
            if (!_isLineDrag) return;           

            // get the position of the mouse relative to the Canvas
            var mousePos = e.GetPosition(canvas);

            // center the rect on the mouse
            if (line.X1 < mousePos.X)
            {
                line.X2 = mousePos.X - 4;
                line.Y2 = mousePos.Y;
            }else
            {
                line.X2 = mousePos.X + 4;
                line.Y2 = mousePos.Y;
            }
        }

        private void canvas_MouseUp(object sender, MouseEventArgs e)
        {            
            if (!_isLineDrag) return;
            if (!canvas.IsMouseDirectlyOver) return;
            //clickCount++;
            //if (clickCount < 3) return;
            _isLineDrag = false;
            canvas.Children.Remove(line);
            ChangePinHitVisibility(true);
            line = null;
            polyline = null;
            rectFrom = null;
        }

        private Polyline GenerateLine(double startX, double startY, double endX, double endY, string name, PolylineTagData data)
        {               
            if (startX >= endX)
            {
                double tmpX = startX, tmpY = startY;
                startX = endX;
                startY = endY;
                endX = tmpX;
                endY = tmpY;
            }            

            polyline = new Polyline
            {
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                Effect = GetShadowEffect()
            };
            polyline.MouseLeftButtonUp += poly_MouseLeftButtonUp;
            polyline.MouseEnter += EventMouseOverLine;
            polyline.MouseLeave += EventMouseLeaveLine;
           // Create a collection of points for a polyline  
           Point Point1 = new Point(startX, startY);
            double distance = Math.Abs(startX - endX);
            Point Point3 = new Point(startX + (distance / 2), startY);
            Point Point4 = new Point(startX + (distance / 2), endY);
            Point Point5 = new Point(endX, endY);

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
            Canvas.SetLeft(text, startX+10);
            Canvas.SetTop(text, startY-25);
              
            polyline.Points = polygonPoints;
            
            canvas.Children.Add(polyline);
            canvas.Children.Add(text);

            return polyline;

        }

        private void poly_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            string message = "Do you want to delete this connection?";
            string title = "Delete connection";
            MessageBoxImage icon = MessageBoxImage.Warning;
            MessageBoxButton buttons = MessageBoxButton.YesNo;
            MessageBoxResult result = MessageBox.Show(message, title, buttons, icon);
            if (result == MessageBoxResult.Yes)
            {
                Polyline poly = (Polyline)sender;
                Label label = (Label)poly.Tag;
                PolylineTagData data = (PolylineTagData)label.Tag;

                Pin inPin = (Pin)data.RecPinIn.Tag;
                Pin outPin = (Pin)data.RecPinOut.Tag;

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

        private void EventMouseOverLine(object sender, MouseEventArgs e)
        {
            Polyline poly = (Polyline)sender;
            poly.StrokeThickness = 4;
            poly.Stroke = Brushes.Red;
            if (this.Cursor != Cursors.Wait)
                Mouse.OverrideCursor = Cursors.Cross;

        }

        private void EventMouseLeaveLine(object sender, MouseEventArgs e)
        {
            Polyline poly = (Polyline)sender;
            poly.StrokeThickness = 2;
            poly.Stroke = Brushes.Black;
            if (this.Cursor != Cursors.Wait)
                Mouse.OverrideCursor = Cursors.Arrow;

        }

        private void CalculateNewCoordinates(Rectangle rectangle, out double xOut, out double yOut)
        {
            double x = Canvas.GetLeft(rectangle);
            double y = Canvas.GetTop(rectangle);
            xOut = x + rectangle.Margin.Left + (rectangle.ActualWidth / 2);
            yOut = y + rectangle.Margin.Top + (rectangle.ActualHeight / 2);
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
    }
}
