using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOscope.SampleReceiving.Impl
{
    public class RisingEdgeTriggeringFrameReceiver : SampleFrameReceiver
    {
        private SampleFrameDisplayer displayer;
        private int triggerLevel;
        private uint triggerScanLength;
        private uint triggerScanStartIndex;

        public RisingEdgeTriggeringFrameReceiver(SampleFrameDisplayer displayer)
        {
            this.displayer = displayer;
        }

        public RisingEdgeTriggeringFrameReceiver(SampleFrameDisplayer displayer, MainWindow mainWindow)
        {
            this.displayer = displayer;
            mainWindow.TriggerLevelChangedEvent += triggerLevelChanged;
        }

        private void triggerLevelChanged(object sender, TriggerLevelChangedEventArgs args)
        {
            this.triggerLevel = args.triggerLevel;
        }

        /* Searches a given samples frame from a configured start index
         * and for a configured scan length, for sample index that sets off the trigger. */
        public void FrameAssembled(ushort[] samples, uint numSamples)
        {
            if(numSamples == 0 || numSamples > samples.Length)
            {
                throw new ArgumentException("num samples length is invalid: got " + numSamples);
            }
            if(this.triggerScanLength + this.triggerScanStartIndex > numSamples)
            {
                throw new Exception("scan length and scan start index > num samples." 
                    + ". scan length: " + this.triggerScanLength
                    + ". scan start index: " + this.triggerScanStartIndex
                    + ". num samples: " + numSamples);
            }

            uint lastSample = UInt16.MaxValue;
            int start = -1;

            for(uint i = this.triggerScanStartIndex; i < this.triggerScanStartIndex + this.triggerScanLength; i++)
            {
                if(lastSample < triggerLevel && samples[i] >= triggerLevel)
                {
                    start = (int)i;
                    break;
                }
                lastSample = samples[i];
            }

            if(start >= 0)
            {
                displayer.DisplaySampleFrame((uint)start, numSamples, samples);
            }
        }

        public void SetScanLength(uint triggerScanLength)
        {
            if(triggerScanLength < 2)
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
