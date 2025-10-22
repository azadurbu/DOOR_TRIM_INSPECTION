using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DOOR_TRIM_INSPECTION.Class
{
    public class ZoomBorder : Border
    {
        private UIElement child = null;
        private double ScreenRatio = .2;
        private double previousWidth = 0;
        public bool _IsDrawing = false;

        public TranslateTransform GetTranslateTransform(UIElement element)
        {
            return (TranslateTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is TranslateTransform);
        }

        public ScaleTransform GetScaleTransform(UIElement element)
        {
            return (ScaleTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is ScaleTransform);
        }

        public override UIElement Child
        {
            get { return base.Child; }
            set
            {
                if (value != null && value != this.Child)
                    this.Initialize(value);
                base.Child = value;
            }
        }

        public void Initialize(UIElement element)
        {
            this.child = element;
            if (child != null)
            {
                TransformGroup group = new TransformGroup();
                ScaleTransform st = new ScaleTransform();
                group.Children.Add(st);
                TranslateTransform tt = new TranslateTransform();
                group.Children.Add(tt);
                child.RenderTransform = group;
                child.RenderTransformOrigin = new Point(0.0, 0.0);
                child.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                {
                    Child_Loaded();
                }));
            }
        }

        public void ImageLoad(UIElement child)
        {
            if (child is Canvas childCanvas)
            {
                var image = childCanvas.Children.OfType<Image>().FirstOrDefault();
                if (image != null)
                {
                    image.LayoutUpdated += (s, e) =>
                    {
                        if (Math.Abs(image.ActualWidth - previousWidth) > 1)
                        {
                            Reset();
                            previousWidth = image.ActualWidth;
                            Reset();
                        }
                    };
                }
            }
        }

        public void IsDrawing(bool IsDrawing)
        {
            _IsDrawing = IsDrawing;
        }

        private void Child_Loaded()
        {
            child.UpdateLayout();
        }

        public ScaleTransform Scale(UIElement child)
        {
            return GetScaleTransform(child);
        }

        public void Reset()
        {
            if (child != null)
            {

                // reset pan
                ZoomOutViewCentered();

                // reset zoom
                var st = GetScaleTransform(child);
                st.ScaleX = ScreenRatio;
                st.ScaleY = ScreenRatio;

               
            }
        }

        #region Child Events


        private List<KeyValuePair<string, double>> GetImageHeightWidth(UIElement element)
        {
            List<KeyValuePair<string, double>> maxSize = new List<KeyValuePair<string, double>>();
            if (child != null)
            {
                double acWidth = 0;
                double acHeight = 0;
                var st = GetScaleTransform(child);
                UIElement parent = VisualTreeHelper.GetParent(child) as UIElement;
                // Get the scaled size of the child element
                double childWidth = parent.RenderSize.Width * st.ScaleX;
                double childHeight = parent.RenderSize.Height * st.ScaleY;

                //getting image height width
                Canvas childCanvas = (Canvas)child;
                try
                {
                    if (childCanvas.Children.Count > 0)
                    {
                        Image childCtl = childCanvas.Children[0] is Image ? (Image)childCanvas.Children[0] : null;
                        if (childCtl != null)
                        {
                            acWidth = childCtl.ActualWidth;
                            acHeight = childCtl.ActualHeight;
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show(ex.ToString().ToString()); }
                if (acHeight > 0)
                    ScreenRatio = parent.RenderSize.Height / acHeight;

                maxSize.Add(new KeyValuePair<string, double>("X", acWidth * st.ScaleX - childWidth));
                maxSize.Add(new KeyValuePair<string, double>("Y", acHeight * st.ScaleY - childHeight));
                maxSize.Add(new KeyValuePair<string, double>("acWidth", acWidth));
            }
            return maxSize;
        }

        public void ConstrainTranslationToBounds()
        {
            if (child != null)
            {
                UIElement parent = VisualTreeHelper.GetParent(child) as UIElement;

                var st = GetScaleTransform(child);
                var tt = GetTranslateTransform(child);

                // Get the scaled size of the child element
                double childWidth = parent.RenderSize.Width * st.ScaleX;
                double childHeight = parent.RenderSize.Height * st.ScaleY;

                double maxX = 0, maxY = 0, acWidth = 0;
                var maxSize = GetImageHeightWidth(child);
                foreach (var pair in maxSize)
                {
                    if (pair.Key == "X")
                    {
                        maxX = pair.Value;
                    }
                    else if (pair.Key == "Y")
                    {
                        maxY = pair.Value;
                    }
                    else if (pair.Key == "acWidth")
                    {
                        acWidth = pair.Value;
                    }
                }

                //bottom block
                var scloffsetX = childWidth * (1 / st.ScaleX) - childWidth;
                var scloffsetY = childHeight * (1 / st.ScaleY) - childHeight;
                if (((maxX + tt.X - scloffsetX)) < 0)
                {
                    tt.X = -maxX + scloffsetX;
                }
                //right block
                if ((maxY + tt.Y - scloffsetY) < 0)
                {
                    tt.Y = -maxY + scloffsetY;
                }

                //top block
                if (tt.X > 0)
                {
                    tt.X = 0;

                }
                //left block
                if (tt.Y > 0)
                {
                    tt.Y = 0;
                }

                ZoomOutViewCentered();
            }
        }

        public void ZoomOutViewCentered()
        {
            UIElement parent = VisualTreeHelper.GetParent(child) as UIElement;
            var tt = GetTranslateTransform(child);
            double maxX = 0, maxY = 0, acWidth = 0;
            var maxSize = GetImageHeightWidth(child);
            foreach (var pair in maxSize)
            {
                if (pair.Key == "X")
                {
                    maxX = pair.Value;
                }
                else if (pair.Key == "Y")
                {
                    maxY = pair.Value;
                }
                else if (pair.Key == "acWidth")
                {
                    acWidth = pair.Value;
                }
            }
            var childRenderSize = maxX * (acWidth / (acWidth - parent.RenderSize.Width));
            if (childRenderSize < parent.RenderSize.Width)
            {
                var offsetx = parent.RenderSize.Width - childRenderSize;
                tt.X = 0 + offsetx / 2;
                tt.Y = 0;
            }
        }
        #endregion
    }
}
