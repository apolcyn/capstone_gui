using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOscope.SampleReceiving
{
    public interface SampleFrameAssembler
    {
        void SampleAssembled(ushort nextSample);
    }
}
