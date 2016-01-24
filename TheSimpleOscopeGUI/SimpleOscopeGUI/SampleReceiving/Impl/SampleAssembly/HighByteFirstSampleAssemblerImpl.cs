using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOscope.SampleReceiving.Impl.SampleAssembly
{
    public class HighByteFirstSampleAssemblerImpl : SampleAssembler
    {
        private SampleFrameAssembler sampleFrameAssembler;
        private ushort curSample;
        private int curByte;
        private double maxSampleSize = 1;
        private double oscopeHeight = 1;
        private double sampleScaler = 1;
        private double sampleOffset = 0;

        public HighByteFirstSampleAssemblerImpl(SampleFrameAssembler sampleFrameAssembler)
        {
            this.sampleFrameAssembler = sampleFrameAssembler;
        }

        public HighByteFirstSampleAssemblerImpl(SampleFrameAssembler sampleFrameAssembler
            , MainWindow mainWindow)
        {
            this.sampleFrameAssembler = sampleFrameAssembler;
            mainWindow.OscopeHeightChangedEvent += oscopeHeightChanged;
            mainWindow.MaxSampleSizeChangedEvent += maxSampleSizeChanged;
            mainWindow.SampleScalerChangedEvent += sampleScalerChanged;
            mainWindow.SampleOffsetChangedEvent += sampleOffsetChanged;
        }

        public void AddReceivedByte(byte newByte)
        {
            curSample = (ushort)(newByte + (curSample << (8 * curByte++)));

            if(curByte == 2)
            {
                ushort convertedSample = (ushort)(sampleOffset + curSample / maxSampleSize * oscopeHeight * sampleScaler);
                sampleFrameAssembler.SampleAssembled(convertedSample);
                curByte = 0;
                curSample = 0;
            }
        }

        private void sampleOffsetChanged(object sender, SampleOffsetChangedEventArgs args)
        {
            this.sampleOffset = args.sampleOffset;
        }

        private void maxSampleSizeChanged(object sender, MaxSampleSizeChangedEventArgs args)
        {
            this.maxSampleSize = args.maxSampleSize;
        }

        private void sampleScalerChanged(object sender, SampleScalerChangedEventArgs args)
        {
            this.sampleScaler = args.sampleScaler;
        }

        private void oscopeHeightChanged(object sender, OscopeHeightChangedEventArgs args)
        {
            this.oscopeHeight= args.newHeight;
        }
    }
}
