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
        private Double zoom = 1;

        private Line line = new Line();                   
        private Rectangle rectFrom, rectTo;

        private List<Rectangle> modules;
        private List<Rectangle> deactivated;

        private FileControler fileControler;



        public MainWindow()
        {
            InitializeComponent();

            fileControler = new FileControler(new RoutedEventHandler(menuItemGenerateModule));

            generateMenuItems();            
            moduleId = 0;
            wireId = 0;

            modules = new List<Rectangle>();
            deactivated = new List<Rectangle>();

            canvas.MouseMove += canvas_MouseMove;
            canvas.MouseLeftButtonUp += canvas_MouseUp;
            
            /*MenuItem newMenuItem2 = new MenuItem();
            MenuItem newExistMenuItem = (MenuItem)this.mmMenu.Items[0];*/

           /* MenuItem newMenuItem3 = new MenuItem();
            newMenuItem3.Header = "Generuj";
            newExistMenuItem.Items.Add(newMenuItem3);
            
            newMenuItem3.Click += new RoutedEventHandler(menuItemClick);*/
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

            int counter = 100;
            Module module = (Module)rec.Tag;
            Panel.SetZIndex(el, counter++);

            foreach(Pin pin in module.InPins)
            {
                Panel.SetZIndex(pin.Rectangle, counter++);
            }
            foreach (Pin pin in module.OutPins)
            {
                Panel.SetZIndex(pin.Rectangle, counter++);
            }                     
        }

        private void rect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isLineDrag) return;

            Grid el = (Grid)sender;
            Rectangle rec = (Rectangle)el.Tag;
            _isRectDragInProg = false;

            Module module = (Module)rec.Tag;            
            Panel.SetZIndex(el, 1);

            foreach (Pin pin in module.InPins)
            {
                Panel.SetZIndex(pin.Rectangle, 1);
            }
            foreach (Pin pin in module.OutPins)
            {
                Panel.SetZIndex(pin.Rectangle, 1);
            }

            PinsRestoreLines(module.InPins);
            PinsRestoreLines(module.OutPins);
        }

        private void rect_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isRectDragInProg) return;
            Grid el = (Grid)sender;
            Rectangle rec = (Rectangle)el.Tag;
            // get the position of the mouse relative to the Canvas
            var mousePos = e.GetPosition(canvas);

            // center the rect on the mouse
            double left = mousePos.X - (el.ActualWidth / 2) - 20;
            double top = mousePos.Y - (el.ActualHeight / 2) - 20;
            Canvas.SetLeft(el, left);
            Canvas.SetTop(el, top);

            Module module = (Module)rec.Tag;

            MovePinsOnMouseMove(left, top, module.InPins);
            MovePinsOnMouseMove(left, top, module.OutPins);
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
            Pin pinInfo = (Pin)el.Tag;
            Types startPinType = pinInfo.Type;

            if (startPinType == Types.IN && pinInfo.Connected)
            {               
                return;
            }

            _isLineDrag = true;  
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
            Console.WriteLine("jop");
        }

        private void generateMenuItems()
        {
            fileControler.GenerateMenuItems(mmMenu);           
        }

        private void menuItemGenerateModule(object sender, RoutedEventArgs e)
        {
            MenuItem el = sender as MenuItem;
            MenuData data = el.Tag as MenuData;           

            IEnumerable<string> inPins = data.InPins.Except(data.HiddenPins);
            IEnumerable<string> outPins = data.OutPins.Except(data.HiddenPins);

            int pinsCount = Math.Max(inPins.Count(), outPins.Count());
            CreateModule(data, out Module module, out Grid hlavni, 10 + pinsCount * 30);

            int topMargin = 30;
            int leftInMargin = 10, leftOutMargin = 130;
            int count = 0;

            foreach (string pin in inPins)
            {
                Pin inpin1 = CreatePin(leftInMargin, topMargin + topMargin * count, Types.IN, pin);
                hlavni.Children.Add(CreateTextBlock(15, 15 + topMargin * (count++), pin));
                module.InPins.Add(inpin1);
            }

            count = 0;
            foreach (string pin in outPins)
            {
                Pin outpin1 = CreatePin(leftOutMargin, topMargin + topMargin * count, Types.OUT, pin);
                hlavni.Children.Add(CreateTextBlock(90, 15 + topMargin * (count++), pin));
                module.OutPins.Add(outpin1);
            }

            hlavni.Children.Add(CreateTextBlock(40, 5, module.Name));
            hlavni.Children.Add(CreateTextBlock(30, (int)hlavni.Height - 15, module.Id));          

            
        }

        private static TextBlock CreateTextBlock(int marginLeft, int marginTop, string text)
        {            
            return new TextBlock
            {
                Text = text,
                Foreground = Brushes.Black,
                FontSize = 9,
                Margin = new Thickness(marginLeft, marginTop, 0, 0)                
            };
        }

        private void CreateModule(MenuData data, out Module module, out Grid hlavni, int height)
        {
            module = new Module
            {
                Id = "b" + (++moduleId),
                Name = data.Name,
                Path = data.FilePath
            };
            Rectangle g = new Rectangle
            {
                Margin = new Thickness(0, 0, 0, 0),
                Width = 120,
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
                Width = 120,
                Height = height,
                Background = Brushes.Transparent,
                Tag = g,
                Effect = GetShadowEffect()
            };
            Canvas.SetLeft(hlavni, 0);
            Canvas.SetTop(hlavni, 0);

            /*Panel.SetZIndex(hlavni, 1);
            Panel.SetZIndex(vedHorni, 1);
            Panel.SetZIndex(vedDolni, 1);*/
            hlavni.Children.Add(g);
            modules.Add(g);

            hlavni.MouseEnter += EventMouseOverGrid;
            hlavni.MouseLeave += EventMouseLeaveGrid;
            hlavni.MouseLeftButtonDown += rect_MouseLeftButtonDown;
            hlavni.MouseLeftButtonUp += rect_MouseLeftButtonUp;
            hlavni.MouseMove += rect_MouseMove;

            canvas.Children.Add(hlavni);
        }

        private Pin CreatePin(int MarginLeft, int MarginTop, Types pinType, string name)
        {
            Rectangle rectangle = new Rectangle
            {
                Margin = new Thickness(MarginLeft, MarginTop, 0, 0),
                Width = 20,
                Height = 20,
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
                Rectangle = rectangle
            };

            Canvas.SetTop(rectangle, 0);
            Canvas.SetLeft(rectangle, 0);
            SetPinEvents(rectangle);
            rectangle.Tag = pin;
            canvas.Children.Add(rectangle);

            return pin;
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
            
            _isLineDrag = false;
            canvas.Children.Remove(line);
            ChangePinHitVisibility(true);
            line = null;            
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

            Polyline polyline = new Polyline
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
            Canvas.SetLeft(text, startX + 10);
            Canvas.SetTop(text, startY - 25);

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
            if (!poly.IsHitTestVisible)
            {
                return;
            }
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

        private void Event_OpenProject(object sender, RoutedEventArgs e)
        {

        }

        private void Event_NewProject(object sender, RoutedEventArgs e)
        {

        }

        private void Event_OpenBlock(object sender, RoutedEventArgs e)
        {

        }

        private void Event_NewBlock(object sender, RoutedEventArgs e)
        {

        }

        private void Event_Build(object sender, RoutedEventArgs e)
        {

        }

        private void Event_Upload(object sender, RoutedEventArgs e)
        {

        }

        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control)
                return;

            double zoomMax = 2;
            double zoomMin = 0.9999;
            double zoomSpeed = 0.001;
            

            zoom += zoomSpeed * e.Delta; // Ajust zooming speed (e.Delta = Mouse spin value )
            if (zoom < zoomMin) { zoom = zoomMin; } // Limit Min Scale
            if (zoom > zoomMax) { zoom = zoomMax; } // Limit Max Scale

            Point mousePos = e.GetPosition(canvas);

            if (zoom > 1)
            {
                canvas.LayoutTransform = new ScaleTransform(zoom, zoom, mousePos.X, mousePos.Y); // transform Canvas size from mouse position
            }
            else
            {                
                canvas.LayoutTransform = new ScaleTransform(zoom, zoom); // transform Canvas size
            }
            e.Handled = true;
        }

    }
}
