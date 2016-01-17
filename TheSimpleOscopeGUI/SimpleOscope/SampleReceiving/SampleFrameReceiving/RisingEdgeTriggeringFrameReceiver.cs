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
        private ushort triggerLevel;

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

            uint lastSample = UInt16.MaxValue;
            int start = -1, numTriggers = 0;

            for(int i = 0; i < numSamples; i++)
            {
                if(lastSample < triggerLevel && samples[i] >= triggerLevel && numTriggers++ == 1)
                {
                    start = i;
                    break;
                }
                lastSample = samples[i];
            }

            if(start >= 0)
            {
                displayer.DisplaySampleFrame((uint)start, numSamples, samples);
            }
        }

        public void SetTriggerLevel(ushort triggerLevel)
        {
            this.triggerLevel = triggerLevel;
        }
    }
}
