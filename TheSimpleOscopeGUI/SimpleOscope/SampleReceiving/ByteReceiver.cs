using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOscope.SampleReceiving
{
    /* FSM that parses frames from PSOC and figures out which bytes belong to samples. 
    An implementation should be provided with a SampleAssembler as a callback for when
    sample bytes are received. */
    interface ByteReceiver
    {
        void byteReceived(byte newByte);
    }
}
