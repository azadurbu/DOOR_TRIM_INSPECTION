using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOOR_TRIM_INSPECTION.Class
{
    public enum eSeqStep
    {
        SEQ_STOP,
        SEQ_START,
        SEQ_READ_BARCODE,

        SEQ_WAIT_SGINAL_FIRST,
        SEQ_GRAB_FIRST,
        SEQ_WAIT_GRAB_END_FIRST,
        SEQ_STOP_GRAB_FIRST,
        SEQ_INSP_FIRST,
        SEQ_RESET_LIGHT_FIRST,
        SEQ_WAIT_INSP_END_FIRST,
        SEQ_SEND_RESULT_FIRST,
        SEQ_SAVE_IMAGE_FIRST,

        SEQ_WAIT_OFF_CAPTURE_SIGNAL,

        SEQ_WAIT_SGINAL_SECOND,
        SEQ_GRAB_SECOND,
        SEQ_WAIT_GRAB_END_SECOND,
        SEQ_STOP_GRAB_SECOND,
        SEQ_INSP_SECOND,
        SEQ_RESET_LIGHT_SECOND,
        SEQ_WAIT_INSP_END_SECOND,
        SEQ_SEND_RESULT_SECOND,
        SEQ_SAVE_IMAGE_SECOND,

        SEQ_END,
        SEQ_ERROR,
    }
    public enum DIO_INPUT_DATA
    {
        CAPTURE_START,
        FRONT_REAR_DOOR,
        RIGHT_LEFT_DOOR,
        BUTTON_PRESS
    }

    public enum DEEP_CLASS
    {
        NONE = -1,
        SILVER_SCREW = 0,
        BLACK_SCREW = 1,
        WHITE_SCREW = 2,
    }

    public enum ALGORITHM_OPTION
    {
        MaskMinThreshold,
        MaskMaxThreshold,
        MaskThresholdType,
        MorphMinThreshold,
        MorphMaxThreshold,
        MorphThresholdType,
        MinLengthError,
        MaxLengthError,
        MaxLengthX,
        MaxLengthY,
        MinContourArea,
        MinError,
        MaxError,
        ContourArea,
        MinContour,
        MinTotalArea,
        MaxTotalArea,
        Color,
        Bound,
        MinThreshold,
        MaxThreshold,
        Accuracy,
        TemplatePath,
        TemplatePath1,
        TemplatePath2,
        Direction,
        PlugDistanceX,
        PlugDistanceY,
        AvgColor,
        Variance,
        WhitePixelCount,
        AlternateRoi,
        BLOBCount,
#if USE_COGNEX
        PlugVppPath1,
        PlugVppPath2,
        PlugCogDistanceX,
        PlugCogDistanceY,
        InnerConfidence,
        OuterConfidence
#endif
    }

    public enum INSPECTION_RESULT
    {
        OK,
        NG, 
        NOT_FOUND,
    }

    public enum DIO_OUTPUT_DATA
    {
        MODEL_FRONT_REAR,
        START_MOVE,
        RESULT_OK,
        RESULT_NG,
        MODEL_LEFT_RIGHT,
        LIGHT_RESET=5,
    }
    
    public enum eAnalysisMethod
    {
        RULE,
        DEEP,
        HYBRID
    }

    public enum DOOR
    {
        FRONT_RIGHT,
        FRONT_LEFT,
        REAR_RIGHT,
        REAR_LEFT,
    }

    public enum eProgramMode
    {
        Inspection,
        Stop,
        Test,
    }

    public enum FORM_MODE
    {
        DISABLED,
        ADD,
        EDIT
    }
    public enum RECIPE_SETUP_TABS
    {
        RECIPE_LIST,
        RECIPE_WIZARD
    };

    public enum RECIPE_SETUP_STEPS
    {
        BASIC_INFO,
        FRONT_ROI,
        REAR_ROI
    };

    public enum INSPECTION_MODE
    {
        DEFAULT,
        MASTER,
        HISTORY,
    }
}
