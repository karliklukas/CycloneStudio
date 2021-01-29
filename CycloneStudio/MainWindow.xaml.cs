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
        private int clickCount;

        private Line line = new Line();
        Polyline polyline;
        private Point p;
        private bool isdragging = false;
        private Rectangle rectFrom, rectTo;

        private List<Rectangle> inPins;
        private List<Rectangle> outPins;

        public MainWindow()
        {
            InitializeComponent();
            generateMenuItems();
            clickCount = 0;

            inPins = new List<Rectangle>();
            outPins = new List<Rectangle>();

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
                Margin = new Thickness(50,50, 0, 0),
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
                Stroke = Brushes.Gray,
                StrokeThickness = 0
            };

            Rectangle[] pole = new Rectangle[2];
            pole[0] = vedHorni;
            pole[1] = vedDolni;

            Rectangle hlavni = new Rectangle
            {
                Margin = new Thickness(20,20, 0, 0),
                Width = 50 * 2,
                Height = 50 * 2,
                RadiusX = 5,
                RadiusY = 5,
                Fill = Brushes.Black,
                Stroke = Brushes.Gray,
                StrokeThickness = 0,
                Tag = pole
            };

            Canvas.SetLeft(hlavni, 0);
            Canvas.SetTop(hlavni, 0);
            Canvas.SetLeft(vedDolni, 0); 
            Canvas.SetTop(vedDolni, 0);
            Canvas.SetTop(vedHorni, 0);
            Canvas.SetLeft(vedHorni, 0);
           

            Panel.SetZIndex(hlavni, 1);
            Panel.SetZIndex(vedHorni, 1);
            Panel.SetZIndex(vedDolni, 1);

            hlavni.MouseEnter += EventMouseOver;
            vedHorni.MouseEnter += EventMouseOver;
            vedDolni.MouseEnter += EventMouseOver;
            hlavni.MouseLeave += EventMouseLeave;
            vedHorni.MouseLeave += EventMouseLeave;
            vedDolni.MouseLeave += EventMouseLeave;

            hlavni.MouseLeftButtonDown += rect_MouseLeftButtonDown;
            hlavni.MouseLeftButtonUp += rect_MouseLeftButtonUp;
            hlavni.MouseMove += rect_MouseMove;

            //vedHorni.MouseLeftButtonDown += ved_MouseLeftButtonDown;
            vedHorni.MouseLeftButtonUp += ved_MouseLeftButtonUp;

            vedDolni.MouseLeftButtonDown += ved_MouseLeftButtonDown;

            vedDolni.MouseLeftButtonUp += ved_MouseLeftButtonUp;
            vedHorni.MouseLeftButtonDown += ved_MouseLeftButtonDown;

            vedHorni.Tag = new List<PolylineTagData>();
            vedDolni.Tag = new List<PolylineTagData>();

            canvas.Children.Add(hlavni);
            canvas.Children.Add(vedHorni);
            canvas.Children.Add(vedDolni);
            //canvas.Children.Add(popis);

            canvas.MouseMove += canvas_MouseMove;
            canvas.MouseLeftButtonUp += canvas_MouseUp;
            inPins.Add(vedHorni);
            outPins.Add(vedDolni);
            
        }

        private void rect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isLineDrag) return;
            Rectangle el = (Rectangle)sender;
            _isRectDragInProg = true;
            //el.CaptureMouse();            
            Rectangle[] pol = (Rectangle[])el.Tag;
            Panel.SetZIndex(el, 100);
            Panel.SetZIndex(pol[0], 101);
            Panel.SetZIndex(pol[1], 102);
        }

        private void rect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isLineDrag) return;
            
            Rectangle el = (Rectangle)sender;
            _isRectDragInProg = false;
           
            Rectangle[] pol = (Rectangle[])el.Tag;
            Panel.SetZIndex(el, 1);
            Panel.SetZIndex(pol[0], 1);
            Panel.SetZIndex(pol[1], 1);

            foreach (Rectangle r in pol)
            {
                foreach (PolylineTagData p in (List<PolylineTagData>)r.Tag)
                {
                    //PolylineTagData p = (PolylineTagData)r?.Tag;
                    if (p != null)
                    {
                        CalculateNewCoordinates(p.RecFrom, out double xs, out double ys);
                        CalculateNewCoordinates(p.RecTo, out double xe, out double ye);

                        p.Polyline = GenerateLine(xs, ys, xe, ye);
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
            Console.WriteLine("start");
            Rectangle el = (Rectangle)sender;
            rectFrom = el;
            _isLineDrag = true;
            clickCount = 0;

            line = new Line
            {
                Visibility = Visibility.Visible,
                StrokeThickness = 2,
                Stroke = Brushes.Black
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

            isdragging = true;
        }

        private void rect_MouseMove(object sender, MouseEventArgs e)
        {
            Rectangle el = (Rectangle)sender;
            if (!_isRectDragInProg) return;

            // get the position of the mouse relative to the Canvas
            var mousePos = e.GetPosition(canvas);

            // center the rect on the mouse
            double left = mousePos.X - (el.ActualWidth / 2);
            double top = mousePos.Y - (el.ActualHeight / 2);
            Canvas.SetLeft(el, left);
            Canvas.SetTop(el, top);
           
            foreach (Rectangle r in (Rectangle[])el.Tag)
            {
                if (r != null)
                {
                    Canvas.SetLeft(r, left);
                    Canvas.SetTop(r, top);
                    foreach (PolylineTagData p in (List<PolylineTagData>)r.Tag)
                    {
                        if (p != null)
                        {
                            canvas.Children.Remove(p.Polyline);
                        }
                    }
                    //PolylineTagData p = (PolylineTagData)r?.Tag;
                    
                }
            }  
        }

        private void EventMouseOver(object sender, MouseEventArgs e)
        {
            Rectangle el = (Rectangle)sender;
            el.StrokeThickness = 3;           
            
        }

        private void EventMouseLeave(object sender, MouseEventArgs e)
        {
            Rectangle el = (Rectangle)sender;
            el.StrokeThickness = 0;
           
        }

        private void menuItemClick(object sender, RoutedEventArgs e)
        {
            GenerujBlok();
        }

        private void generateMenuItems()
        {
            DirectoryInfo d = new DirectoryInfo(@"../../components");
            DirectoryInfo[] subdir = d.GetDirectories();
            
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
                    newSubMenuItem.Header = file.Name;
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
            
            _isLineDrag = false;            
            GenerateLine(line.X1, line.Y1, line.X2, line.Y2);
            PolylineTagData data = new PolylineTagData
            {
                Id = 1,
                RecFrom = rectFrom,
                RecTo = rectTo,
                Polyline = polyline
            };
            List<PolylineTagData> list = (List<PolylineTagData>)rectFrom.Tag;
            list.Add(data);
            list = (List<PolylineTagData>)rectTo.Tag;
            list.Add(data);

            canvas.Children.Remove(line);
            line = null;
            rectFrom = null;
            rectTo = null;
            Console.WriteLine("asd");
        }

        /*private void ved_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Line el = (Line)sender;
            isdragging = false;
            el.ReleaseMouseCapture();
            Console.WriteLine("up");
        }*/

        private void ved_MouseMove(object sender, MouseEventArgs e)
        {
            Line el = (Line)sender;
            if (!isdragging) return;

            if (isdragging == true && e.LeftButton == MouseButtonState.Pressed)
            {
                Console.WriteLine("asd");
            }

                // get the position of the mouse relative to the Canvas
                var mousePos = e.GetPosition(canvas);

            // center the rect on the mouse
            double left = mousePos.X - (el.ActualWidth / 2);
            double top = mousePos.Y - (el.ActualHeight / 2);
            el.X2 = mousePos.X;
            el.Y2 = mousePos.Y;           

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
            line = null;
            polyline = null;
            rectFrom = null;
        }

        private Polyline GenerateLine(double startX, double startY, double endX, double endY)
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
                StrokeThickness = 2
            };
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
            // Set Polyline.Points properties  
            polyline.Points = polygonPoints;
            // Add polyline to the page  
            canvas.Children.Add(polyline);

            return polyline;

        }

        private void CalculateNewCoordinates(Rectangle rectangle, out double xOut, out double yOut)
        {
            double x = Canvas.GetLeft(rectangle);
            double y = Canvas.GetTop(rectangle);
            xOut = x + rectangle.Margin.Left + (rectangle.ActualWidth / 2);
            yOut = y + rectangle.Margin.Top + (rectangle.ActualHeight / 2);
        }

    }
}
