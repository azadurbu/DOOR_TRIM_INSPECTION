using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOOR_TRIM_INSPECTION.Class
{
    public class Recipe
    {
        public int RecipeID;
        public string Name;
        public string Model;
        public string Year;
        public string FrontImagePath;
        public string BackImagePath;
        public DOOR Door;

        private List<DetectionROIDetails> detectionROIDetails = new List<DetectionROIDetails>();

        // FRONT ROIs
        public List<DetectionROIDetails> FrontColorInspectionROIs = new List<DetectionROIDetails>();
        public List<DetectionROIDetails> FrontColorMatchInspectionROIs = new List<DetectionROIDetails>();
        public List<DetectionROIDetails> FrontPlugInspectionROIs = new List<DetectionROIDetails>();
        public List<DetectionROIDetails> FrontScrewInspectionROIs = new List<DetectionROIDetails>();
        public List<DetectionROIDetails> FrontBoltInspectionROIs = new List<DetectionROIDetails>();
        public List<DetectionROIDetails> FrontPadInspectionROIs = new List<DetectionROIDetails>();
        public List<DetectionROIDetails> FrontSpeakerInspectionROIs = new List<DetectionROIDetails>();
        public List<DetectionROIDetails> FrontSmallPadInspectionROIs = new List<DetectionROIDetails>();
        public List<DetectionROIDetails> FrontScrewMatchInspectionROIs = new List<DetectionROIDetails>();
        public List<DetectionROIDetails> FrontWhitePadInspectionROIs = new List<DetectionROIDetails>();

        // REAR ROIs
        public List<DetectionROIDetails> RearColorInspectionROIs = new List<DetectionROIDetails>();
        public List<DetectionROIDetails> RearPlugInspectionROIs = new List<DetectionROIDetails>();
        public List<DetectionROIDetails> RearDeepInspectionROIs = new List<DetectionROIDetails>();
        public List<DetectionROIDetails> RearScrewInspectionROIs = new List<DetectionROIDetails>();
        public List<DetectionROIDetails> RearBoltInspectionROIs = new List<DetectionROIDetails>();
        public List<DetectionROIDetails> RearPadInspectionROIs = new List<DetectionROIDetails>();
        public List<DetectionROIDetails> RearSpeakerInspectionROIs = new List<DetectionROIDetails>();
        public List<DetectionROIDetails> RearSmallPadInspectionROIs = new List<DetectionROIDetails>();
        public List<DetectionROIDetails> RearScrewMatchInspectionROIs = new List<DetectionROIDetails>();
        public List<DetectionROIDetails> RearPlugMatchInspectionROIs = new List<DetectionROIDetails>();
        public List<DetectionROIDetails> RearWhitePadInspectionROIs = new List<DetectionROIDetails>();

        public List<DetectionROIDetails> ALCLIst = new List<DetectionROIDetails>();
 #if USE_COGNEX
        public List<DetectionROIDetails> PlugCognexInspectionROIs = new List<DetectionROIDetails>();
 #endif
        // Dictionary<string, System.Windows.Media.Color> ColorDict = new Dictionary<string, System.Windows.Media.Color>(); // MEER 2024.05.14 REMOVING UNNECESSARY ITEMS
        public Recipe(int RecipeID)
        {
            this.RecipeID = RecipeID;
            detectionROIDetails = Machine.hmcDBHelper.GetDetectionROIs(RecipeID);

            // FILTER FRONT ROIs // MEER 2025.05.14
            FrontColorInspectionROIs = detectionROIDetails.Where(x => x.front_door == 1 && x.detection_class_ID == Machine.hmcDBHelper.dETECT_LIST.Items[0].RuleID).ToList();
            try
            {
                FrontColorMatchInspectionROIs = detectionROIDetails.Where(x => x.front_door == 1 && x.detection_class_ID == Machine.hmcDBHelper.dETECT_LIST.Items[10].RuleID).ToList();
            }
            catch { }
            FrontPlugInspectionROIs = detectionROIDetails.Where(x => x.front_door == 1 && x.detection_class_ID == Machine.hmcDBHelper.dETECT_LIST.Items[1].RuleID).ToList();

            FrontScrewInspectionROIs = detectionROIDetails.Where(x => x.front_door == 1 && x.detection_class_ID == Machine.hmcDBHelper.dETECT_LIST.Items[2].RuleID).ToList();
            FrontBoltInspectionROIs = detectionROIDetails.Where(x => x.front_door == 1 && x.detection_class_ID == Machine.hmcDBHelper.dETECT_LIST.Items[3].RuleID).ToList();
            FrontPadInspectionROIs = detectionROIDetails.Where(x => x.front_door == 1 && x.detection_class_ID == Machine.hmcDBHelper.dETECT_LIST.Items[4].RuleID).ToList();
            FrontSpeakerInspectionROIs = detectionROIDetails.Where(x => x.front_door == 1 && x.detection_class_ID == Machine.hmcDBHelper.dETECT_LIST.Items[6].RuleID).ToList();
            try { FrontSmallPadInspectionROIs = detectionROIDetails.Where(x => x.front_door == 1 && x.detection_class_ID == Machine.hmcDBHelper.dETECT_LIST.Items[7].RuleID).ToList(); } catch { }
            try { FrontScrewMatchInspectionROIs = detectionROIDetails.Where(x => x.front_door == 1 && x.detection_class_ID == Machine.hmcDBHelper.dETECT_LIST.Items[8].RuleID).ToList(); } catch { }
            try { FrontWhitePadInspectionROIs = detectionROIDetails.Where(x => x.front_door == 1 && x.detection_class_ID == Machine.hmcDBHelper.dETECT_LIST.Items[12].RuleID).ToList(); } catch { }


            // FILTER REAR ROIs // MEER 2025.05.14
            RearPlugInspectionROIs = detectionROIDetails.Where(x => x.front_door == 0 && x.detection_class_ID == Machine.hmcDBHelper.dETECT_LIST.Items[1].RuleID).ToList();
            RearScrewInspectionROIs = detectionROIDetails.Where(x => x.front_door == 0 && x.detection_class_ID == Machine.hmcDBHelper.dETECT_LIST.Items[2].RuleID).ToList();
            RearBoltInspectionROIs = detectionROIDetails.Where(x => x.front_door == 0 && x.detection_class_ID == Machine.hmcDBHelper.dETECT_LIST.Items[3].RuleID).ToList();
            RearPadInspectionROIs = detectionROIDetails.Where(x => x.front_door == 0 && x.detection_class_ID == Machine.hmcDBHelper.dETECT_LIST.Items[4].RuleID).ToList();
            //DeepInspectionROIs = detectionROIDetails.Where(x => x.front_door == 0 && x.detection_class_ID == Machine.hmcDBHelper.dETECT_LIST.Items[5].RuleID).ToList();
            RearSpeakerInspectionROIs = detectionROIDetails.Where(x => x.front_door == 0 && x.detection_class_ID == Machine.hmcDBHelper.dETECT_LIST.Items[6].RuleID).ToList();
            try { RearSmallPadInspectionROIs = detectionROIDetails.Where(x => x.front_door == 0 && x.detection_class_ID == Machine.hmcDBHelper.dETECT_LIST.Items[7].RuleID).ToList(); } catch { }
            try { RearScrewMatchInspectionROIs = detectionROIDetails.Where(x => x.front_door == 0 && x.detection_class_ID == Machine.hmcDBHelper.dETECT_LIST.Items[8].RuleID).ToList(); } catch { }
            try { RearPlugMatchInspectionROIs = detectionROIDetails.Where(x => x.front_door == 0 && x.detection_class_ID == Machine.hmcDBHelper.dETECT_LIST.Items[9].RuleID).ToList(); } catch { }
            try { RearWhitePadInspectionROIs = detectionROIDetails.Where(x => x.front_door == 0 && x.detection_class_ID == Machine.hmcDBHelper.dETECT_LIST.Items[12].RuleID).ToList(); } catch { }

            try
            {
                RearDeepInspectionROIs = detectionROIDetails.Where(x => x.front_door == 0 && x.detection_class_ID == Machine.hmcDBHelper.dETECT_LIST.Items[11].RuleID).ToList();
                //DeepInspectionROIs.AddRange(detectionROIDetails.Where(x => x.front_door == 0 && x.detection_class_ID == Machine.hmcDBHelper.dETECT_LIST.Items[2].RuleID).ToList()); //ScrewInspectionROIs // MEER 2025.04.25 REMOVING SCREW FROM DEEP
                //DeepInspectionROIs.AddRange(detectionROIDetails.Where(x => x.front_door == 0 && x.detection_class_ID == Machine.hmcDBHelper.dETECT_LIST.Items[8].RuleID).ToList()); //ScrewMatchInspectionROIs // MEER 2025.04.25 REMOVING SCREW FROM DEEP
            }
            catch { } // MEER 2025.01.24 DL
            ALCLIst = detectionROIDetails.Where(x => x.front_door == 0 && x.ALC_CODE != "").ToList();
            // ColorDict = Machine.hmcDBHelper.GetColorDictionary(); // MEER 2024.05.14 REMOVING UNNECESSARY ITEMS
#if USE_COGNEX
			try { PlugCognexInspectionROIs = detectionROIDetails.Where(x => x.front_door == 0 && (x.detection_class_ID == Machine.hmcDBHelper.dETECT_LIST.Items[13].RuleID || x.detection_class_ID == Machine.hmcDBHelper.dETECT_LIST.Items[14].RuleID)).ToList(); } catch { }
#endif
        }

        public void FilterALCCode()
        {

        }

        public string GetALCName(int RegionID)
        {
            DetectionROIDetails currentItem = detectionROIDetails.Where(x => x.detection_roi_ID == RegionID).First();
            if (currentItem != null)
            {
                string ALC_CODE = currentItem.ALC_CODE;
                return string.IsNullOrEmpty(ALC_CODE) ? string.Empty : ALC_CODE;
            }
            return string.Empty;
        }

        public string GetALCGroup(int RegionID)
        {
            DetectionROIDetails currentItem = detectionROIDetails.Where(x => x.detection_roi_ID == RegionID).First();
            if (currentItem != null)
            {
                string ALC_GROUP = currentItem.group_name;
                return string.IsNullOrEmpty(ALC_GROUP) ? string.Empty : ALC_GROUP;
            }
            return string.Empty;
        }

    }
}
