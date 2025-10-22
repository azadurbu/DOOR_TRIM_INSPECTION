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

namespace DOOR_TRIM_INSPECTION.Class
{
    public class MainUIHelper
    {
        public static BitmapImage LoadImageWpf(Bitmap temp)
        {
            BitmapImage bitmapImage;
            using (var memoryStream = new MemoryStream())
            {
                temp.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                memoryStream.Position = 0;

                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }
            return bitmapImage;
        }

        private static double scaleFactor = 0.035;

        public static void FrontImageHandle(Border border, Canvas canvas, System.Windows.Controls.Image image)
        {
            if (scaleFactor == 0.035)
            {
                ScaleTransform scaleTransform = new ScaleTransform();
                scaleFactor = .15;
                scaleTransform.ScaleX = scaleFactor;
                scaleTransform.ScaleY = scaleFactor;

                canvas.RenderTransform = scaleTransform;
                if (image.ActualHeight * scaleFactor != 0)
                {
                    border.Height = image.ActualHeight * scaleFactor + 10;
                    border.Width = image.ActualWidth * scaleFactor + 10;
                }
                else
                {
                    border.Height = 816;
                    border.Width = 987;
                }

                border.HorizontalAlignment = HorizontalAlignment.Center;
                border.VerticalAlignment = VerticalAlignment.Center;
            }
            else
            {
                ScaleTransform scaleTransform = new ScaleTransform();
                scaleFactor = .035;
                scaleTransform.ScaleX = scaleFactor;
                scaleTransform.ScaleY = scaleFactor;

                canvas.RenderTransform = scaleTransform;
                if (image.ActualHeight * scaleFactor != 0)
                {
                    border.Height = image.ActualHeight * scaleFactor + 10;
                    border.Width = image.ActualWidth * scaleFactor + 10;
                }
                else
                {
                    border.Height = 198;
                    border.Width = 237;
                }

                border.HorizontalAlignment = HorizontalAlignment.Right;
                border.VerticalAlignment = VerticalAlignment.Top;
                border.Margin = new Thickness(20);
            }

        }


        public static void FrontImageHandle(Border border, Canvas canvas, System.Windows.Controls.Image image, double scale)
        {
            if (canvas.ActualWidth < 800)
            {
                ScaleTransform scaleTransform = new ScaleTransform();
                scaleFactor = scale * 3.8;
                scaleTransform.ScaleX = scaleFactor;
                scaleTransform.ScaleY = scaleFactor;

                canvas.RenderTransform = scaleTransform;
                if (image.ActualHeight * scaleFactor != 0)
                {
                    border.Height = image.ActualHeight * scaleFactor + 10;
                    border.Width = image.ActualWidth * scaleFactor + 10;
                }
                else
                {
                    border.Height = 816;
                    border.Width = 987;
                }

                border.HorizontalAlignment = HorizontalAlignment.Center;
                border.VerticalAlignment = VerticalAlignment.Center;
            }
            else
            {
                ScaleTransform scaleTransform = new ScaleTransform();
                scaleFactor = scale;
                scaleTransform.ScaleX = scaleFactor;
                scaleTransform.ScaleY = scaleFactor;

                canvas.RenderTransform = scaleTransform;
                if (image.ActualHeight * scaleFactor != 0)
                {
                    border.Height = image.ActualHeight * scaleFactor + 10;
                    border.Width = image.ActualWidth * scaleFactor + 10;
                }
                else
                {
                    border.Height = 198;
                    border.Width = 237;
                }

                border.HorizontalAlignment = HorizontalAlignment.Right;
                border.VerticalAlignment = VerticalAlignment.Top;
                border.Margin = new Thickness(20);
            }

        }

        
        public static Mat DrawRearInspectionResult(RearInspectionResult inspectionResult, Bitmap ResultImage, Canvas canvas, int RecipeID, bool Rotate180 = false)
        {
            Mat resultImage = BitmapConverter.ToMat(ResultImage);
            if (Rotate180)
                Cv2.Rotate(resultImage, resultImage, OpenCvSharp.RotateFlags.Rotate180);

            foreach (var PlugInspectionResult in inspectionResult.PlugInspectionResult)
            {
                if (PlugInspectionResult.InspectionResult == INSPECTION_RESULT.OK)
                {
                    DrawResultOnImage(resultImage, PlugInspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? PlugInspectionResult.DetectionClassName : PlugInspectionResult.ROIName, Colors.ForestGreen, canvas, PlugInspectionResult.RegionID, true);
                }
                else
                {
                    DrawResultOnImage(resultImage, PlugInspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? PlugInspectionResult.DetectionClassName : PlugInspectionResult.ROIName, Colors.Red, canvas, PlugInspectionResult.RegionID, true);
                }
            }
            foreach (var BoltInspectionResult in inspectionResult.BoltInspectionResult)
            {
                if (BoltInspectionResult.InspectionResult == INSPECTION_RESULT.OK)
                {
                    DrawResultOnImage(resultImage, BoltInspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? BoltInspectionResult.DetectionClassName : BoltInspectionResult.ROIName, Colors.ForestGreen, canvas, BoltInspectionResult.RegionID, true);
                }

                else
                {
                    DrawResultOnImage(resultImage, BoltInspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? BoltInspectionResult.DetectionClassName : BoltInspectionResult.ROIName, Colors.Red, canvas, BoltInspectionResult.RegionID, true);
                }

            }
            foreach (var ScrewInspectionResult in inspectionResult.ScrewInspectionResult)
            {
                if (ScrewInspectionResult.InspectionResult == INSPECTION_RESULT.OK)
                {
                    if (ScrewInspectionResult.DetectionClassName == "스크류(D)")
                    {
                        ScrewInspectionResult.DetectionClassName = ScrewInspectionResult.DetectionClassName.Replace("(D)", "");
                        //DrawResultOnImage(resultImage, ScrewInspectionResult.ROI, ScrewInspectionResult.DetectionClassName, Colors.Blue, canvas, ScrewInspectionResult.RegionID);
                    }
                    DrawResultOnImage(resultImage, ScrewInspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? ScrewInspectionResult.DetectionClassName : ScrewInspectionResult.ROIName, Colors.ForestGreen, canvas, ScrewInspectionResult.RegionID, true);
                }
                else
                {
                    if (ScrewInspectionResult.DetectionClassName == "스크류(D)")
                    {
                        ScrewInspectionResult.DetectionClassName = ScrewInspectionResult.DetectionClassName.Replace("(D)", "");
                        //DrawResultOnImage(resultImage, ScrewInspectionResult.ROI, ScrewInspectionResult.DetectionClassName, Colors.Yellow, canvas, ScrewInspectionResult.RegionID);
                    }
                    DrawResultOnImage(resultImage, ScrewInspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? ScrewInspectionResult.DetectionClassName : ScrewInspectionResult.ROIName, Colors.Red, canvas, ScrewInspectionResult.RegionID, true);
                }
            }
            foreach (var PadInspectionResult in inspectionResult.PadInspectionResult)
            {
                if (PadInspectionResult.InspectionResult == INSPECTION_RESULT.OK)
                {
                    DrawResultOnImage(resultImage, PadInspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? PadInspectionResult.DetectionClassName : PadInspectionResult.ROIName, Colors.ForestGreen, canvas, PadInspectionResult.RegionID, true);
                }
                else
                {
                    DrawResultOnImage(resultImage, PadInspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? PadInspectionResult.DetectionClassName : PadInspectionResult.ROIName, Colors.Red, canvas, PadInspectionResult.RegionID, true);
                }
            }
            foreach (var InspectionResult in inspectionResult.SpeakerInspectionResult)
            {
                if (InspectionResult.InspectionResult == INSPECTION_RESULT.OK)
                {
                    DrawResultOnImage(resultImage, InspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? InspectionResult.DetectionClassName : InspectionResult.ROIName, Colors.ForestGreen, canvas, InspectionResult.RegionID, true);
                }
                else
                {
                    DrawResultOnImage(resultImage, InspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? InspectionResult.DetectionClassName : InspectionResult.ROIName, Colors.Red, canvas, InspectionResult.RegionID, true);
                }
            }
            foreach (var InspectionResult in inspectionResult.SmallPadInspectionResult)
            {
                if (InspectionResult.InspectionResult == INSPECTION_RESULT.OK)
                {
                    DrawResultOnImage(resultImage, InspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? InspectionResult.DetectionClassName : InspectionResult.ROIName, Colors.ForestGreen, canvas, InspectionResult.RegionID, true);
                }
                else
                {
                    DrawResultOnImage(resultImage, InspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? InspectionResult.DetectionClassName : InspectionResult.ROIName, Colors.Red, canvas, InspectionResult.RegionID, true);
                }
            }
            foreach (var InspectionResult in inspectionResult.ScrewMacthInspectionResult)
            {

                if (InspectionResult.InspectionResult == INSPECTION_RESULT.OK)
                {
                    if (InspectionResult.DetectionClassName == "스크류(D)")
                    {
                        InspectionResult.DetectionClassName = InspectionResult.DetectionClassName.Replace("(D)", "");
                        //DrawResultOnImage(resultImage, InspectionResult.ROI, InspectionResult.DetectionClassName, Colors.Blue, canvas, InspectionResult.RegionID);
                    }
                    DrawResultOnImage(resultImage, InspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? InspectionResult.DetectionClassName : InspectionResult.ROIName, Colors.ForestGreen, canvas, InspectionResult.RegionID, true);
                }
                else
                {
                    if (InspectionResult.DetectionClassName == "스크류(D)")
                    {
                        InspectionResult.DetectionClassName = InspectionResult.DetectionClassName.Replace("(D)", "");
                        //DrawResultOnImage(resultImage, InspectionResult.ROI, InspectionResult.DetectionClassName, Colors.Yellow, canvas, InspectionResult.RegionID);
                    }
                    DrawResultOnImage(resultImage, InspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? InspectionResult.DetectionClassName : InspectionResult.ROIName, Colors.Red, canvas, InspectionResult.RegionID, true);

                }
            }

            bool USE_COGNEX_RESULT = Machine.config.setup.USE_COGNEX_RESULT; 
            List<int> OldLeadwireIDs = new List<int>() { 1584, 1903, 1910, 1919, 1911, 1915 };

            foreach (var InspectionResult in inspectionResult.PlugMatchInspectionResult)
            {
                if (USE_COGNEX_RESULT && OldLeadwireIDs.Contains(InspectionResult.RegionID))
                    continue;
                if (InspectionResult.InspectionResult == INSPECTION_RESULT.OK)
                {
                    DrawResultOnImage(resultImage, InspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? InspectionResult.DetectionClassName : InspectionResult.ROIName, Colors.ForestGreen, canvas, InspectionResult.RegionID, true);
                }
                else
                {
                    DrawResultOnImage(resultImage, InspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? InspectionResult.DetectionClassName : InspectionResult.ROIName, Colors.Red, canvas, InspectionResult.RegionID, true);
                }
            }

            //foreach (var InspectionResult in inspectionResult.DeepFusionInspectionResult)
            //{
            //    if (InspectionResult.InspectionResult == INSPECTION_RESULT.OK)
            //    {
            //        DrawResultOnImage(resultImage, InspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? InspectionResult.DetectionClassName : InspectionResult.ROIName, Colors.Blue, canvas, InspectionResult.RegionID, true);
            //    }
            //    else
            //    {
            //        DrawResultOnImage(resultImage, InspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? InspectionResult.DetectionClassName : InspectionResult.ROIName, Colors.Yellow, canvas, InspectionResult.RegionID, true);
            //    }
            //}

            foreach (var InspectionResult in inspectionResult.DeepFusionInspectionResult)
            {
                if (InspectionResult.InspectionResult == INSPECTION_RESULT.OK)
                {
                    DrawResultOnImage(resultImage, InspectionResult.ROI, InspectionResult.DetectionClassName, Colors.Blue, canvas, InspectionResult.RegionID, true);
                }
                else
                {
                    DrawResultOnImage(resultImage, InspectionResult.ROI, InspectionResult.DetectionClassName, Colors.Yellow, canvas, InspectionResult.RegionID, true);
                }
            }


            foreach (var WhitePadInspectionResult in inspectionResult.WhitePadInspectionResult)
            {
                if (WhitePadInspectionResult.InspectionResult == INSPECTION_RESULT.OK)
                {
                    DrawResultOnImage(resultImage, WhitePadInspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? WhitePadInspectionResult.DetectionClassName : WhitePadInspectionResult.ROIName, Colors.ForestGreen, canvas, WhitePadInspectionResult.RegionID, true);
                }
                else
                {
                    DrawResultOnImage(resultImage, WhitePadInspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? WhitePadInspectionResult.DetectionClassName : WhitePadInspectionResult.ROIName, Colors.Red, canvas, WhitePadInspectionResult.RegionID, true);
                }
            }

#if USE_COGNEX
            if (USE_COGNEX_RESULT)
            {
                foreach (var PlugCognexInspectionResult in inspectionResult.PlugCognexInspectionResult)
                {
                    if (PlugCognexInspectionResult.InspectionResult == INSPECTION_RESULT.OK)
                    {
                        DrawResultOnImage(resultImage, PlugCognexInspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? PlugCognexInspectionResult.DetectionClassName : PlugCognexInspectionResult.ROIName, Colors.ForestGreen, canvas, PlugCognexInspectionResult.RegionID, true);
                    }
                    else
                    {
                        DrawResultOnImage(resultImage, PlugCognexInspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? PlugCognexInspectionResult.DetectionClassName : PlugCognexInspectionResult.ROIName, Colors.Red, canvas, PlugCognexInspectionResult.RegionID, true);
                    }
                }
            }
#endif
            return resultImage.Clone();
        }

        public static Mat DrawFrontInspectionResult(FrontInspectionResult inspectionResult, Bitmap ResultImage, Canvas canvas, int RecipeID)
        {
            Mat resultImage = BitmapConverter.ToMat(ResultImage);
            foreach (var ColorInspectionResult in inspectionResult.ColorInspectionResult)
            {
                if (ColorInspectionResult.InspectionResult == INSPECTION_RESULT.OK)
                {
                    DrawResultOnImage(resultImage, ColorInspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? ColorInspectionResult.DetectionClassName : ColorInspectionResult.ROIName, Colors.ForestGreen, canvas, ColorInspectionResult.RegionID);
                }
                else
                {
                    DrawResultOnImage(resultImage, ColorInspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? ColorInspectionResult.DetectionClassName : ColorInspectionResult.ROIName, Colors.Red, canvas, ColorInspectionResult.RegionID);
                }


                //MainUIHelper.DrawResultOnImage(resultImage, ColorInspectionResult.ROI, ColorInspectionResult.ROIName + ColorInspectionResult.Color.ToString(), Colors.ForestGreen, canvas, ColorInspectionResult.RegionID);
            }

            foreach (var ColorMatchInspectionResult in inspectionResult.ColorMatchInspectionResult)
            {
                if (ColorMatchInspectionResult.InspectionResult == INSPECTION_RESULT.OK)
                {
                    DrawResultOnImage(resultImage, ColorMatchInspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? ColorMatchInspectionResult.DetectionClassName : ColorMatchInspectionResult.ROIName, Colors.ForestGreen, canvas, ColorMatchInspectionResult.RegionID);
                }
                else
                {
                    DrawResultOnImage(resultImage, ColorMatchInspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? ColorMatchInspectionResult.DetectionClassName : ColorMatchInspectionResult.ROIName, Colors.Red, canvas, ColorMatchInspectionResult.RegionID);
                }
            }

            foreach (var PlugInspectionResult in inspectionResult.PlugInspectionResult)
            {
                if (PlugInspectionResult.InspectionResult == INSPECTION_RESULT.OK)
                {
                    DrawResultOnImage(resultImage, PlugInspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? PlugInspectionResult.DetectionClassName : PlugInspectionResult.ROIName, Colors.ForestGreen, canvas, PlugInspectionResult.RegionID);
                }
                else
                {
                    DrawResultOnImage(resultImage, PlugInspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? PlugInspectionResult.DetectionClassName : PlugInspectionResult.ROIName, Colors.Red, canvas, PlugInspectionResult.RegionID);
                }
            }

            foreach (var ScrewInspectionResult in inspectionResult.ScrewInspectionResult)
            {
                if (ScrewInspectionResult.InspectionResult == INSPECTION_RESULT.OK)
                {
                    DrawResultOnImage(resultImage, ScrewInspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? ScrewInspectionResult.DetectionClassName : ScrewInspectionResult.ROIName, Colors.ForestGreen, canvas, ScrewInspectionResult.RegionID);
                }
                else
                {
                    DrawResultOnImage(resultImage, ScrewInspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? ScrewInspectionResult.DetectionClassName : ScrewInspectionResult.ROIName, Colors.Red, canvas, ScrewInspectionResult.RegionID);
                }
            }

            foreach (var BoltInspectionResult in inspectionResult.BoltInspectionResult)
            {
                if (BoltInspectionResult.InspectionResult == INSPECTION_RESULT.OK)
                {
                    DrawResultOnImage(resultImage, BoltInspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? BoltInspectionResult.DetectionClassName : BoltInspectionResult.ROIName, Colors.ForestGreen, canvas, BoltInspectionResult.RegionID);
                }
                else
                {
                    DrawResultOnImage(resultImage, BoltInspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? BoltInspectionResult.DetectionClassName : BoltInspectionResult.ROIName, Colors.Red, canvas, BoltInspectionResult.RegionID);
                }
            }

            foreach (var PadInspectionResult in inspectionResult.PadInspectionResult)
            {
                if (PadInspectionResult.InspectionResult == INSPECTION_RESULT.OK)
                {
                    DrawResultOnImage(resultImage, PadInspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? PadInspectionResult.DetectionClassName : PadInspectionResult.ROIName, Colors.ForestGreen, canvas, PadInspectionResult.RegionID);
                }
                else
                {
                    DrawResultOnImage(resultImage, PadInspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? PadInspectionResult.DetectionClassName : PadInspectionResult.ROIName, Colors.Red, canvas, PadInspectionResult.RegionID);
                }
            }

            foreach (var SpeakerInspectionResult in inspectionResult.SpeakerInspectionResult)
            {
                if (SpeakerInspectionResult.InspectionResult == INSPECTION_RESULT.OK)
                {
                    DrawResultOnImage(resultImage, SpeakerInspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? SpeakerInspectionResult.DetectionClassName : SpeakerInspectionResult.ROIName, Colors.ForestGreen, canvas, SpeakerInspectionResult.RegionID);
                }
                else
                {
                    DrawResultOnImage(resultImage, SpeakerInspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? SpeakerInspectionResult.DetectionClassName : SpeakerInspectionResult.ROIName, Colors.Red, canvas, SpeakerInspectionResult.RegionID);
                }
            }

            foreach (var SmallPadInspectionResult in inspectionResult.SmallPadInspectionResult)
            {
                if (SmallPadInspectionResult.InspectionResult == INSPECTION_RESULT.OK)
                {
                    DrawResultOnImage(resultImage, SmallPadInspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? SmallPadInspectionResult.DetectionClassName :  SmallPadInspectionResult.ROIName, Colors.ForestGreen, canvas, SmallPadInspectionResult.RegionID);
                }
                else
                {
                    DrawResultOnImage(resultImage, SmallPadInspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? SmallPadInspectionResult.DetectionClassName : SmallPadInspectionResult.ROIName, Colors.Red, canvas, SmallPadInspectionResult.RegionID);
                }
            }

            foreach (var ScrewMacthInspectionResult in inspectionResult.ScrewMacthInspectionResult)
            {
                if (ScrewMacthInspectionResult.InspectionResult == INSPECTION_RESULT.OK)
                {
                    DrawResultOnImage(resultImage, ScrewMacthInspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? ScrewMacthInspectionResult.DetectionClassName : ScrewMacthInspectionResult.ROIName, Colors.ForestGreen, canvas, ScrewMacthInspectionResult.RegionID);
                }
                else
                {
                    DrawResultOnImage(resultImage, ScrewMacthInspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? ScrewMacthInspectionResult.DetectionClassName : ScrewMacthInspectionResult.ROIName, Colors.Red, canvas, ScrewMacthInspectionResult.RegionID);
                }
            }

            foreach (var WhitePadInspectionResult in inspectionResult.WhitePadInspectionResult)
            {
                if (WhitePadInspectionResult.InspectionResult == INSPECTION_RESULT.OK)
                {
                    DrawResultOnImage(resultImage, WhitePadInspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? WhitePadInspectionResult.DetectionClassName : WhitePadInspectionResult.ROIName, Colors.ForestGreen, canvas, WhitePadInspectionResult.RegionID);
                }
                else
                {
                    DrawResultOnImage(resultImage, WhitePadInspectionResult.ROI, Machine.config.setup.SHOW_ALGO_NAME ? WhitePadInspectionResult.DetectionClassName : WhitePadInspectionResult.ROIName, Colors.Red, canvas, WhitePadInspectionResult.RegionID);
                }
            }


            return resultImage.Clone();
        }

        private static void DrawResultOnImage(Mat resultImage, OpenCvSharp.Rect rect, string Text, System.Windows.Media.Color Color, Canvas canvas, int RegionID, bool Rotate180 = false)
        {
            // Generate random position and size for the rectangle
            double x = Rotate180 ? resultImage.Width - rect.X - rect.Width : rect.X;
            double y = Rotate180 ? resultImage.Height - rect.Y - rect.Height : rect.Y;
            double width = rect.Width;
            double height = rect.Height;

            int roi_name_location = Machine.hmcDBHelper.GetTextDirectionByRegionID(RegionID);

            // Create a rectangle
            System.Windows.Shapes.Rectangle rectangle = new System.Windows.Shapes.Rectangle
            {
                Width = width,
                Height = height,
                Stroke = new SolidColorBrush(Color),  // Red border for the rectangle
                StrokeThickness = 1,
                Fill = System.Windows.Media.Brushes.Transparent
            };

            // Set the position of the rectangle
            Canvas.SetLeft(rectangle, x);
            Canvas.SetTop(rectangle, y);

            // Add the rectangle to the Canvas
            canvas.Children.Add(rectangle);
            int fs = 48;
            // Create a TextBlock to display the name and value near the rectangle
            TextBlock textBlock = new TextBlock
            {
                Text = $"{Text}",
                Foreground = System.Windows.Media.Brushes.White,
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(120, Color.R, Color.G, Color.B)),
                FontSize = fs
            };

            // Position the TextBlock near the rectangle
            Canvas.SetLeft(textBlock, x + 4);
            Canvas.SetTop(textBlock, y - fs - (fs / 2)); // Position it just below the rectangle

            rect = new OpenCvSharp.Rect((int)x, (int)y, (int)width, (int)height);

            RoiNamePosition(textBlock, rect, roi_name_location);

            Cv2.Rectangle(resultImage, rect, Scalar.FromRgb(Color.R, Color.G, Color.B), 8);

            // Add the TextBlock to the Canvas
            canvas.Children.Add(textBlock);
        }

        public static void RoiNamePosition(TextBlock textBlock, OpenCvSharp.Rect rect, int position = 1)
        {
            int fs = 48;
            double start_x = rect.X;
            double start_y = rect.Y;
            double end_x = start_x + rect.Width;
            double end_y = start_y + rect.Height;

            if (position == 1) // TL, T
            {
                Canvas.SetLeft(textBlock, start_x + 4);
                Canvas.SetTop(textBlock, start_y - fs - (fs / 2));
            }
            else if (position == 2) // TR, T
            {
                Canvas.SetLeft(textBlock, end_x + 4 - textBlock.ActualWidth);
                Canvas.SetTop(textBlock, start_y - fs - (fs / 2));
            }
            else if (position == 3) // TR, R
            {
                Canvas.SetLeft(textBlock, end_x + 4);
                Canvas.SetTop(textBlock, start_y);
            }
            else if (position == 4) // BR, R
            {
                Canvas.SetLeft(textBlock, end_x + 4);
                Canvas.SetTop(textBlock, end_y - textBlock.ActualHeight);
            }
            else if (position == 5) // BR, B
            {
                Canvas.SetLeft(textBlock, end_x + 4 - textBlock.ActualWidth);
                Canvas.SetTop(textBlock, end_y);
            }
            else if (position == 6) // BL, B
            {
                Canvas.SetLeft(textBlock, start_x + 4);
                Canvas.SetTop(textBlock, end_y);
            }
            else if (position == 7) // BL, L
            {
                Canvas.SetLeft(textBlock, start_x - 4 - textBlock.ActualWidth);
                Canvas.SetTop(textBlock, end_y - textBlock.ActualHeight);
            }
            else if (position == 8)  // TL, L
            {
                Canvas.SetLeft(textBlock, start_x - 4 - textBlock.ActualWidth);
                Canvas.SetTop(textBlock, start_y);
            }
        }
    }
}
