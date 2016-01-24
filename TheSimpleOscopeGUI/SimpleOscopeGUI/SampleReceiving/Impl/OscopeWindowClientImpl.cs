using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace SimpleOscope.SampleReceiving.Impl
{

    public class OscopeWindowClientImpl : OscopeWindowClient
    {
        private Canvas scopeCanvas { get; set; }
        private int oscopeWidth;

        public OscopeWindowClientImpl() { }

        public OscopeWindowClientImpl(Canvas scopeCanvas, MainWindow mainWindow)
        {
            this.scopeCanvas = scopeCanvas;
            this.oscopeWidth = oscopeWidth;
            mainWindow.OscopeWidthChangedEvent += oscopeWidthChanged;
        }

        public void oscopeWidthChanged(object sender, OscopeWidthChangedEventArgs args)
        {
            this.oscopeWidth = args.newWidth;
        }

        /// <summary>
        ///  TODO: make this adapt to different screen sizes
        /// </summary>
        /// <returns></returns>
        public virtual int getCanvasWidth()
        {
            throw new NotImplementedException("shouldn't be calling this.");
        }

        public void clearScopeCanvas()
        {
            while (this.scopeCanvas.Children.Count > 0)
            {
                this.scopeCanvas.Children.RemoveAt(0);
            }
        }

        /* Creates a line to draw on the oscope canvas */
        private Line createLine(int prevX, int prevY, int curX, int curY)
        {
            Line line = new Line();
            line.Stroke = System.Windows.Media.Brushes.Gold;
            line.X1 = prevX;
            line.X2 = curX;
            line.Y1 = prevY;
            line.Y2 = curY;
            line.HorizontalAlignment = HorizontalAlignment.Left;
            line.VerticalAlignment = VerticalAlignment.Center;
            line.StrokeThickness = 4;
            return line;
        }

        public delegate void drawLinesOnCanvasUIThread(List<LineCoordinates> lines);

        public void drawLinesOnCanvasUIThreadImpl(List<LineCoordinates> lines)
        {
            clearScopeCanvas();
            lines.ForEach(l => this.scopeCanvas.Children.Add(createLine(l.x1, l.y1, l.x2, l.y2)));
        }


        public virtual void drawLinesOnOscope(List<LineCoordinates> lines)
        {
            this.scopeCanvas.Dispatcher.BeginInvoke(new drawLinesOnCanvasUIThread(drawLinesOnCanvasUIThreadImpl), new object[] { lines });
        }
    }
}
