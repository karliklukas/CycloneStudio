using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CycloneStudio.structs
{
    class Module
    {
        private string name;
        private string id;
        private string path;
        private List<Pin> inPins;
        private List<Pin> outPins;

        public string Name { get => name; set => name = value; }
        public string Id { get => id; set => id = value; }
        public string Path { get => path; set => path = value; }
        public List<Pin> InPins { get => inPins; set => inPins = value; }
        public List<Pin> OutPins { get => outPins; set => outPins = value; }

        public Module()
        {
            InPins = new List<Pin>();
            OutPins = new List<Pin>();
        }
    }
}
