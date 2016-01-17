using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
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
                for (int i = 0; i < numSamples; i++)
                {
                    receivedBuf[i] = samples[i];
                }
                numSamplesJustReceived = numSamples;
            }

            public void SetScanStartIndex(uint scanStartIndex)
            {
                throw new NotImplementedException();
            }

            public void SetScanLength(uint scanLength)
            {
                throw new NotImplementedException();
            }

            public void SetTriggerLevel(uint triggerLevel)
            {
                throw new NotImplementedException();
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

        private void VerifySamplesList(ushort[] expected)
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
            VerifySamplesList(samples);
        }

        [TestMethod]
        public void TestSimpleCase()
        {
            TestFrameAssembly(4, new ushort[] { 0, 1, 2, 3 });
        }

        [TestMethod]
        public void TestOneExpectedSample()
        {
            TestFrameAssembly(1, new ushort[] { 50 });
        }

        [TestMethod]
        public void TestRespondsToIncreasedExpectedSamplesCount()
        {
            TestFrameAssembly(4, new ushort[] { 0, 1, 2, 3 });
            TestFrameAssembly(6, new ushort[] { 1, 3, 3, 7, 5, 8 });
        }

        [TestMethod]
        public void TestRespondsToLotsOfChangedExpectedSamplesCount()
        {
            TestFrameAssembly(3, new ushort[] { 0, 1, 2 });
            TestFrameAssembly(6, new ushort[] { 8, 1, 3, 1, 2, 5 });
            TestFrameAssembly(5, new ushort[] { 8, 1, 3, 6, 2 });
            TestFrameAssembly(6, new ushort[] { 8, 1, 2, 6, 2, 9 });
            TestFrameAssembly(2, new ushort[] { 48, 4 });
            TestFrameAssembly(3, new ushort[] { 4, 5, 7 });
            TestFrameAssembly(1, new ushort[] { 99 });
        }

        [TestMethod]
        public void TestWaitsUntilFrameReady()
        {
            assembler.SetNumSamplesExpected(10);
            for (ushort i = 0; i < 9; i++)
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
            for (ushort i = 0; i < expected.Length; i++)
            {
                expected[i] = i;
            }
            TestFrameAssembly(SampleFrameAssemblerImpl.SAMPLES_BUF_SIZE, expected);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void TestNumSampleExpectedSetWhileBufferNotEmpty()
        {
            assembler.SetNumSamplesExpected(2);
            assembler.SampleAssembled(4);
            assembler.SetNumSamplesExpected(2);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void TestNumSampleExpectedRaisedAboveAmountInBuffer()
        {
            assembler.SetNumSamplesExpected(10);
            for (ushort i = 0; i < 9; i++)
            {
                assembler.SampleAssembled(i);
            }
            assembler.SetNumSamplesExpected(11);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void TestNumSampleExpectedLoweredBelowAmountInBuffer()
        {
            assembler.SetNumSamplesExpected(10);
            for (ushort i = 0; i < 9; i++)
            {
                assembler.SampleAssembled(i);
            }
            assembler.SetNumSamplesExpected(8);
            assembler.SampleAssembled(4);
            VerifySamplesList(new ushort[] { 0, 1, 2, 3, 4, 5, 6, 7 });
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void TestNumSampleExpectedLoweredToAmountInCurrentBuffer()
        {
            assembler.SetNumSamplesExpected(10);
            for (ushort i = 0; i < 9; i++)
            {
                assembler.SampleAssembled(i);
            }
            assembler.SetNumSamplesExpected(9);
            assembler.SampleAssembled(4);
            VerifySamplesList(new ushort[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 });

        }
    }

    [TestClass]
    public class SampleFrameAssemblerImplSynchronizationTests
    {
        private class SleepingMockSampleFrameReceiver : SampleFrameReceiver
        {
            SampleFrameAssemblerImplSynchronizationTests outer;
            Thread setExpectedSamplesThread;

            public SleepingMockSampleFrameReceiver(SampleFrameAssemblerImplSynchronizationTests outer, Thread setExpectedSamplesThread)
            {
                this.outer = outer;
                this.setExpectedSamplesThread = setExpectedSamplesThread;
            }

            public void FrameAssembled(ushort[] samples, uint numSamples)
            {
                setExpectedSamplesThread.Priority = ThreadPriority.Highest;
                Thread.CurrentThread.Priority = ThreadPriority.Lowest;
                setExpectedSamplesThread.Start();
                Thread.Sleep(100);
                outer.val = -1;
            }

            public void SetScanLength(uint scanLength)
            {
                throw new NotImplementedException();
            }

            public void SetScanStartIndex(uint scanStartIndex)
            {
                throw new NotImplementedException();
            }

            public void SetTriggerLevel(uint triggerLevel)
            {
                throw new NotImplementedException();
            }
        }

        private class DeadLockingMockSampleFrameReceiver : SampleFrameReceiver
        {
            public SampleFrameAssembler assembler { get; set; }

            public void FrameAssembled(ushort[] samples, uint numSamples)
            {
                assembler.SetNumSamplesExpected(10);
            }

            public void SetScanLength(uint scanLength)
            {
                throw new NotImplementedException();
            }

            public void SetScanStartIndex(uint scanStartIndex)
            {
                throw new NotImplementedException();
            }

            public void SetTriggerLevel(uint triggerLevel)
            {
                throw new NotImplementedException();
            }
        }

        private SampleFrameAssembler assembler;
        int val;

        private void CallSetExpectedNumSamplesAndThenSetVal()
        {
            assembler.SetNumSamplesExpected(10);
            val = 2;
        }

        [TestMethod]
        public void TestSetNumExpectedSamplesAndSampleAssembledCallsAreSynchronized()
        {
            Thread setExpectedNumSamplesThread = new Thread(new ThreadStart(CallSetExpectedNumSamplesAndThenSetVal));
            SampleFrameReceiver mockFrameReceiver = 
                new SleepingMockSampleFrameReceiver(this, setExpectedNumSamplesThread);

            assembler = new SampleFrameAssemblerImpl(mockFrameReceiver);
            assembler.SetNumSamplesExpected(1);
            assembler.SampleAssembled(1);
            setExpectedNumSamplesThread.Join();
            Assert.AreEqual(2, this.val);
        }

        [TestMethod]
        [ExpectedException(typeof(ThreadStateException))]
        public void TestDeadLockExceptionOccursWhenSetFrameSizeFromWithinSampleAssembledCall()
        {
            DeadLockingMockSampleFrameReceiver mockFrameReceiver 
                = new DeadLockingMockSampleFrameReceiver();
            assembler = new SampleFrameAssemblerImpl(mockFrameReceiver);
            mockFrameReceiver.assembler = assembler;

            assembler.SetNumSamplesExpected(1);
            assembler.SampleAssembled(2);
        }
    }
}
