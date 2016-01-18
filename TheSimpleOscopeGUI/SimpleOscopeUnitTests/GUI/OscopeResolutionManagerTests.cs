using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WpfApplication1;
using Moq;
using SimpleOscope.SampleReceiving;
using SimpleOscope.SampleReceiving.Impl.ByteReceiving;

namespace SimpleOscopeUnitTests.GUI
{
    [TestClass]
    public class OscopeResolutionManagerTests
    {
        private Mock<SampleFrameAssembler> sampleFrameAssembler;
        private Mock<SampleFrameReceiver> sampleFrameReceiver;
        private Mock<OscopeWindowClient> oscopeWindowClient;
        private Mock<SerialPortClient> serialPortClient;
        private Mock<SampleFrameDisplayer> sampleFrameDispalyer;
        private Mock<ByteReceiver> byteReceiver;

        [TestInitialize]
        public void setup()
        {
        }

        /* Note: When adjusting the sample spacing to meet a new horizontal time unit, 
         * the resulting spacing should be agnostic of the number of samples in a
         * frame, but should take into account the size of the oscope window. */

         /* 
          */
        [TestMethod]
        public void TestOscopeWindowWidthMatchesTimeUnitExactly()
        {
            Mock<OscopeResolutionManager> mockManager = new Mock<OscopeResolutionManager>();
            mockManager.Setup(x => x.curADCSamplePeriod).Returns(4);
            mockManager.Setup(x => x.oscopeWindowClient.getCanvasWidth()).Returns(23);
            mockManager.CallBase = true;
            int newTimePerWholeWindow = 23;
            int sampleSpacingRes = mockManager.Object.AdjustSampleSpacing(newTimePerWholeWindow);
            Assert.AreEqual(sampleSpacingRes, 4);
        }

        [TestMethod]
        public void TestOscopeWindoWidthIsDoubleTheTimeUnit()
        {
            Mock<OscopeResolutionManager> mockManager = new Mock<OscopeResolutionManager>();
            mockManager.Setup(x => x.curADCSamplePeriod).Returns(4);
            mockManager.Setup(x => x.oscopeWindowClient.getCanvasWidth()).Returns(46);
            mockManager.CallBase = true;
            int newTimePerWholeWindow = 23;
            int sampleSpacingRes = mockManager.Object.AdjustSampleSpacing(newTimePerWholeWindow);
            Assert.AreEqual(sampleSpacingRes, 8);
        }

        [TestMethod]
        public void TestOscopeWindoWidthNotEvenlyDivisibleByTimeUnit()
        {
            Mock<OscopeResolutionManager> mockManager = new Mock<OscopeResolutionManager>();
            mockManager.Setup(x => x.curADCSamplePeriod).Returns(4);
            mockManager.Setup(x => x.oscopeWindowClient.getCanvasWidth()).Returns(51);
            mockManager.CallBase = true;
            int newTimePerWholeWindow = 23;
            int sampleSpacingRes = mockManager.Object.AdjustSampleSpacing(newTimePerWholeWindow);
            Assert.AreEqual(sampleSpacingRes, 4);
        }
    }
}
