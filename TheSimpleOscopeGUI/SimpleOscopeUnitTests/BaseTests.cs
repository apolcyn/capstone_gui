using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Linq;

namespace SimpleOscopeUnitTests
{
    public class BaseTests
    {
        public Line CreateLine(int x1, int y1, int x2, int y2)
        {
            Line line = new Line();
            line.X1 = x1;
            line.X2 = x2;
            line.Y1 = y1;
            line.Y2 = y2;
            return line;
        }

        public bool LinesCoordinatesEqual(Line a, Line b)
        {
            return a.X1 == b.X1 && a.X2 == b.X2 && a.Y1 == b.Y1 && a.Y2 == b.Y2;
        }

        public bool ListOfLinesHaveEqualCoordinates(List<Line> a, List<Line> b)
        {
            return a.Count == b.Count
                && Enumerable.Range(0, a.Count)
                .All<int>(i => LinesCoordinatesEqual(a[i], b[i]));
        }
    }
}
