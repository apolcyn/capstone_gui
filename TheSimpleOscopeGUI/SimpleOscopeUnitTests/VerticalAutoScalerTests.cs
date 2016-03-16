using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SimpleOscope;
using System.Collections;
using System.Collections.Generic;

namespace SimpleOscopeUnitTests
{
    [TestClass]
    public class VerticalAutoScalerTests
    {
        void changeVerticalOffsetDelegate(double newOffset)
        {

        }

        void changeVerticalScalerDelegate(double newScaler)
        {

        }

        void verticalScalerDoneDelegate(double bestOffset, double bestScaler)
        {

        }

      //  const double EXPECTED_BEST_OFFSET;
       // const double EXPECTED_BEST_SCALER;

        struct MinMax
        {
            int min;
            int max;
            MinMax(int min, int max)
            {
                this.min = min;
                this.max = max;
            }
        }

        List<double> offsets = new List<double>(new double[] { 0.0, 1.0, 2.0 });
        List<double> scalers = new List<double>(new double[] {0.4, 1.2,1.9});

        Hashtable minMaxTable = new Hashtable();

        void buildMinMaxTable()
        {
            foreach(double offset in offsets)
            {
                Hashtable scalersTable = new Hashtable();
                foreach(double scaler in scalers)
                {
                   // scalersTable.Add(scaler, )
                }
            }
        }



        [TestMethod]
        public void TestVerticalAutoScalerNormalCase()
        {

        }
    }
}
