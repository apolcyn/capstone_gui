using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOscope.SampleReceiving.Impl.SampleAssembly
{
    public class HighByteFirstSampleAssemblerImpl : SampleAssembler
    {
        private SampleFrameAssembler sampleFrameAssembler;
        private ushort curSample;
        private int curByte;

        public HighByteFirstSampleAssemblerImpl(SampleFrameAssembler sampleFrameAssembler)
        {
            this.sampleFrameAssembler = sampleFrameAssembler;
        }

        public void AddReceivedByte(byte newByte)
        {
            curSample = (ushort)(newByte + (curSample << (8 * curByte++)));

            if(curByte == 2)
            {
                sampleFrameAssembler.SampleAssembled((ushort)(curSample / 4096.0 * 300));
                curByte = 0;
                curSample = 0;
            }
        }
    }
}
