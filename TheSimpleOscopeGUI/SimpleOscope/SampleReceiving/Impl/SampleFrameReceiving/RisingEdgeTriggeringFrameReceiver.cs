using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOscope.SampleReceiving.Impl.SampleFrameReceiving
{
    public class RisingEdgeTriggeringFrameReceiver : SampleFrameReceiver
    {
        private SampleFrameDisplayer displayer;
        private uint triggerLevel;
        private uint scanLength;
        private uint scanStartIndex;

        public RisingEdgeTriggeringFrameReceiver(SampleFrameDisplayer displayer)
        {
            this.displayer = displayer;
        }

        public void FrameAssembled(ushort[] samples, uint numSamples)
        {
            if(numSamples == 0 || numSamples > samples.Length)
            {
                throw new ArgumentException();
            }
            if(this.scanLength + this.scanStartIndex > numSamples)
            {
                throw new Exception();
            }

            uint lastSample = UInt16.MaxValue;
            int start = -1;

            for(uint i = this.scanStartIndex; i < this.scanStartIndex + this.scanLength; i++)
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

        public void SetScanLength(uint scanLength)
        {
            if(scanLength < 2)
            {
                throw new ArgumentException();
            }
            this.scanLength = scanLength;
        }

        public void SetScanStartIndex(uint scanStartIndex)
        {
            this.scanStartIndex = scanStartIndex;
        }

        public void SetTriggerLevel(uint triggerLevel)
        {
            this.triggerLevel = triggerLevel;
        }
    }
}
