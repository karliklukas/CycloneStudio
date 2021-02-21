using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace CycloneStudio.structs
{
    [Serializable]
    enum Types
    {
        IN,
        OUT        
    }
    [Serializable]
    class Pin
    {
        private Types type;        
        private string name;
        private bool connected;
        private string name_wire;
        private bool hidden;
        private List<PolylineTagData> activeConnections;
        private string moduleId;
        [NonSerialized]
        private Rectangle rectangle;

        public Types Type { get => type; set => type = value; }
        public string Name { get => name; set => name = value; }
        public bool Connected { get => connected; set => connected = value; }
        public string Name_wire { get => name_wire; set => name_wire = value; }
        public bool Hidden { get => hidden; set => hidden = value; }
        public List<PolylineTagData> ActiveConnections { get => activeConnections; set => activeConnections = value; }
        public string ModuleId { get => moduleId; set => moduleId = value; }
        public Rectangle Rectangle { get => rectangle; set => rectangle = value; }

        public Pin()
        {
            activeConnections = new List<PolylineTagData>();
            name_wire = "";
        }

        public bool CompareConnections(Rectangle endPin)
        {
            foreach (PolylineTagData data in ActiveConnections)
            {
                if (data.RecPinOut == endPin || data.RecPinIn == endPin)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
