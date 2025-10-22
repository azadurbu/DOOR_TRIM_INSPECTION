using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DOOR_TRIM_INSPECTION.Class
{
    public class ColorMatchInspectionItem
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

        public Color InspectedColor;
        public INSPECTION_RESULT InspectionResult;

        public ColorMatchInspectionItem(int RegionID, string DetectionClassName,  Rect ROI, Mat ImageRegion,  string ROIName, string ALC_CODE, string ALC_NAME, string group_name, Color RequiredColor)
        {
            this.RegionID = RegionID;
            this.DetectionClassName = DetectionClassName;
            this.ROI = ROI;
            this.ImageRegion = ImageRegion;
            this.ROIName = ROIName;
            this.ALC_CODE = ALC_CODE;
            this.ALC_NAME = ALC_NAME;
            this.group_name = group_name;
            this.RequiredColor = RequiredColor;
        }
    }

    public class ColorMatchInspection
    {
        public List<ColorMatchInspectionItem> ColorInspectionItems = new List<ColorMatchInspectionItem>();

        public ColorMatchInspection(Mat image, List<DetectionROIDetails> detROIs)
        {
            Color RequiredColor = Color.FromArgb(255, 0, 0, 0);
            foreach (DetectionROIDetails detROI in detROIs)
            {
                string ALCSPA = detROI.ALC_CODE;
                if (ALCSPA != "")
                {
                    string ALCData = Machine.ALCData.GetSPA(ALCSPA).Trim();
                    if (ALCData == "X")
                        continue;
                    if (detROI.group_name.Trim() != ALCData)
                        continue;
                }
                List<KeyValuePair<string, string>> keyValuePairs = HanMechDBHelper.ParseParam(detROI.Parameters);
                foreach (KeyValuePair<string, string> keyValuePair in keyValuePairs)
                {
                    if (ALGORITHM_OPTION.Color.ToString().Equals(keyValuePair.Key))
                    {
                        string[] valus = keyValuePair.Value.Split(',');
                        RequiredColor = Color.FromArgb(255, byte.Parse(valus[0]), byte.Parse(valus[1]), byte.Parse(valus[2]));
                    }
                }

                Rect ROI = new Rect(detROI.start_x, detROI.start_y, detROI.end_x - detROI.start_x, detROI.end_y - detROI.start_y);
                ColorInspectionItems.Add(new ColorMatchInspectionItem(
                    detROI.detection_roi_ID, detROI.DetectionClassName, ROI, new Mat(image, ROI), detROI.roi_name, detROI.ALC_CODE, detROI.ALC_NAME, detROI.group_name, RequiredColor
                ));
            }
        }

        private void CompareColor(ColorMatchInspectionItem colInspItem)
        {
            if (colInspItem.ImageRegion.Empty())
            {
                //throw new ArgumentException("The provided image region is empty.", nameof(image_region));
                colInspItem.InspectedColor = System.Windows.Media.Color.FromArgb(255, 0, 0, 0); // RETURN BLACK IN CASE OF EMPTY IMAGE
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

            colInspItem.InspectedColor = System.Windows.Media.Color.FromArgb(a, r, g, b);

            if (colInspItem.InspectedColor.R == colInspItem.RequiredColor.R &&
                colInspItem.InspectedColor.G == colInspItem.RequiredColor.G &&
                colInspItem.InspectedColor.B == colInspItem.RequiredColor.B)
                colInspItem.InspectionResult = INSPECTION_RESULT.OK;
            else
                colInspItem.InspectionResult = INSPECTION_RESULT.NG;
        }

        public void Execute()
        {
            foreach (ColorMatchInspectionItem colInspItem in this.ColorInspectionItems)
                CompareColor(colInspItem);

        }
    }
}
