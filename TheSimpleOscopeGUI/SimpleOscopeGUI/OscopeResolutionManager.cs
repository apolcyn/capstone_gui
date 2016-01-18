using SimpleOscope.SampleReceiving;
using SimpleOscope.SampleReceiving.Impl.ByteReceiving;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication1
{
    public class OscopeResolutionManager
    {
        private SampleFrameAssembler sampleFrameAssembler;
        private SampleFrameReceiver sampleFrameReceiver;
        private SampleFrameDisplayer sampleFrameDispalyer;
        public virtual OscopeWindowClient oscopeWindowClient { get; set; }
        private SerialPortClient serialPortClient;

        private int curHorizontalResolution;
        private int curVerticalResolution;
        private int curFrameSize;
        public virtual int curADCSamplePeriod { get; set; }
        private int curHorizontalShift;
        // TODO: private int curVerticalShift;

        private int numVerticalDivisions { get; }
        private int numHorizontalDivisions { get; }

        public OscopeResolutionManager() { }

        public OscopeResolutionManager(
            SampleFrameAssembler sampleFrameAssembler,
            SampleFrameReceiver sampleFrameReceiver,
            OscopeWindowClient oscopeWindowClient, 
            SerialPortClient serialPortClient,
            SampleFrameDisplayer sampleFrameDispalyer)
        {
            this.sampleFrameAssembler = sampleFrameAssembler;
            this.sampleFrameReceiver = sampleFrameReceiver;
            this.oscopeWindowClient = oscopeWindowClient;
            this.serialPortClient = serialPortClient;
            this.sampleFrameDispalyer = sampleFrameDispalyer;
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
          * samples (and then possibly the sample frame size too to compensate).
          * If the desired horizontal time unit is too large to be achieved by only setting the
          * spacing between samples, then we can request to set the ADC sample rate to a lower
          * value, and then adjusting from there. 
          */
        public virtual void HorizontalResolutionChangeRequest(int newTimePerWholeWindow)
        {
            // first see if its possible to achieve the desired time unit
            // by only adjusting the spacing between samples.
            if(curADCSamplePeriod * (oscopeWindowClient.getCanvasWidth() - 1) 
                >= newTimePerWholeWindow)
            {
                AdjustSampleSpacing(newTimePerWholeWindow);
            }
            else
            {
                AdjustADCSampleRate(newTimePerWholeWindow);
            }
            throw new NotImplementedException();
        }

        public virtual int AdjustSampleSpacing(int newTimePerWholeWindow)
        {
            int idealNumSamplesInWindow = newTimePerWholeWindow / curADCSamplePeriod + 1;
            return 0;
        }

        public virtual void AdjustADCSampleRate(int newTimePerWholeWindow)
        {
            throw new NotImplementedException();
        }

        public virtual void verticalResolutionUpdated(int newVoltagePerWholeWindow)
        {
            throw new NotImplementedException();
        }
    }
}
