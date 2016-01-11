using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOscope.SampleReceiving.Impl.SampleFrameReceiving
{
    public class SampleFrameReceiverImpl : SampleFrameReceiver
    {
        public void FrameAssembled(ushort[] samples, uint numSamples)
        {
            throw new NotImplementedException();
        }

        public int getNumSamplesExpected()
        {
            throw new NotImplementedException();
        }
    }
}
