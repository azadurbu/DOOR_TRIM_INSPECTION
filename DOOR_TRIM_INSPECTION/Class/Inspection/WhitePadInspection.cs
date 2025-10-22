using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace DOOR_TRIM_INSPECTION.Class
{
    public class WhitePadInspectionItem
    {
        public int RegionID;
        public string DetectionClassName;
        public string ROIName;
        public Rect ROI;
        public Mat ImageRegion;
        public int MinThreshold;
        public int MaxThreshold;
        public int MaxTotalArea;
        public int MinTotalArea;

        public Mat ProcessedImageRegion;
        public INSPECTION_RESULT InspectionResult;

        public string ALC_CODE;
        public string ALC_NAME;
        public string group_name;

        public WhitePadInspectionItem(int RegionID, string DetectionClassName, string ROIName, Rect ROI, Mat ImageRegion, Mat ProcessedImageRegion,
           int MinThreshold, int MaxThreshold, int MinTotalArea, int MaxTotalArea, string ALC_CODE, string ALC_NAME, string group_name/*, ThresholdTypes PadThresholdType, RULES_CLASS PadType*/)
        {
            this.RegionID = RegionID;
            this.DetectionClassName = DetectionClassName;
            this.ROIName = ROIName;
            this.ROI = ROI;
            this.ImageRegion = ImageRegion;
            this.ProcessedImageRegion = ProcessedImageRegion;
            this.MinThreshold = MinThreshold;
            this.MaxThreshold = MaxThreshold;
            this.MinTotalArea = MinTotalArea;
            this.MaxTotalArea = MaxTotalArea;
            this.ALC_CODE = ALC_CODE;
            this.ALC_NAME = ALC_NAME;
            this.group_name = group_name;
        }
    }
    public class WhitePadInspection
    {
        public int GammaDivisor = 5;
        public int GammaMultiplier = 3;

        public List<WhitePadInspectionItem> PadInspectionItems = new List<WhitePadInspectionItem>();

        public WhitePadInspection(Mat image, List<DetectionROIDetails> detROIs)
        {
            int MinThreshold = 0;
            int MaxThreshold = 0;
            int MinTotalArea = 0;
            int MaxTotalArea = 0;

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
                    if (ALGORITHM_OPTION.MaxTotalArea.ToString().Equals(keyValue.Key))
                        MaxTotalArea = Convert.ToInt32(keyValue.Value);
                    if (ALGORITHM_OPTION.MinTotalArea.ToString().Equals(keyValue.Key))
                        MinTotalArea = Convert.ToInt32(keyValue.Value);
                }

                Mat ImageRegion = new Mat(image, ROI);
                Mat MorphedImageRegion = PreProcessForVariance(ImageRegion, MinThreshold, MaxThreshold, PadThresholdType);

                PadInspectionItems.Add(new WhitePadInspectionItem(
                   detROI.detection_roi_ID, detROI.DetectionClassName, detROI.roi_name, ROI, ImageRegion, MorphedImageRegion, MinThreshold, MaxThreshold, MinTotalArea,MaxTotalArea, detROI.ALC_CODE, detROI.ALC_NAME, detROI.group_name/*, PadThresholdType, detROI.Rule*/));

            }
        }



        public void Execute()
        {
            foreach (WhitePadInspectionItem padInspItem in this.PadInspectionItems)
                FindPadByVariance(padInspItem);
        }

        private void FindPadByVariance(WhitePadInspectionItem padInspItem)
        {
            Mat foamRegion = new Mat();
            //padInspItem.ImageRegion.CopyTo(foamRegion, padInspItem.ProcessedImageRegion);
            foamRegion = padInspItem.ProcessedImageRegion.Clone();
            Mat foamGray = new Mat();
            if (foamRegion.Channels() != 1)
                Cv2.CvtColor(foamRegion, foamGray, ColorConversionCodes.BGR2GRAY);
            else
                foamGray = foamRegion.Clone();

            Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(5, 5));
            Cv2.MorphologyEx(foamGray, foamGray, MorphTypes.Close, kernel);
            Mat Mask = new Mat();
            Cv2.Threshold(foamGray, Mask, padInspItem.MaxThreshold, 255, ThresholdTypes.BinaryInv);
            // Mask를 foamGray에 적용
            Mat maskedImage = new Mat();
            Cv2.BitwiseAnd(foamGray, foamGray, maskedImage, Mask);
            
            // 새로운 Threshold 처리
            Mat threshHoled = new Mat();
            Cv2.Threshold(maskedImage, threshHoled, padInspItem.MinThreshold, 255, ThresholdTypes.Binary);

            int nonZeroCount = Cv2.CountNonZero(threshHoled);

            padInspItem.InspectionResult = (nonZeroCount > padInspItem.MaxTotalArea) ? INSPECTION_RESULT.OK : INSPECTION_RESULT.NG;

            if (padInspItem.MinTotalArea <= nonZeroCount && nonZeroCount <= padInspItem.MaxTotalArea)
            {
                padInspItem.InspectionResult = INSPECTION_RESULT.OK;
            }
            else
            {
                padInspItem.InspectionResult = INSPECTION_RESULT.NG;
            }
        }
        
        private Mat PreProcessForVariance(Mat imageRegion, int minThreshold, int maxThreshold, ThresholdTypes padThresholdType)
        {
            try
            {
                Mat gray = new Mat();
                if(imageRegion.Channels()!=1)
                Cv2.CvtColor(imageRegion, gray, ColorConversionCodes.BGR2GRAY);
                Mat thresholded = new Mat();
                Cv2.GaussianBlur(gray, thresholded, new Size(5, 5), 1);

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
