using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOscope
{
    public class PSOCMessageLogger
    {
        System.IO.StreamWriter fileStream;
        string fullPath;

        public PSOCMessageLogger(string fileName)
        {
            this.fullPath = System.IO.Path.GetFullPath(fileName);
            this.fileStream = new System.IO.StreamWriter(fileName, false);
        }

        public void appendMessage(string message)
        {
            this.fileStream.WriteLine(message);
            this.fileStream.Flush();
        }
    }
}
