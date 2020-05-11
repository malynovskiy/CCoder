using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace CCoder.Controls
{
    public class DrawingElement : FrameworkElement
    { 
        public DrawingElement()
        {
            drawingVisual = new DrawingVisual();
            visualsCollection = new VisualCollection(this);

            visualsCollection.Add(drawingVisual);
        }

        public DrawingContext GetContext()
        {
            return drawingVisual.RenderOpen();
        }

        protected override int VisualChildrenCount
        {
            get { return visualsCollection.Count; }
        }

        protected override Visual GetVisualChild(int i)
        {
            if (i < 0 || i >= visualsCollection.Count)
                throw new ArgumentOutOfRangeException();
            return visualsCollection[i];
        }

        private DrawingVisual drawingVisual;
        private VisualCollection visualsCollection;
    }
}
