using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DOOR_TRIM_INSPECTION.Class
{
    public class ScrewInspectionItem
    {
        public int RegionID;
        public string DetectionClassName;
        public string ROIName;
        public Rect ROI;
        public Mat ImageRegion;
        public int MinThreshold;
        public int MaxThreshold;
        ThresholdTypes ThresholdType;
        public int MinContourArea;

        public Mat PreprocessedImageRegion;
        public Point[] DetectedContour;
        public Rect DetectedRect;
        public int WhitePixelCount;
        public INSPECTION_RESULT InspectionResult;

        public string ALC_CODE;
        public string ALC_NAME;
        public string group_name; 
        public ScrewInspectionItem(int RegionID, string DetectionClassName, string ROIName, Rect ROI, Mat ImageRegion, Mat PreprocessedImageRegion,
            int MinThreshold, int MaxThreshold, ThresholdTypes ThresholdType, 
            int MinContourArea, string ALC_CODE,string ALC_NAME, string group_name)
        {
            this.RegionID = RegionID;
            this.DetectionClassName = DetectionClassName;
            this.ROIName = ROIName;
            this.ROI = ROI;
            this.ImageRegion = ImageRegion;
            this.PreprocessedImageRegion = PreprocessedImageRegion;
            this.MinThreshold = MinThreshold;
            this.MaxThreshold = MaxThreshold;
            this.ThresholdType = ThresholdType;
            this.MinContourArea = MinContourArea;
            this.ALC_CODE = ALC_CODE;
            this.ALC_NAME = ALC_NAME;
            this.group_name = group_name;
        }        
    }
    public class ScrewInspection
    {
        public List<ScrewInspectionItem> ScrewInspectionItems = new List<ScrewInspectionItem>();

        private Mat PreProcess(Mat ColorImage,
                        int MinThreshold, int MaxThreshold, ThresholdTypes ThresholdType)
        {
            try
            {
                if (ColorImage.Empty())
                {
                    throw new Exception("이미지를 불러오는 데 실패했습니다.");
                }

                // 최종 이미지를 이진화 처리
                Mat GrayScaleImage = new Mat();
                Cv2.CvtColor(ColorImage, GrayScaleImage, ColorConversionCodes.BGR2GRAY);
                Mat MorphedImage = new Mat();
                //Cv2.MorphologyEx(BlurredImage, MorphedImage, MorphTypes.Erode, kernel);

                 //이진화 처리 (흰색은 100 그레이는 60)
                Cv2.Threshold(GrayScaleImage, MorphedImage, MinThreshold, 255, ThresholdType);
                //Cv2.ImShow("MorphedImage", MorphedImage);
                //Cv2.WaitKey();
                return MorphedImage;
            }
            catch (Exception ex)
            {
                Console.WriteLine("오류 발생: " + ex.ToString());
                Machine.logger.Write(eLogType.ERROR, ex.ToString());
                return new Mat(); // 빈 이미지 반환
            }
        }

        public ScrewInspection(Mat image, List<DetectionROIDetails> detROIs)
        {
            int MinThreshold = 0;
            int MaxThreshold = 0;
            int MinContourArea = 0;
            ThresholdTypes ThresholdType = ThresholdTypes.Binary;
            foreach (DetectionROIDetails detROI in detROIs)
            {
                string ALCSPA = detROI.ALC_CODE;
                if (ALCSPA != "")
                {
                    string ALCData = Machine.ALCData.GetSPA(ALCSPA).Trim();
                    string[] ALCDataList = detROI.group_name.Trim().Split(',');

                    if (ALCData == "-")
                        continue;
                    bool isPass = false;
                    foreach (string data in ALCDataList)
                    {
                        if (data.Trim() == ALCData)
                            isPass = true;
                    }
                    if (isPass == false)
                        continue;
                }
                Rect ROI = new Rect(detROI.start_x, detROI.start_y, detROI.end_x - detROI.start_x, detROI.end_y - detROI.start_y);
                Mat ImageRegion = new Mat(image, ROI);


                List<KeyValuePair<string, string>> keyValuePairs = HanMechDBHelper.ParseParam(detROI.Parameters);

                foreach (KeyValuePair<string, string> keyValue in keyValuePairs)
                {
                    if (ALGORITHM_OPTION.MinThreshold.ToString().Equals(keyValue.Key))
                        MinThreshold = Convert.ToInt32(keyValue.Value);
                    if (ALGORITHM_OPTION.MaxThreshold.ToString().Equals(keyValue.Key))
                        MaxThreshold = Convert.ToInt32(keyValue.Value);
                    if (ALGORITHM_OPTION.MinContourArea.ToString().Equals(keyValue.Key))
                        MinContourArea = Convert.ToInt32(keyValue.Value);
                }

                Mat PreProcessed = PreProcess(ImageRegion, MinThreshold, MaxThreshold, ThresholdType);

                ScrewInspectionItems.Add(new ScrewInspectionItem(
                    detROI.detection_roi_ID, detROI.DetectionClassName, detROI.roi_name, ROI, ImageRegion, PreProcessed,
                    MinThreshold, MaxThreshold, ThresholdType, MinContourArea, detROI.ALC_CODE, detROI.ALC_NAME, detROI.group_name));
            }
        }

        private void FindScrew(ScrewInspectionItem screwInspItem)
        {
            try
            {
                // 윤곽선 찾기
                Cv2.FindContours(screwInspItem.PreprocessedImageRegion, out Point[][] contours, out HierarchyIndex[] hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);
                Cv2.ImWrite(@"d:\screw\screw_Pre_"+screwInspItem.RegionID + ".bmp", screwInspItem.PreprocessedImageRegion);
                Cv2.ImWrite(@"d:\screw\screw_Crop_" + screwInspItem.RegionID + ".bmp", screwInspItem.ImageRegion);
                Point[] largestContour = null;
                double largestArea = 0;

                foreach (var contour in contours)
                {
                    double area = Cv2.ContourArea(contour);

                    if (area > largestArea && area > screwInspItem.MinContourArea)
                    {
                        largestArea = area;
                        largestContour = contour;
                    }
                }

                if (largestContour != null)
                {
                    // contour의 경계 사각형을 구함
                    Rect boundingRect = Cv2.BoundingRect(largestContour);

                    // contour가 포함된 영역을 클론하여 이진화
                    Mat image = screwInspItem.ImageRegion.Clone();
                    Mat grayImage = new Mat();
                    Cv2.CvtColor(image, grayImage, ColorConversionCodes.BGR2GRAY); // 그레이스케일 변환
                    Mat binaryImage = new Mat();
                    Cv2.Threshold(grayImage, binaryImage, 200, 255, ThresholdTypes.Binary); // 이진화

                    // 흰색 픽셀 수 계산
                    //int whitePixelCount = Cv2.CountNonZero(binaryImage); // 흰색 픽셀 

                    //if (whitePixelCount >= screwInspItem.MinContourArea)
                    //{
                    //    screwInspItem.InspectionResult = INSPECTION_RESULT.OK; // 흰색 픽셀이 기준 이상일 경우 OK
                    //}
                    //else
                    //{
                    //    screwInspItem.InspectionResult = INSPECTION_RESULT.NG; // 그렇지 않으면 NG
                    //}
                    screwInspItem.WhitePixelCount = Cv2.CountNonZero(binaryImage); // 흰색 픽셀 

                    if (screwInspItem.WhitePixelCount >= screwInspItem.MinContourArea)
                    {
                        screwInspItem.InspectionResult = INSPECTION_RESULT.OK; // 흰색 픽셀이 기준 이상일 경우 OK
                    }
                    else
                    {
                        screwInspItem.InspectionResult = INSPECTION_RESULT.NG; // 그렇지 않으면 NG
                    }

                }
                else
                {
                    screwInspItem.WhitePixelCount = 0;
                    screwInspItem.InspectionResult = INSPECTION_RESULT.NOT_FOUND; // contour가 없을 경우
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("오류 발생: " + ex.ToString());
                Machine.logger.Write(eLogType.ERROR, ex.ToString());
                screwInspItem.InspectionResult = INSPECTION_RESULT.NOT_FOUND;

            }
        }

        private Point[] TranslateToOrigin(int OffsetX, int OffsetY, Point[] largestContour)
        {
            for (int i = 0; i < largestContour.Length; i++)
            {
                largestContour[i] = new Point(OffsetX + largestContour[i].X, OffsetY + largestContour[i].Y);
            }
            return largestContour;
        }

        private Rect TranslateToOrigin(int OffsetX, int OffsetY, Rect boundingBox)
        {
            return new Rect(boundingBox.X + OffsetX, boundingBox.Y + OffsetY, boundingBox.Width, boundingBox.Height);
        }

        public void Execute()
        {
            foreach (ScrewInspectionItem screwInspItem in this.ScrewInspectionItems)
                FindScrew(screwInspItem);

            //Console.WriteLine($"SCREW INSPECTION");
            //Console.WriteLine($"RegionID\tClass\tWhitePixelCount\tMinContourArea");
            //foreach (var item in this.ScrewInspectionItems.OrderBy(item => item.DetectionClassName).ThenBy(item => item.RegionID).ToList())
            //{
            //    Console.WriteLine($"{item.RegionID}\t{item.DetectionClassName}\t{item.WhitePixelCount}\t{item.MinContourArea}");
            //}
        }

    }
}
