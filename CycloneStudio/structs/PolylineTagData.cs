using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace CycloneStudio.structs
{
    class PolylineTagData
    {
        private string id;        
        public string Id { get => id; set => id = value; }

        private Rectangle recpinin;
        public Rectangle RecPinIn
        {
            get { return recpinin; }
            set { recpinin = value; }
        }

        private Rectangle recpinout;
        public Rectangle RecPinOut
        {
            get { return recpinout; }
            set { recpinout = value; }
        }

        private Polyline polyline;
        public Polyline Polyline
        {
            get { return polyline; }
            set { polyline = value; }
        }

    }
}
