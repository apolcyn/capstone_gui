using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Linq;
using SimpleOscope.SampleReceiving;

namespace SimpleOscopeUnitTests
{
    public class BaseTests
    {
        public LineCoordinates CreateLineCoordinates(int x1, int y1, int x2, int y2)
        {
            return new LineCoordinates(x1, y1, x2, y2);
        }

        public bool LineCoordinateStructsEqual(LineCoordinates a, LineCoordinates b)
        {
            return a.x1 == b.x1 && a.x2 == b.x2 && a.y1 == b.y1 && a.y2 == b.y2;
        }

        public bool ListOfLineCoordinatesStructsEqual(List<LineCoordinates> a
            , List<LineCoordinates> b)
        {
            return a.Count == b.Count
                && Enumerable.Range(0, a.Count)
                .All(i => LineCoordinateStructsEqual(a[i], b[i]));
        }

        public bool LinesCoordinatesEqual(Line a, Line b)
        {
            return a.X1 == b.X1 && a.X2 == b.X2 && a.Y1 == b.Y1 && a.Y2 == b.Y2;
        }
    }
}
