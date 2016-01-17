using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleOscope.SampleReceiving.Impl.SampleAssembly;
using SimpleOscope.SampleReceiving;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
