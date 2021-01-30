using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace AppSpace.structs
{
    enum Types
    {
        IN,
        OUT        
    }

    class Pin
    {
        private Types type;        
        private string name;
        private bool connected;
        private string name_wire;
        private bool hidden;
        private List<PolylineTagData> activeConnections;
        private Rectangle rectangle;

        public Types Type { get => type; set => type = value; }
        public string Name { get => name; set => name = value; }
        public bool Connected { get => connected; set => connected = value; }
        public string Name_wire { get => name_wire; set => name_wire = value; }
        public bool Hidden { get => hidden; set => hidden = value; }
        public List<PolylineTagData> ActiveConnections { get => activeConnections; set => activeConnections = value; }
        public Rectangle Rectangle { get => rectangle; set => rectangle = value; }

        public Pin()
        {
            activeConnections = new List<PolylineTagData>();
        }
    }
}
