using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOscope.SampleReceiving.Impl.ByteReceiving
{
    public class ByteReceiverImpl : ByteReceiver
    {
        private enum ReceiveState { FIND_FIRST_HEADER_CHAR, FIND_SECOND_HEADER_CHAR,
        READING_NUM_SAMPLES, RECEIVING_SAMPLES};

        private ReceiveState curState;
        private uint numExpectedSamples;
        private uint numBytesReceived;
        private uint numExpectedBytes;

        private SampleAssembler sampleAssembler;
        private SampleFrameAssembler sampleFrameAssembler;

        public ByteReceiverImpl(SampleAssembler sampleAssembler
            , SampleFrameAssembler sampleFrameAssembler)
        {
            this.sampleAssembler = sampleAssembler;
            this.sampleFrameAssembler = sampleFrameAssembler;
        }

        public void byteReceived(byte newByte)
        {
            switch(curState)
            {
                case ReceiveState.FIND_FIRST_HEADER_CHAR:
                    if(newByte == '#')
                    {
                        curState = ReceiveState.FIND_SECOND_HEADER_CHAR;
                    }
                    else
                    {
                        throw new ArgumentException("invalid state");
                    }
                    break;

                case ReceiveState.FIND_SECOND_HEADER_CHAR:
                    if(newByte == 'F')
                    {
                        curState = ReceiveState.READING_NUM_SAMPLES;
                        numExpectedSamples = 0;
                    }
                    else
                    {
                        throw new ArgumentException("invalid state");
                    }
                    break;

                case ReceiveState.READING_NUM_SAMPLES:
                    if(newByte == 'D')
                    {
                        if(numExpectedSamples == 0)
                        {
                            throw new ArgumentException("can't have a frame size of zero");
                        }
                        numBytesReceived = 0;
                        numExpectedBytes = numExpectedSamples * 2;
                        sampleFrameAssembler.SetNumSamplesExpected(numExpectedSamples);
                        curState = ReceiveState.RECEIVING_SAMPLES;
                    }
                    else if(newByte >= '0' && newByte <= '9')
                    {
                        numExpectedSamples *= 10;
                        numExpectedSamples += (uint)(newByte - '0');
                    }
                    else
                    {
                        throw new ArgumentException("invalid state");
                    }
                    break;

                case ReceiveState.RECEIVING_SAMPLES:
                    sampleAssembler.AddReceivedByte(newByte);
                    if(++numBytesReceived == numExpectedBytes)
                    {
                        curState = ReceiveState.FIND_FIRST_HEADER_CHAR;
                    }
                    break;
            }
        }
    }
}
