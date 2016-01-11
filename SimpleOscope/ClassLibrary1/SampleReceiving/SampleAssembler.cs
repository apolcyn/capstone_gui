using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOscope.SampleReceiving
{
    /* One can add received bytes to this. An implementation should provide
    a means to set a callback method to get called when a sample has been assembled. */
    public interface SampleAssembler
    {
        void AddReceivedByte(byte newByte);
    }
}
