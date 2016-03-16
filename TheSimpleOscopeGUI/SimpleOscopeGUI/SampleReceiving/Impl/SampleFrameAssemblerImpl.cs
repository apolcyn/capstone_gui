using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace SimpleOscope.SampleReceiving.Impl
{
    public class NewFrameWithNewMinMaxEventArgs : EventArgs
    {
        public int min;
        public int max;
        public NewFrameWithNewMinMaxEventArgs(int min, int max)
        {
            this.min = min;
            this.max = max;
        }
    }

    public class SampleFrameAssemblerImpl : SampleFrameAssembler
    {
        public event EventHandler<NewFrameWithNewMinMaxEventArgs> NewFrameWithNewMinMaxEvent;

        private SampleFrameReceiver sampleReceiver;
        private uint curBufIndex;
        public const int SAMPLES_BUF_SIZE = 10000;
        private ushort[] samplesBuf = new ushort[SAMPLES_BUF_SIZE];
        private uint numSamplesExpected;
        private Object numSamplesLockingObject = new object();
        private Thread currentThreadCallingSampleAssembled;

        private int minInCurrentFrame = int.MaxValue;
        private int maxInCurrentFrame = int.MinValue;

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
                    fireNewMinMaxFrameEventAndReset();
                }

                samplesBuf[curBufIndex++] = nextSample;
                minInCurrentFrame = Math.Min(minInCurrentFrame, nextSample);
                maxInCurrentFrame = Math.Max(maxInCurrentFrame, nextSample);

                if (curBufIndex >= numSamplesExpected)
                {
                    curBufIndex = 0;
                    sampleReceiver.FrameAssembled(samplesBuf, numSamplesExpected);
                    fireNewMinMaxFrameEventAndReset();
                }

                currentThreadCallingSampleAssembled = null;
            }
        }

        private void fireNewMinMaxFrameEventAndReset()
        {
            if(NewFrameWithNewMinMaxEvent != null)
            {
                NewFrameWithNewMinMaxEvent(this
                    , new NewFrameWithNewMinMaxEventArgs(this.minInCurrentFrame
                    , this.maxInCurrentFrame));
            }
            this.maxInCurrentFrame = int.MinValue;
            this.minInCurrentFrame = int.MaxValue;
        }
    }
}
