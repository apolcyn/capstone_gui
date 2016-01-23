using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace SimpleOscope.SampleReceiving
{
    public interface SampleFrameDisplayer
    {
        void DisplaySampleFrame(uint start, uint totalSamplesInFrame, ushort[] samples);

        void SetNumSamplesToDisplay(uint numSampleToDisplay);

        void SetSpacing(uint spacing);

        void SetTriggerRelativeDispalyStartIndex(int triggerRelativeDisplayStartIndex);
    }

    public interface OscopeWindowClient
    {
        void drawLinesOnOscope(List<LineCoordinates> lines);

        int getCanvasWidth();

        void clearScopeCanvas();
    }

    public struct LineCoordinates
    {
        public int x1 { get; }
        public int x2 { get; }
        public int y1 { get; }
        public int y2 { get; }

        public LineCoordinates(int x1, int y1, int x2, int y2)
        {
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
        }
    }
}
