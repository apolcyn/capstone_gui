using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOscope.SampleReceiving
{
    public interface SampleFrameDisplayer
    {
        void DisplaySampleFrame(uint start, uint totalSamplesInFrame, ushort[] samples);

        void SetNumSamplesToDisplay(uint numSampleToDisplay);
    }

    public interface OscopeWindowClient
    {
        void drawOscopeLine(int prevX, int prevY, int curX, int curY);

        int getCanvasWidth();

        void clearScopeCanvas();
    }
}
