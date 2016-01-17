using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleOscope.SampleReceiving;
using SimpleOscope.SampleReceiving.Impl.ByteReceiving;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace SimpleOscopeUnitTests.SampleReceiving.Impl
{
    [TestClass]
    public class ByteReceivingImplTests
    {
        private ByteReceiverImpl byteReceiver;
        Mock<SampleAssembler> mockSampleAssembler = new Mock<SampleAssembler>();

        [TestInitialize]
        public void setup()
        {
            byteReceiver = new ByteReceiverImpl(mockSampleAssembler.Object);
        }

        private void SendHeader(int numSamples)
        {
            foreach (char c in "#F" + numSamples + "D")
            {
                byteReceiver.byteReceived((byte)c);
            }
        }

        private void SendSamples(byte low, byte high, Times numTimesEachByte, Times numTotalBytes)
        {
            for (byte b = low; b < high; b++)
            {
                byteReceiver.byteReceived(b);
                mockSampleAssembler.Verify(x => x.AddReceivedByte(b), numTimesEachByte);
            }
            mockSampleAssembler.Verify(x => x.AddReceivedByte(It.IsAny<byte>()), numTotalBytes);
        }

        private void TestSamples(int numSamples, Times numTimesEachByte, Times numTotalBytes)
        {
            SendHeader(numSamples);
            SendSamples(0, (byte)(numSamples * 2), numTimesEachByte, numTotalBytes);
        }

        [TestMethod]
        public void TestFiveSamples()
        {
            TestSamples(5, Times.Once(), Times.Exactly(10));
        }

        [TestMethod]
        public void TestOneSample()
        {
            TestSamples(1, Times.Once(), Times.Exactly(2));
        }

        [TestMethod]
        public void TestTwoSamples()
        {
            TestSamples(2, Times.Once(), Times.Exactly(4));
        }

        [TestMethod]
        public void TestOneAndThenTwoSamples()
        {
            TestSamples(1, Times.Once(), Times.Exactly(2));
            SendHeader(2);
            SendSamples(0, 2, Times.Exactly(2), Times.Exactly(4));
            SendSamples(2, 4, Times.Exactly(1), Times.Exactly(6));
        }

        [TestMethod]
        public void TestTwoAndThenOneSamples()
        {
            TestSamples(2, Times.Once(), Times.Exactly(4));
            SendHeader(1);
            SendSamples(0, 2, Times.Exactly(2), Times.Exactly(6));
        }

        [TestMethod]
        public void TestChangeNumSamplesMultipleTimes()
        {
            TestSamples(2, Times.Once(), Times.Exactly(4));
            TestSamples(1, Times.Exactly(2), Times.Exactly(6));
            SendHeader(2);
            SendSamples(0, 2, Times.Exactly(3), Times.Exactly(8));
            SendSamples(2, 4, Times.Exactly(2), Times.Exactly(10));
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestTooManySamples()
        {
            SendHeader(2);
            for(byte i = 0; i < 5; i++)
            {
                byteReceiver.byteReceived(i);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestEmptySamplesCountField()
        {
            foreach (char c in "#FD")
            {
                byteReceiver.byteReceived((byte)c);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestZeroSamplesFound()
        {
            foreach (char c in "#F0D")
            {
                byteReceiver.byteReceived((byte)c);
            }
        }

        [TestMethod]
        public void TestLeadingZeroInHeader()
        {
            foreach (char c in "#F05D")
            {
                byteReceiver.byteReceived((byte)c);
            }
            SendSamples(0, 10, Times.Once(), Times.Exactly(10));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestBadHeader()
        {
            foreach (char c in "#f5D")
            {
                byteReceiver.byteReceived((byte)c);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestBadHeaderEnd()
        {
            foreach (char c in "#F5r")
            {
                byteReceiver.byteReceived((byte)c);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestBadHeaderStart()
        {
            TestSamples(2, Times.Once(), Times.Exactly(4));
            byteReceiver.byteReceived((byte)'F');
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestNumbersInHeaderStart()
        {
            foreach (char c in "#FR5D")
            {
                byteReceiver.byteReceived((byte)c);
            }
        }
    }
}
