using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace DOOR_TRIM_INSPECTION.Class
{
    public class PadInspectionItem
    {
        public int RegionID;
        public string DetectionClassName;
        public string ROIName;
        public Rect ROI;
        public Mat ImageRegion;
        public int MinThreshold;
        public int MaxThreshold;
        public int Variance;
        //public ThresholdTypes PadThresholdType;
        //public RULES_CLASS PadType;
        public Mat ProcessedImageRegion;
        public INSPECTION_RESULT InspectionResult;

        public string ALC_CODE;
        public string ALC_NAME;
        public string group_name;

        public int MinTotalArea; // UNUSED
        public int MaxTotalArea; // UNUSED

        public PadInspectionItem(int RegionID, string DetectionClassName, string ROIName, Rect ROI, Mat ImageRegion, Mat ProcessedImageRegion,
           int MinThreshold, int MaxThreshold, int Variance, string ALC_CODE, string ALC_NAME, string group_name/*, ThresholdTypes PadThresholdType, RULES_CLASS PadType*/)
        {
            this.RegionID = RegionID;
            this.DetectionClassName = DetectionClassName;
            this.ROIName = ROIName;
            this.ROI = ROI;
            this.ImageRegion = ImageRegion;
            this.ProcessedImageRegion = ProcessedImageRegion;
            this.MinThreshold = MinThreshold;
            this.MaxThreshold = MaxThreshold;
            //this.PadThresholdType = PadThresholdType;


            this.Variance = Variance;
            this.ALC_CODE = ALC_CODE;
            this.ALC_NAME = ALC_NAME;
            this.group_name = group_name;
        }
    }
    public class PadInspection
    {
        public int GammaDivisor = 5;
        public int GammaMultiplier = 3;

        public List<PadInspectionItem> PadInspectionItems = new List<PadInspectionItem>();

        public PadInspection(Mat image, List<DetectionROIDetails> detROIs)
        {
            int MinThreshold = 0;
            int MaxThreshold = 0;
            int Variance = 0;

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
                }

                Mat ImageRegion = new Mat(image, ROI);
                Mat MorphedImageRegion = PreProcessForVariance(ImageRegion, MinThreshold, MaxThreshold, PadThresholdType);

                PadInspectionItems.Add(new PadInspectionItem(
                   detROI.detection_roi_ID, detROI.DetectionClassName, detROI.roi_name, ROI, ImageRegion, MorphedImageRegion, MinThreshold, MaxThreshold, Variance, detROI.ALC_CODE, detROI.ALC_NAME, detROI.group_name/*, PadThresholdType, detROI.Rule*/));

            }
        }



        public void Execute()
        {
            foreach (PadInspectionItem padInspItem in this.PadInspectionItems)
                FindPadByVariance(padInspItem);
        }

        private void FindPadByVariance(PadInspectionItem padInspItem)
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
            int nonZeroCount = Cv2.CountNonZero(laplacian);

            padInspItem.InspectionResult = (variance > padInspItem.Variance) ? INSPECTION_RESULT.OK : INSPECTION_RESULT.NG;
        }

        private void FindPad(PadInspectionItem padInspItem)
        {
            Point[][] contours;
            HierarchyIndex[] hierarchy;

            Cv2.FindContours(padInspItem.ProcessedImageRegion, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
            Mat mat = new Mat(new Size(padInspItem.ProcessedImageRegion.Width, padInspItem.ProcessedImageRegion.Height), padInspItem.ImageRegion.Type());
            Cv2.ImWrite(@"D:\Pad\pad_" + padInspItem.RegionID + ".bmp", padInspItem.ProcessedImageRegion);

            // 241203 LDH
            double totalArea = 0;

            if (contours.Length != 0)
            {

                foreach (var contour in contours)
                {
                    double area = Cv2.ContourArea(contour);

                    if (area >= padInspItem.Variance)
                    {
                        // 윤곽선을 그릴 색상과 두께를 설정합니다.
                        Cv2.DrawContours(mat, new Point[][] { contour }, -1, new Scalar(0, 255, 0), 1);
                        totalArea += area;
                    }
                }

                padInspItem.InspectionResult = (padInspItem.MinTotalArea <= totalArea && totalArea <= padInspItem.MaxTotalArea) ? INSPECTION_RESULT.OK : INSPECTION_RESULT.NG;
                Console.WriteLine("[FindPad]Result " + padInspItem.InspectionResult + "totalArea: " + totalArea + ", MinTotalArea" + padInspItem.MinTotalArea+ ", MaxTotalArea" + padInspItem.MaxTotalArea);
                Machine.logger.Write(eLogType.INSPECTION, "[FindPad]Result " + padInspItem.InspectionResult + "totalArea: " + totalArea + ", MinTotalArea" + padInspItem.MinTotalArea + ", MaxTotalArea" + padInspItem.MaxTotalArea);

            }



            else
            {
                padInspItem.InspectionResult = INSPECTION_RESULT.NOT_FOUND;
            }

            Console.WriteLine(padInspItem.DetectionClassName + "_"+ padInspItem .RegionID+ "_Total Area = " + totalArea);

        }

        private Mat PreProcessForVariance(Mat imageRegion, int minThreshold, int maxThreshold, ThresholdTypes padThresholdType)
        {
            try
            {
                Mat gray = new Mat();
                Cv2.CvtColor(imageRegion, gray, ColorConversionCodes.BGR2GRAY);
                Mat thresholded = new Mat();
                // Threshold Adjustment for background color (beige or black) NEEDED? I THINK NO SO FAR I TESTED
                Cv2.InRange(gray, Scalar.FromRgb(minThreshold, minThreshold, minThreshold),
                    Scalar.FromRgb(maxThreshold, maxThreshold, maxThreshold), thresholded);
                //Cv2.Threshold(gray, thresholded, minThreshold, maxThreshold, padThresholdType);
                return thresholded;
            }

            catch (Exception ex)
            {
                Console.WriteLine("오류 발생: " + ex.ToString());
                return new Mat(); // 빈 이미지 반환
            }
        }

        private Mat PreProcess(Mat ColorImage,
                       int MinThreshold, int MaxThreshold, ThresholdTypes PadThresholdType)
        {
            try
            {
                int totalArea = 0;

                // #1. Histogram 2024-12-03 LDH
                Mat equalizedImage = new Mat();

                Mat gray = new Mat();
                Cv2.CvtColor(ColorImage, gray, ColorConversionCodes.BGR2GRAY);
                Cv2.GaussianBlur(gray, gray, new Size(5, 5), 1);
                //Cv2.ImWrite(@"D:\Pad\grayImage.bmp", gray);

                Cv2.EqualizeHist(gray, equalizedImage);

                // #2. Threshold Binary 2024-12-03 LDH
                Mat binaryImage = new Mat();
                Cv2.Threshold(equalizedImage, binaryImage, MinThreshold, MaxThreshold, PadThresholdType);
                //Cv2.ImWrite(@"D:\Pad\binaryImage.bmp", binaryImage);

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
