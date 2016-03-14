using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SimpleOscope
{
    public delegate void SampleFrameHook(ushort[] sampleFrame, int frameLen, int triggerPoint);

    public class DataDumper
    {
        private ushort[] mostRecentFrame = new ushort[10000];
        private int mostRecentFrameSize;
        private string fileName;
        public const int MAX_DUMPS = 5;
        private int curDump;
        private System.IO.StreamWriter file;
        public const string DUMPS_DIRNAME = "./frame_dumps/";
        private LinearInterpolator linearInterpolator;

        public DataDumper(string fileName)
        {
            this.fileName = fileName;
            this.curDump = 0;
            Directory.CreateDirectory(DUMPS_DIRNAME);
            this.file = new System.IO.StreamWriter(DUMPS_DIRNAME + fileName, false);
            this.linearInterpolator = linearInterpolator;
        }

        public void dumpNewFrame(ushort[] frame, int frameLen, int triggerPoint)
        {
            setFrameBuffer(frame, frameLen);
            dumpFromTriggerPoint(triggerPoint);
        }

        public void setFrameBuffer(ushort[] frame, int frameSize)
        {
            this.mostRecentFrame = frame;
            this.mostRecentFrameSize = frameSize;
        }

        public void dumpFromTriggerPoint(int triggerPoint)
        {
            if (curDump < MAX_DUMPS) {
                StringBuilder strBuilder = new StringBuilder();
                for (int i = triggerPoint; i < mostRecentFrameSize; i++)
                {
                    strBuilder.Append(this.mostRecentFrame[i]);
                    strBuilder.Append(",");
                }
                strBuilder.Append("\n\r\n\r");
                file.WriteLine(strBuilder.ToString());
                file.Flush();
                curDump++;
            }
            else
            {
                file.Close();
            }
        }
    }
}
