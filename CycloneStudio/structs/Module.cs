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
        private bool customPin;
        private HashSet<string> modulesUsedInBlock;
        private HashSet<string> modulesPathUsedInBlock;
        private List<Pin> inPins;
        private List<Pin> outPins;
        private List<BoardInfo> boardInfo;

        public string Name { get => name; set => name = value; }
        public string Id { get => id; set => id = value; }
        public string Path { get => path; set => path = value; }
        public List<Pin> InPins { get => inPins; set => inPins = value; }
        public List<Pin> OutPins { get => outPins; set => outPins = value; }
        public double MarginLeft { get => marginLeft; set => marginLeft = value; }
        public double MarginTop { get => marginTop; set => marginTop = value; }
        public bool CustomPin { get => customPin; set => customPin = value; }
        public HashSet<string> ModulesUsedInBlock { get => modulesUsedInBlock; set => modulesUsedInBlock = value; }
        public HashSet<string> ModulesPathUsedInBlock { get => modulesPathUsedInBlock; set => modulesPathUsedInBlock = value; }
        internal List<BoardInfo> BoardInfo { get => boardInfo; set => boardInfo = value; }

        public Module()
        {
            InPins = new List<Pin>();
            OutPins = new List<Pin>();
            modulesPathUsedInBlock = new HashSet<string>();
            modulesUsedInBlock = new HashSet<string>();
        }
    }
}
