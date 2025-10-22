using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOOR_TRIM_INSPECTION.Class
{

    public class SpeakerInspectionItem
    {
        public int RegionID;
        public string DetectionClassName;
        public string ROIName;
        public Rect ROI;
        public Mat ImageRegion;
        public string TemplatePath;
        public double Accuracy;
        public Mat ProcessedImageRegion;
        public INSPECTION_RESULT InspectionResult;
        public string ALC_CODE;
        public string ALC_NAME;
        public string group_name;

        public SpeakerInspectionItem(int RegionID, string DetectionClassName, string ROIName, Rect ROI, Mat ImageRegion, Mat ProcessedImageRegion, double Accuracy, string templatePath, string ALC_CODE,string ALC_NAME, string group_name)
        {
            this.RegionID = RegionID;
            this.DetectionClassName = DetectionClassName.Trim();
            this.ROIName = ROIName;
            this.ROI = ROI;
            this.ImageRegion = ImageRegion;
            this.ProcessedImageRegion = ProcessedImageRegion;
            this.Accuracy = Accuracy;
            TemplatePath = templatePath;
            this.ALC_CODE = ALC_CODE;
            this.ALC_NAME = ALC_NAME;
            this.group_name = group_name;
        }

        public class SpeakerInspection
        {
            public List<SpeakerInspectionItem> SpeakerInspectionItems = new List<SpeakerInspectionItem>();

            public SpeakerInspection(Mat image, List<DetectionROIDetails> detROIs)
            {
                double Accuracy = 0;
                string TemplatePath = "";

                foreach (DetectionROIDetails detROI in detROIs)
                {
                    
                    string ALCSPA = detROI.ALC_CODE;
                    string ALCData = Machine.ALCData.GetSPA(ALCSPA).Trim();
                    if (ALCData == "-")
                        continue;
                    if (detROI.group_name.Trim() != ALCData)
                        continue;
                    
                    Rect ROI = new Rect(detROI.start_x, detROI.start_y, detROI.end_x - detROI.start_x, detROI.end_y - detROI.start_y);

                    List<KeyValuePair<string, string>> keyValuePairs = HanMechDBHelper.ParseParam(detROI.Parameters);

                    foreach (KeyValuePair<string, string> keyValue in keyValuePairs)
                    {
                        if (ALGORITHM_OPTION.Accuracy.ToString().Equals(keyValue.Key))
                            Accuracy = Convert.ToDouble(keyValue.Value);
                        if (ALGORITHM_OPTION.TemplatePath.ToString().Equals(keyValue.Key))
                            TemplatePath = keyValue.Value;
                    }

                    int newWidth = (int)(ROI.Width * 1.5);
                    int newHeight = (int)(ROI.Height * 1.5);
                    ROI = new Rect(ROI.X - ((newWidth - ROI.Width) / 2), ROI.Y - ((newHeight - ROI.Height) / 2), newWidth, newHeight);

                    Mat ImageRegion = new Mat(image, ROI);
                    //Mat MorphedImageRegion = PreProcess(ImageRegion, MinThreshold, MaxThreshold, SpeakerThresholdType);

                    SpeakerInspectionItems.Add(new SpeakerInspectionItem(
                       detROI.detection_roi_ID, detROI.DetectionClassName, detROI.roi_name, ROI, ImageRegion, ImageRegion, Accuracy, TemplatePath, detROI.ALC_CODE, detROI.ALC_NAME, detROI.group_name));
                }
            }

            public void Execute()
            {
                foreach (SpeakerInspectionItem SpeakerInspItem in this.SpeakerInspectionItems)
                    FindSpeaker(SpeakerInspItem);
            }

            private void FindSpeaker(SpeakerInspectionItem SpeakerInspItem)
            {
                try
                {
                    double Accuracy = SpeakerInspItem.Accuracy;
                    List<double> results = new List<double>();
                    string templatePath = SpeakerInspItem.TemplatePath;
                    Mat templateBgr = Cv2.ImRead(templatePath);
                    if (templateBgr.Empty())
                    {
                        Machine.logger.Write(eLogType.ERROR, $"Template {templatePath} not found.");
                        return;
                    }
                    Mat result = new Mat();
                    Cv2.MatchTemplate(SpeakerInspItem.ImageRegion, templateBgr, result, TemplateMatchModes.CCoeffNormed);

                    // 가장 높은 매칭 점수를 가진 좌표 찾기 
                    double minVal, maxVal;
                    Point minLoc, maxLoc;
                    Cv2.MinMaxLoc(result, out minVal, out maxVal, out minLoc, out maxLoc);

                    bool inSpeaker = false;

                    if (maxVal * 100 > Accuracy)
                    {
                        inSpeaker = true;
                    }
                    SpeakerInspItem.InspectionResult = inSpeaker ? INSPECTION_RESULT.OK : INSPECTION_RESULT.NG;
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Machine.logger.Write(eLogType.ERROR, ex.ToString());
                    SpeakerInspItem.InspectionResult = INSPECTION_RESULT.NG;
                }
            }

            private Mat PreProcess(Mat ColorImage,
                           int MinThreshold, int MaxThreshold, ThresholdTypes SpeakerThresholdType)
            {
                try
                {
                    // #1. Histogram 2024-12-03 LDH
                    Mat equalizedImage = new Mat();
                    Cv2.EqualizeHist(ColorImage, equalizedImage);

                    // #2. Threshold Binary 2024-12-03 LDH
                    Mat binaryImage = new Mat();
                    Cv2.Threshold(equalizedImage, binaryImage, MinThreshold, MaxThreshold, SpeakerThresholdType);

                    return binaryImage;
                }

                catch (Exception ex)
                {
                    Console.WriteLine("오류 발생: " + ex.ToString());
                    Machine.logger.Write(eLogType.ERROR, ex.ToString());
                    return new Mat(); // 빈 이미지 반환
                }
            }
        }
    }

    public static class TemplateMatching
    {
        public static (List<(Point point, int angle, double scale)>, double) InvariantMatchTemplate(
            string templateName,
            Mat rgbImage,
            Mat rgbTemplate,
            string method,
            double matchedThresh,
            int[] rotRange,
            int rotInterval,
            int[] scaleRange,
            int scaleInterval,
            bool removeRedundant,
            bool minMax,
            double rgbDiffThresh = double.MaxValue)
        {
            Mat imgGray = new Mat();
            Mat templateGray = new Mat();
            Cv2.CvtColor(rgbImage, imgGray, ColorConversionCodes.RGB2GRAY);
            Cv2.CvtColor(rgbTemplate, templateGray, ColorConversionCodes.RGB2GRAY);

            Size imageMaxSize = new Size(imgGray.Width, imgGray.Height);
            int templateHeight = templateGray.Height;
            int templateWidth = templateGray.Width;

            List<(Point point, int angle, double scale)> allPoints = new List<(Point point, int angle, double scale)>();
            double bestResult = 0;
            string bestResultName = string.Empty;

            for (int angle = rotRange[0]; angle < rotRange[1]; angle += rotInterval)
            {
                for (int scale = scaleRange[0]; scale < scaleRange[1]; scale += scaleInterval)
                {
                    Mat scaledTemplate = ScaleImage(templateGray, scale, imageMaxSize, out double actualScale);
                    Mat rotatedTemplate = angle == 0 ? scaledTemplate : RotateImage(scaledTemplate, angle);

                    Mat result = new Mat();
                    MatchTemplate(imgGray, rotatedTemplate, result, method);

                    Point matchLocation;
                    double minVal, maxVal;
                    Cv2.MinMaxLoc(result, out minVal, out maxVal, out _, out matchLocation);

                    double value = minMax ? maxVal : minVal;
                    if (minMax && maxVal > bestResult)
                    {
                        bestResult = maxVal;
                        bestResultName = templateName;
                    }

                    if (minMax ? maxVal >= matchedThresh : minVal <= matchedThresh)
                    {
                        allPoints.Add((matchLocation, angle, actualScale));
                    }
                }
            }

            if (removeRedundant)
            {
                allPoints = RemoveRedundantMatches(allPoints, templateWidth, templateHeight);
            }

            if (rgbDiffThresh != double.MaxValue)
            {
                allPoints = FilterByRgbDifference(allPoints, rgbImage, rgbTemplate, rgbDiffThresh, templateWidth, templateHeight);
            }

            if (bestResult*100 < 65)
            {
                Console.WriteLine($"Maybe {bestResultName} is not here.");
                Machine.logger.Write(eLogType.INSPECTION, $"Maybe {bestResultName} is not here.");
            }
            else
            {
                Console.WriteLine($"Accuracy: {bestResult}, template: {bestResultName}");
                Machine.logger.Write(eLogType.INSPECTION, $"Accuracy: {bestResult}, template: {bestResultName}");
            }

            return (allPoints, bestResult);
        }

        private static Mat ScaleImage(Mat image, int scale, Size maxSize, out double actualScale)
        {
            double scaleFactor = scale / 100.0;
            actualScale = scaleFactor;
            Size newSize = new Size((int)(image.Width * scaleFactor), (int)(image.Height * scaleFactor));

            if (newSize.Width > maxSize.Width || newSize.Height > maxSize.Height)
            {
                actualScale = Math.Min((double)maxSize.Width / image.Width, (double)maxSize.Height / image.Height);
                newSize = new Size((int)(image.Width * actualScale), (int)(image.Height * actualScale));
            }

            Mat scaledImage = new Mat();
            Cv2.Resize(image, scaledImage, newSize);
            return scaledImage;
        }

        private static Mat RotateImage(Mat image, int angle)
        {
            Point2f center = new Point2f(image.Width / 2f, image.Height / 2f);
            Mat rotationMatrix = Cv2.GetRotationMatrix2D(center, angle, 1.0);
            Mat rotatedImage = new Mat();
            Cv2.WarpAffine(image, rotatedImage, rotationMatrix, new Size(image.Width, image.Height));
            return rotatedImage;
        }

        private static void MatchTemplate(Mat img, Mat templ, Mat result, string method)
        {
            switch (method)
            {
                case "TM_CCOEFF":
                    Cv2.MatchTemplate(img, templ, result, TemplateMatchModes.CCoeff);
                    break;
                case "TM_CCOEFF_NORMED":
                    Cv2.MatchTemplate(img, templ, result, TemplateMatchModes.CCoeffNormed);
                    break;
                case "TM_CCORR":
                    Cv2.MatchTemplate(img, templ, result, TemplateMatchModes.CCorr);
                    break;
                case "TM_CCORR_NORMED":
                    Cv2.MatchTemplate(img, templ, result, TemplateMatchModes.CCorrNormed);
                    break;
                case "TM_SQDIFF":
                    Cv2.MatchTemplate(img, templ, result, TemplateMatchModes.SqDiff);
                    break;
                case "TM_SQDIFF_NORMED":
                    Cv2.MatchTemplate(img, templ, result, TemplateMatchModes.SqDiffNormed);
                    break;
                default:
                    throw new ArgumentException("Unsupported template matching method.");
            }
        }

        private static List<(Point point, int angle, double scale)> RemoveRedundantMatches(
            List<(Point point, int angle, double scale)> matches, int templateWidth, int templateHeight)
        {
            var nonRedundantMatches = new List<(Point, int, double)>();
            var visitedPoints = new List<Point>();

            foreach (var match in matches)
            {
                bool isRedundant = visitedPoints.Any(v =>
                    Math.Abs(v.X - match.point.X) < templateWidth * match.scale / 100 &&
                    Math.Abs(v.Y - match.point.Y) < templateHeight * match.scale / 100);

                if (!isRedundant)
                {
                    nonRedundantMatches.Add(match);
                    visitedPoints.Add(match.point);
                }
            }

            return nonRedundantMatches;
        }

        private static List<(Point point, int angle, double scale)> FilterByRgbDifference(
            List<(Point point, int angle, double scale)> matches, Mat rgbImage, Mat rgbTemplate, double threshold,
            int templateWidth, int templateHeight)
        {
            var filteredMatches = new List<(Point point, int angle, double scale)>();
            Scalar templateMean = Cv2.Mean(rgbTemplate);

            foreach (var match in matches)
            {
                Rect roi = new Rect(match.point, new Size(templateWidth, templateHeight));
                if (roi.X < 0 || roi.Y < 0 || roi.X + roi.Width > rgbImage.Width || roi.Y + roi.Height > rgbImage.Height)
                {
                    continue;
                }

                Mat cropped = new Mat(rgbImage, roi);
                Scalar croppedMean = Cv2.Mean(cropped);

                double totalDiff = Math.Abs(templateMean.Val0 - croppedMean.Val0) +
                                   Math.Abs(templateMean.Val1 - croppedMean.Val1) +
                                   Math.Abs(templateMean.Val2 - croppedMean.Val2);

                if (totalDiff < threshold)
                {
                    filteredMatches.Add(match);
                }
            }
            return filteredMatches;
        }
    }
}
