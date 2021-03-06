﻿using System;
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
        private LinearInterpolator linearInterpolator;

        public static HighByteFirstSampleAssemblerImpl newHighByteFirstSampleAssemblerImpl(
            SampleFrameAssembler sampleFrameAssembler, MainWindow mainWindow, LinearInterpolator linearInterpolator)
        {
            HighByteFirstSampleAssemblerImpl assembler 
                = new HighByteFirstSampleAssemblerImpl(sampleFrameAssembler);
            mainWindow.OscopeHeightChangedEvent += assembler.oscopeHeightChanged;
            mainWindow.MaxSampleSizeChangedEvent += assembler.maxSampleSizeChanged;
            mainWindow.SampleScalerChangedEvent += assembler.sampleScalerChanged;
            mainWindow.SampleOffsetChangedEvent += assembler.sampleOffsetChanged;

            assembler.linearInterpolator = linearInterpolator;
            return assembler;
        }

        public HighByteFirstSampleAssemblerImpl(SampleFrameAssembler sampleFrameAssembler)
        {
            this.sampleFrameAssembler = sampleFrameAssembler;
        }

        public void AddReceivedByte(byte newByte)
        {
            curSample = (ushort)(curSample + (newByte << (8 * curByte++)));

            if (curByte == 2)
            {
                if(newByte > 0x0f)
                {
                    throw new Exception();
                }
                if ((curSample & 0xf000) != 0) throw new Exception("" + curSample);
                ushort convertedSample = (ushort)linearInterpolator.sampleToPixel(curSample);
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
