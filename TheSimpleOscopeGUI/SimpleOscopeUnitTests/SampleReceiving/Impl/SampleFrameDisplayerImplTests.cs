using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SimpleOscope.SampleReceiving.Impl.SampleFrameDisplaying;
using System.Windows.Controls;
using SimpleOscope.SampleReceiving;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading;
using SimpleOscopeUnitTests;

namespace SimpleOscopeUnitTests.SampleReceiving.Impl
{
    [TestClass]
    public class SampleFrameDisplayerImplTests : BaseTests
    {
        Mock<OscopeWindowClient> mockDrawer;

        [TestInitialize]
        public void setup()
        {
            mockDrawer = new Mock<OscopeWindowClient>();
        }

        [TestMethod]
        public void TestCreatesLineCorrectly()
        {
            mockDrawer.Setup(x => x.getCanvasWidth()).Returns(10);
            SampleFrameDisplayerImpl displayer
                = new SampleFrameDisplayerImpl(mockDrawer.Object, 10, 3);

            Line expected = new Line();
            expected.X1 = 2;
            expected.X2 = 5;
            expected.Y1 = 8;
            expected.Y2 = 6;
            Line actual = displayer.createLine(2, 8, 5, 6);
            Assert.AreEqual(actual.X1, expected.X1);
            Assert.AreEqual(actual.X2, expected.X2);
            Assert.AreEqual(actual.Y1, expected.Y1);
            Assert.AreEqual(actual.Y2, expected.Y2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCreateLineErrorsIfLineSpacingIsBad()
        {
            mockDrawer.Setup(x => x.getCanvasWidth()).Returns(40);
            SampleFrameDisplayerImpl displayer
                = new SampleFrameDisplayerImpl(mockDrawer.Object, 10, 3);
            displayer.createLine(2, 5, 4, 6);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCreateLineErrorsIfLineGoesPastEndOfPossibleIsBad()
        {
            mockDrawer.Setup(x => x.getCanvasWidth()).Returns(20);
            SampleFrameDisplayerImpl displayer
                = new SampleFrameDisplayerImpl(mockDrawer.Object, 10, 3);
            displayer.createLine(40, 5, 43, 6);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCreateLineErrorsIfItIsGoingBackwards()
        {
            mockDrawer.Setup(x => x.getCanvasWidth()).Returns(40);
            SampleFrameDisplayerImpl displayer
                = new SampleFrameDisplayerImpl(mockDrawer.Object, 10, 3);
            displayer.createLine(2, 5, 1, 6);
        }

        [TestMethod]
        public void TestNormalCase()
        {
            SampleFrameDisplayerImpl displayer 
                = new SampleFrameDisplayerImpl(mockDrawer.Object, 10, 3);
            ushort[] samples = new ushort[21];
            for(int i = 0; i < 21; i++)
            {
                samples[i] = (ushort)(i * 2);
            }

            displayer.SetNumSamplesToDisplay(11);
            mockDrawer.Setup(x => x.getCanvasWidth()).Returns(31);
            displayer.DisplaySampleFrameFromStartIndex(10, 21, samples);

            List<Line> expectedLines = new List<Line>();
            // Test that all of the expected lines got called to be drawn
            for(int i = 0; i < 10; i++)
            {
                int expX1 = i * 3;
                int expY1 = samples[i + 10];
                int expX2 = i * 3 + 3;
                int expY2 = samples[i + 11];
                Line expectedLine = displayer.createLine(expX1, expY1, expX2, expY2);
                expectedLines.Add(expectedLine);
            }
            // The canvas should be cleared every time before drawing
            mockDrawer.Verify(x => x.drawLinesOnOscope(It.Is<List<Line>>(
                lines => ListOfLinesHaveEqualCoordinates(lines, expectedLines))), Times.Once);
        }

        /* If the nunmber of samples to display times the spacing is greater than canvas
         * width, then samples should still be displayed. The last line drawn need only have
         * its leftmost point within the canvas bounds, and its rightmost point can be past
         * the boundaries. */
        [TestMethod]
        public void testSpacingTimesNumSamplesToDisplayGreatherThanCanvasWidth()
        {
            Mock<OscopeWindowClient> mockDrawer = new Mock<OscopeWindowClient>();
            SampleFrameDisplayerImpl displayer
                = new SampleFrameDisplayerImpl(mockDrawer.Object, 10, 3);
            ushort[] samples = new ushort[21];
            for (int i = 0; i < 21; i++)
            {
                samples[i] = (ushort)(i * 2);
            }

            displayer.SetNumSamplesToDisplay(11);
            // Setting canvas width to 17. The line beginning at 15 and going to 18 should
            // still be drawn though.
            mockDrawer.Setup(x => x.getCanvasWidth()).Returns(17);
            displayer.DisplaySampleFrameFromStartIndex(10, 21, samples);

            // The canvas should be cleared every time before drawing
            List<Line> expLines = new List<Line>();

            // Test that all of the expected lines got called to be drawn
            for (int i = 0; i < 6; i++)
            {
                int expX1 = i * 3;
                int expY1 = samples[i + 10];
                int expX2 = i * 3 + 3;
                int expY2 = samples[i + 11];
                expLines.Add(displayer.createLine(expX1, expY1, expX2, expY2));
            }

            mockDrawer.Verify(x => x.drawLinesOnOscope(It.Is<List<Line>>
                (lines => ListOfLinesHaveEqualCoordinates(lines, expLines)))
                , Times.Once);
        }

        [TestMethod]
        public void TestSpacingOfTwo()
        {
            Mock<OscopeWindowClient> mockDrawer = new Mock<OscopeWindowClient>();
            SampleFrameDisplayerImpl displayer
                = new SampleFrameDisplayerImpl(mockDrawer.Object, 10, 2);
            ushort[] samples = new ushort[] { 0, 1, 2, 3 };

            displayer.SetNumSamplesToDisplay(3);
            mockDrawer.Setup(x => x.getCanvasWidth()).Returns(4);
            displayer.DisplaySampleFrameFromStartIndex(1, 4, samples);

            List<Line> expLines = new List<Line>();
            expLines.Add(CreateLine(0, 1, 2, 2));
            expLines.Add(CreateLine(2, 2, 4, 3));

            mockDrawer.Verify(x => x.drawLinesOnOscope(It.Is<List<Line>>
                (lines => ListOfLinesHaveEqualCoordinates(lines, expLines)))
                , Times.Once);
        }

        /* Number of samples to display, and spacing, are both parameters set
         * on the displayer. An exception should be thrown if these aren't enought o cover the canvas. */
        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void testSpacingTimesNumSamplesToDisplayNotEnoughToCoverCanvas()
        {
            Mock<OscopeWindowClient> mockDrawer = new Mock<OscopeWindowClient>();
            SampleFrameDisplayerImpl displayer
                = new SampleFrameDisplayerImpl(mockDrawer.Object, 10, 3);
            ushort[] samples = new ushort[21];
            for (int i = 0; i < 21; i++)
            {
                samples[i] = (ushort)(i * 2);
            }

            displayer.SetNumSamplesToDisplay(11);
            // Setting canvas width to 17. The line beginning at 15 and going to 18 should
            // still be drawn though.
            mockDrawer.Setup(x => x.getCanvasWidth()).Returns(32);

            displayer.DisplaySampleFrameFromStartIndex(10, 21, samples);
        }

        /* If there arn't enough samples to avialable in a frame to cover the desired number
        of samples to display, then this is an error. */
        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void testNumSamplesAvailableLessThanNumSamplesToDisplay()
        {
            Mock<OscopeWindowClient> mockDrawer = new Mock<OscopeWindowClient>();
            SampleFrameDisplayerImpl displayer
                = new SampleFrameDisplayerImpl(mockDrawer.Object, 10, 3);
            ushort[] samples = new ushort[21];
            for (int i = 0; i < 21; i++)
            {
                samples[i] = (ushort)(i * 2);
            }

            displayer.SetNumSamplesToDisplay(11);
            mockDrawer.Setup(x => x.getCanvasWidth()).Returns(20);

            // calling display samples with a start of 11 and 21 total, so 10 samples available.
            displayer.DisplaySampleFrameFromStartIndex(11, 21, samples);
        }
    }
}

