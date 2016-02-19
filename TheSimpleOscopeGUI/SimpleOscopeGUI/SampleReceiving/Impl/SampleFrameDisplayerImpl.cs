using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;
using SimpleOscope;

namespace SimpleOscope.SampleReceiving.Impl
{
    public class SampleFrameDisplayerImpl : SampleFrameDisplayer, SampleFrameReceiver
    {
        private HorizontalResolutionConfiguration displayConfig;
        private OscopeWindowClient scopeLineDrawer { get; set; }

        private int triggerRelativeDisplayStartIndex;

        /// <summary>
        /// The horizontal position of the triggering line on the oscope screen.
        /// </summary>
        private uint triggerHorizontalPosition;
        private int triggerLevel;
        private uint triggerScanLength;
        private uint triggerScanStartIndex;

        public static SampleFrameDisplayerImpl newSampleFrameDisplayerImpl(
            OscopeWindowClient scopeLineDrawer
            , MainWindow mainWindow)
        {
            SampleFrameDisplayerImpl displayer = new SampleFrameDisplayerImpl(scopeLineDrawer);
            mainWindow.TriggerLevelChangedEvent += displayer.triggerLevelChanged;
            mainWindow.HorizonalResolutionConfigChangedEvent += displayer.horizontalResolutionConfigChanged;
            mainWindow.TriggerHorizontalPositionChangedEvent 
                += displayer.triggerHorizontalPositionChanged;
            return displayer;
        }

        public SampleFrameDisplayerImpl(OscopeWindowClient scopeLineDrawer)
        {
            this.scopeLineDrawer = scopeLineDrawer;
        }

        private uint roundDownTriggerHorizontalPosition(uint curTriggerHorizontalPosition
            , uint pixelsPerSample)
        {
            return (curTriggerHorizontalPosition / pixelsPerSample) * pixelsPerSample;
        }

        private void adjustTriggerScanning(uint samplesPerPixel, uint triggerHorizontalPosition
            , uint samplesPerFrame, uint samplesPerWindow)
        {
            if(this.triggerHorizontalPosition % samplesPerPixel != 0)
            {
                throw new ArgumentException("what are you doing");
            }
            this.triggerScanStartIndex = triggerHorizontalPosition / samplesPerPixel;
            this.triggerScanLength = samplesPerWindow;

            if(this.triggerScanStartIndex + this.triggerScanLength > samplesPerFrame)
            {
                throw new ArgumentException("bad frame size");
            }
            this.triggerRelativeDisplayStartIndex = - (int)this.triggerScanStartIndex;
        }

        private void horizontalResolutionConfigChanged(object sender
            , HorizontalResolutionConfigChangedEventArgs args)
        {
            this.displayConfig = args.config;
            if((args.config.oscopeWindowSize - 1) % (args.config.pixelSpacing + 1) != 0)
            {
                throw new ArgumentException(String.Format("scope window size and pizel spacing don't match"
                    + " up. Window width of {0}, but num pixels between each sample is {1}"
                    , args.config.oscopeWindowSize, args.config.pixelSpacing));
            }
            this.triggerHorizontalPosition = roundDownTriggerHorizontalPosition(
                this.triggerHorizontalPosition
                , args.config.pixelSpacing + 1);
            adjustTriggerScanning(args.config.pixelSpacing + 1
                , this.triggerHorizontalPosition, args.config.frameSize, args.config.numSamplesToDispaly);
            if(this.triggerScanLength < args.config.numSamplesToDispaly)
            {
                throw new ArgumentException("what are youd doing. scan length is way too small");
            }
        }

        private void triggerLevelChanged(object sender, TriggerLevelChangedEventArgs args)
        {
            this.triggerLevel = args.triggerLevel;
        }

        private void triggerHorizontalPositionChanged(object sender
            , TriggerHorizontalPositionChangedEventArgs args)
        {
            this.triggerHorizontalPosition 
                = roundDownTriggerHorizontalPosition(args.triggerHorizontalPosition
                , this.displayConfig.pixelSpacing + 1);
            adjustTriggerScanning(this.displayConfig.pixelSpacing + 1
                , this.triggerHorizontalPosition, this.displayConfig.frameSize
                , this.displayConfig.numSamplesToDispaly);
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
            if(totalSamplesInFrame - start < this.displayConfig.numSamplesToDispaly)
            {
                throw new Exception(String.Format("trying to display more samples than what was " 
                    + " received in frame. total samples in frame is {0} but display start is {1} "
                    + " and wanted to display {2} samples"
                    , totalSamplesInFrame, start, this.displayConfig.numSamplesToDispaly));
            }

            if((this.displayConfig.numSamplesToDispaly - 1) * (this.displayConfig.pixelSpacing  + 1)
                + 1 < this.displayConfig.oscopeWindowSize)
            {
                throw new Exception("number of samples to display with current spacing"
                    + " isn't enough to cover width of scope canvas");
            }

            int prevX = 0;
            int prevY = samples[start];
            List<LineCoordinates> linesToDraw = new List<LineCoordinates>();

            for(uint i = start + 1; i < start + this.displayConfig.numSamplesToDispaly; i++)
            {
                int curX = (int)((this.displayConfig.pixelSpacing + 1) * (i - start));
                int curY = samples[i];
                linesToDraw.Add(new LineCoordinates(prevX, prevY, curX, curY)); 
                if (curX == this.displayConfig.oscopeWindowSize - 1)
                {
                    break;
                }
                else if(curX >= this.displayConfig.oscopeWindowSize)
                {
                    throw new ArgumentException("what are you doing");
                }
                prevX = curX;
                prevY = curY;
            }
            scopeLineDrawer.drawLinesOnOscope(linesToDraw);
        }

        public void SetTriggerRelativeDispalyStartIndex(int triggerRelativeDisplayStartIndex)
        {
            throw new NotImplementedException();
        }

        public void SetNumSamplesToDisplay(uint numSamplesToDisplay)
        {
            throw new NotImplementedException();
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

        public void SetSpacing(uint spacing)
        {
            throw new NotImplementedException();
        }
    }
}
