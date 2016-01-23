using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOscope.SampleReceiving
{
    /* Receives assembled samples. An implementation should be provided a SampleFrameReceiver
    callback to be called when a sample frame has been assembled. */
    public interface SampleFrameAssembler
    {
        void SampleAssembled(ushort nextSample);

        /* Should set the number of expected samples and should cause the buffer to reset. */
        void SetNumSamplesExpected(uint numSamplesExpected);
    }
}
