using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DOOR_TRIM_INSPECTION.Class
{
    public class BoltInspectionItem
    {
        public int RegionID;
        public string DetectionClassName;
        public string ROIName;
        public Rect ROI;
        public Mat ImageRegion;

        public Color ColorValue;
        public int MinTotalArea;
        public int Bound;

        public Color DetectedColor;
        public INSPECTION_RESULT InspectionResult;

        public string ALC_CODE;
        public string ALC_NAME;
        public string group_name;

        public BoltInspectionItem(int RegionID, string DetectionClassName,  string ROIName, Rect ROI, Mat ImageRegion, Color ColorValue, int Bound,int MinTotalArea, string ALC_CODE, string ALC_NAME, string group_name)
        {
            this.RegionID = RegionID;
            this.DetectionClassName = DetectionClassName;
            this.ROI = ROI;
            this.ROIName = ROIName;
            this.ImageRegion = ImageRegion;
            this.ColorValue = ColorValue;
            this.Bound = Bound;
            this.MinTotalArea = MinTotalArea;
            this.ALC_CODE = ALC_CODE;
            this.ALC_NAME = ALC_NAME;
            this.group_name = group_name;
        }
    }
    public class BoltInspection
    {
        public List<BoltInspectionItem> BoltInspectionItems = new List<BoltInspectionItem>();
        public int Bound = 20;
        public int MinTotalArea = 2000;
        public Color ColorValue;
        public BoltInspection(Mat image, List<DetectionROIDetails> detROIs)
        {
            try
            {
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
                    foreach (KeyValuePair<string, string> keyValuePair in keyValuePairs)
                    {
                        if (ALGORITHM_OPTION.Bound.ToString().Equals(keyValuePair.Key))
                            Bound = Convert.ToInt32(keyValuePair.Value);
                        if (ALGORITHM_OPTION.MinTotalArea.ToString().Equals(keyValuePair.Key))
                            MinTotalArea = Convert.ToInt32(keyValuePair.Value);
                        if (ALGORITHM_OPTION.Color.ToString().Equals(keyValuePair.Key))
                        {
                            string[] valus = keyValuePair.Value.Split(',');
                            ColorValue = Color.FromArgb(255, byte.Parse(valus[0]), byte.Parse(valus[1]), byte.Parse(valus[2]));
                        }
                    }
                    BoltInspectionItems.Add(new BoltInspectionItem(detROI.detection_roi_ID, detROI.DetectionClassName, detROI.roi_name, ROI, new Mat(image, ROI), ColorValue, Bound, MinTotalArea, detROI.ALC_CODE, detROI.ALC_NAME, detROI.group_name));
                    Bound = 0;
                }
            }
            catch (Exception ex) { Machine.logger.Write(eLogType.ERROR, ex.ToString()); }
        }

        private void FindBolt(BoltInspectionItem boltInspItem)
        {
            if (boltInspItem.ImageRegion.Empty())
            {
                //throw new ArgumentException("The provided image region is empty.", nameof(image_region));
                boltInspItem.ColorValue = System.Windows.Media.Color.FromArgb(255, 0, 0, 0); // RETURN BLACK IN CASE OF EMPTY IMAGE
                return;
            }

            if (!boltInspItem.ImageRegion.IsContinuous())
            {
                boltInspItem.ImageRegion = boltInspItem.ImageRegion.Clone();
            }

            Cv2.ImWrite(@"D:\bolt\bolt" + boltInspItem.RegionID + ".bmp", boltInspItem.ImageRegion);

            Mat CropImg = boltInspItem.ImageRegion.Clone();

            Mat graymat = new Mat();
            Cv2.CvtColor(boltInspItem.ImageRegion, graymat, ColorConversionCodes.BGR2GRAY);
            // 이진화
            Mat binary = new Mat();
            Cv2.Threshold(graymat, binary, 20, 255, ThresholdTypes.Binary);
            // 컨투어 감지
            Cv2.FindContours(binary, out Point[][] contours, out HierarchyIndex[] hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
            // 컨투어를 Rect로 변환
            List<Rect> rects = new List<Rect>();
            //foreach (var contour in contours)

            double maxArea = 0;
            Rect rectMax = new Rect();
            foreach (var contour in contours)
            {
                double area = Cv2.ContourArea(contour);
                if (area > maxArea)
                {
                    maxArea = area;
                    rectMax = Cv2.BoundingRect(contour);
                }
            }
            if (rectMax.Width > 0)
            {
                CropImg = new Mat(CropImg, rectMax);

                if (!CropImg.IsContinuous())
                {
                    CropImg = CropImg.Clone();
                }
            }
            //Cv2.CvtColor(CropImg, CropImg, ColorConversionCodes.BGR2RGB);
            Cv2.ImWrite(@"D:\bolt\bolt_crop" + boltInspItem.RegionID + ".bmp", CropImg);
            // Reshape the region to a 1D array of pixels (rows * cols x channels)
            var pixels = CropImg.Reshape(1, CropImg.Rows * CropImg.Cols);
            //var pixels = boltInspItem.ImageRegion.Reshape(1, boltInspItem.ImageRegion.Rows * boltInspItem.ImageRegion.Cols);

            // 32F for K-means 
            pixels.ConvertTo(pixels, MatType.CV_32F);

            int k = 1;
            //var criteria = new TermCriteria(CriteriaTypes.Eps | CriteriaTypes.MaxIter, 100, 1.0);
            var criteria = new TermCriteria(CriteriaTypes.Eps | CriteriaTypes.MaxIter, 100, 1.0);
            var labels = new Mat();
            var centers = new Mat();

            Cv2.Kmeans(pixels, k, labels, criteria, 10, KMeansFlags.PpCenters, centers);

            // Get the dominant color (center of the first cluster)
            var dominantColorVec = centers.At<Vec3f>(0, 0);

            byte r = (byte)dominantColorVec[2];  // Red (from BGR)
            byte g = (byte)dominantColorVec[1];  // Green (from BGR)
            byte b = (byte)dominantColorVec[0];  // Blue (from BGR)
            byte a = 255;  // Set alpha to 255 (fully opaque)

            boltInspItem.DetectedColor = System.Windows.Media.Color.FromArgb(a, r, g, b);

            Cv2.ImWrite(@"C:\doortrim_wpf\Runtime\cropimage\bollt\bolt_" + boltInspItem.RegionID + "r_" + r + "b_" + g + "b_" + b + ".bmp", CropImg);

            if ((boltInspItem.ColorValue.R + boltInspItem.Bound > r && boltInspItem.ColorValue.R - boltInspItem.Bound < r)
                && (boltInspItem.ColorValue.G + boltInspItem.Bound > g && boltInspItem.ColorValue.G - boltInspItem.Bound < g)
                && (boltInspItem.ColorValue.B + boltInspItem.Bound > b && boltInspItem.ColorValue.B - boltInspItem.Bound < b))
            {
                boltInspItem.InspectionResult = INSPECTION_RESULT.OK;
            }
            else
            {
                boltInspItem.InspectionResult = INSPECTION_RESULT.NG;
            }

            boltInspItem.InspectionResult = INSPECTION_RESULT.NOT_FOUND;
        }


        private void FindBolt2(BoltInspectionItem boltInspItem)
        {
            int bound = boltInspItem.MinTotalArea;
            if (boltInspItem.ImageRegion.Empty())
            {
                //throw new ArgumentException("The provided image region is empty.", nameof(image_region));
                boltInspItem.ColorValue = System.Windows.Media.Color.FromArgb(255, 0, 0, 0); // RETURN BLACK IN CASE OF EMPTY IMAGE
                return;
            }

            if (!boltInspItem.ImageRegion.IsContinuous())
            {
                boltInspItem.ImageRegion = boltInspItem.ImageRegion.Clone();
            }

            Cv2.ImWrite(@"D:\bolt\bolt_" + boltInspItem.RegionID + ".bmp", boltInspItem.ImageRegion);
            Mat CropImg = boltInspItem.ImageRegion.Clone();

            Cv2.MedianBlur(CropImg, CropImg, 15);

            double min = 0;
            if (boltInspItem.Bound != 0)
                min = boltInspItem.Bound / 5;

            Scalar lowerBound = new Scalar(
                Math.Max((int)boltInspItem.ColorValue.B - boltInspItem.Bound, 0),         // 최소 Hue (0보다 작아지지 않도록)
                Math.Max((int)boltInspItem.ColorValue.G - boltInspItem.Bound, 0),         // 최소 Saturation
                Math.Max((int)boltInspItem.ColorValue.R - boltInspItem.Bound, 0)          // 최소 Value
            );

            Scalar upperBound = new Scalar(
                Math.Min((int)boltInspItem.ColorValue.B + boltInspItem.Bound, 255),       // 최대 Hue (OpenCV 기준 180)
                Math.Min((int)boltInspItem.ColorValue.G + boltInspItem.Bound, 255),       // 최대 Saturation
                Math.Min((int)boltInspItem.ColorValue.R + boltInspItem.Bound, 255)        // 최대 Value
            );

            // 마스크 생성
            Mat mask = new Mat();
            Cv2.InRange(CropImg, lowerBound, upperBound, mask);

            // 넓이 계산 (마스크에서 흰색 픽셀 개수)
            int areaPixels = Cv2.CountNonZero(mask);

            // 결과 이미지 생성
            Mat result = new Mat();
            Cv2.BitwiseAnd(CropImg, CropImg, result, mask);

            //Cv2.ImWrite(@"D:\bolt\bolt_" + boltInspItem.RegionID + "crop.bmp", CropImg);

            // 241206 임시
            if (areaPixels > bound)
                boltInspItem.InspectionResult = INSPECTION_RESULT.OK;
            else
                boltInspItem.InspectionResult = INSPECTION_RESULT.NG;


            //Scalar meanColor = Cv2.Mean(result, mask);

            //byte r = (byte)meanColor[2];  // Red (from BGR)
            //byte g = (byte)meanColor[1];  // Green (from BGR)
            //byte b = (byte)meanColor[0];  // Blue (from BGR)
            //byte a = 255;  // Set alpha to 255 (fully opaque)

            //boltInspItem.DetectedColor = System.Windows.Media.Color.FromArgb(a, r, g, b);

            //// MEER FOR WHITE BOLTS
            //if (boltInspItem.InspectionResult == INSPECTION_RESULT.NG && boltInspItem.ColorValue.R == boltInspItem.ColorValue.G && boltInspItem.ColorValue.G == boltInspItem.ColorValue.B)
            //{
            //    if (r > 190 && g > 190 && b > 190)
            //        boltInspItem.InspectionResult = INSPECTION_RESULT.OK;
            //}


            Cv2.ImWrite(@"D:\bolt\bolt_" + boltInspItem.RegionID + "_insp.bmp", result);
            return;
        }

        public void Execute()
        {
            foreach (BoltInspectionItem boltInspItem in this.BoltInspectionItems)
                FindBolt2(boltInspItem);

        }
    }
}
