using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using OpenCvSharp;
using System.Windows.Media;
using OpenCvSharp.Extensions;
using DOOR_TRIM_INSPECTION.Class;
using DOOR_TRIM_INSPECTION.Controls;

namespace Doortrim_Inspection.Class
{
    public delegate void UpdateRoICallback(DetectionROIDetailsUI roi);

    class ImageEditorHelper
    {
        
        public static bool _isEditingROIHandle = false;
        private static List<System.Windows.Shapes.Rectangle> handlesList = new List<System.Windows.Shapes.Rectangle>();

        public static event UpdateRoICallback OnRoiUpdated;

        public enum HandlePosition
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
            Top,
            Bottom,
            Left,
            Right
        }
                
        private static System.Windows.Shapes.Rectangle CreateHandle(DetectionROIDetailsUI roi, HandlePosition position, Canvas canvas)
        {
            var c = canvas as UIElement;
            double handleSize = 15;

            var scaleTransform = (ScaleTransform)((TransformGroup)canvas.RenderTransform)
              .Children.First(tr => tr is ScaleTransform);

            if (scaleTransform != null)
            {
                handleSize *= 1 / scaleTransform.ScaleX;
            }

            var handle = new System.Windows.Shapes.Rectangle
            {
                Width = handleSize,
                Height = handleSize,
                Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(150, 0, 0, 255))
            };

            handle.Tag = position;

            MouseEventHandler ResizeHandler = (sender, args) =>
            {
                ResizeROI(roi, args.GetPosition(canvas), position, canvas);
            };


            handle.MouseLeftButtonDown += (s, e) =>
            {
                _isEditingROIHandle = true;
                handle.CaptureMouse();
                canvas.MouseMove += ResizeHandler;
            };

            handle.MouseLeftButtonUp += (s, e) =>
            {
                _isEditingROIHandle = false;
                canvas.MouseMove -= ResizeHandler;

                handle.ReleaseMouseCapture();
            };

            return handle;
        }

        public static void UpdateHandlePosition(DetectionROIDetailsUI roi, System.Windows.Shapes.Rectangle handle)
        {
            var position = (HandlePosition)handle.Tag;

            switch (position)
            {
                case HandlePosition.TopLeft:
                    Canvas.SetLeft(handle, Canvas.GetLeft(roi) - handle.Width / 2);
                    Canvas.SetTop(handle, Canvas.GetTop(roi) - handle.Height / 2);
                    break;
                case HandlePosition.TopRight:
                    Canvas.SetLeft(handle, Canvas.GetLeft(roi) + roi.Width - handle.Width / 2);
                    Canvas.SetTop(handle, Canvas.GetTop(roi) - handle.Height / 2);
                    break;
                case HandlePosition.BottomLeft:
                    Canvas.SetLeft(handle, Canvas.GetLeft(roi) - handle.Width / 2);
                    Canvas.SetTop(handle, Canvas.GetTop(roi) + roi.Height - handle.Height / 2);
                    break;
                case HandlePosition.BottomRight:
                    Canvas.SetLeft(handle, Canvas.GetLeft(roi) + roi.Width - handle.Width / 2);
                    Canvas.SetTop(handle, Canvas.GetTop(roi) + roi.Height - handle.Height / 2);
                    break;
                case HandlePosition.Top:
                    Canvas.SetLeft(handle, Canvas.GetLeft(roi) + roi.Width / 2 - handle.Width / 2);
                    Canvas.SetTop(handle, Canvas.GetTop(roi) - handle.Height / 2);
                    break;
                case HandlePosition.Bottom:
                    Canvas.SetLeft(handle, Canvas.GetLeft(roi) + roi.Width / 2 - handle.Width / 2);
                    Canvas.SetTop(handle, Canvas.GetTop(roi) + roi.Height - handle.Height / 2);
                    break;
                case HandlePosition.Left:
                    Canvas.SetLeft(handle, Canvas.GetLeft(roi) - handle.Width / 2);
                    Canvas.SetTop(handle, Canvas.GetTop(roi) + roi.Height / 2 - handle.Height / 2);
                    break;
                case HandlePosition.Right:
                    Canvas.SetLeft(handle, Canvas.GetLeft(roi) + roi.Width - handle.Width / 2);
                    Canvas.SetTop(handle, Canvas.GetTop(roi) + roi.Height / 2 - handle.Height / 2);
                    break;
            }
            double updatedRoiLeft = Canvas.GetLeft(roi);
            double updatedRoiTop = Canvas.GetTop(roi);

            double topLeftX = updatedRoiLeft;
            double topLeftY = updatedRoiTop;
            double bottomRightX = updatedRoiLeft + roi.Width;
            double bottomRightY = updatedRoiTop + roi.Height;

            roi.start_x = topLeftX;
            roi.start_y = topLeftY;
            roi.end_x = bottomRightX;
            roi.end_y = bottomRightY;

            OnRoiUpdated?.Invoke(roi);

        }

        private static void ResizeROI(DetectionROIDetailsUI roi, System.Windows.Point position, HandlePosition positionHandle, Canvas canvas)
        {
            System.Windows.Point currentMousePosition = position;

            double x = Canvas.GetLeft(roi);
            double y = Canvas.GetTop(roi);
            double width = roi.Width;
            double height = roi.Height;

            var image = canvas.Children.OfType<System.Windows.Controls.Image>().FirstOrDefault();

            double imageWidth = image.ActualWidth;
            double imageHeight = image.ActualHeight;

            switch (positionHandle)
            {
                case HandlePosition.TopLeft:
                    if (x > 0) width += x - position.X;
                    if (y > 0) height += y - position.Y;

                    x = position.X;
                    y = position.Y;

                    width = Math.Max(width, 0);
                    height = Math.Max(height, 0);

                    x = Math.Max(x, 0);
                    y = Math.Max(y, 0);
                    break;

                case HandlePosition.TopRight:
                    width = position.X - x;
                    if (y > 0) height += y - position.Y;
                    y = position.Y; 

                    width = Math.Max(width, 0);
                    height = Math.Max(height, 0);

                    if (x + width > imageWidth)
                    {
                        width = imageWidth - x;
                    }
                    if (y + height > imageHeight)
                    {
                        height = imageHeight - y;
                    }
                    break;

                case HandlePosition.BottomLeft:
                    if (x > 0) width += x - position.X;
                    height = position.Y - y;
                    x = position.X;

                    width = Math.Max(width, 0);
                    height = Math.Max(height, 0);

                    if (x + width > imageWidth)
                    {
                        width = imageWidth - x;
                    }
                    if (y + height > imageHeight)
                    {
                        height = imageHeight - y;
                    }
                    break;

                case HandlePosition.BottomRight:
                    width = position.X - x;
                    height = position.Y - y;

                    width = Math.Max(width, 0);
                    height = Math.Max(height, 0);

                    if (x + width > imageWidth)
                    {
                        width = imageWidth - x;
                    }
                    if (y + height > imageHeight)
                    {
                        height = imageHeight - y;
                    }
                    break;

                case HandlePosition.Top:
                    if (y > 0) height += y - position.Y;
                    y = position.Y;

                    height = Math.Max(height, 0);
                    if (y + height > imageHeight)
                    {
                        height = imageHeight - y;
                    }
                    break;

                case HandlePosition.Bottom:
                    height = position.Y - y;

                    height = Math.Max(height, 0);
                    if (y + height > imageHeight)
                    {
                        height = imageHeight - y;
                    }
                    break;

                case HandlePosition.Left:
                    if (x > 0) width += x - position.X;
                    x = position.X;

                    width = Math.Max(width, 0);
                    if (x + width > imageWidth)
                    {
                        width = imageWidth - x;
                    }
                    break;

                case HandlePosition.Right:
                    width = position.X - x;

                    width = Math.Max(width, 0);
                    if (x + width > imageWidth)
                    {
                        width = imageWidth - x;
                    }
                    break;
            }

            roi.Width = width;
            roi.Height = height;

            x = Math.Max(0, Math.Min(x, imageWidth - width));
            y = Math.Max(0, Math.Min(y, imageHeight - height));

            Canvas.SetLeft(roi, x);
            Canvas.SetTop(roi, y);

            foreach (var handle in canvas.Children.OfType<System.Windows.Shapes.Rectangle>())
            {
                if (handle.Tag is HandlePosition positionTag)
                {
                    UpdateHandlePosition(roi, handle);
                }
            }

        }

        public static void AddResizeHandles(DetectionROIDetailsUI roi, Canvas canvas)
        {
            var handles = new List<System.Windows.Shapes.Rectangle>
            {
                CreateHandle(roi, HandlePosition.TopLeft, canvas),
                CreateHandle(roi, HandlePosition.TopRight, canvas),
                CreateHandle(roi, HandlePosition.BottomLeft, canvas),
                CreateHandle(roi, HandlePosition.BottomRight, canvas),
                CreateHandle(roi, HandlePosition.Top, canvas),
                CreateHandle(roi, HandlePosition.Bottom, canvas),
                CreateHandle(roi, HandlePosition.Left, canvas),
                CreateHandle(roi, HandlePosition.Right, canvas)
            };

            foreach (var handle in handles)
            {
                handlesList.Add(handle);
                canvas.Children.Add(handle);
                UpdateHandlePosition(roi, handle);
            }
        }

        public static void RemoveResizeHandles(Canvas canvas)
        {
            foreach (var handle in handlesList)
            {
                canvas.Children.Remove(handle);
            }
            handlesList.Clear();
        }

        public static void LoadROIsList(DataGrid ROIsDataGrid, List<DetectionROIDetailsUI> ROIsList)
        {
            ROIsDataGrid.Items.Refresh();
            ROIsDataGrid.ItemsSource = ROIsList;
        }

        public static void ClearGroup(StackPanel _stkPanelAddGroup, List<DetectionROIDetailsUI> _selectedROIsForChk)
        {
            _stkPanelAddGroup.Visibility = Visibility.Hidden;
            _selectedROIsForChk.Clear();
        }
    }
}
