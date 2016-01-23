using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOscope.SampleReceiving.Impl.ByteReceiving
{
    public class PsocReadyEventArgs : EventArgs { }

    public class ByteReceiverImpl : ByteReceiver
    {
        private enum ReceiveState { FIND_FIRST_HEADER_CHAR, FIND_SECOND_HEADER_CHAR,
        READING_NUM_SAMPLES, RECEIVING_SAMPLES, RECEIVING_PSOC_READY_ACK};

        private ReceiveState curState;
        private uint numExpectedSamples;
        private uint numBytesReceived;
        private uint numExpectedBytes;

        public event EventHandler<PsocReadyEventArgs> RaisePsocReadyEvent;

        private class PsocReadyAckStringIterator
        {
            const string PSOC_READY_ACK_STR = "PSOC_READY";
            int curIndex = 0;

            public void reset()
            {
                curIndex = 0;
            }

            public char next()
            {
                return PSOC_READY_ACK_STR[curIndex++];
            }

            public bool finished()
            {
                return curIndex == PSOC_READY_ACK_STR.Length;
            }
        }

        private PsocReadyAckStringIterator psocReadyAckStringIterator = new PsocReadyAckStringIterator();

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
                    else if(newByte == 'P')
                    {
                        psocReadyAckStringIterator.next();
                        curState = ReceiveState.RECEIVING_PSOC_READY_ACK;
                    }
                    else
                    {
                        throw new ArgumentException("invalid state");
                    }
                    break;

                case ReceiveState.RECEIVING_PSOC_READY_ACK:
                    if(newByte == psocReadyAckStringIterator.next())
                    {
                        if(psocReadyAckStringIterator.finished())
                        {
                            psocReadyAckStringIterator.reset();
                            curState = ReceiveState.FIND_FIRST_HEADER_CHAR;
                            if(RaisePsocReadyEvent != null) RaisePsocReadyEvent(this, new PsocReadyEventArgs());
                        }
                    }
                    else
                    {
                        throw new Exception("invalid state");
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
