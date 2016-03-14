using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOscope
{
    public class AutoScaler
    {
        const int FRAMES_PER_CONFIG = 5;
        const int IDEAL_AVE_TRIGGERS = 10;

        EventHandler<TriggerLevelChangedEventArgs> triggerLevelChangedEvent;
        EventHandler<HorizontalResolutionConfigChangedEventArgs> horizontalConfigChangedEvent;

        List<HorizontalResolutionConfiguration> horizontalResolutionConfigs;

        int curFrameNumber = 0;
        int triggerCount = 0;

        int bestConfigIndex = -1;
        int bestTriggerLevel = -1;

        int curConfigIndex = 0;
        int curTriggerLevel;

        public AutoScaler(List<HorizontalResolutionConfiguration> horizontalResolutionConfigs
            , EventHandler<TriggerLevelChangedEventArgs> triggerLevelChangedEvent
            , EventHandler<HorizontalResolutionConfigChangedEventArgs> horizontalConfigChangedEvent
            , int minTriggerLevel
            , int maxTriggerLevel)
        {
            this.horizontalResolutionConfigs = horizontalResolutionConfigs;
            this.triggerLevelChangedEvent = triggerLevelChangedEvent;
            this.horizontalConfigChangedEvent = horizontalConfigChangedEvent;
            this.curTriggerLevel = minTriggerLevel;

            horizontalConfigChangedEvent(this
                , new HorizontalResolutionConfigChangedEventArgs(
                    horizontalResolutionConfigs[this.curConfigIndex++]));
            triggerLevelChangedEvent(this
                , new TriggerLevelChangedEventArgs(minTriggerLevel));
        }

        public void IncrementFrameNumber()
        {
            if(this.curFrameNumber++ >= FRAMES_PER_CONFIG)
            {
                if(this.curConfigIndex >= horizontalResolutionConfigs.Count)
                {
                    setBest();
                }
                else
                {
                    horizontalConfigChangedEvent(this
                        , new HorizontalResolutionConfigChangedEventArgs(
                            horizontalResolutionConfigs[this.curConfigIndex++]));
                    this.curFrameNumber = 0;
                }
            }
        }

        public void setBest()
        {

        }

        public void IncrementTriggerCount()
        {

        }
    }
}
