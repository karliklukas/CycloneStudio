using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace AppSpace.structs
{
    class PolylineTagData
    {
        private int id;        
        public int Id { get => id; set => id = value; }

        private Rectangle recfrom;
        public Rectangle RecFrom
        {
            get { return recfrom; }
            set { recfrom = value; }
        }

        private Rectangle recto;
        public Rectangle RecTo
        {
            get { return recto; }
            set { recto = value; }
        }

        private Polyline polyline;
        public Polyline Polyline
        {
            get { return polyline; }
            set { polyline = value; }
        }

    }
}
