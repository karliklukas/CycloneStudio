using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AppSpace.structs
{
    class Directory
    {
        public Directory() {

        }

        public void ReadDirectory()
        {
            DirectoryInfo d = new DirectoryInfo(@"../../components");
            DirectoryInfo[] subdir = d.GetDirectories();           
           

            string str = "";
            foreach (DirectoryInfo sub in subdir)
            {
                Console.WriteLine(sub.Name);
                FileInfo[] Files = sub.GetFiles("*.v");
                foreach (FileInfo file in Files)
                {
                    str = str + ", " + file.Name;
                    Console.WriteLine("a " + file.Name);
                }                
            }
            
        }

    }
}
