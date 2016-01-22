using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SimpleOscope.SampleReceiving.Impl.SampleFrameDisplaying
{
    public class SampleFrameDisplayerImpl : SampleFrameDisplayer
    {
        private uint numSamplesToDisplay { get; set; }
        private uint spacing { get; set; }
        private OscopeWindowClient scopeLineDrawer { get; set; }
        private int triggerRelativeDisplayStartIndex;

        public SampleFrameDisplayerImpl(OscopeWindowClient scopeLineDrawer
            , uint numSamplesToDisplay, uint spacing)
        {
            if(scopeLineDrawer == null)
            {
                throw new ArgumentNullException();
            }
            this.scopeLineDrawer = scopeLineDrawer;
            this.numSamplesToDisplay = numSamplesToDisplay;
            this.spacing = spacing;
        }

        public void DisplaySampleFrame(uint triggerIndex, uint totalSamplesInFrame, ushort[] samples)
        {
            if(triggerIndex + this.triggerRelativeDisplayStartIndex < 0)
            {
                throw new ArgumentException();
            }
            if(triggerIndex >= totalSamplesInFrame 
                || triggerIndex + this.triggerRelativeDisplayStartIndex >= totalSamplesInFrame)
            {
                throw new ArgumentException();
            }
            DisplaySampleFrameFromStartIndex(
                (uint)(triggerIndex + this.triggerRelativeDisplayStartIndex),
                totalSamplesInFrame,
                samples);
        }

        public void DisplaySampleFrameFromStartIndex(uint start, uint totalSamplesInFrame, ushort[] samples)
        {
            if(totalSamplesInFrame - start < this.numSamplesToDisplay)
            {
                throw new Exception("trying to display more samples than what was received in frame");
            }

            if((this.numSamplesToDisplay - 1) * this.spacing + 1 < scopeLineDrawer.getCanvasWidth())
            {
                throw new Exception("number of samples to display with current spacing"
                    + " isn't enough to cover width of scope canvas");
            }

            int prevX = 0;
            int prevY = samples[start];
            List<Line> linesToDraw = new List<Line>();

            for(uint i = start + 1; i < start + numSamplesToDisplay; i++)
            {
                int curX = (int)(this.spacing * (i - start));
                int curY = samples[i];
                linesToDraw.Add(createLine(prevX, prevY, curX, curY));
                if (curX > this.scopeLineDrawer.getCanvasWidth())
                {
                    break;
                }
                prevX = curX;
                prevY = curY;
            }
            scopeLineDrawer.drawLinesOnOscope(linesToDraw);
        }

        /* Creates a line to draw on the oscope canvas */
        public Line createLine(int prevX, int prevY, int curX, int curY)
        {
            if (prevX > scopeLineDrawer.getCanvasWidth())
            {
                throw new ArgumentException("trying to draw a line that goes way past the end of this canvas");
            }
            else if(curX - prevX != spacing)
            {
                throw new ArgumentException("spacing on these lines is bad");
            }
            Line line = new Line();
            line.Stroke = System.Windows.Media.Brushes.Gold;
            line.X1 = prevX;
            line.X2 = curX;
            line.Y1 = prevY;
            line.Y2 = curY;
            line.HorizontalAlignment = HorizontalAlignment.Left;
            line.VerticalAlignment = VerticalAlignment.Center;
            line.StrokeThickness = 4;
            return line;
        }

        public void SetTriggerRelativeDispalyStartIndex(int triggerRelativeDisplayStartIndex)
        {
            this.triggerRelativeDisplayStartIndex = triggerRelativeDisplayStartIndex;
        }

        public void SetNumSamplesToDisplay(uint numSamplesToDisplay)
        {
            this.numSamplesToDisplay = numSamplesToDisplay;
        }

        public void SetSpacing(uint spacing)
        {
            this.spacing = spacing;
        }
    }

    public class OscopeWindowClientImpl : OscopeWindowClient
    {
        private Canvas scopeCanvas { get; set; }

        public OscopeWindowClientImpl() { }

        public OscopeWindowClientImpl(Canvas scopeCanvas)
        {
            this.scopeCanvas = scopeCanvas;
        }

        public virtual int getCanvasWidth()
        {
            return (int)this.scopeCanvas.Width;
        }

        public void clearScopeCanvas()
        {
            while (this.scopeCanvas.Children.Count > 0)
            {
                this.scopeCanvas.Children.RemoveAt(0);
            }
        }

        public virtual void drawLinesOnOscope(List<Line> lines)
        {
            clearScopeCanvas();
            lines.ForEach(x => this.scopeCanvas.Children.Add(x));
        }
    }
}
