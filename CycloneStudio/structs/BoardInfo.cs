using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CycloneStudio.structs
{
    [Serializable]
    class BoardInfo
    {
        private string boardName;
        private int marginLeft;
        private int marginTop;

        public string BoardName { get => boardName; set => boardName = value; }
        public int MarginLeft { get => marginLeft; set => marginLeft = value; }
        public int MarginTop { get => marginTop; set => marginTop = value; }
    }
}
