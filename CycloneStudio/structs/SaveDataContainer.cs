using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CycloneStudio.structs
{
    [Serializable]
    class SaveDataContainer
    {
        private List<Module> modules;
        private int moduleId;
        private int wireId;
        private string board;

        public SaveDataContainer()
        {
            modules = new List<Module>();
        }

        public int ModuleId { get => moduleId; set => moduleId = value; }
        public int WireId { get => wireId; set => wireId = value; }
        public string Board { get => board; set => board = value; }
        internal List<Module> Modules { get => modules; set => modules = value; }

        
    }
}
