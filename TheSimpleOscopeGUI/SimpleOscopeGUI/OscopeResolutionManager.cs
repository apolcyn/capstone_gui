using SimpleOscope.SampleReceiving;
using SimpleOscope.SampleReceiving.Impl.ByteReceiving;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication1
{
    class OscopeResolutionManager
    {
        private SampleFrameAssembler sampleFrameAssembler;
        private SampleFrameReceiver sampleFrameReceiver;
        private OscopeWindowClient oscopeWindowClient;
        private SerialPortClient serialPortClient;

        private int curHorizontalResolution;
        private int curVerticalResolution;
        private int curFrameSize;
        private int curADCSamplePeriod;

        private int numVerticalDivisions { get; }
        private int numHorizontalDivisions { get; }

        public OscopeResolutionManager(
            SampleFrameAssembler sampleFrameAssembler,
            SampleFrameReceiver sampleFrameReceiver,
            OscopeWindowClient oscopeWindowClient, 
            SerialPortClient serialPortClient)
        {
            this.sampleFrameAssembler = sampleFrameAssembler;
            this.sampleFrameReceiver = sampleFrameReceiver;
            this.oscopeWindowClient = oscopeWindowClient;
            this.serialPortClient = serialPortClient;
        }

        public void ADCSampleRateChanged(int newADCSamplePeriod)
        {
            this.curADCSamplePeriod = newADCSamplePeriod;
        }

         /* In general, when setting the horizontal time unit, our priorities are:
          * 1. Have the ADC sample rate as high as possible.
          * 2. Have as many samples on the screen as possible.
          * 
          * To achieve a certain horizontal time unit, we first see if we can reach it
          * by keeping the ADC's sample rate at a max, and only setting the distance between
          * samples (and thn possibly the sample frame size too to compensate).
          * If the desired horizontal time unit is too large to be achieved by only setting the
          * spacing between samples, then we can request to set the ADC sample rate to a lower
          * value, and then adjusting from there. 
          */
        public void horizontalResolutionChangeRequest(int newTimePerWholeWindow)
        {
            
            throw new NotImplementedException();
        }

        public void verticalResolutionUpdated(int newVoltagePerWholeWindow)
        {
            throw new NotImplementedException();
        }
    }
}
