using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOscope.SampleReceiving.Impl.SampleFrameAssembly
{
    public class SampleFrameAssemblerImpl : SampleFrameAssembler
    {
        private SampleFrameReceiver sampleReceiver;
        private uint curBufIndex;
        public const int SAMPLES_BUF_SIZE = 10000;
        private ushort[] samplesBuf = new ushort[SAMPLES_BUF_SIZE];
        private uint numSamplesExpected;

        public SampleFrameAssemblerImpl(SampleFrameReceiver sampleReceiver)
        {
            this.sampleReceiver = sampleReceiver;
        }

        public void SetNumSamplesExpected(uint numSamplesExpected)
        {
            if(numSamplesExpected == 0 || numSamplesExpected > SAMPLES_BUF_SIZE)
            {
                throw new ArgumentException();
            }
            this.numSamplesExpected = numSamplesExpected;
        }

        public void SampleAssembled(ushort nextSample)
        {
            if(curBufIndex >= numSamplesExpected)
            {
                sampleReceiver.FrameAssembled(samplesBuf, numSamplesExpected);
                curBufIndex = 0;
            }

            samplesBuf[curBufIndex++] = nextSample;

            if (curBufIndex >= numSamplesExpected)
            {
                sampleReceiver.FrameAssembled(samplesBuf, numSamplesExpected);
                curBufIndex = 0;
            }
        }
    }
}
