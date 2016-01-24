using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SimpleOscope.SampleReceiving.Impl;
using SimpleOscope.SampleReceiving;

namespace SimpleOscopeUnitTests.SampleReceiving.Impl
{
    [TestClass]
    public class RisingEdgeTriggeringFrameReceiverTests
    {
        private RisingEdgeTriggeringFrameReceiver frameReceiver;
        private Mock<SampleFrameDisplayer> mockFrameDisplayer;

        [TestInitialize]
        public void setup()
        {
            mockFrameDisplayer = new Mock<SampleFrameDisplayer>();
            frameReceiver = new RisingEdgeTriggeringFrameReceiver(mockFrameDisplayer.Object);
        }

        [TestMethod]
        public void TestNormalCase()
        {
            ushort[] samples = new ushort[] { 0, 1, 2, 3, 4, 0, 1, 2, 3, 4, 0, 1, 2, 3, 4 };
            frameReceiver.SetTriggerLevel(1);
            frameReceiver.SetScanLength(4);
            frameReceiver.SetScanStartIndex(4);
            frameReceiver.FrameAssembled(samples, (uint)samples.Length);
            mockFrameDisplayer.Verify(x => x.DisplaySampleFrame(6, (uint)samples.Length, samples), Times.Once);
        }

        [TestMethod]
        public void TestFirstSampleAtTriggerButFallingEdge()
        {
            ushort[] samples = new ushort[] { 1, 0, 3, 4, 1, 0, 3, 4, 1, 2, 3, 4 };
            frameReceiver.SetTriggerLevel(1);
            frameReceiver.SetScanLength(4);
            frameReceiver.SetScanStartIndex(4);
            frameReceiver.FrameAssembled(samples, (uint)samples.Length);
            mockFrameDisplayer.Verify(x => x.DisplaySampleFrame(6, (uint)samples.Length, samples), Times.Once);
        }

        [TestMethod]
        public void TestTriggerTooLow()
        {
            ushort[] samples = new ushort[] { 1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4 };
            frameReceiver.SetTriggerLevel(1);
            frameReceiver.SetScanLength(12);
            frameReceiver.SetScanStartIndex(0);

            frameReceiver.FrameAssembled(samples, (uint)samples.Length);
            mockFrameDisplayer.Verify(x => x.DisplaySampleFrame(It.IsAny<uint>(), 
                It.IsAny<uint>(), It.IsAny<ushort[]>()), Times.Never);
        }

        [TestMethod]
        public void TestTriggerTooHigh()
        {
            ushort[] samples = new ushort[] { 1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4 };
            frameReceiver.SetTriggerLevel(5);
            frameReceiver.SetScanLength(12);
            frameReceiver.SetScanStartIndex(0);

            frameReceiver.FrameAssembled(samples, (uint)samples.Length);
            mockFrameDisplayer.Verify(x => x.DisplaySampleFrame(It.IsAny<uint>(),
                It.IsAny<uint>(), It.IsAny<ushort[]>()), Times.Never);
        }

        [TestMethod]
        public void TestTriggerRisingEdgeAtTopOfWave()
        {
            ushort[] samples = new ushort[] { 1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4 };
            frameReceiver.SetTriggerLevel(4);
            frameReceiver.SetScanLength(4);
            frameReceiver.SetScanStartIndex(8);

            frameReceiver.FrameAssembled(samples, (uint)samples.Length);
            mockFrameDisplayer.Verify(x => x.DisplaySampleFrame(11, (uint)samples.Length, samples), Times.Once);
        }

        [TestMethod]
        public void TestSteadyWaveAtTriggerLevel()
        {
            ushort[] samples = new ushort[] { 4, 4, 4, 4, 4, 4, 4, 4};
            frameReceiver.SetTriggerLevel(4);
            frameReceiver.SetScanLength(8);
            frameReceiver.SetScanStartIndex(0);
            frameReceiver.FrameAssembled(samples, (uint)samples.Length);
            mockFrameDisplayer.Verify(x => x.DisplaySampleFrame(It.IsAny<uint>(),
                 It.IsAny<uint>(), It.IsAny<ushort[]>()), Times.Never);
        }

        [TestMethod]
        public void TestScanStoppedBeforeTriggerEdge()
        {
            ushort[] samples = new ushort[] { 1, 2, 4, 1, 2, 3 };
            frameReceiver.SetTriggerLevel(4);
            frameReceiver.SetScanLength(2);
            frameReceiver.SetScanStartIndex(0);
            frameReceiver.FrameAssembled(samples, (uint)samples.Length);
            mockFrameDisplayer.Verify(x => x.DisplaySampleFrame(It.IsAny<uint>(),
                 It.IsAny<uint>(), It.IsAny<ushort[]>()), Times.Never);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void TestUsesNumSamplesParameterForErrorChecking()
        {
            ushort[] samples = new ushort[] { 1, 2, 1, 2, 1, 2 };
            frameReceiver.SetTriggerLevel(2);
            frameReceiver.SetScanLength(3);
            frameReceiver.SetScanStartIndex(1);
            frameReceiver.FrameAssembled(samples, 3);
        }

        [TestMethod]
        public void TestStartsAtScanStartIndex()
        {
            ushort[] samples = new ushort[] { 1, 2, 1, 2 , 1, 2, 1, 2, 1, 2};
            frameReceiver.SetTriggerLevel(2);
            frameReceiver.SetScanLength(2);
            frameReceiver.SetScanStartIndex(8);
            frameReceiver.FrameAssembled(samples, (uint)samples.Length);
            mockFrameDisplayer.Verify(x => x.DisplaySampleFrame(9, (uint)samples.Length, samples), Times.Once);
        }

        [TestMethod]
        public void TestFrameOfLengthFourTriggersOnLastPoint()
        {
            ushort[] samples = new ushort[] { 1, 2, 1, 2 };
            frameReceiver.SetTriggerLevel(2);
            frameReceiver.SetScanLength(3);
            frameReceiver.SetScanStartIndex(1);
            frameReceiver.FrameAssembled(samples, (uint)samples.Length);
            mockFrameDisplayer.Verify(x => x.DisplaySampleFrame(3, (uint)samples.Length, samples), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestScanLengthOfOne()
        {
            ushort[] samples = new ushort[] { 1 };
            frameReceiver.SetTriggerLevel(1);
            frameReceiver.SetScanLength(1);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void TestFrameOfLengthTooShortForScanLength()
        {
            ushort[] samples = new ushort[] { 1 };
            frameReceiver.SetTriggerLevel(0);
            frameReceiver.SetScanLength(2);
            frameReceiver.FrameAssembled(samples, (uint)samples.Length);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void TestScanStartIndexTooHighForScanLengthAndFrameSize()
        {
            ushort[] samples = new ushort[] { 1, 2, 3 };
            frameReceiver.SetTriggerLevel(0);
            frameReceiver.SetScanLength(2);
            frameReceiver.SetScanStartIndex(2);
            frameReceiver.FrameAssembled(samples, 2);
        }

        [TestMethod]
        public void TestWorksTwiceInARow()
        {
            ushort[] samples = new ushort[] { 1, 2, 3, 1, 2, 3 };
            frameReceiver.SetTriggerLevel(2);
            frameReceiver.SetScanLength(3);
            frameReceiver.SetScanStartIndex(2);

            frameReceiver.FrameAssembled(samples, (uint)samples.Length);
            mockFrameDisplayer.Verify(x => x.DisplaySampleFrame(4, (uint)samples.Length, samples),
                Times.Once);

            frameReceiver.FrameAssembled(samples, (uint)samples.Length);
            mockFrameDisplayer.Verify(x => x.DisplaySampleFrame(4, (uint)samples.Length, samples),
                Times.Exactly(2));

        }
    }
}
