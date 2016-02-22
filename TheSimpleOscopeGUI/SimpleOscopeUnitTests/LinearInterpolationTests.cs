using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleOscope;

namespace SimpleOscopeUnitTests
{
    [TestClass]
    public class LinearInterpolationTests
    {
        public event EventHandler<PixelVoltageRelationshipChangedEventArgs> PixelVoltageRelationshipChangedEvent;
        public event EventHandler<SampleToVoltageRelationshipChangedEventArgs> SampleToVoltageRelationshipChangedEvent;

        [TestMethod]
        public void TestPixelsToVoltageAndVoltageToPixels()
        {
            int lowerPixel = 0;
            double lowerVoltage = 0;
            int upperPixel = 301;
            double upperVoltage = 5.0;

            LinearInterpolator interpolator = new LinearInterpolator();

            PixelVoltageRelationshipChangedEvent += interpolator.pixelVoltageRelationshipChanged;

            PixelVoltageRelationshipChangedEvent(this
                , new PixelVoltageRelationshipChangedEventArgs(lowerVoltage, lowerPixel
                , upperVoltage, upperPixel));

            Assert.AreEqual(upperVoltage, interpolator.pixelToVoltage(upperPixel));
            Assert.AreEqual(lowerVoltage, interpolator.pixelToVoltage(lowerPixel));

            Assert.AreEqual(upperPixel, interpolator.voltageToPixel(upperVoltage));
            Assert.AreEqual(lowerPixel, interpolator.voltageToPixel(lowerVoltage));

            //Assert.AreEqual((upperVoltage - lowerVoltage) / 2.0, interpolator.pixelToVoltage((upperPixel - lowerPixel) / 2));
            //Assert.AreEqual((upperPixel - lowerPixel) / 2.0, interpolator.voltageToPixel((upperVoltage - lowerVoltage) / 2));
        }

        [TestMethod]
        public void TestSamplesToVoltageAndVoltageToSamples()
        {
            double lowerVoltage = 0;
            double upperVoltage = 5;
            int lowerSample = 0;
            int upperSample = 4095;

            LinearInterpolator interpolator = new LinearInterpolator();

            SampleToVoltageRelationshipChangedEvent += interpolator.sampleToVoltageRelationShipChanged;

            SampleToVoltageRelationshipChangedEvent(this
                , new SampleToVoltageRelationshipChangedEventArgs(lowerVoltage, lowerSample
                , upperVoltage, upperSample));

            Assert.AreEqual(lowerVoltage, interpolator.sampleToVoltage(lowerSample));
            Assert.AreEqual(upperVoltage, interpolator.sampleToVoltage(upperSample));
        }

        [TestMethod]
        public void TestSamplesToPixel()
        {
            double lowerVoltage = 0;
            double upperVoltage = 5;
            int lowerSample = 0;
            int upperSample = 4095;
            int lowerPixel = 0;
            int upperPixel = 301;

            LinearInterpolator interpolator = new LinearInterpolator();

            SampleToVoltageRelationshipChangedEvent += interpolator.sampleToVoltageRelationShipChanged;

            SampleToVoltageRelationshipChangedEvent(this
                , new SampleToVoltageRelationshipChangedEventArgs(lowerVoltage, lowerSample
                , upperVoltage, upperSample));

            PixelVoltageRelationshipChangedEvent += interpolator.pixelVoltageRelationshipChanged;

            PixelVoltageRelationshipChangedEvent(this
                , new PixelVoltageRelationshipChangedEventArgs(lowerVoltage, lowerPixel
                , upperVoltage, upperPixel));

            Assert.AreEqual(lowerPixel, interpolator.sampleToPixel(lowerSample));
            Assert.AreEqual(upperPixel, interpolator.sampleToPixel(upperSample));
        }
    }
}
