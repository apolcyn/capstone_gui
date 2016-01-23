using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOscope.SampleReceiving
{
    /* Receives assembled frames of samples. */
    public interface SampleFrameReceiver
    {
        void FrameAssembled(ushort[] samples, uint numSamples);

        void SetScanStartIndex(uint scanStartIndex);

        void SetScanLength(uint scanLength);

        void SetTriggerLevel(uint triggerLevel);
    }
}
