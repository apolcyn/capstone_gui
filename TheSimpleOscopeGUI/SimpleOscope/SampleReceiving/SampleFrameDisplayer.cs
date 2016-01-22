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
        void drawLinesOnOscope(List<Line> lines);

        int getCanvasWidth();

        void clearScopeCanvas();
    }
}
