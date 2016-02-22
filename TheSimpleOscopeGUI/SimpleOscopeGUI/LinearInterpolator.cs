using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOscope
{
    public class PixelVoltageRelationshipUpdatedEventArgs { }

    public class LinearInterpolator
    {
        private int lowerSample_VS, upperSample_VS;
        private double lowerVoltage_VS, upperVoltage_VS;

        private int lowerPixel_PV, upperPixel_PV;
        private double lowerVoltage_PV, upperVoltage_PV;

        public event EventHandler<PixelVoltageRelationshipUpdatedEventArgs> 
            PixelVoltageRelationshipUpdatedEvent;

        public double pixelToVoltage(int pixel)
        {
            return interpolate(lowerPixel_PV, lowerVoltage_PV, upperPixel_PV, upperVoltage_PV, pixel);
        }

        public int voltageToPixel(double voltage)
        {
            return (int)interpolate(lowerVoltage_PV, lowerPixel_PV, upperVoltage_PV, upperPixel_PV, voltage);
        }

        public double sampleToVoltage(int sample)
        {
            return interpolate(lowerSample_VS, lowerVoltage_VS, upperSample_VS, upperVoltage_VS, sample);
        }

        public int sampleToPixel(int sample)
        {
            return voltageToPixel(sampleToVoltage(sample));
        }

        private double interpolate(double x1, double y1, double x2, double y2, double xVal)
        {
            double slope = (y2 - y1) / (x2 - x1);
            return (xVal - x1) * slope + y1;
        }

        public void pixelVoltageRelationshipChanged(object sender, PixelVoltageRelationshipChangedEventArgs args)
        {
            this.lowerPixel_PV = (int)args.lowerPixel;
            this.upperPixel_PV = (int)args.upperPixel;
            this.lowerVoltage_PV = args.lowerVoltage;
            this.upperVoltage_PV = args.upperVoltage;

            if(PixelVoltageRelationshipUpdatedEvent != null)
            {
                PixelVoltageRelationshipUpdatedEvent(this
                    , new PixelVoltageRelationshipUpdatedEventArgs());
            }
        }

        public void sampleToVoltageRelationShipChanged(object sender, SampleToVoltageRelationshipChangedEventArgs args)
        {
            this.lowerSample_VS = args.lowerSample;
            this.upperSample_VS = args.upperSample;
            this.lowerVoltage_VS = args.lowerVoltage;
            this.upperVoltage_VS = args.upperVoltage;
        }
    }
}
