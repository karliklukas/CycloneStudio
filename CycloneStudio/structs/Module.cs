using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppSpace.structs
{
    class Module
    {
        private string name;
        private int id;
        private List<Pin> inPins;
        private List<Pin> outPins;

        public string Name { get => name; set => name = value; }
        public int Id { get => id; set => id = value; }
        public List<Pin> InPins { get => inPins; set => inPins = value; }
        public List<Pin> OutPins { get => outPins; set => outPins = value; }

        public Module()
        {
            InPins = new List<Pin>();
            OutPins = new List<Pin>();
        }
    }
}
