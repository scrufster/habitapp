using System;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MergedProgram.Controls
{
    class Graphics_Canvas : Canvas
    {
        private List<DrawingVisual> visuals = new List<DrawingVisual>();

        protected override int VisualChildrenCount
        {
            get { return visuals.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return visuals[index];
        }

        public void AddVisual(DrawingVisual visual)
        {
            visuals.Add(visual);
          
            base.AddVisualChild(visual);
            base.AddLogicalChild(visual);
        }

        public bool ContainsVisual(DrawingVisual visual)
        {
            return visuals.Contains(visual);
        }

        public void DeleteVisual(int index)
        {
            if (index >= 0 && index < visuals.Count)
            {
                base.RemoveVisualChild(visuals[index]);
                visuals.Remove(visuals[index]);
            }
        }

        /// <summary>
        /// Converts all visuals on the canvas into a RenderTargetBitmap.
        /// </summary>
        /// <returns>A RenderTargetBitmap copy of the current DrawingVisual content of the canvas.</returns>
        public RenderTargetBitmap GetRenderDataBitmap()
        {
            int width = (int)ActualWidth;
            int height = (int)ActualHeight;

            RenderTargetBitmap rTB = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);

            //first fill with the background colour:
            DrawingVisual dV = new DrawingVisual();

            using (DrawingContext dC = dV.RenderOpen())
            {
                dC.DrawRectangle(Background, null, new Rect(new Size(width, height)));
            }

            rTB.Render(dV);

            for (int i = 0; i < visuals.Count; i++)
            {
                rTB.Render(visuals[i]);
            }

            return rTB;
        }

        public bool HasVisuals
        {
            get { return visuals.Count > 0; }
        }

        public void RemoveAllVisuals()
        {
            for (int i = 0; i < visuals.Count; i++)
            {
                base.RemoveVisualChild(visuals[i]);
            }

            visuals.Clear();
        }

        public void RemoveLastVisual()
        {
            if (visuals.Count > 0)
            {
                base.RemoveVisualChild(visuals[visuals.Count - 1]);
                visuals.Remove(visuals[visuals.Count - 1]);
            }     
        }

        public void RemoveVisual(DrawingVisual visual)
        {
            visuals.Remove(visual);
            base.RemoveVisualChild(visual);
        }
    }

    //class LayeredVisuals
    //{
    //    public List<Layer> MyLayers { get; private set; }

    //    public LayeredVisuals()
    //    {
    //        MyLayers = new List<Layer>();
    //    }

    //    public void AddVisual(DrawingVisual visual, Layer.Nature nature)
    //    {
    //        MyLayers.Add(new Layer(visual, nature));
    //    }

    //    public class Layer
    //    {
    //        public DrawingVisual Visual { get; private set; }
    //        public Nature VisualNature { get; private set; }

    //        public Layer(DrawingVisual visual, Nature nature)
    //        {
    //            Visual = visual;
    //            VisualNature = nature;
    //        }

    //        public enum Nature
    //        {
    //            BASE_UNDERLAY, IMAGE_FILES, DATA, SWATHE, LINE_AND_AREA, POINT
    //        }
    //    }
    //}
}
