using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOscope
{
    public delegate void UpdateOffsetAndScalerDelegate(double offset, double scaler);
    public delegate void VerticalAutoScalerDoneDelegate(double bestVerticalOffset
        , double bestVerticalScaler);

    public class VerticalAutoScaler
    {
        public const int NUM_FRAMES_PER_SETTING = 1;

        public const double IDEAL_WINDOW_COVERAGE = 0.5;

        private List<double> offsets;
        private List<double> scalers;

        double bestVerticalOffset = Double.MinValue;
        double bestVerticalScaling = Double.MinValue;

        private int numFramesReceivedForCurSetting;
        int curOffsetsIndex = 0;
        int curScalersIndex = 0;

        int minForCurSetting;
        int maxForCurSetting;

        int bestOffsetSoFarIndex = -1;
        int bestScalersSoFarIndex = -1;

        double nearestToIdealSoFar = Double.MaxValue;

        private int windowBottom;
        private int windowTop;

        private UpdateOffsetAndScalerDelegate changeOffsetAndScalerDelegate;
        private VerticalAutoScalerDoneDelegate verticalAutoScalerDoneDelegate;

        public VerticalAutoScaler(UpdateOffsetAndScalerDelegate changeOffsetAndScalerDelegate
            , VerticalAutoScalerDoneDelegate verticalAutoScalerDoneDelegate
            , List<double> offsets
            , List<double> scalers
            , int windowBottom
            , int windowTop)
        {
            this.changeOffsetAndScalerDelegate = changeOffsetAndScalerDelegate;
            this.verticalAutoScalerDoneDelegate = verticalAutoScalerDoneDelegate;
            this.offsets = offsets;
            this.scalers = scalers;
            this.windowBottom = windowBottom;
            this.windowTop = windowTop;

            changeOffsetAndScalerDelegate(offsets[0], scalers[0]);
        }

        public void FrameReceived(int minPixel, int maxPixel)
        {
            this.maxForCurSetting = Math.Max(this.maxForCurSetting, maxPixel);
            this.minForCurSetting = Math.Min(this.minForCurSetting, minPixel);

            if(++numFramesReceivedForCurSetting >= NUM_FRAMES_PER_SETTING)
            {
                updateBestSoFar();

                this.numFramesReceivedForCurSetting = 0;

                if(++curScalersIndex >= scalers.Count)
                {
                    if(++curOffsetsIndex >= offsets.Count)
                    {
                        verticalAutoScalerDoneDelegate(offsets[bestOffsetSoFarIndex]
                            , scalers[bestScalersSoFarIndex]);
                        return;
                    }

                    curScalersIndex = 0;
                }

                changeOffsetAndScalerDelegate(offsets[curOffsetsIndex], scalers[curScalersIndex]);

                this.maxForCurSetting = int.MinValue;
                this.minForCurSetting = int.MaxValue;
            }
        }

        private void updateBestSoFar()
        {
            if (this.minForCurSetting < windowBottom || this.maxForCurSetting > windowTop
                || this.maxForCurSetting == int.MinValue || this.minForCurSetting == int.MaxValue)
            {
                return;
            }

            double upperWindowIdeal = (windowTop - windowBottom) * 0.75 + windowBottom;
            double upperMetric = Math.Pow(Math.Abs(this.maxForCurSetting - upperWindowIdeal), 2);

            double lowerWindowIdeal = (windowTop - windowBottom) * 0.25 + windowBottom;
            double lowerMetric = Math.Pow(Math.Abs(this.minForCurSetting - lowerWindowIdeal), 2);


            if (lowerMetric + upperMetric < nearestToIdealSoFar)
            {
                this.bestOffsetSoFarIndex = this.curOffsetsIndex;
                this.bestScalersSoFarIndex = this.curScalersIndex;
                this.nearestToIdealSoFar = lowerMetric + upperMetric;
            }
                        /*   double spread = this.maxForCurSetting - this.minForCurSetting;

               double scaleMetric = Math.Abs(spread / (windowTop - windowBottom) - IDEAL_WINDOW_COVERAGE);

               double meanForCurSetting = (this.maxForCurSetting + this.minForCurSetting) / 2.0;
               double meanForWindow = (this.windowTop + this.windowBottom) / 2.0;

               double averageMetric = Math.Abs(meanForCurSetting - meanForWindow) / meanForWindow;

               if (scaleMetric + averageMetric < nearestToIdealSoFar)
               {
                   this.bestOffsetSoFarIndex = this.curOffsetsIndex;
                   this.bestScalersSoFarIndex = this.curScalersIndex;
                   this.nearestToIdealSoFar = scaleMetric + averageMetric;
               } */
        }
    }
}
