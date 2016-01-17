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
        private Dispatcher uiThreadDispatcher { get; set; }
        private int triggerRelativeDisplayStartIndex;

        public SampleFrameDisplayerImpl(OscopeWindowClient scopeLineDrawer
            , uint numSamplesToDisplay, uint spacing, Dispatcher uiThreadDispatcher)
        {
            if(scopeLineDrawer == null)
            {
                throw new ArgumentNullException();
            }
            this.scopeLineDrawer = scopeLineDrawer;
            this.numSamplesToDisplay = numSamplesToDisplay;
            this.spacing = spacing;
            this.uiThreadDispatcher = uiThreadDispatcher;
        }

        public delegate void DisplayDeleg(uint triggerIndex, uint totalSamplesInFrame, ushort[] samples);

        public void DisplaySampleFrame(uint triggerIndex, uint totalSamplesInFrame, ushort[] samples)
        {
            uiThreadDispatcher.BeginInvoke(new DisplayDeleg(DisplayTriggeredSampleFrame)
                , new object[] { triggerIndex, totalSamplesInFrame, samples });
        }

        public void DisplayTriggeredSampleFrame(uint triggerIndex, uint totalSamplesInFrame, ushort[] samples)
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

            scopeLineDrawer.clearScopeCanvas();

            if((this.numSamplesToDisplay - 1) * this.spacing + 1 < scopeLineDrawer.getCanvasWidth())
            {
                throw new Exception("number of samples to display with current spacing"
                    + " isn't enough to cover width of scope canvas");
            }

            int prevX = 0;
            int prevY = samples[start];

            for(uint i = start + 1; i < start + numSamplesToDisplay; i++)
            {
                int curX = (int)(this.spacing * (i - start));
                int curY = samples[i];
                scopeLineDrawer.drawOscopeLine(prevX, prevY, curX, curY);
                if (curX > this.scopeLineDrawer.getCanvasWidth())
                {
                    break;
                }
                prevX = curX;
                prevY = curY;
            }
        }

        public void SetTriggerRelativeDispalyStartIndex(int triggerRelativeDisplayStartIndex)
        {
            this.triggerRelativeDisplayStartIndex = triggerRelativeDisplayStartIndex;
        }

        public void SetNumSamplesToDisplay(uint numSamplesToDisplay)
        {
            this.numSamplesToDisplay = numSamplesToDisplay;
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

        public int getCanvasWidth()
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

        /* Draws a line on the canvas between two points. */
        public void drawOscopeLine(int prevX, int prevY, int curX, int curY)
        {
            if(prevX > getCanvasWidth())
            {
                throw new Exception("trying to draw a line that goes way past the end of this canvas");
            }
            Line myLine = new Line();
            myLine.Stroke = System.Windows.Media.Brushes.Gold;
            myLine.X1 = prevX;
            myLine.X2 = curX;
            myLine.Y1 = prevY;
            myLine.Y2 = curY;
            myLine.HorizontalAlignment = HorizontalAlignment.Left;
            myLine.VerticalAlignment = VerticalAlignment.Center;
            myLine.StrokeThickness = 4;
            this.scopeCanvas.Children.Add(myLine);
        }
    }
}
