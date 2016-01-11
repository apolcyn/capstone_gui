using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOscope.SampleReceiving
{
    public interface SampleFrameReceiver
    {
        void FrameAssembled(ushort[] samples, uint numSamples);
    }
}
