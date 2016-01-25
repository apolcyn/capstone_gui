using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOscope.SampleReceiving.Impl
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

        public static HighByteFirstSampleAssemblerImpl newHighByteFirstSampleAssemblerImpl(
            SampleFrameAssembler sampleFrameAssembler, MainWindow mainWindow)
        {
            HighByteFirstSampleAssemblerImpl assembler 
                = new HighByteFirstSampleAssemblerImpl(sampleFrameAssembler);
            mainWindow.OscopeHeightChangedEvent += assembler.oscopeHeightChanged;
            mainWindow.MaxSampleSizeChangedEvent += assembler.maxSampleSizeChanged;
            mainWindow.SampleScalerChangedEvent += assembler.sampleScalerChanged;
            mainWindow.SampleOffsetChangedEvent += assembler.sampleOffsetChanged;
            return assembler;
        }

        public HighByteFirstSampleAssemblerImpl(SampleFrameAssembler sampleFrameAssembler)
        {
            this.sampleFrameAssembler = sampleFrameAssembler;
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

        public void sampleOffsetChanged(object sender, SampleOffsetChangedEventArgs args)
        {
            this.sampleOffset = args.sampleOffset;
        }

        public void maxSampleSizeChanged(object sender, MaxSampleSizeChangedEventArgs args)
        {
            this.maxSampleSize = args.maxSampleSize;
        }

        public void sampleScalerChanged(object sender, SampleScalerChangedEventArgs args)
        {
            this.sampleScaler = args.sampleScaler;
        }

        public void oscopeHeightChanged(object sender, OscopeHeightChangedEventArgs args)
        {
            this.oscopeHeight= args.newHeight;
        }
    }
}
