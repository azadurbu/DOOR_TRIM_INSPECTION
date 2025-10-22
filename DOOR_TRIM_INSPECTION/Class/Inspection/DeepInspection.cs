using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOOR_TRIM_INSPECTION.Class
{
    public class DeepInspectionItem
    {
        public int RegionID;
        public string DetectionClassName;
        public string ROIName;
        public Rect ROI;

        public INSPECTION_RESULT InspectionResult;
        public bool IsDetected;
        public double Confidence;

        public DeepInspectionItem(int RegionID, string DetectionClassName, string ROIName ,Rect ROI)
        {
            this.RegionID = RegionID;
            this.DetectionClassName = DetectionClassName;
            this.ROIName = ROIName;
            this.ROI = ROI;
            this.InspectionResult = INSPECTION_RESULT.NG;
        }
    }
    public class DeepInspection
    {
        private List<DeepInspectionItem> DeepInspectionItems = new List<DeepInspectionItem>();
        public string tempFileName = "infer.bmp";

        //drkim 2025.04.30
        //public List<DeepInspectionItem> DeepScrewInspectionItems { get { return DeepInspectionItems.Where(x => x.DetectionClassName != "융착").ToList(); } }
        //public List<DeepInspectionItem> DeepFusionInspectionItems { get { return DeepInspectionItems.Where(x => x.DetectionClassName == "융착").ToList(); } }
        public List<DeepInspectionItem> DeepScrewInspectionItems { get { return DeepInspectionItems.Where(x => !x.DetectionClassName.Contains("융착")).ToList(); } }
        public List<DeepInspectionItem> DeepFusionInspectionItems { get { return DeepInspectionItems.Where(x => x.DetectionClassName.Contains("융착")).ToList(); } }
        public DeepInspection(List<DetectionROIDetails> detROIs)
        {
            foreach (DetectionROIDetails detROI in detROIs)
            {
                Rect ROI = new Rect(detROI.start_x, detROI.start_y, detROI.end_x - detROI.start_x, detROI.end_y - detROI.start_y);
                DeepInspectionItems.Add(
                    new DeepInspectionItem(detROI.detection_roi_ID, detROI.DetectionClassName, detROI.roi_name, ROI)
                       );
            }

        }

        public void Execute()
        {
            // CALL DEEP LEARNING METHOD AND GET THE IFEATURE LIST

#if USE_DEEP
            var detectedFeatures = Machine.cognexDeepVision.Infer(tempFileName).ToList<ViDi2.IFeature>();//new List<ViDi2.IFeature>(); 

            DeepInspectionComparison.MatchItems(this.DeepInspectionItems, detectedFeatures, 0.3, 50);
#endif
            // Display updated predefined items

            //Console.WriteLine($"DEEP DETECTION");
            //Console.WriteLine($"RegionID\tClass\tConfidence\tIsDetected");
            //foreach (var item in this.DeepInspectionItems.OrderBy(item => item.DetectionClassName).ThenBy(item => item.RegionID).ToList())
            //{
            //    Console.WriteLine($"{item.RegionID}\t{item.DetectionClassName}\t{item.Confidence}\t{item.IsDetected}");
            //}
            //foreach (var item in this.DeepInspectionItems)
            //{
            //    Console.WriteLine($"Predefined Item: RegionID={item.RegionID}, Class={item.DetectionClassName}, ROI={item.ROI}, IsDetected={item.IsDetected}, Confidence={item.Confidence}");
            //}


        }
    }

    public class SpatialHashTable
    {
        private readonly int cellSize;
        private readonly Dictionary<(int, int), List<DeepInspectionItem>> grid;

        public SpatialHashTable(int cellSize)
        {
            this.cellSize = cellSize;
            grid = new Dictionary<(int, int), List<DeepInspectionItem>>();
        }

        public void AddItem(DeepInspectionItem item)
        {
            var cells = GetCellsForItem(item);
            foreach (var cell in cells)
            {
                if (!grid.ContainsKey(cell))
                {
                    grid[cell] = new List<DeepInspectionItem>();
                }
                grid[cell].Add(item);
            }
        }

        public List<DeepInspectionItem> GetCandidates(DeepInspectionItem item)
        {
            var candidates = new List<DeepInspectionItem>();
            var cells = GetCellsForItem(item);

            foreach (var cell in cells)
            {
                if (grid.ContainsKey(cell))
                {
                    candidates.AddRange(grid[cell]);
                }
            }

            return candidates;
        }

        private List<(int, int)> GetCellsForItem(DeepInspectionItem item)
        {
            var cells = new List<(int, int)>();

            int startX = (int)(item.ROI.X / cellSize);
            int startY = (int)(item.ROI.Y / cellSize);
            int endX = (int)((item.ROI.X + item.ROI.Width) / cellSize);
            int endY = (int)((item.ROI.Y + item.ROI.Height) / cellSize);

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    cells.Add((x, y));
                }
            }

            return cells;
        }
    }

    public class DeepInspectionComparison
    {
        public static void MatchItems(List<DeepInspectionItem> predefinedItems, List<ViDi2.IFeature> detectedFeatures, double overlapThreshold, int cellSize)
        {
            var spatialHashTable = new SpatialHashTable(cellSize);

            // Add predefined items to the spatial hash table
            foreach (var item in predefinedItems)
            {
                spatialHashTable.AddItem(item);
            }

            // Convert IFeature objects to DeepInspectionItem and process
            foreach (var feature in detectedFeatures)
            {
                var detectedItem = new DeepInspectionItem(
                    RegionID: -1, // Unknown for detected features
                    DetectionClassName: feature.Name,
                    ROIName: "",
                    ROI: new Rect((int)feature.Position.X, (int)feature.Position.Y, (int)feature.Size.Width, (int)feature.Size.Height)
                )
                {
                    Confidence = feature.Score,
                    IsDetected = feature.IsSelected
                };

                var candidates = spatialHashTable.GetCandidates(detectedItem);
                bool matched = false;

                foreach (var candidate in candidates)
                {
                    //if (candidate.DetectionClassName == detectedItem.DetectionClassName &&
                    //    CalculateOverlap(candidate.ROI, detectedItem.ROI) >= overlapThreshold)

                    string detClassName = detectedItem.DetectionClassName;
                    AIClass aiClass = Machine.config.aiConfig.AIClasses.Find(x => x.Name == detClassName);
                    if (aiClass != null)
                    {
                        bool overLapPass = CalculateOverlap(candidate.ROI, detectedItem.ROI) >= overlapThreshold;

                        bool confidencePass = aiClass.Type == "OK" ? detectedItem.Confidence > aiClass.Score / 100.0 : detectedItem.Confidence < aiClass.Score / 100.0;
                        
                        //drkim 2025.04.30                        
                        if (
                            confidencePass
                            && overLapPass
                        )
                        {
                            //drkim 2025.04.30
                            candidate.DetectionClassName = string.Format("{0}({1}:{2})", "융착", detClassName, Math.Truncate(detectedItem.Confidence * 100));
                            candidate.IsDetected = true;
                            candidate.Confidence = detectedItem.Confidence;
                            candidate.InspectionResult = INSPECTION_RESULT.OK;
                            matched = true;
                            break;
                        }
                        else if (//drkim 2025.04.30
                            aiClass.Type == "NG"
                            && overLapPass
                        )
                        {
                            candidate.DetectionClassName = string.Format("{0}({1}:{2})", "융착", detClassName, Math.Truncate(detectedItem.Confidence * 100));
                            candidate.IsDetected = true;
                            candidate.Confidence = detectedItem.Confidence;
                            candidate.InspectionResult = INSPECTION_RESULT.NG;
                            matched = false;
                            break;
                        }
                        else if (aiClass.Type == "OK" && overLapPass)
                        {
                            candidate.DetectionClassName = string.Format("{0}({1}:{2})", "융착", "점수미달", Math.Truncate(detectedItem.Confidence * 100));
                            candidate.IsDetected = true;
                            candidate.Confidence = detectedItem.Confidence;
                            candidate.InspectionResult = INSPECTION_RESULT.NG;
                            matched = true;
                            break;
                        }
                    }
                }

                //if (!matched)
                //{
                //    // Optionally log or handle unmatched features
                //    Console.WriteLine($"Unmatched feature: Name={feature.Name}, Position={feature.Position}, Size={feature.Size}");
                //}
            }
        }

        private static double CalculateOverlap(Rect roi1, Rect roi2)
        {
            double xOverlap = Math.Max(0, Math.Min(roi1.Right, roi2.Right) - Math.Max(roi1.Left, roi2.Left));
            double yOverlap = Math.Max(0, Math.Min(roi1.Bottom, roi2.Bottom) - Math.Max(roi1.Top, roi2.Top));
            double intersection = xOverlap * yOverlap;

            double roi1Area = roi1.Width * roi1.Height;
            double roi2Area = roi2.Width * roi2.Height;

            return intersection / Math.Min(roi1Area, roi2Area);
        }
    }
}
