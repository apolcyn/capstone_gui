using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SimpleOscope;
using System.Collections.Generic;

namespace SimpleOscopeUnitTests
{
    [TestClass]
    public class AutoScalerTests
    {
        private void autoScalingComplete(int bestTriggerLevel, int bestConfigIndex)
        {
            Assert.AreEqual(BEST_TRIGGER_LEVEL, bestTriggerLevel);
            Assert.AreEqual(BEST_CONFIG_INDEX, bestConfigIndex);
            autoScaleCompleteCalled = true;
        }

        private void triggerLevelChanged(int newTriggerLevel)
        {
            triggerLevelChangedCallCount++;
        }

        private void horizontalResolutionConfigChanged(int newConfigIndex)
        {
            configChangedCallCount++;
        }

        private int[][] triggerCountTable = new int[][]
        {
            new int[] {0, 3, 6, 2, 7}
            , new int[] {5, 22, 6, 8, 5}
            , new int[] {2, 10, 4, 9, 3}
        };

        private const int BEST_CONFIG_INDEX = 2; // these indices index the best
        private const int BEST_TRIGGER_LEVEL = 3; // value in triggerCountTable
        private const int IDEAL_AVE_TRIGGERS = 8;

        private bool autoScaleCompleteCalled = false;

        // note that all values in both these arrays need to match their indices to this test to work
        private int[] triggersToTry = new int[] { 0, 1, 2, 3, 4 };
        private int[] horizontalResolutionConfigIndices = new int[] { 0, 1, 2};

        private int configChangedCallCount = 0;
        private int triggerLevelChangedCallCount = 0;

        [TestMethod]
        public void TestAutoScalerNormalCase()
        {
            AutoScaler autoScaler = new AutoScaler(new List<int>(horizontalResolutionConfigIndices)
                , triggerLevelChanged
                , horizontalResolutionConfigChanged
                , autoScalingComplete
                , new List<int>(triggersToTry)
                , IDEAL_AVE_TRIGGERS);

            foreach(int configIndex in horizontalResolutionConfigIndices)
            {
                foreach (int triggerLevel in triggersToTry)
                {
                    for(int i = 0; i < AutoScaler.FRAMES_PER_TRIGGER_LEVEL; i++)
                    {
                        autoScaler.IncrementFrameNumber();
                        for(int k = 0; k < triggerCountTable[configIndex][triggerLevel]; k++)
                        {
                            autoScaler.IncrementTriggerCount();
                        }
                    }
                }
            }

            Assert.AreEqual(this.horizontalResolutionConfigIndices.Length, this.configChangedCallCount);
            Assert.AreEqual(this.horizontalResolutionConfigIndices.Length
                * this.triggersToTry.Length, this.triggerLevelChangedCallCount);
            Assert.IsTrue(this.autoScaleCompleteCalled);
        }
    }
}
