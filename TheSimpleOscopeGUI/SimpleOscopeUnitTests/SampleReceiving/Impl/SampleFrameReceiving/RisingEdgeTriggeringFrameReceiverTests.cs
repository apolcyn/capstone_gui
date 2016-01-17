using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SimpleOscope.SampleReceiving.Impl.SampleFrameReceiving;
using SimpleOscope.SampleReceiving;

namespace SimpleOscopeUnitTests.SampleReceiving.Impl.SampleFrameReceiving
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
            frameReceiver.FrameAssembled(samples, (uint)samples.Length);
            mockFrameDisplayer.Verify(x => x.DisplaySampleFrame(6, (uint)samples.Length, samples), Times.Once);
        }

        [TestMethod]
        public void TestFirstSampleAtTriggerButFallingEdge()
        {
            ushort[] samples = new ushort[] { 1, 0, 3, 4, 1, 0, 3, 4, 1, 2, 3, 4 };
            frameReceiver.SetTriggerLevel(1);
            frameReceiver.FrameAssembled(samples, (uint)samples.Length);
            mockFrameDisplayer.Verify(x => x.DisplaySampleFrame(6, (uint)samples.Length, samples), Times.Once);
        }

        [TestMethod]
        public void TestTriggerTooLow()
        {
            ushort[] samples = new ushort[] { 1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4 };
            frameReceiver.SetTriggerLevel(1);
            frameReceiver.FrameAssembled(samples, (uint)samples.Length);
            mockFrameDisplayer.Verify(x => x.DisplaySampleFrame(It.IsAny<uint>(), 
                It.IsAny<uint>(), It.IsAny<ushort[]>()), Times.Never);
        }

        [TestMethod]
        public void TestTriggerTooHigh()
        {
            ushort[] samples = new ushort[] { 1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4 };
            frameReceiver.SetTriggerLevel(5);
            frameReceiver.FrameAssembled(samples, (uint)samples.Length);
            mockFrameDisplayer.Verify(x => x.DisplaySampleFrame(It.IsAny<uint>(),
                It.IsAny<uint>(), It.IsAny<ushort[]>()), Times.Never);
        }

        [TestMethod]
        public void TestTriggerRisingEdgeAtTopOfWave()
        {
            ushort[] samples = new ushort[] { 1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4 };
            frameReceiver.SetTriggerLevel(4);
            frameReceiver.FrameAssembled(samples, (uint)samples.Length);
            mockFrameDisplayer.Verify(x => x.DisplaySampleFrame(7, (uint)samples.Length, samples), Times.Once);
        }

        [TestMethod]
        public void TestSteadyWaveAtTriggerLevel()
        {
            ushort[] samples = new ushort[] { 4, 4, 4, 4, 4, 4, 4, 4};
            frameReceiver.SetTriggerLevel(4);
            frameReceiver.FrameAssembled(samples, (uint)samples.Length);
            mockFrameDisplayer.Verify(x => x.DisplaySampleFrame(It.IsAny<uint>(),
                 It.IsAny<uint>(), It.IsAny<ushort[]>()), Times.Never);
        }

        [TestMethod]
        public void TestNotEnoughTriggeringEdges()
        {
            ushort[] samples = new ushort[] { 1, 2, 4, 1, 2, 3 };
            frameReceiver.SetTriggerLevel(4);
            frameReceiver.FrameAssembled(samples, (uint)samples.Length);
            mockFrameDisplayer.Verify(x => x.DisplaySampleFrame(It.IsAny<uint>(),
                 It.IsAny<uint>(), It.IsAny<ushort[]>()), Times.Never);
        }

        [TestMethod]
        public void TestFrameOfLengthFour()
        {
            ushort[] samples = new ushort[] { 1, 2, 1, 2 };
            frameReceiver.SetTriggerLevel(2);
            frameReceiver.FrameAssembled(samples, (uint)samples.Length);
            mockFrameDisplayer.Verify(x => x.DisplaySampleFrame(3, (uint)samples.Length, samples), Times.Once);
        }

        [TestMethod]
        public void TestFrameOfLengthOne()
        {
            ushort[] samples = new ushort[] { 1 };
            frameReceiver.SetTriggerLevel(1);
            frameReceiver.FrameAssembled(samples, (uint)samples.Length);
            mockFrameDisplayer.Verify(x => x.DisplaySampleFrame(It.IsAny<uint>(),
                 It.IsAny<uint>(), It.IsAny<ushort[]>()), Times.Never);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestFrameOfLengthZero()
        {
            ushort[] samples = new ushort[] { };
            frameReceiver.SetTriggerLevel(0);
            frameReceiver.FrameAssembled(samples, (uint)samples.Length);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestTooManySamplesForFrame()
        {
            ushort[] samples = new ushort[] { 1 };
            frameReceiver.SetTriggerLevel(0);
            frameReceiver.FrameAssembled(samples, 2);
        }

        [TestMethod]
        public void TestWorksTwiceInARow()
        {
            ushort[] samples = new ushort[] { 1, 2, 3, 1, 2, 3 };
            frameReceiver.SetTriggerLevel(2);
            frameReceiver.FrameAssembled(samples, (uint)samples.Length);
            mockFrameDisplayer.Verify(x => x.DisplaySampleFrame(4, (uint)samples.Length, samples),
                Times.Once);

            frameReceiver.FrameAssembled(samples, (uint)samples.Length);
            mockFrameDisplayer.Verify(x => x.DisplaySampleFrame(4, (uint)samples.Length, samples),
                Times.Exactly(2));

        }
    }
}
