using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace SimpleOscope.SampleReceiving.Impl.SampleFrameAssembly
{
    public class SampleFrameAssemblerImpl : SampleFrameAssembler
    {
        private SampleFrameReceiver sampleReceiver;
        private uint curBufIndex;
        public const int SAMPLES_BUF_SIZE = 10000;
        private ushort[] samplesBuf = new ushort[SAMPLES_BUF_SIZE];
        private uint numSamplesExpected;
        private Object numSamplesLockingObject = new object();
        private Thread currentThreadCallingSampleAssembled;

        public SampleFrameAssemblerImpl(SampleFrameReceiver sampleReceiver)
        {
            this.sampleReceiver = sampleReceiver;
        }

        public virtual void SetNumSamplesExpected(uint numSamplesExpected)
        {
            if(currentThreadCallingSampleAssembled == Thread.CurrentThread)
            {
                throw new ThreadStateException("Deadlock error from circular wait.");
            }
            lock(numSamplesLockingObject)
            {
                if (numSamplesExpected == 0 || numSamplesExpected > SAMPLES_BUF_SIZE)
                {
                    throw new ArgumentException();
                }
                this.numSamplesExpected = numSamplesExpected;
                if(this.curBufIndex > 0)
                {
                    throw new Exception("Getting this ready for an incoming frame but " + 
                        " last frame hasn't been sent out yet");
                }
            }
        }

        public virtual void SampleAssembled(ushort nextSample)
        {
            lock(numSamplesLockingObject)
            {
                currentThreadCallingSampleAssembled = Thread.CurrentThread;

                if (curBufIndex >= numSamplesExpected)
                {
                    curBufIndex = 0;
                    sampleReceiver.FrameAssembled(samplesBuf, numSamplesExpected);
                }

                samplesBuf[curBufIndex++] = nextSample;

                if (curBufIndex >= numSamplesExpected)
                {
                    curBufIndex = 0;
                    sampleReceiver.FrameAssembled(samplesBuf, numSamplesExpected);
                }

                currentThreadCallingSampleAssembled = null;
            }
        }
    }
}
