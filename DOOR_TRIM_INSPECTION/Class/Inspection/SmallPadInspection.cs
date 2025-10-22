using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DOOR_TRIM_INSPECTION.Class
{
    public class SmallPadInspectionItem
    {
        public int RegionID;
        public string DetectionClassName;
        public string ROIName;
        public Rect ROI;

        public Mat ImageRegion;
        public int MinThreshold;
        public int MaxThreshold;
        public int Variance;
        public int WhitePixelCount;
        public Mat ProcessedImageRegion;
        public INSPECTION_RESULT InspectionResult;


        public string ALC_CODE;
        public string ALC_NAME;
        public string group_name;

        public SmallPadInspectionItem(int RegionID, string DetectionClassName, string ROIName, Rect ROI, Mat ImageRegion, Mat ProcessedImageRegion,
           int MinThreshold, int MaxThreshold, int Variance, int WhitePixelCount, string ALC_CODE, string ALC_NAME, string group_name/*, ThresholdTypes PadThresholdType, RULES_CLASS PadType*/)
        {
            this.RegionID = RegionID;
            this.DetectionClassName = DetectionClassName;
            this.ROIName = ROIName;
            this.ROI = ROI;
            this.ImageRegion = ImageRegion;
            this.ProcessedImageRegion = ProcessedImageRegion;
            this.MinThreshold = MinThreshold;
            this.MaxThreshold = MaxThreshold;
            this.Variance = Variance;
            this.WhitePixelCount = WhitePixelCount;

            this.ALC_CODE = ALC_CODE;
            this.ALC_NAME = ALC_NAME;
            this.group_name = group_name;
        }
    }
    public class SmallPadInspection
    {
        public List<SmallPadInspectionItem> PadInspectionItems = new List<SmallPadInspectionItem>();

        public SmallPadInspection(Mat image, List<DetectionROIDetails> detROIs)
        {
            int MinThreshold = 0;
            int MaxThreshold = 0;
            int Variance = 0;
            int WhitePixelCount = 0;

            ThresholdTypes PadThresholdType = ThresholdTypes.Binary;

            foreach (DetectionROIDetails detROI in detROIs)
            {

                string ALCSPA = detROI.ALC_CODE;
                if (ALCSPA != "")
                {
                    string ALCData = Machine.ALCData.GetSPA(ALCSPA).Trim();
                    if (ALCData == "-")
                        continue;
                    if (detROI.group_name.Trim() != ALCData)
                        continue;
                }

                Rect ROI = new Rect(detROI.start_x, detROI.start_y, detROI.end_x - detROI.start_x, detROI.end_y - detROI.start_y);

                List<KeyValuePair<string, string>> keyValuePairs = HanMechDBHelper.ParseParam(detROI.Parameters);

                foreach (KeyValuePair<string, string> keyValue in keyValuePairs)
                {
                    if (ALGORITHM_OPTION.MinThreshold.ToString().Equals(keyValue.Key))
                        MinThreshold = Convert.ToInt32(keyValue.Value);
                    if (ALGORITHM_OPTION.MaxThreshold.ToString().Equals(keyValue.Key))
                        MaxThreshold = Convert.ToInt32(keyValue.Value);
                    if (ALGORITHM_OPTION.Variance.ToString().Equals(keyValue.Key))
                        Variance = Convert.ToInt32(keyValue.Value);
                    if (ALGORITHM_OPTION.WhitePixelCount.ToString().Equals(keyValue.Key))
                        WhitePixelCount = Convert.ToInt32(keyValue.Value);
                }

                Mat ImageRegion = new Mat(image, ROI);
                Mat MorphedImageRegion = PreProcess(ImageRegion, MinThreshold, MaxThreshold, PadThresholdType);

                PadInspectionItems.Add(new SmallPadInspectionItem(
                   detROI.detection_roi_ID, detROI.DetectionClassName, detROI.roi_name,  ROI, ImageRegion, MorphedImageRegion, MinThreshold, MaxThreshold, Variance, WhitePixelCount, detROI.ALC_CODE, detROI.ALC_NAME, detROI.group_name));

            }
        }

        public void Execute()
        {
            foreach (SmallPadInspectionItem padInspItem in this.PadInspectionItems)
                FindPad(padInspItem);
        }

        private void FindPadByVariance(SmallPadInspectionItem padInspItem)
        {
            Mat foamRegion = new Mat();
            padInspItem.ImageRegion.CopyTo(foamRegion, padInspItem.ProcessedImageRegion);

            Mat foamGray = new Mat();
            Cv2.CvtColor(foamRegion, foamGray, ColorConversionCodes.BGR2GRAY);

            Mat edges = new Mat();
            Cv2.Canny(foamGray, edges, padInspItem.MinThreshold, padInspItem.MaxThreshold);

            Mat laplacian = new Mat();
            Cv2.Laplacian(foamGray, laplacian, MatType.CV_64F);
            Scalar mean, stddev;
            Cv2.MeanStdDev(laplacian, out mean, out stddev);
            double variance = stddev.Val0 * stddev.Val0;
            int nonZeroCount = Cv2.CountNonZero(edges);

            string textureType = variance > padInspItem.Variance ? "Grainy" : "Plain";

            if (variance > padInspItem.Variance && nonZeroCount > padInspItem.WhitePixelCount) 
                padInspItem.InspectionResult = INSPECTION_RESULT.OK;
            else
                padInspItem.InspectionResult = INSPECTION_RESULT.NG;
        }

        private void FindPad(SmallPadInspectionItem padInspItem)
        {
            Mat Result = new Mat();
            //Cv2.FindContours(padInspItem.ProcessedImageRegion, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
            //Cv2.Canny(padInspItem.ProcessedImageRegion, Result, padInspItem.MinThreshold, padInspItem.MaxThreshold);

            Cv2.ImWrite(@"D:\smallpad\smallpad_" + padInspItem.RegionID + ".bmp", padInspItem.ProcessedImageRegion);
            Cv2.Laplacian(padInspItem.ProcessedImageRegion, padInspItem.ProcessedImageRegion, -1, 5, 1, 0, BorderTypes.Default);
            Cv2.CvtColor(padInspItem.ProcessedImageRegion, padInspItem.ProcessedImageRegion, ColorConversionCodes.RGB2GRAY);
            Cv2.GaussianBlur(padInspItem.ProcessedImageRegion, padInspItem.ProcessedImageRegion, new Size(3, 3), 0);
            Cv2.Threshold(padInspItem.ProcessedImageRegion, Result, 1, 255, ThresholdTypes.Otsu);
            CircleSegment[] circles = Cv2.HoughCircles(Result, HoughModes.Gradient, 1, 1, 200, 10, 10, 50);
            Cv2.ImWrite(@"D:\smallpad\smallpad_" + padInspItem.RegionID + "_canny.bmp", Result);

            int areaPixels = Cv2.CountNonZero(Result);

            if (areaPixels > padInspItem.WhitePixelCount&&circles.Length> padInspItem.Variance)
                padInspItem.InspectionResult = INSPECTION_RESULT.OK;
            else
                padInspItem.InspectionResult = INSPECTION_RESULT.NG;

            Console.WriteLine(padInspItem.DetectionClassName + "White Pixels = " + areaPixels);
            Machine.logger.Write(eLogType.INSPECTION, padInspItem.DetectionClassName + "White Pixels = " + areaPixels);
        }

        private Mat PreProcess(Mat ColorImage,
                       int MinThreshold, int MaxThreshold, ThresholdTypes PadThresholdType)
        {
            try
            {
                Mat gray = new Mat();
                Cv2.CvtColor(ColorImage, gray, ColorConversionCodes.BGR2RGB);
                Cv2.GaussianBlur(gray, gray, new Size(9, 9), 0);
                return gray;
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
