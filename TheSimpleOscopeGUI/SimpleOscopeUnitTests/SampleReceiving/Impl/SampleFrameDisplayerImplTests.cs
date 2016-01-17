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

namespace SimpleOscopeUnitTests.SampleReceiving.Impl
{
    [TestClass]
    public class SampleFrameDisplayerImplTests
    {
        Button button = new Button();
        Dispatcher fakeDispatcher;

        [TestInitialize]
        public void setup()
        {
            fakeDispatcher = button.Dispatcher;
        }

        [TestMethod]
        public void testNormalCase()
        {
            Mock<OscopeWindowClient> mockDrawer = new Mock<OscopeWindowClient>();
            SampleFrameDisplayer displayer 
                = new SampleFrameDisplayerImpl(mockDrawer.Object, 10, 3, fakeDispatcher);
            ushort[] samples = new ushort[21];
            for(int i = 0; i < 21; i++)
            {
                samples[i] = (ushort)(i * 2);
            }

            displayer.SetNumSamplesToDisplay(11);
            mockDrawer.Setup(x => x.getCanvasWidth()).Returns(31);
            displayer.DisplaySampleFrame(10, 21, samples);

            // The canvas should be cleared every time before drawing
            mockDrawer.Verify(x => x.clearScopeCanvas(), Times.Once());

            // Test that all of the expected lines got called to be drawn
            for(int i = 0; i < 10; i++)
            {
                int expX1 = i * 3;
                int expY1 = samples[i + 10];
                int expX2 = i * 3 + 3;
                int expY2 = samples[i + 11];
                mockDrawer.Verify(x => x.drawOscopeLine(expX1, expY1, expX2, expY2), Times.Once);
            }

            mockDrawer.Verify(x => x.clearScopeCanvas(), Times.Once());
            mockDrawer.Verify(x => x.drawOscopeLine(It.IsAny<int>()
                , It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(10));
        }

        /* If the nunmber of samples to display times the spacing is greater than canvas
         * width, then samples should still be displayed. The last line drawn need only have
         * its leftmost point within the canvas bounds, and its rightmost point can be past
         * the boundaries. */
        [TestMethod]
        public void testSpacingTimesNumSamplesToDisplayGreatherThanCanvasWidth()
        {
            Mock<OscopeWindowClient> mockDrawer = new Mock<OscopeWindowClient>();
            SampleFrameDisplayer displayer
                = new SampleFrameDisplayerImpl(mockDrawer.Object, 10, 3, fakeDispatcher);
            ushort[] samples = new ushort[21];
            for (int i = 0; i < 21; i++)
            {
                samples[i] = (ushort)(i * 2);
            }

            displayer.SetNumSamplesToDisplay(11);
            // Setting canvas width to 17. The line beginning at 15 and going to 18 should
            // still be drawn though.
            mockDrawer.Setup(x => x.getCanvasWidth()).Returns(17);
            displayer.DisplaySampleFrame(10, 21, samples);

            // The canvas should be cleared every time before drawing
            mockDrawer.Verify(x => x.clearScopeCanvas(), Times.Once());

            // Test that all of the expected lines got called to be drawn
            for (int i = 0; i < 6; i++)
            {
                int expX1 = i * 3;
                int expY1 = samples[i + 10];
                int expX2 = i * 3 + 3;
                int expY2 = samples[i + 11];
                mockDrawer.Verify(x => x.drawOscopeLine(expX1, expY1, expX2, expY2), Times.Once);
            }

            mockDrawer.Verify(x => x.clearScopeCanvas(), Times.Once());
            mockDrawer.Verify(x => x.drawOscopeLine(It.IsAny<int>()
                , It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(6));
        }

        /* Number of samples to display, and spacing, are both parameters set
         * on the displayer. An exception should be thrown if these aren't enought o cover the canvas. */
        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void testSpacingTimesNumSamplesToDisplayNotEnoughToCoverCanvas()
        {
            Mock<OscopeWindowClient> mockDrawer = new Mock<OscopeWindowClient>();
            SampleFrameDisplayer displayer
                = new SampleFrameDisplayerImpl(mockDrawer.Object, 10, 3, fakeDispatcher);
            ushort[] samples = new ushort[21];
            for (int i = 0; i < 21; i++)
            {
                samples[i] = (ushort)(i * 2);
            }

            displayer.SetNumSamplesToDisplay(11);
            // Setting canvas width to 17. The line beginning at 15 and going to 18 should
            // still be drawn though.
            mockDrawer.Setup(x => x.getCanvasWidth()).Returns(32);

            displayer.DisplaySampleFrame(10, 21, samples);
        }

        /* If there arn't enough samples to avialable in a frame to cover the desired number
        of samples to display, then this is an error. */
        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void testNumSamplesAvailableLessThanNumSamplesToDisplay()
        {
            Mock<OscopeWindowClient> mockDrawer = new Mock<OscopeWindowClient>();
            SampleFrameDisplayer displayer
                = new SampleFrameDisplayerImpl(mockDrawer.Object, 10, 3, fakeDispatcher);
            ushort[] samples = new ushort[21];
            for (int i = 0; i < 21; i++)
            {
                samples[i] = (ushort)(i * 2);
            }

            displayer.SetNumSamplesToDisplay(11);
            mockDrawer.Setup(x => x.getCanvasWidth()).Returns(20);

            // calling display samples with a start of 11 and 21 total, so 10 samples available.
            displayer.DisplaySampleFrame(11, 21, samples);
        }
    }
}

