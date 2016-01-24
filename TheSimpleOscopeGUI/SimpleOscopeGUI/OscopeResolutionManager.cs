﻿using SimpleOscope.SampleReceiving;
using SimpleOscope.SampleReceiving.Impl.ByteReceiving;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOscope
{
    public class OscopeResolutionManager
    {
        private SampleFrameAssembler sampleFrameAssembler;
        private SampleFrameReceiver sampleFrameReceiver;
        private SampleFrameDisplayer sampleFrameDisplayer;
        public virtual OscopeWindowClient oscopeWindowClient { get; set; }
        private SerialPortClient serialPortClient;
        private HorizontalResolutionConfiguration[] horizontalResolutionConfigs { get; set; }
        private ulong expectedPsocSPS;
        private uint expectedFrameSize;

        private int curHorizontalResolution;
        private int curVerticalResolution;
        private int curFrameSize;
        public virtual int curADCSamplePeriod { get; set; }
        private int curHorizontalShift;
        // TODO: private int curVerticalShift;

        private int numVerticalDivisions { get; }
        private int numHorizontalDivisions { get; }

        public OscopeResolutionManager() { }

        public static OscopeResolutionManager newOscopeResolutionManager(
            SampleFrameAssembler sampleFrameAssembler,
            SampleFrameReceiver sampleFrameReceiver,
            OscopeWindowClient oscopeWindowClient,
            SerialPortClient serialPortClient,
            SampleFrameDisplayer sampleFrameDispalyer,
            HorizontalResolutionConfiguration[] horizontalResolutionConfigs)
        {
            OscopeResolutionManager resManager = new OscopeResolutionManager(
                sampleFrameAssembler,
                sampleFrameReceiver,
                oscopeWindowClient,
                serialPortClient,
                sampleFrameDispalyer);
            resManager.horizontalResolutionConfigs = horizontalResolutionConfigs;

            return resManager;
        }

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
            this.sampleFrameDisplayer = sampleFrameDispalyer;
        }

        /// <summary>
        /// Requests that settings be changed to enact a certain time/division
        /// on the oscope screen.
        /// </summary>
        /// <param name="horizontalConfigIndex">An index
        /// into the horizontal reslution configuration constants table.</param>
        public void RequestTimePerDivisionChange(int horizontalConfigIndex)
        {
            HorizontalResolutionConfiguration config
                = horizontalResolutionConfigs[horizontalConfigIndex];
            serialPortClient.SendPsocCommand(String.Format("#AS{0}#", config.psocSPS));
            serialPortClient.SendPsocCommand(String.Format("#AF{0}#", config.frameSize));
            this.sampleFrameDisplayer.SetSpacing(config.pixelSpacing);
            this.expectedPsocSPS = config.psocSPS;
            this.expectedFrameSize = config.frameSize;
        }

        /// <summary>
        /// Requests to change the voltage per division on the oscope screen.
        /// </summary>
        /// <param name="voltagePerDivision">
        /// the desired voltage per division.</param>
        public void RequestVoltagePerDivisionChange(double voltagePerDivision)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Requests to shift the horizontal position on the 
        /// oscope window that the triggering point is displayed at.
        /// </summary>
        /// <param name="triggerHorizontalPosition">
        /// The desired pixel number on the oscope window
        /// to display the triggering point at.</param>
        public void ShiftTiggerHorizontally(uint triggerHorizontalPosition)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Requests to change the vertical position on the oscope
        /// screen of where zero volts is.
        /// </summary>
        /// <param name="positionOfZeroVolts">
        /// The desired vertical pixel number on the oscope window
        /// where zero volts should be</param>
        public void ShiftVoltageOffset(uint positionOfZeroVolts)
        {
            throw new NotImplementedException();
        }
    }
}
