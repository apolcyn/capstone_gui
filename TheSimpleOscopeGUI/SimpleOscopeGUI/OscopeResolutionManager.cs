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

        private int numVerticalDivisions { get; }
        private int numHorizontalDivisions { get; }

        public OscopeResolutionManager(SampleFrameAssembler sampleFrameAssembler
            , SampleFrameReceiver sampleFrameReceiver
            , OscopeWindowClient oscopeWindowClient, SerialPortClient serialPortClient)
        {
            this.sampleFrameAssembler = sampleFrameAssembler;
            this.sampleFrameReceiver = sampleFrameReceiver;
            this.oscopeWindowClient = oscopeWindowClient;
            this.serialPortClient = serialPortClient;
        }

        public void horizontalResolutionUpdated(int newTimePerDivision)
        {
            throw new NotImplementedException();
        }

        public void verticalResolutionUpdated(int newVoltagePerDivision)
        {
            throw new NotImplementedException();
        }
    }
}
