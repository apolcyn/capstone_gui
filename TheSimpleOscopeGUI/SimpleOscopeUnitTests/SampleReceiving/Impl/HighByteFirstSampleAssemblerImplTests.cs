using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleOscope.SampleReceiving.Impl;
using SimpleOscope.SampleReceiving;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleOscope;
using Moq;

namespace SimpleOscopeUnitTests.SampleReceiving.Impl
{
    [TestClass]
    public class HighByteFirstSampleAssemblerImplTests
    {
        private class MockSampleFrameAssembler : SampleFrameAssembler
        {
            public ushort[] list = new ushort[100];
            public int curIndex = 0;

            public void SampleAssembled(ushort nextSample)
            {
                list[curIndex++] = nextSample;
            }

            public void SetNumSamplesExpected(uint numSamplesExpected)
            {
                throw new NotImplementedException();
            }
        }

        private void VerifyListsAreEqual(MockSampleFrameAssembler receiver, ushort[] b)
        {
            Assert.AreEqual(receiver.curIndex, b.Length);
            for(int i = 0; i < b.Length; i++)
            {
                Assert.AreEqual(b[i], receiver.list[i]);
            }
        }

        [TestMethod]
        public void TestSimpleCase()
        {
            MockSampleFrameAssembler receiver = new MockSampleFrameAssembler();
            SampleAssembler assembler = new HighByteFirstSampleAssemblerImpl(receiver);

            assembler.AddReceivedByte(3);
            assembler.AddReceivedByte(5);

            ushort[] expected = new ushort[] { 256 * 3 + 5 };
            VerifyListsAreEqual(receiver, expected);
        }

        [TestMethod]
        public void BreakThis()
        {
            Mock<SampleFrameAssembler> receiver = new Mock<SampleFrameAssembler>();
            HighByteFirstSampleAssemblerImpl assembler 
                = new HighByteFirstSampleAssemblerImpl(receiver.Object);

            assembler.maxSampleSizeChanged(null, new MaxSampleSizeChangedEventArgs(4095));
            assembler.sampleOffsetChanged(null, new SampleOffsetChangedEventArgs(0));
            assembler.oscopeHeightChanged(null, new OscopeHeightChangedEventArgs(301));
            assembler.sampleScalerChanged(null, new SampleScalerChangedEventArgs(1));

            assembler.AddReceivedByte(0x0f);
            assembler.AddReceivedByte(0xff);

            receiver.Verify(x => x.SampleAssembled(301), Times.Once);
        }

        [TestMethod]
        public void BreakThisAgain()
        {
            Mock<SampleFrameAssembler> receiver = new Mock<SampleFrameAssembler>();
            HighByteFirstSampleAssemblerImpl assembler
                = new HighByteFirstSampleAssemblerImpl(receiver.Object);

            assembler.maxSampleSizeChanged(null, new MaxSampleSizeChangedEventArgs(4095));
            assembler.sampleOffsetChanged(null, new SampleOffsetChangedEventArgs(0));
            assembler.oscopeHeightChanged(null, new OscopeHeightChangedEventArgs(301));
            assembler.sampleScalerChanged(null, new SampleScalerChangedEventArgs(1));

            assembler.AddReceivedByte(0x08);
            assembler.AddReceivedByte(0x00);

            receiver.Verify(x => x.SampleAssembled(150), Times.Once);
        }

        [TestMethod]
        public void BreakThisOnceMore()
        {
            Mock<SampleFrameAssembler> receiver = new Mock<SampleFrameAssembler>();
            HighByteFirstSampleAssemblerImpl assembler
                = new HighByteFirstSampleAssemblerImpl(receiver.Object);

            assembler.maxSampleSizeChanged(null, new MaxSampleSizeChangedEventArgs(4095));
            assembler.sampleOffsetChanged(null, new SampleOffsetChangedEventArgs(0));
            assembler.oscopeHeightChanged(null, new OscopeHeightChangedEventArgs(301));
            assembler.sampleScalerChanged(null, new SampleScalerChangedEventArgs(1));

            assembler.AddReceivedByte(0x06);
            assembler.AddReceivedByte(0x00);

            receiver.Verify(x => x.SampleAssembled(112), Times.Once);
        }

        [TestMethod]
        public void BreakThisOnceMoreAgain()
        {
            Mock<SampleFrameAssembler> receiver = new Mock<SampleFrameAssembler>();
            HighByteFirstSampleAssemblerImpl assembler
                = new HighByteFirstSampleAssemblerImpl(receiver.Object);

            assembler.maxSampleSizeChanged(null, new MaxSampleSizeChangedEventArgs(4095));
            assembler.sampleOffsetChanged(null, new SampleOffsetChangedEventArgs(0));
            assembler.oscopeHeightChanged(null, new OscopeHeightChangedEventArgs(301));
            assembler.sampleScalerChanged(null, new SampleScalerChangedEventArgs(1));

            assembler.AddReceivedByte(0x06);
            assembler.AddReceivedByte(3);

            receiver.Verify(x => x.SampleAssembled(113), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void UpperNibbleTooLarge()
        {
            Mock<SampleFrameAssembler> receiver = new Mock<SampleFrameAssembler>();
            HighByteFirstSampleAssemblerImpl assembler
                = new HighByteFirstSampleAssemblerImpl(receiver.Object);

            assembler.maxSampleSizeChanged(null, new MaxSampleSizeChangedEventArgs(4095));
            assembler.sampleOffsetChanged(null, new SampleOffsetChangedEventArgs(0));
            assembler.oscopeHeightChanged(null, new OscopeHeightChangedEventArgs(301));
            assembler.sampleScalerChanged(null, new SampleScalerChangedEventArgs(1));

            assembler.AddReceivedByte(0x32);
        }

        [TestMethod]
        public void BreakThisOnceMoreAgainOver()
        {
            Mock<SampleFrameAssembler> receiver = new Mock<SampleFrameAssembler>();
            HighByteFirstSampleAssemblerImpl assembler
                = new HighByteFirstSampleAssemblerImpl(receiver.Object);

            assembler.maxSampleSizeChanged(null, new MaxSampleSizeChangedEventArgs(4095));
            assembler.sampleOffsetChanged(null, new SampleOffsetChangedEventArgs(0));
            assembler.oscopeHeightChanged(null, new OscopeHeightChangedEventArgs(301));
            assembler.sampleScalerChanged(null, new SampleScalerChangedEventArgs(1));

            assembler.AddReceivedByte(0x03);
            assembler.AddReceivedByte(67);
        

            receiver.Verify(x => x.SampleAssembled(61), Times.Once);
        }

        [TestMethod]
        public void BreakAll()
        {
            BreakThis();
            BreakThisAgain();
            BreakThisOnceMore();
            BreakThisOnceMoreAgain();
            BreakThisOnceMoreAgainOver();
        }

        [TestMethod]
        public void TestWorksMoreThanOnce()
        {
            MockSampleFrameAssembler receiver = new MockSampleFrameAssembler();
            SampleAssembler assembler = new HighByteFirstSampleAssemblerImpl(receiver);

            assembler.AddReceivedByte(0);
            assembler.AddReceivedByte(1);

            VerifyListsAreEqual(receiver, new ushort[] { 1 });

            assembler.AddReceivedByte(3);
            assembler.AddReceivedByte(7);

            VerifyListsAreEqual(receiver, new ushort[] { 1, 256 * 3 + 7 });

            assembler.AddReceivedByte(4);
            assembler.AddReceivedByte(1);
          
            VerifyListsAreEqual(receiver, new ushort[] { 1, 256 * 3 + 7, 256 * 4 + 1 });
        }
    }
}
