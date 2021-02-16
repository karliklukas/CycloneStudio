using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CycloneStudio.structs
{
    [Serializable]
    class Module
    {
        private string name;
        private string id;
        private string path;
        private double marginLeft;
        private double marginTop;
        private List<Pin> inPins;
        private List<Pin> outPins;

        public string Name { get => name; set => name = value; }
        public string Id { get => id; set => id = value; }
        public string Path { get => path; set => path = value; }
        public List<Pin> InPins { get => inPins; set => inPins = value; }
        public List<Pin> OutPins { get => outPins; set => outPins = value; }
        public double MarginLeft { get => marginLeft; set => marginLeft = value; }
        public double MarginTop { get => marginTop; set => marginTop = value; }

        public Module()
        {
            InPins = new List<Pin>();
            OutPins = new List<Pin>();
        }
    }
}
