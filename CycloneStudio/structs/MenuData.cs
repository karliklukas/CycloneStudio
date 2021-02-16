using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CycloneStudio.structs
{
    [Serializable()]
    class MenuData
    {
        private string filePath;
        private string name;
        private List<string> inPins;
        private List<string> outPins;
        private List<string> hiddenPins;

        public MenuData()
        {
            inPins = new List<string>();
            outPins = new List<string>();
            hiddenPins = new List<string>();
        }

        public string FilePath { get => filePath; set => filePath = value; }
        public string Name { get => name; set => name = value; }
        public List<string> InPins { get => inPins; set => inPins = value; }
        public List<string> OutPins { get => outPins; set => outPins = value; }
        public List<string> HiddenPins { get => hiddenPins; set => hiddenPins = value; }
    }
}
