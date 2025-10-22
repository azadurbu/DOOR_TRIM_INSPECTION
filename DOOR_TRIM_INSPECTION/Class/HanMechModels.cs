using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DOOR_TRIM_INSPECTION.Class
{
   
    public class RecipeDB
    {
        public int RecipeID { get; set; }
        public string RecipeName { get; set; }
        public string Model { get; set; }
        public string Year { get; set; }
        public string FrontImagePath { get; set; }
        public string RearImagePath { get; set; }
        public string RearSub1ImagePath { get; set; }
        public string DoorType { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ModifyDate { get; set; }

        public RecipeDB() { }
    }


    public class DetectionROIDetails
    {
        public int detection_roi_ID { get; set; }
        public int start_x { get; set; }
        public int start_y { get; set; }
        public int end_x { get; set; }
        public int end_y { get; set; }
        public int roi_name_location { get; set; }
        public string roi_name { get; set; }
        /// <summary>
        /// RuleID
        /// </summary>
        public int detection_class_ID { get; set; }
        public string DetectionClassName { get; set; }
        public string Parameters { get; set; }
        public int recipe_ID { get; set; }
        public int front_door { get; set; }
        public string ALC_CODE { get; set; }
        public string ALC_NAME { get; set; }
        public string group_name { get; set; }
        public bool Use { get; set; }

        public DetectionROIDetails() { }

        public int detection_roi_counter = 0;

        public DetectionROIDetailsUI toDetectionROIDetailsUI()
        {
            return new DetectionROIDetailsUI()
            {
                detection_roi_ID = this.detection_roi_ID,
                start_x = (double)this.start_x,
                start_y = (double)this.start_y,
                end_x = (double)this.end_x,
                end_y = (double)this.end_y,
                roi_name_location = this.roi_name_location,
                roi_name = this.roi_name.Trim(),
                detection_class_ID = this.detection_class_ID,
                DetectionClassName = this.DetectionClassName.Trim(),
                Parameters = this.Parameters.Trim(),
                recipe_ID = this.recipe_ID,
                front_door = this.front_door,
                ALC_CODE = this.ALC_CODE.Trim(),
                ALC_NAME = this.ALC_NAME.Trim(),
                group_name = this.group_name.Trim(),
                Use = this.Use
            };
        }
    }

    public class DetectionROIDetailsUI : Shape
    {
        public int roi_ui_counter { get; set; } // for ui only
        public bool isChecked { get; set; } // for ui only
        public int roi_name_location { get; set; }
        public int detection_roi_ID { get; set; }
        public double start_x { get; set; }
        public double start_y { get; set; }
        public double end_x { get; set; }
        public double end_y { get; set; }
        public string roi_name { get; set; }
        /// <summary>
        /// RuleID
        /// </summary>
        public int detection_class_ID { get; set; }
        public string DetectionClassName { get; set; }
        public string Parameters { get; set; }
        public int recipe_ID { get; set; }
        public int front_door { get; set; }
        public string ALC_CODE { get; set; } = "";
        public string ALC_NAME { get; set; } = "";
        public string group_name { get; set; } = "";
        public bool Use { get; set; }

        protected override Geometry DefiningGeometry
        {
            get
            {
                return new RectangleGeometry(new Rect(0, 0, this.Width, this.Height));
            }
        }

        public DetectionROIDetailsUI() { }

        public DetectionROIDetails toDetectionROIDetails()
        {
            return new DetectionROIDetails()
            {
                detection_roi_ID = this.detection_roi_ID,
                start_x = (int)this.start_x,
                start_y = (int)this.start_y,
                end_x = (int)this.end_x,
                end_y = (int)this.end_y,
                roi_name_location =  this.roi_name_location,
                roi_name = this.roi_name,
                detection_class_ID = this.detection_class_ID,
                DetectionClassName = this.DetectionClassName,
                Parameters = this.Parameters,
                recipe_ID = this.recipe_ID,
                front_door = this.front_door,
                ALC_CODE = this.ALC_CODE,
                ALC_NAME = this.ALC_NAME,
                group_name = this.group_name,
                Use = this.Use
            };
        }
    } 

    public class DETECT_NAME
    {
        public string NameEn { get; set; }
        public string NameKr { get; set; }
        public int RuleID { get; set; }
        public DETECT_NAME() { }
    }

    public class DETECT_LIST
    {
        public List<DETECT_NAME> Items = new List<DETECT_NAME>();
        public DETECT_LIST(List<DETECT_NAME> Items)
        { 
            this.Items = Items;
        }
    }

    // MEER 2024.12.05

    public class InspectionResult
    {
        public int InspectionResultID { get; set; }
        public DateTime InspectionTime { get; set; }
        public string DoorTrimID { get; set; }

        public int RecipeID { get; set; }
        public string Result { get; set; }

        public InspectionResult() { }
    }

    public class InspectionNGResult
    {
        public int InspectionNGResultID { get; set; }
        public int InspectionResultID { get; set; }
        public int DetectionRoiID { get; set; }
        public InspectionNGResult() { }
    }

    public class InspectionALCResult
    {
        public int InspectionALCResultID { get; set; }
        public int InspectionResultID { get; set; }
        public string ALCName { get; set; }
        public string ALCCode { get; set; }
        public string Result { get; set; }
        public InspectionALCResult() { }
    }

    public class DoorTrimInsp
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string InspResult { get; set; }

        public DoorTrimInsp() { }

        public DoorTrimInsp(string Name, string Type, string InspResult)
        {
            this.Name = Name;
            this.Type = Type;
            this.InspResult = InspResult;
        }
    }

    // MEER 2024.12.05

    // MEER 2025.01.10
    public class RecipeImage
    {
        public int DoorType { get; set; }
        public int RecipeID { get; set; }
        public System.Windows.Media.Imaging.BitmapImage FrontImage { get; set; }
        public System.Windows.Media.Imaging.BitmapImage RearImage { get; set; }
        public System.Windows.Media.Imaging.BitmapImage RearSub1Image { get; set; }

        public string FrontImagePath { get; set; } // MEER 2025.02.11
        public string RearImagePath { get; set; } // MEER 2025.02.11
        public string RearSub1ImagePath { get; set; } // MEER 2025.02.11


        public RecipeImage() { }

    }


    // MEER 2025.01.10

    // MEER 2025.01.14
    public class InspectionSummary
    {
        public int numScrewOK { get; set; }
        public int numScrewTotal { get; set; }
        public int numPadOK { get; set; }
        public int numPadTotal { get; set; }
        public int numSpeakerOK { get; set; }
        public int numSpeakerTotal { get; set; }
        public int numLeadwireOK { get; set; }
        public int numLeadwireTotal { get; set; }
        public int numFastenerOK { get; set; }
        public int numFastenerTotal { get; set; }
        public int numFusionOK { get; set; }
        public int numFusionTotal { get; set; }

        public InspectionSummary() { }

        public void SetScrewInfo(int OK, int Total)
        {
            numScrewOK = OK;
            numScrewTotal = Total;
        }
        public void SetPadInfo(int OK, int Total)
        {
            numPadOK = OK;
            numPadTotal = Total;
        }
        public void SetSpeakerInfo(int OK, int Total)
        {
            numSpeakerOK = OK;
            numSpeakerTotal = Total;
        }
        public void SetLeadwireInfo(int OK, int Total)
        {
            numLeadwireOK = OK;
            numLeadwireTotal = Total;
        }
        public void SetFastenerInfo(int OK, int Total)
        {
            numFastenerOK = OK;
            numFastenerTotal = Total;
        }
        public void SetFusionInfo(int OK, int Total)
        {
            numFusionOK = OK;
            numFusionTotal = Total;
        }

        public string ScrewInfo { get { return $"{numScrewOK}/{numScrewTotal}"; }}
        public string PadInfo { get { return $"{numPadOK}/{numPadTotal}"; }}
        public string SpeakerInfo { get { return $"{numSpeakerOK}/{numSpeakerTotal}"; }}
        public string LeadwireInfo { get { return $"{numLeadwireOK}/{numLeadwireTotal}"; } }
        public string FastenerInfo { get { return $"{numFastenerOK}/{numFastenerTotal}"; } }
        public string FusionInfo { get { return $"{numFusionOK}/{numFusionTotal}"; } }
    }
    // MEER 2025.01.14


    // MEER 2025.01.30
    public class ColorCode {
        public int ColorCodeID { get; set; }
        public string ColorName { get; set; }
        public string ColorValue { get; set; }
        public ColorCode() { }
    }
    // MEER 2025.01.30

    public class AIClass
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public double Score { get; set; } = 0;

        public AIClass()
        {
        }
    }
}
