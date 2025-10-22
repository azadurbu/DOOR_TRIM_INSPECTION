using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DOOR_TRIM_INSPECTION.Class
{
    public class ColorInspectionItem
    {
        public int RegionID;
        public Rect ROI;
        public Mat ImageRegion;
        public Color RequiredColor;
        
        public string ROIName;
        public string DetectionClassName;
        public string ALC_CODE;
        public string ALC_NAME;
        public string group_name;
        public INSPECTION_RESULT InspectionResult;
        public Color Color;

        public ColorInspectionItem(int RegionID,string DetectionClassName, Rect ROI, Mat ImageRegion,string ROIName, string ALC_CODE, string ALC_NAME, string group_name)
        {
            this.RegionID = RegionID;
            this.DetectionClassName = DetectionClassName;
            this.ROI = ROI;
            this.ImageRegion = ImageRegion;
            this.ROIName = ROIName;
            this.ALC_CODE = ALC_CODE;
            this.ALC_NAME = ALC_NAME;
            this.group_name = group_name;
        }
    }
    public class ColorInspection
    {
        private int Bound = 30;

        public List<ColorInspectionItem> ColorInspectionItems = new List<ColorInspectionItem>();

        public ColorInspection(Mat image, List<DetectionROIDetails> detROIs)
        {
            foreach (DetectionROIDetails detROI in detROIs)
            {
                string ALCSPA = detROI.ALC_CODE;
                if (ALCSPA != "")
                {
                    string ALCData = Machine.ALCData.GetSPA(ALCSPA).Trim();
                    if (ALCData == "-")
                        continue;
                    if (!detROI.group_name.Trim().Contains(ALCData))
                        continue;
                }
                Rect ROI = new Rect(detROI.start_x, detROI.start_y, detROI.end_x - detROI.start_x, detROI.end_y - detROI.start_y);
                ColorInspectionItems.Add(new ColorInspectionItem(
                    detROI.detection_roi_ID,detROI.DetectionClassName, ROI, new Mat(image, ROI),detROI.roi_name, detROI.ALC_CODE, detROI.ALC_NAME, detROI.group_name
                ));
            }
        }

        private void FindDominantColor(ColorInspectionItem colInspItem)
        {
            if (colInspItem.ImageRegion.Empty())
            {
                //throw new ArgumentException("The provided image region is empty.", nameof(image_region));
                colInspItem.Color = System.Windows.Media.Color.FromArgb(255, 0, 0, 0); // RETURN BLACK IN CASE OF EMPTY IMAGE
                return;
            }

            if (!colInspItem.ImageRegion.IsContinuous())
            {
                colInspItem.ImageRegion = colInspItem.ImageRegion.Clone();
            }

            // Reshape the region to a 1D array of pixels (rows * cols x channels)
            var pixels = colInspItem.ImageRegion.Reshape(1, colInspItem.ImageRegion.Rows * colInspItem.ImageRegion.Cols);

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

            colInspItem.Color = System.Windows.Media.Color.FromArgb(a, r, g, b);

            //if (!string.IsNullOrEmpty(colInspItem.group_name))
            //{
            //    ColorCode colorCode = Machine.hmcDBHelper.GetColorByALC(colInspItem.group_name, $"{r},{g},{b}");
            //    if (colorCode != null)
            //    {
            //        string[] colorVal = colorCode.ColorValue.Split(',');
            //        System.Windows.Media.Color RequiredColor = System.Windows.Media.Color.FromArgb(255, byte.Parse(colorVal[0]), byte.Parse(colorVal[1]), byte.Parse(colorVal[2]));

            //        // CHECK COLOR RANGE
            //        if ((RequiredColor.R + Bound > r && RequiredColor.R - Bound < r)
            //           && (RequiredColor.G + Bound > g && RequiredColor.G - Bound < g)
            //           && (RequiredColor.B + Bound > b && RequiredColor.B - Bound < b))
            //        {
            //            colInspItem.InspectionResult = INSPECTION_RESULT.OK;
            //        }
            //        else
            //        {
            //            colInspItem.InspectionResult = INSPECTION_RESULT.NG;
            //        }
            //    }
            //    else
            //    {
            //        colInspItem.InspectionResult = INSPECTION_RESULT.OK;
            //    }
            //}
            colInspItem.InspectionResult = INSPECTION_RESULT.OK;
        }

        public void Execute()
        {
            foreach (ColorInspectionItem colInspItem in this.ColorInspectionItems)
                FindDominantColor(colInspItem);

        }
    }
}
