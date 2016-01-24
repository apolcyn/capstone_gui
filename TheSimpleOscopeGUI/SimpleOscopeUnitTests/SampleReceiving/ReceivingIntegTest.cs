using System;
using System.Windows.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleOscope.SampleReceiving;
using SimpleOscope.SampleReceiving.Impl;
using SimpleOscope.SampleReceiving.Impl;
using SimpleOscope.SampleReceiving.Impl;
using SimpleOscope.SampleReceiving.Impl;
using SimpleOscope.SampleReceiving.Impl;
using Moq;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows;
using System.Threading;
using System.Collections.Generic;
using SimpleOscope;

namespace SimpleOscopeUnitTests.SampleReceiving
{
    [TestClass]
    public class ReceivingIntegTest : BaseTests
    {
        Mock<OscopeWindowClient> mockScopeWindowClient = new Mock<OscopeWindowClient>();
        SampleFrameDisplayer sampleFrameDisplayer;
        SampleFrameReceiver sampleFrameReceiver;
        SampleFrameAssembler sampleFrameAssembler;
        SampleAssembler sampleAssembler;
        ByteReceiver byteReceiver;
        Mock<MainWindow> mockMainWindow = new Mock<MainWindow>();


        [TestInitialize]
        public void setup()
        {
            sampleFrameDisplayer
                = new SampleFrameDisplayerImpl(mockScopeWindowClient.Object, mockMainWindow.Object);
            sampleFrameReceiver = new RisingEdgeTriggeringFrameReceiver(sampleFrameDisplayer);
            sampleFrameAssembler = new SampleFrameAssemblerImpl(sampleFrameReceiver);
            sampleAssembler = new HighByteFirstSampleAssemblerImpl(sampleFrameAssembler);
            byteReceiver = new ByteReceiverImpl(sampleAssembler, sampleFrameAssembler);
        }

        [TestMethod]
        public void FullDrawsExpectedLinesOnCanvasWithSmalFrame()
        {
            mockScopeWindowClient.Setup(x => x.getCanvasWidth()).Returns(3);
            sampleFrameDisplayer.SetNumSamplesToDisplay(3);
            sampleFrameDisplayer.SetSpacing(1);
            sampleFrameDisplayer.SetTriggerRelativeDispalyStartIndex(0);

            sampleFrameReceiver.SetScanStartIndex(0);
            sampleFrameReceiver.SetScanLength(3);
            sampleFrameReceiver.SetTriggerLevel(1);
            
            foreach(char c in "#F4D")
            {
                byteReceiver.byteReceived((byte)c);
            }
            foreach (char c in "00010203")
            {
                byteReceiver.byteReceived((byte)(c - '0'));
            }
            List<LineCoordinates> expectedLines = new List<LineCoordinates>();
            expectedLines.Add(CreateLineCoordinates(0, 1, 1, 2));
            expectedLines.Add(CreateLineCoordinates(1, 2, 2, 3));

            mockScopeWindowClient.Verify(x => x.drawLinesOnOscope(It.Is<List<LineCoordinates>>
                (lines => ListOfLineCoordinatesStructsEqual(lines, expectedLines)))
                , Times.Once);
        }

        [TestMethod]
        public void DrawsExpectedLinesWithNegativeTriggeringIndex()
        {
            mockScopeWindowClient.Setup(x => x.getCanvasWidth()).Returns(3);
            sampleFrameDisplayer.SetNumSamplesToDisplay(3);
            sampleFrameDisplayer.SetSpacing(1);
            sampleFrameDisplayer.SetTriggerRelativeDispalyStartIndex(-1);

            sampleFrameReceiver.SetScanStartIndex(0);
            sampleFrameReceiver.SetScanLength(3);
            sampleFrameReceiver.SetTriggerLevel(2);

            foreach (char c in "#F4D")
            {
                byteReceiver.byteReceived((byte)c);
            }
            foreach (char c in "00010203")
            {
                byteReceiver.byteReceived((byte)(c - '0'));
            }
            List<LineCoordinates> expectedLines = new List<LineCoordinates>();
            expectedLines.Add(CreateLineCoordinates(0, 1, 1, 2));
            expectedLines.Add(CreateLineCoordinates(1, 2, 2, 3));

            mockScopeWindowClient.Verify(x => x.drawLinesOnOscope(It.Is<List<LineCoordinates>>
                (lines => ListOfLineCoordinatesStructsEqual(lines, expectedLines)))
                , Times.Once);
        }

        [TestMethod]
        public void SpacingOfTwo()
        {
            mockScopeWindowClient.Setup(x => x.getCanvasWidth()).Returns(4);
            sampleFrameDisplayer.SetNumSamplesToDisplay(3);
            sampleFrameDisplayer.SetSpacing(2);
            sampleFrameDisplayer.SetTriggerRelativeDispalyStartIndex(-1);

            sampleFrameReceiver.SetScanStartIndex(0);
            sampleFrameReceiver.SetScanLength(3);
            sampleFrameReceiver.SetTriggerLevel(2);

            foreach (char c in "#F4D")
            {
                byteReceiver.byteReceived((byte)c);
            }
            foreach (char c in "00010203")
            {
                byteReceiver.byteReceived((byte)(c - '0'));
            }
            List<LineCoordinates> expectedLines = new List<LineCoordinates>();
            expectedLines.Add(CreateLineCoordinates(0, 1, 2, 2));
            expectedLines.Add(CreateLineCoordinates(2, 2, 4, 3));

            mockScopeWindowClient.Verify(x => x.drawLinesOnOscope(It.Is<List<LineCoordinates>>
                (lines => ListOfLineCoordinatesStructsEqual(lines, expectedLines)))
                , Times.Once);
        }

        [TestMethod]
        public void LargeSpacingBetweenSamples()
        {
            mockScopeWindowClient.Setup(x => x.getCanvasWidth()).Returns(300);
            sampleFrameDisplayer.SetNumSamplesToDisplay(4);
            sampleFrameDisplayer.SetSpacing(100);
            sampleFrameDisplayer.SetTriggerRelativeDispalyStartIndex(-2);

            sampleFrameReceiver.SetScanStartIndex(1);
            sampleFrameReceiver.SetScanLength(2);
            sampleFrameReceiver.SetTriggerLevel(2);

            foreach (char c in "#F4D")
            {
                byteReceiver.byteReceived((byte)c);
            }
            foreach (char c in "00010203")
            {
                byteReceiver.byteReceived((byte)(c - '0'));
            }
            List<LineCoordinates> expectedLines = new List<LineCoordinates>();
            expectedLines.Add(CreateLineCoordinates(0, 0, 100, 1));
            expectedLines.Add(CreateLineCoordinates(100, 1, 200, 2));
            expectedLines.Add(CreateLineCoordinates(200, 2, 300, 3));

            mockScopeWindowClient.Verify(x => x.drawLinesOnOscope(It.Is<List<LineCoordinates>>
                (lines => ListOfLineCoordinatesStructsEqual(lines, expectedLines)))
                , Times.Once);
        }

        [TestMethod]
        public void TestWorksWithChangingConditions()
        {
            LargeSpacingBetweenSamples();
            FullDrawsExpectedLinesOnCanvasWithSmalFrame();
            SpacingOfTwo();
        }

        [TestMethod]
        public void TestLastSampleGoesOverFrameSize()
        {
            mockScopeWindowClient.Setup(x => x.getCanvasWidth()).Returns(300);
            sampleFrameDisplayer.SetNumSamplesToDisplay(4);
            sampleFrameDisplayer.SetSpacing(101);
            sampleFrameDisplayer.SetTriggerRelativeDispalyStartIndex(-2);

            sampleFrameReceiver.SetScanStartIndex(1);
            sampleFrameReceiver.SetScanLength(2);
            sampleFrameReceiver.SetTriggerLevel(2);

            foreach (char c in "#F4D")
            {
                byteReceiver.byteReceived((byte)c);
            }
            foreach (char c in "00010203")
            {
                byteReceiver.byteReceived((byte)(c - '0'));
            }
            List<LineCoordinates> expectedLines = new List<LineCoordinates>();
            expectedLines.Add(CreateLineCoordinates(0, 0, 101, 1));
            expectedLines.Add(CreateLineCoordinates(101, 1, 202, 2));
            expectedLines.Add(CreateLineCoordinates(202, 2, 303, 3));

            mockScopeWindowClient.Verify(x => x.drawLinesOnOscope(It.Is<List<LineCoordinates>>
                (lines => ListOfLineCoordinatesStructsEqual(lines, expectedLines)))
                , Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void TestNotEnoughSamplesToFillFrameSize()
        {
            mockScopeWindowClient.Setup(x => x.getCanvasWidth()).Returns(300);
            sampleFrameDisplayer.SetNumSamplesToDisplay(4);
            sampleFrameDisplayer.SetSpacing(99);
            sampleFrameDisplayer.SetTriggerRelativeDispalyStartIndex(-2);

            sampleFrameReceiver.SetScanStartIndex(1);
            sampleFrameReceiver.SetScanLength(2);
            sampleFrameReceiver.SetTriggerLevel(2);

            foreach (char c in "#F4D")
            {
                byteReceiver.byteReceived((byte)c);
            }
            foreach (char c in "00010203")
            {
                byteReceiver.byteReceived((byte)(c - '0'));
            }
        }
    }
}
