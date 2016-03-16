using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOscope
{ 
    public delegate void AutoScalingCompleteDelegate(int bestTriggerLevel, int bestConfigIndex);

    public delegate void HorizontalResolutionConfigIndexChangedDelegate(int newHorizontalResolutionConfigIndex);

    public delegate void TriggerLevelChangedDelegate(int newTriggerLevel);

    public class AutoScaler
    {
        public const int FRAMES_PER_TRIGGER_LEVEL = 5;
        int idealAveTriggers;

        HorizontalResolutionConfigIndexChangedDelegate horizontalResolutionConfigIndexChangedDelegate;

        TriggerLevelChangedDelegate triggerLevelChangedDelegate;

        AutoScalingCompleteDelegate autoScalingCompleteDelegate;

        List<int> horizontalResolutionConfigIndices;

        int curFrameNumber = 0;
        int triggerCount = 0;

        int bestConfigIndicesIndex = -1;
        int bestTriggerIndex = -1;

        double bestAveNumTriggersSoFar = 0.0;

        int curConfigIndicesIndex = 0;
        int curTriggerIndex = 0;

        List<int> triggersToTry;

        private object triggerFrameIncrementLockObject = new object();

        public AutoScaler(List<int> horizontalResolutionConfigIndices
            , TriggerLevelChangedDelegate triggerLevelChangedDelegate
            , HorizontalResolutionConfigIndexChangedDelegate horizontalResolutionConfigIndexChangedDelegate
            , AutoScalingCompleteDelegate autoScalingCompleteDelegate
            , List<int> triggersToTry
            , int idealAveTriggers)
        {
            this.idealAveTriggers = idealAveTriggers;
            this.triggersToTry = triggersToTry;
            this.autoScalingCompleteDelegate = autoScalingCompleteDelegate;
            this.horizontalResolutionConfigIndices = horizontalResolutionConfigIndices;
            this.triggerLevelChangedDelegate = triggerLevelChangedDelegate;
            this.horizontalResolutionConfigIndexChangedDelegate = horizontalResolutionConfigIndexChangedDelegate;

            horizontalResolutionConfigIndexChangedDelegate(horizontalResolutionConfigIndices[0]);
            triggerLevelChangedDelegate(triggersToTry[0]);
        }

        public void IncrementFrameNumber()
        {
            lock(triggerFrameIncrementLockObject)
            {
                if (++this.curFrameNumber >= FRAMES_PER_TRIGGER_LEVEL)
                {
                    double aveTriggers = (double)this.triggerCount / FRAMES_PER_TRIGGER_LEVEL;
                    if (newAveNumTriggersIsBestSoFar(aveTriggers))
                    {
                        this.bestAveNumTriggersSoFar = aveTriggers;
                        this.bestTriggerIndex = this.curTriggerIndex;
                        this.bestConfigIndicesIndex = this.curConfigIndicesIndex;
                    }
                    this.curFrameNumber = 0;
                    this.triggerCount = 0;

                    if (++this.curTriggerIndex >= triggersToTry.Count)
                    {
                        if (++this.curConfigIndicesIndex >= horizontalResolutionConfigIndices.Count)
                        {
                            autoScalingCompleteDelegate(this.triggersToTry[this.bestTriggerIndex]
                                , this.horizontalResolutionConfigIndices[this.bestConfigIndicesIndex]);
                            return;
                        }

                        horizontalResolutionConfigIndexChangedDelegate(
                            this.horizontalResolutionConfigIndices[this.curConfigIndicesIndex]);

                        this.curTriggerIndex = 0;
                    }

                    triggerLevelChangedDelegate(this.triggersToTry[this.curTriggerIndex]);
                }
            }
        }

        public void IncrementTriggerCount()
        {
            lock(triggerFrameIncrementLockObject)
            {
                ++triggerCount;
            }
        }

        private bool newAveNumTriggersIsBestSoFar(double aveNumTriggers)
        {
            return Math.Abs(aveNumTriggers - this.idealAveTriggers)
                < Math.Abs(this.bestAveNumTriggersSoFar - this.idealAveTriggers);
        }
    }
}
