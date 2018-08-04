using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warlog_Launcher
{
    public class Warlog_Launcher_Model
    {
        public string filename { get; set; }
        public string sha1 { get; set; }

        public Warlog_Launcher_Model() { }

        public Warlog_Launcher_Model(string filename, string md5)
        {
            this.filename = filename;
            this.sha1 = sha1;
        }
    }

}
