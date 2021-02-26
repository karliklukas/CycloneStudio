using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace CycloneStudio.structs
{
    [Serializable]
    class PolylineTagData
    {        
        private string wirename;        
        public string Wirename { get => wirename; set => wirename = value; }

        private string moduleInId;
        public string ModuleInId
        {
            get { return moduleInId; }
            set { moduleInId = value; }
        }

        private string moduleOutId;
        public string ModuleOutId
        {
            get { return moduleOutId; }
            set { moduleOutId = value; }
        }

        private string pinInName;
        public string PinInName
        {
            get { return pinInName; }
            set { pinInName = value; }
        }

        private string pinOutName;
        public string PinOutName
        {
            get { return pinOutName; }
            set { pinOutName = value; }
        }

        [NonSerialized]
        private Rectangle recpinin;
        public Rectangle RecPinIn
        {
            get { return recpinin; }
            set
            {
                recpinin = value;
                Pin pin = recpinin.Tag as Pin;
                pinInName = pin.Name;
                moduleInId = pin.ModuleId;
            }
        }

        [NonSerialized]
        private Rectangle recpinout;
        public Rectangle RecPinOut
        {
            get { return recpinout; }
            set
            {
                recpinout = value;
                Pin pin = recpinout.Tag as Pin;
                pinOutName = pin.Name;
                moduleOutId = pin.ModuleId;
            }
        }

        [NonSerialized]
        private Path polyline;
        public Path Polyline
        {
            get { return polyline; }
            set { polyline = value; }
        }

    }
}
