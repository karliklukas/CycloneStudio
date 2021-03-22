using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace CycloneStudio.structs
{
    [Serializable]
    class ConnectionData
    {
        private string wirename;
        private string moduleInId;
        private string moduleOutId;
        private string pinInName;
        private string pinOutName;
        [NonSerialized]
        private Rectangle recpinin;
        [NonSerialized]
        private Rectangle recpinout;
        [NonSerialized]
        private Path connectionPath;

        public string Wirename { get => wirename; set => wirename = value; }

        
        public string ModuleInId
        {
            get { return moduleInId; }
            set { moduleInId = value; }
        }
        
        public string ModuleOutId
        {
            get { return moduleOutId; }
            set { moduleOutId = value; }
        }
        
        public string PinInName
        {
            get { return pinInName; }
            set { pinInName = value; }
        }
        
        public string PinOutName
        {
            get { return pinOutName; }
            set { pinOutName = value; }
        }
        
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
        
        public Path ConnectionPath
        {
            get { return connectionPath; }
            set { connectionPath = value; }
        }

    }
}
