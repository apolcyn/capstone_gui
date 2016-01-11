using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleOscope.SampleReceiving.Impl.SampleFrameAssembly;
using SimpleOscope.SampleReceiving;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleOscopeUnitTests.SampleReceiving.Impl.SampleFrameAssembly
{
    [TestClass]
    public class SampleFrameAssemblerImplTests
    {
        private class MockSampleFrameReceiver : SampleFrameReceiver
        {
            public ushort[] receivedBuf = new ushort[SampleFrameAssemblerImpl.SAMPLES_BUF_SIZE * 2];
            public uint numSamplesJustReceived;

            public void FrameAssembled(ushort[] samples, uint numSamples)
            {
                for(int i = 0; i < numSamples; i++)
                {
                    receivedBuf[i] = samples[i];
                }
                numSamplesJustReceived = numSamples;
            }
        }

        MockSampleFrameReceiver mockFrameReceiver;
        SampleFrameAssemblerImpl assembler;

        [TestInitialize]
        public void setup()
        {
            mockFrameReceiver = new MockSampleFrameReceiver();
            assembler = new SampleFrameAssemblerImpl(mockFrameReceiver);
        }

        private void VerifySamplesList(MockSampleFrameReceiver mockFrameReceiver, ushort[] expected)
        {
            Assert.AreEqual(mockFrameReceiver.numSamplesJustReceived, (uint)expected.Length);

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(mockFrameReceiver.receivedBuf[i], expected[i]);
            }
        }

        private void TestFrameAssembly(uint numSamplesExpectedByReceiver, ushort[] samples)
        {
            assembler.SetNumSamplesExpected(numSamplesExpectedByReceiver);
            for (ushort i = 0; i < samples.Length; i++)
            {
                assembler.SampleAssembled(samples[i]);
            }
            VerifySamplesList(mockFrameReceiver, samples);
        }

        [TestMethod]
        public void TestSimpleCase()
        {
            TestFrameAssembly(4, new ushort[] { 0, 1, 2, 3 });
        }

        [TestMethod]
        public void TestOneExpectedSample()
        {
            TestFrameAssembly(1, new ushort[] { 50});
        }

        [TestMethod]
        public void TestRespondsToIncreasedExpectedSamplesCount()
        {
            TestFrameAssembly(4, new ushort[] { 0, 1, 2, 3 });
            TestFrameAssembly(6, new ushort[] { 1, 3, 3, 7, 5, 8});
        }

        [TestMethod]
        public void TestRespondsToLotsOfChangedExpectedSamplesCount()
        {
            TestFrameAssembly(3, new ushort[] { 0, 1, 2 });
            TestFrameAssembly(6, new ushort[] { 8, 1, 3, 1, 2, 5 });
            TestFrameAssembly(5, new ushort[] { 8, 1, 3, 6, 2 });
            TestFrameAssembly(6, new ushort[] { 8, 1, 2, 6, 2, 9});
            TestFrameAssembly(2, new ushort[] {48, 4 });
            TestFrameAssembly(3, new ushort[] { 4, 5, 7 });
            TestFrameAssembly(1, new ushort[] { 99 });
        }

        [TestMethod]
        public void TestWaitsUntilFrameReady()
        {
            assembler.SetNumSamplesExpected(10);
            for(ushort i = 0; i < 9; i++)
            {
                assembler.SampleAssembled(i);
            }
            Assert.AreEqual(mockFrameReceiver.numSamplesJustReceived, (uint)0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestZeroExpectedSamplesCount()
        {
            assembler.SetNumSamplesExpected(0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestTooManyExpectedSamples()
        {
            assembler.SetNumSamplesExpected(SampleFrameAssemblerImpl.SAMPLES_BUF_SIZE + 1);
        }

        [TestMethod]
        public void TestWorksAtMaxBufSize()
        {
            ushort[] expected = new ushort[SampleFrameAssemblerImpl.SAMPLES_BUF_SIZE];
            for(ushort i = 0; i < expected.Length; i++)
            {
                expected[i] = i;
            }
            TestFrameAssembly(SampleFrameAssemblerImpl.SAMPLES_BUF_SIZE, expected);
        }
    }
}
