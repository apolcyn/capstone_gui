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
        private uint sampleSpacing { get; set; }
        private OscopeWindowClient scopeLineDrawer { get; set; }
        private int triggerRelativeDisplayStartIndex;
        private int oscopeCanvasWidth;

        public SampleFrameDisplayerImpl(OscopeWindowClient scopeLineDrawer
            , uint numSamplesToDisplay, uint sampleSpacing)
        {
            if(scopeLineDrawer == null)
            {
                throw new ArgumentNullException();
            }
            this.scopeLineDrawer = scopeLineDrawer;
            this.numSamplesToDisplay = numSamplesToDisplay;
            this.sampleSpacing = sampleSpacing;
        }

        public SampleFrameDisplayerImpl(OscopeWindowClient scopeLineDrawer
            , MainWindow mainWindow)
        {
            this.scopeLineDrawer = scopeLineDrawer;
            mainWindow.NumSamplesToDisplayChangedEvent += numSamplesToDisplayChanged;
            mainWindow.SampleSpacingChangedEvent += sampleSpacingChanged;
            mainWindow.TriggerRelativeDisplayStartChangedEvent += triggerRelativeDisplayStartChanged;
            mainWindow.OscopeWidthChangedEvent += oscopeWidthChanged;
        }

        private void oscopeWidthChanged(object sender, OscopeWidthChangedEventArgs args)
        {
            oscopeCanvasWidth = args.newWidth;
        }

        private void numSamplesToDisplayChanged(object sender, NumSamplesToDisplayChangedEventArgs args)
        {
            this.numSamplesToDisplay = args.numSamples;
        }

        private void sampleSpacingChanged(object sender, SampleSpacingChangedEventArgs args)
        {
            this.sampleSpacing = args.sampleSpacing;
        }

        private void triggerRelativeDisplayStartChanged(object sender
            , TriggerRelativeDisplayStartChangedEventArgs args)
        {
            this.triggerRelativeDisplayStartIndex = args.triggerRelativeDisplayStart;
        }

        public void DisplaySampleFrame(uint triggerIndex, uint totalSamplesInFrame, ushort[] samples)
        {
            if(triggerIndex + this.triggerRelativeDisplayStartIndex < 0)
            {
                throw new ArgumentException(String.Format("trigger index of sample frame is {0} but trigger relatvie display start idnex is {1}"
                    , triggerIndex , this.triggerRelativeDisplayStartIndex));
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
                throw new Exception(String.Format("trying to display more samples than what was " 
                    + " received in frame. total samples in frame is {0} but display start is {1}."
                    , totalSamplesInFrame, start));
            }

            if((this.numSamplesToDisplay - 1) * this.sampleSpacing + 1 < oscopeCanvasWidth)
            {
                throw new Exception("number of samples to display with current spacing"
                    + " isn't enough to cover width of scope canvas");
            }

            int prevX = 0;
            int prevY = samples[start];
            List<LineCoordinates> linesToDraw = new List<LineCoordinates>();

            for(uint i = start + 1; i < start + numSamplesToDisplay; i++)
            {
                int curX = (int)(this.sampleSpacing * (i - start));
                int curY = samples[i];
                linesToDraw.Add(new LineCoordinates(prevX, prevY, curX, curY)); 
                if (curX > oscopeCanvasWidth)
                {
                    break;
                }
                prevX = curX;
                prevY = curY;
            }
            scopeLineDrawer.drawLinesOnOscope(linesToDraw);
        }

        public void SetTriggerRelativeDispalyStartIndex(int triggerRelativeDisplayStartIndex)
        {
            this.triggerRelativeDisplayStartIndex = triggerRelativeDisplayStartIndex;
        }

        public void SetNumSamplesToDisplay(uint numSamplesToDisplay)
        {
            this.numSamplesToDisplay = numSamplesToDisplay;
        }

        public void SetSpacing(uint sampleSpacing)
        {
            this.sampleSpacing = sampleSpacing;
        }
    }

    public class OscopeWindowClientImpl : OscopeWindowClient
    {
        private Canvas scopeCanvas { get; set; }
        private int oscopeWidth;

        public OscopeWindowClientImpl() { }

        public OscopeWindowClientImpl(Canvas scopeCanvas, MainWindow mainWindow, int oscopeWidth)
        {
            this.scopeCanvas = scopeCanvas;
            this.oscopeWidth = oscopeWidth;
            mainWindow.OscopeWidthChangedEvent += oscopeWidthChanged;
        }

        public void oscopeWidthChanged(object sender, OscopeWidthChangedEventArgs args)
        {
            this.oscopeWidth = args.newWidth;
        }

        /// <summary>
        ///  TODO: make this adapt to different screen sizes
        /// </summary>
        /// <returns></returns>
        public virtual int getCanvasWidth()
        {
            return this.oscopeWidth;
        }

        public void clearScopeCanvas()
        {
            while (this.scopeCanvas.Children.Count > 0)
            {
                this.scopeCanvas.Children.RemoveAt(0);
            }
        }

        /* Creates a line to draw on the oscope canvas */
        private Line createLine(int prevX, int prevY, int curX, int curY)
        {
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

        public delegate void drawLinesOnCanvasUIThread(List<LineCoordinates> lines);

        public void drawLinesOnCanvasUIThreadImpl(List<LineCoordinates> lines)
        {
            clearScopeCanvas();
            lines.ForEach(l => this.scopeCanvas.Children.Add(createLine(l.x1, l.y1, l.x2, l.y2)));
        }


        public virtual void drawLinesOnOscope(List<LineCoordinates> lines)
        {
            this.scopeCanvas.Dispatcher.BeginInvoke(new drawLinesOnCanvasUIThread(drawLinesOnCanvasUIThreadImpl), new object[] { lines });
        }
    }
}
