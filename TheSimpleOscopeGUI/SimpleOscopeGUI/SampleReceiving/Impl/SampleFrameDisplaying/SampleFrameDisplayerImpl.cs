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
    public class SampleFrameDisplayerImpl : SampleFrameDisplayer, SampleFrameReceiver
    {
        private uint numSamplesToDisplay { get; set; }
        private uint sampleSpacing { get; set; }
        private OscopeWindowClient scopeLineDrawer { get; set; }
        private int triggerRelativeDisplayStartIndex;
        private int oscopeCanvasWidth;

        private int triggerLevel;
        private uint triggerScanLength;
        private uint triggerScanStartIndex;
        private OscopeHorizontalResolutionConfiguration 

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

        /* Searches a given samples frame from a configured start index
  * and for a configured scan length, for sample index that sets off the trigger. */
        public void FrameAssembled(ushort[] samples, uint numSamples)
        {
            if (numSamples == 0 || numSamples > samples.Length)
            {
                throw new ArgumentException("num samples length is invalid: got " + numSamples);
            }
            if (this.triggerScanLength + this.triggerScanStartIndex > numSamples)
            {
                throw new Exception("scan length and scan start index > num samples."
                    + ". scan length: " + this.triggerScanLength
                    + ". scan start index: " + this.triggerScanStartIndex
                    + ". num samples: " + numSamples);
            }

            uint lastSample = UInt16.MaxValue;
            int start = -1;

            for (uint i = this.triggerScanStartIndex; i < this.triggerScanStartIndex + this.triggerScanLength; i++)
            {
                if (lastSample < triggerLevel && samples[i] >= triggerLevel)
                {
                    start = (int)i;
                    break;
                }
                lastSample = samples[i];
            }

            if (start >= 0)
            {
                DisplaySampleFrame((uint)start, numSamples, samples);
            }
        }

        public void SetScanLength(uint triggerScanLength)
        {
            if (triggerScanLength < 2)
            {
                throw new ArgumentException();
            }
            this.triggerScanLength = triggerScanLength;
        }

        public void SetScanStartIndex(uint triggerScanStartIndex)
        {
            this.triggerScanStartIndex = triggerScanStartIndex;
        }

        public void SetTriggerLevel(int triggerLevel)
        {
            this.triggerLevel = triggerLevel;
        }
    }
}
