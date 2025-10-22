using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media;

namespace DOOR_TRIM_INSPECTION.Class
{
    public class TrialBoltInspectionItem
    {
        public Mat ImageRegion;
        public Color ColorValue;
        public int MinTotalArea;
        public int Bound;

        public Color DetectedColor;
        public INSPECTION_RESULT InspectionResult;
        public Mat ResultImageRegion;
        public string Conditions;

        public TrialBoltInspectionItem(Mat ImageRegion, Color ColorValue, int Bound, int MinTotalArea)
        {
            this.ImageRegion = ImageRegion;
            this.ColorValue = ColorValue;
            this.Bound = Bound;
            this.MinTotalArea = MinTotalArea;
        }
    }


    public class TrialPadInspectionItem
    {
        public Mat ImageRegion;
        public int MinThreshold;
        public int MaxThreshold;
        public int Variance;
        public int MinTotalArea;
        public int MaxTotalArea;

        public INSPECTION_RESULT InspectionResult;
        public Mat ResultImageRegion;
        public string Conditions;

        public TrialPadInspectionItem(Mat ImageRegion, int MinThreshold, int MaxThreshold, int Variance)
        {
            this.ImageRegion = ImageRegion;
            this.MinThreshold = MinThreshold;
            this.MaxThreshold = MaxThreshold;
            this.Variance = Variance;

        }
    }

    public class TrialWhitePadInspectionItem
    {
        public Mat ImageRegion;
        public int MinThreshold;
        public int MaxThreshold;
        public int MinTotalArea;
        public int MaxTotalArea;

        public INSPECTION_RESULT InspectionResult;
        public Mat ResultImageRegion;
        public string Conditions;

        public TrialWhitePadInspectionItem(Mat ImageRegion, int MinThreshold, int MaxThreshold, int MinTotalArea, int MaxTotalArea)
        {
            this.ImageRegion = ImageRegion;
            this.MinThreshold = MinThreshold;
            this.MaxThreshold = MaxThreshold;
            this.MinTotalArea = MinTotalArea;
            this.MaxTotalArea = MaxTotalArea;

        }
    }

    public class TrialPlugMatchInspectionItem
    {
        public Mat ImageRegion;
        public string TemplatePath01;
        public string TemplatePath02;
        public double Accuracy;
        public int MaxLengthX;
        public int MaxLengthY;
        public string Direction;
        public int PlugDistanceX;
        public int PlugDistanceY;

        public INSPECTION_RESULT InspectionResult;
        public Mat ResultImageRegion;
        public string Conditions;

        public TrialPlugMatchInspectionItem(Mat ImageRegion, string templatePath01, string templatePath02, int PlugDistanceX, int PlugDistanceY, double Accuracy, int MaxLengthX, int MaxLengthY, string Direction)
        {
            this.ImageRegion = ImageRegion;
            this.TemplatePath01 = templatePath01;
            this.TemplatePath02 = templatePath02;
            this.Accuracy = Accuracy;
            this.MaxLengthX = MaxLengthX;
            this.MaxLengthY = MaxLengthY;
            this.Direction = Direction;
            this.PlugDistanceX = PlugDistanceX;
            this.PlugDistanceY = PlugDistanceY;
        }
    }
#if USE_COGNEX
    public class TrialCognexPlugMatchInspectionItem
    {
        public Mat ImageRegion;
        public string VppPath01;
        public string VppPath02;
        public double Accuracy;
        public int OuterConfidence;
        public int InnerConfidence;
        public int MaxLengthX;
        public int MaxLengthY;
        public string Direction;
        public int PlugCogDistanceX;
        public int PlugCogDistanceY;

        public INSPECTION_RESULT InspectionResult;
        public Mat ResultImageRegion;
        public string Conditions;

        public TrialCognexPlugMatchInspectionItem(Mat ImageRegion, string VppPath01, string VppPath02, int PlugCogDistanceX, int PlugCogDistanceY, int OuterConfidence, int InnerConfidence, int MaxLengthX, int MaxLengthY, string Direction)
        {
            this.ImageRegion = ImageRegion;
            this.VppPath01 = VppPath01;
            this.VppPath02 = VppPath02;
            this.OuterConfidence = OuterConfidence;
            this.InnerConfidence = InnerConfidence;
            this.MaxLengthX = MaxLengthX;
            this.MaxLengthY = MaxLengthY;
            this.Direction = Direction;
            this.PlugCogDistanceX = PlugCogDistanceX;
            this.PlugCogDistanceY = PlugCogDistanceY;
        }
    }
#endif
    public class TrialScrewInspectionItem
    {
        public Mat ImageRegion;
        public int MinThreshold;
        public int MaxThreshold;
        ThresholdTypes ThresholdType;
        public int MinContourArea;

        public Point[] DetectedContour;
        public Rect DetectedRect;

        public INSPECTION_RESULT InspectionResult;
        public Mat ResultImageRegion;
        public string Conditions;

        public TrialScrewInspectionItem(Mat ImageRegion, int MinThreshold, int MaxThreshold, ThresholdTypes ThresholdType, int MinContourArea)
        {
            this.ImageRegion = ImageRegion;
            this.MinThreshold = MinThreshold;
            this.MaxThreshold = MaxThreshold;
            this.ThresholdType = ThresholdType;
            this.MinContourArea = MinContourArea;
        }
    }


    public class TrialSmallPadInspectionItem
    {
        public Mat ImageRegion;
        public int MinThreshold;
        public int MaxThreshold;
        public int Variance;
        public int WhitePixelCount;
        public INSPECTION_RESULT InspectionResult;
        public Mat ResultImageRegion;
        public string Conditions;

        public TrialSmallPadInspectionItem(Mat ImageRegion, int MinThreshold, int MaxThreshold, int Variance, int WhitePixelCount)
        {
            this.ImageRegion = ImageRegion;
            this.MinThreshold = MinThreshold;
            this.MaxThreshold = MaxThreshold;
            this.Variance = Variance;
            this.WhitePixelCount = WhitePixelCount;
        }
    }


    public class TrialSpeakerInspectionItem
    {
        public Mat ImageRegion;
        public string TemplatePath;
        public double Accuracy;
        public INSPECTION_RESULT InspectionResult;
        public Mat ResultImageRegion;
        public string Conditions;

        public TrialSpeakerInspectionItem(Mat ImageRegion, double Accuracy, string templatePath)
        {
            this.ImageRegion = ImageRegion;
            this.Accuracy = Accuracy;
            this.TemplatePath = templatePath;
        }
    }


    public class TrialColorMatchInspectionItem
    {
        public Mat ImageRegion;
        public Color InspectedColor;
        public string Conditions;
        public Color RequiredColor;
        public int Bound;
        public INSPECTION_RESULT InspectionResult;
        public Mat ResultImageRegion;

        public TrialColorMatchInspectionItem(Mat imageRegion, Color RequiredColor, int bound)
        {
            this.ImageRegion = imageRegion;
            this.RequiredColor = RequiredColor;
            Bound = bound;
        }
    }

    public class TrialColorInspectionItem
    {
        public Mat ImageRegion;
        public Color Color;
        public string Conditions;

        public TrialColorInspectionItem(Mat imageRegion, Color color)
        {
            this.ImageRegion = imageRegion;
            this.Color = color;
        }
    }

    public class TrialPlugInspectionItem
    {
        public Mat ImageRegion;
        public int MaskMinThreshold;
        public int MaskMaxThreshold;
        public ThresholdTypes MaskThresholdType;
        public int MorphMinThreshold;
        public int MorphMaxThreshold;
        public ThresholdTypes MorphThresholdType;
        public int MinError;
        public int MaxError;
        public int MinContourArea;

        public Mat MorphedImageRegion;

        public Point[] DetectedContour;
        public Rect DetectedRect;
        public INSPECTION_RESULT InspectionResult;
        public Mat ResultImageRegion;
        public string Conditions;

        public TrialPlugInspectionItem(Mat ImageRegion,
            int MaskMinThreshold, int MaskMaxThreshold, ThresholdTypes MaskThresholdType, int MorphMinThreshold, int MorphMaxThreshold, ThresholdTypes MorphThresholdType,
            int MinError, int MaxError, int MinContourArea)
        {
            this.ImageRegion = ImageRegion;
            this.MaskMinThreshold = MaskMinThreshold;
            this.MaskMaxThreshold = MaskMaxThreshold;
            this.MaskThresholdType = MaskThresholdType;
            this.MorphMinThreshold = MorphMinThreshold;
            this.MorphMaxThreshold = MorphMaxThreshold;
            this.MorphThresholdType = MorphThresholdType;
            this.MinError = MinError;
            this.MaxError = MaxError;
            this.MinContourArea = MinContourArea;
        }
    }

    public class TrialScrewMacthInspectionItem
    {
        public Mat ImageRegion;
        public string TemplatePath;
        public double Accuracy;
        public INSPECTION_RESULT InspectionResult;
        public Mat ResultImageRegion;
        public string Conditions;

        public TrialScrewMacthInspectionItem(Mat ImageRegion, double Accuracy, string templatePath)
        {
            this.ImageRegion = ImageRegion;
            this.Accuracy = Accuracy;
            TemplatePath = templatePath;
        }
    }

    public class TrialInspection
    {
        public static int MaskMinThreshold { get; set; }
        public static int MaskMaxThreshold { get; set; }
        public static int MorphMinThreshold { get; set; }
        public static int MorphMaxThreshold { get; set; }
        public static int MinLengthError { get; set; }
        public static int MaxLengthError { get; set; }
        public static int MaxLengthX { get; set; }
        public static int MaxLengthY { get; set; }
        public static int MinContourArea { get; set; }
        public static int MinContour { get; set; }
        public static int MinError { get; set; }
        public static int MaxError { get; set; }
        public static int ContourArea { get; set; }
        public static int MinTotalArea { get; set; }
        public static int MaxTotalArea { get; set; }
        public static int Bound { get; set; }
        public static int MinThreshold { get; set; }
        public static int MaxThreshold { get; set; }
        public static int Accuracy { get; set; }
        public static string TemplatePath { get; set; }
        public static string TemplatePath1 { get; set; }
        public static string TemplatePath2 { get; set; }
        public static string Direction { get; set; }
        public static ThresholdTypes MaskThresholdType { get; set; }
        public static ThresholdTypes MorphThresholdType { get; set; }
        public static Color Color { get; set; }
        public static Color AvgColor { get; set; }
        public static int PlugDistanceX { get; set; }
        public static int PlugDistanceY { get; set; }
        public static int Variance { get; set; }
        public static int WhitePixelCount { get; set; }
        public static Rect AlternateRoi { get; set; } = new Rect(0, 0, 0, 0);

        public static string AlternateImagePath { get; set; }

        public static int BLOBCount { get; set; }
#if USE_COGNEX
        public static string PlugVppPath1 { get; set; }
        public static string PlugVppPath2 { get; set; }
        public static int PlugCogDistanceX { get; set; }
        public static int PlugCogDistanceY { get; set; }
        public static int InnerConfidence { get; set; }
        public static int OuterConfidence { get; set; }
#endif

        public static void ClearParameters()
        {
            MaskMinThreshold = int.MinValue;
            MaskMaxThreshold = int.MinValue;
            MorphMinThreshold = int.MinValue;
            MorphMaxThreshold = int.MinValue;
            MinLengthError = int.MinValue;
            MaxLengthError = int.MinValue;
            MaxLengthX = int.MinValue;
            MaxLengthY = int.MinValue;
            MinContourArea = int.MinValue;
            MinContour = int.MinValue;
            MinError = int.MinValue;
            MaxError = int.MinValue;
            ContourArea = int.MinValue;
            MinTotalArea = int.MinValue;
            MaxTotalArea = int.MinValue;
            Bound = int.MinValue;
            MinThreshold = int.MinValue;
            MaxThreshold = int.MinValue;
            Accuracy = int.MinValue;
            TemplatePath = string.Empty;
            TemplatePath1 = string.Empty;
            TemplatePath2 = string.Empty;
            Direction = string.Empty;
            PlugDistanceX = int.MinValue;
            PlugDistanceY = int.MinValue;
            Variance = int.MinValue;
            WhitePixelCount = int.MinValue;
            AlternateRoi = new Rect(0, 0, 0, 0);
            BLOBCount = int.MinValue;
#if USE_COGNEX
            PlugVppPath1 = string.Empty;
            PlugVppPath2 = string.Empty;
            PlugCogDistanceX = int.MinValue;
            PlugCogDistanceY = int.MinValue;
            InnerConfidence = int.MinValue;
            OuterConfidence = int.MinValue;
#endif
        }

        public static void SetParameters(List<string> paramOptions)
        {
            foreach (string paramOption in paramOptions)
            {
                string[] keyVal = paramOption.Split(';');
                if (ALGORITHM_OPTION.MaskMinThreshold.ToString().Equals(keyVal[0]))
                    MaskMinThreshold = Convert.ToInt32(keyVal[1]);
                else if (ALGORITHM_OPTION.MaskMaxThreshold.ToString().Equals(keyVal[0]))
                    MaskMaxThreshold = Convert.ToInt32(keyVal[1]);
                else if (ALGORITHM_OPTION.MorphMinThreshold.ToString().Equals(keyVal[0]))
                    MorphMinThreshold = Convert.ToInt32(keyVal[1]);
                else if (ALGORITHM_OPTION.MorphMaxThreshold.ToString().Equals(keyVal[0]))
                    MorphMaxThreshold = Convert.ToInt32(keyVal[1]);
                else if (ALGORITHM_OPTION.MinLengthError.ToString().Equals(keyVal[0]))
                    MinLengthError = Convert.ToInt32(keyVal[1]);
                else if (ALGORITHM_OPTION.MaxLengthError.ToString().Equals(keyVal[0]))
                    MaxLengthError = Convert.ToInt32(keyVal[1]);
                else if (ALGORITHM_OPTION.MaxLengthX.ToString().Equals(keyVal[0]))
                    MaxLengthX = Convert.ToInt32(keyVal[1]);
                else if (ALGORITHM_OPTION.MaxLengthY.ToString().Equals(keyVal[0]))
                    MaxLengthY = Convert.ToInt32(keyVal[1]);
                else if (ALGORITHM_OPTION.MinContourArea.ToString().Equals(keyVal[0]))
                    MinContourArea = Convert.ToInt32(keyVal[1]);
                else if (ALGORITHM_OPTION.MinContour.ToString().Equals(keyVal[0]))
                    MinContour = Convert.ToInt32(keyVal[1]);
                else if (ALGORITHM_OPTION.MinError.ToString().Equals(keyVal[0]))
                    MinError = Convert.ToInt32(keyVal[1]);
                else if (ALGORITHM_OPTION.MaxError.ToString().Equals(keyVal[0]))
                    MaxError = Convert.ToInt32(keyVal[1]);
                else if (ALGORITHM_OPTION.ContourArea.ToString().Equals(keyVal[0]))
                    ContourArea = Convert.ToInt32(keyVal[1]);
                else if (ALGORITHM_OPTION.MinTotalArea.ToString().Equals(keyVal[0]))
                    MinTotalArea = Convert.ToInt32(keyVal[1]);
                else if (ALGORITHM_OPTION.MaxTotalArea.ToString().Equals(keyVal[0]))
                    MaxTotalArea = Convert.ToInt32(keyVal[1]);
                else if (ALGORITHM_OPTION.Bound.ToString().Equals(keyVal[0]))
                    Bound = Convert.ToInt32(keyVal[1]);
                else if (ALGORITHM_OPTION.MinThreshold.ToString().Equals(keyVal[0]))
                    MinThreshold = Convert.ToInt32(keyVal[1]);
                else if (ALGORITHM_OPTION.MaxThreshold.ToString().Equals(keyVal[0]))
                    MaxThreshold = Convert.ToInt32(keyVal[1]);
                else if (ALGORITHM_OPTION.Accuracy.ToString().Equals(keyVal[0]))
                    Accuracy = Convert.ToInt32(keyVal[1]);
                else if (ALGORITHM_OPTION.TemplatePath.ToString().Equals(keyVal[0]))
                    TemplatePath = Convert.ToString(keyVal[1]);
                else if (ALGORITHM_OPTION.TemplatePath1.ToString().Equals(keyVal[0]))
                    TemplatePath1 = Convert.ToString(keyVal[1]);
                else if (ALGORITHM_OPTION.TemplatePath2.ToString().Equals(keyVal[0]))
                    TemplatePath2 = Convert.ToString(keyVal[1]);
                else if (ALGORITHM_OPTION.PlugDistanceX.ToString().Equals(keyVal[0]))
                    PlugDistanceX = Convert.ToInt32(keyVal[1]);
                else if (ALGORITHM_OPTION.PlugDistanceY.ToString().Equals(keyVal[0]))
                    PlugDistanceY = Convert.ToInt32(keyVal[1]);
                else if (ALGORITHM_OPTION.Direction.ToString().Equals(keyVal[0]))
                    Direction = Convert.ToString(keyVal[1]);
                else if (ALGORITHM_OPTION.MaskThresholdType.ToString().Equals(keyVal[0]))
                    MaskThresholdType = ThresholdTypes.BinaryInv;
                else if (ALGORITHM_OPTION.MorphThresholdType.ToString().Equals(keyVal[0]))
                    MorphThresholdType = ThresholdTypes.Binary;
                else if (ALGORITHM_OPTION.Color.ToString().Equals(keyVal[0]))
                {
                    string[] valus = keyVal[1].Split(',');
                    Color = Color.FromArgb(255, byte.Parse(valus[0]), byte.Parse(valus[1]), byte.Parse(valus[2]));
                }
                else if (ALGORITHM_OPTION.AvgColor.ToString().Equals(keyVal[0]))
                {
                    string[] valus = keyVal[1].Split(',');
                    AvgColor = Color.FromArgb(255, byte.Parse(valus[0]), byte.Parse(valus[1]), byte.Parse(valus[2]));
                }
                else if (ALGORITHM_OPTION.Variance.ToString().Equals(keyVal[0]))
                    Variance = Convert.ToInt32(keyVal[1]);
                else if (ALGORITHM_OPTION.WhitePixelCount.ToString().Equals(keyVal[0]))
                    WhitePixelCount = Convert.ToInt32(keyVal[1]);
                else if (ALGORITHM_OPTION.AlternateRoi.ToString().ToLower().Equals(keyVal[0].ToLower()))
                {
                    string[] valus = keyVal[1].Split(',');
                    AlternateRoi = new Rect(int.Parse(valus[0]), int.Parse(valus[1]), int.Parse(valus[2]), int.Parse(valus[3]));
                    if (valus.Count() > 4)
                        AlternateImagePath = valus[4];
                }
                else if (ALGORITHM_OPTION.BLOBCount.ToString().Equals(keyVal[0]))
                {
                    BLOBCount = Convert.ToInt32(keyVal[1]);
                }
#if USE_COGNEX
                else if (ALGORITHM_OPTION.PlugVppPath1.ToString().Equals(keyVal[0]))
                {
                    PlugVppPath1 = Convert.ToString(keyVal[1]);
                }
                else if (ALGORITHM_OPTION.PlugVppPath2.ToString().Equals(keyVal[0]))
                {
                    PlugVppPath2 = Convert.ToString(keyVal[1]);
                }
                else if (ALGORITHM_OPTION.PlugCogDistanceX.ToString().Equals(keyVal[0]))
                {
                    PlugCogDistanceX = Convert.ToInt32(keyVal[1]);
                }
                else if (ALGORITHM_OPTION.PlugCogDistanceY.ToString().Equals(keyVal[0]))
                {
                    PlugCogDistanceY = Convert.ToInt32(keyVal[1]);
                }
                else if (ALGORITHM_OPTION.InnerConfidence.ToString().Equals(keyVal[0]))
                {
                    InnerConfidence = Convert.ToInt32(keyVal[1]);
                }
                else if (ALGORITHM_OPTION.OuterConfidence.ToString().Equals(keyVal[0]))
                {
                    OuterConfidence = Convert.ToInt32(keyVal[1]);
                }
#endif
            }
        }

        public static TrialBoltInspectionItem ExecuteTrialBoltInpection(TrialBoltInspectionItem tBoltInspItem)
        {
            int bound = tBoltInspItem.MinTotalArea;
            if (tBoltInspItem.ImageRegion.Empty())
            {
                //throw new ArgumentException("The provided image region is empty.", nameof(image_region));
                tBoltInspItem.ColorValue = System.Windows.Media.Color.FromArgb(255, 0, 0, 0); // RETURN BLACK IN CASE OF EMPTY IMAGE
                return tBoltInspItem;
            }

            if (!tBoltInspItem.ImageRegion.IsContinuous())
            {
                tBoltInspItem.ImageRegion = tBoltInspItem.ImageRegion.Clone();
            }

            Mat CropImg = tBoltInspItem.ImageRegion.Clone();

            Cv2.MedianBlur(CropImg, CropImg, 15);

            double min = 0;
            if (tBoltInspItem.Bound != 0)
                min = tBoltInspItem.Bound / 5;

            Scalar lowerBound = new Scalar(
                Math.Max((int)tBoltInspItem.ColorValue.B - tBoltInspItem.Bound, 0),         // 최소 Hue (0보다 작아지지 않도록)
                Math.Max((int)tBoltInspItem.ColorValue.G - tBoltInspItem.Bound, 0),         // 최소 Saturation
                Math.Max((int)tBoltInspItem.ColorValue.R - tBoltInspItem.Bound, 0)          // 최소 Value
            );

            Scalar upperBound = new Scalar(
                Math.Min((int)tBoltInspItem.ColorValue.B + tBoltInspItem.Bound, 255),       // 최대 Hue (OpenCV 기준 180)
                Math.Min((int)tBoltInspItem.ColorValue.G + tBoltInspItem.Bound, 255),       // 최대 Saturation
                Math.Min((int)tBoltInspItem.ColorValue.R + tBoltInspItem.Bound, 255)        // 최대 Value
            );

            // 마스크 생성
            Mat mask = new Mat();
            Cv2.InRange(CropImg, lowerBound, upperBound, mask);

            // 넓이 계산 (마스크에서 흰색 픽셀 개수)
            int areaPixels = Cv2.CountNonZero(mask);

            // 결과 이미지 생성
            Mat result = new Mat();
            Cv2.BitwiseAnd(CropImg, CropImg, result, mask);
            tBoltInspItem.ResultImageRegion = result;

            bool boundaryCondPassed = Math.Abs((int)tBoltInspItem.ColorValue.B - lowerBound.Val0) <= Bound
                && Math.Abs((int)tBoltInspItem.ColorValue.G - lowerBound.Val1) <= Bound
                && Math.Abs((int)tBoltInspItem.ColorValue.R - lowerBound.Val2) <= Bound
                && Math.Abs((int)tBoltInspItem.ColorValue.B - upperBound.Val0) <= Bound
                && Math.Abs((int)tBoltInspItem.ColorValue.B - upperBound.Val0) <= Bound
                && Math.Abs((int)tBoltInspItem.ColorValue.B - upperBound.Val0) <= Bound;

            string strConditionResult = areaPixels < bound ? $"Pixel Area {areaPixels} < Min Area {bound}\n" : $"Pixel Area {areaPixels} > Min Area {bound}\n";
            strConditionResult += $"Required Color ({tBoltInspItem.ColorValue.R}, {tBoltInspItem.ColorValue.G}, {tBoltInspItem.ColorValue.B})\n";
            strConditionResult += $"Min Detected Color ({lowerBound.Val0}, {lowerBound.Val1}, {lowerBound.Val2})\n";
            strConditionResult += $"Max Detected Color ({upperBound.Val0}, {upperBound.Val1}, {upperBound.Val2})\n";
            strConditionResult += $"Boundary Condition (" + (boundaryCondPassed ? "Passed" : "Failed") + ")";


            // 241206 임시
            if (areaPixels > bound)
            {
                tBoltInspItem.InspectionResult = INSPECTION_RESULT.OK;
                tBoltInspItem.Conditions = strConditionResult; // @TODO ADD TRANSLATION
            }
            else
            {
                tBoltInspItem.InspectionResult = INSPECTION_RESULT.NG;
                tBoltInspItem.Conditions = strConditionResult;// @TODO ADD TRANSLATION
            }
            return tBoltInspItem;
        }

        public static TrialPadInspectionItem ExecuteTrialPadInpection(TrialPadInspectionItem tPadInspItem)
        {
            int setVariance = tPadInspItem.Variance; // TEMP USE FOR VARIANCE

            bool useSaltPapperMethod = true;
            if (useSaltPapperMethod)
            {
                if (tPadInspItem.ImageRegion.Empty())
                {
                    return tPadInspItem;
                }

                if (!tPadInspItem.ImageRegion.IsContinuous())
                {
                    tPadInspItem.ImageRegion = tPadInspItem.ImageRegion.Clone();
                }

                Mat grayImg = new Mat();
                Cv2.CvtColor(tPadInspItem.ImageRegion, grayImg, ColorConversionCodes.BGR2GRAY);

                Mat thresholded = new Mat();
                Cv2.InRange(grayImg, Scalar.FromRgb(tPadInspItem.MinThreshold, tPadInspItem.MinThreshold, tPadInspItem.MinThreshold),
                    Scalar.FromRgb(tPadInspItem.MaxThreshold, tPadInspItem.MaxThreshold, tPadInspItem.MaxThreshold), thresholded);


                // Threshold Adjustment for background color (beige or black) NEEDED? I THINK NO SO FAR I TESTED
                //Cv2.Threshold(grayImg, thresholded, tPadInspItem.MinThreshold, tPadInspItem.MaxThreshold, ThresholdTypes.Binary); 

                Mat foamRegion = new Mat();
                tPadInspItem.ImageRegion.CopyTo(foamRegion, thresholded);

                Mat foamGray = new Mat();
                Cv2.CvtColor(foamRegion, foamGray, ColorConversionCodes.BGR2GRAY);

                Mat edges = new Mat();
                Cv2.Canny(foamGray, edges, tPadInspItem.MinThreshold, tPadInspItem.MaxThreshold);

                Mat laplacian = new Mat();
                Cv2.Laplacian(foamGray, laplacian, MatType.CV_64F);
                Scalar mean, stddev;
                Cv2.MeanStdDev(laplacian, out mean, out stddev);
                double variance = stddev.Val0 * stddev.Val0;
                int nonZeroCount = Cv2.CountNonZero(laplacian);

                string textureType = variance > setVariance ? "Grainy" : "Plain";

                string strConditionResult = "";
                strConditionResult += variance < setVariance ? $"Variance {variance.ToString("00.00")} < Preset {setVariance}\n" : $"Variance {variance.ToString("00.00")} > Preset {setVariance}\n";
                strConditionResult += $"Texture Type({textureType})\n";
                //strConditionResult += $"Non Zero Pizel({nonZeroCount})\n";
                strConditionResult += $"Min Threshold({tPadInspItem.MinThreshold})\n";
                strConditionResult += $"Max Threshold({tPadInspItem.MaxThreshold})\n";

                tPadInspItem.Conditions = strConditionResult;
                tPadInspItem.ResultImageRegion = edges;

                tPadInspItem.InspectionResult = (variance > setVariance) ? INSPECTION_RESULT.OK : INSPECTION_RESULT.NG;
                tPadInspItem.ResultImageRegion = edges;


                return tPadInspItem;
            }
            else
            {
                if (tPadInspItem.ImageRegion.Empty())
                {
                    return tPadInspItem;
                }

                if (!tPadInspItem.ImageRegion.IsContinuous())
                {
                    tPadInspItem.ImageRegion = tPadInspItem.ImageRegion.Clone();
                }
                tPadInspItem.ResultImageRegion = TrialPadPreProcess(tPadInspItem.ImageRegion, tPadInspItem.MinThreshold, tPadInspItem.MaxThreshold, ThresholdTypes.Binary);
                Point[][] contours;
                HierarchyIndex[] hierarchy;

                Cv2.FindContours(tPadInspItem.ResultImageRegion, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
                Mat mat = tPadInspItem.ImageRegion.Clone(); //  new Mat(new Size(tPadInspItem.ResultImageRegion.Width, tPadInspItem.ResultImageRegion.Height), tPadInspItem.ImageRegion.Type());

                // 241203 LDH
                double totalArea = 0;

                if (contours.Length != 0)
                {
                    foreach (var contour in contours)
                    {
                        double area = Cv2.ContourArea(contour);
                        Console.WriteLine(area);
                        if (area >= tPadInspItem.Variance)
                        {
                            // 윤곽선을 그릴 색상과 두께를 설정합니다.
                            Cv2.DrawContours(mat, new Point[][] { contour }, -1, new Scalar(0, 255, 0), 1);
                            totalArea += area;
                        }
                    }

                    string strConditionResult = "";
                    strConditionResult += totalArea < tPadInspItem.MinTotalArea ? $"Pixel Area {totalArea} < Min Area {tPadInspItem.MinTotalArea}\n" : $"Pixel Area {totalArea} > Min Area {tPadInspItem.MinTotalArea}\n";
                    strConditionResult += $"Min Total Area({tPadInspItem.MinTotalArea})\n";
                    strConditionResult += $"Max Total Area({tPadInspItem.MaxTotalArea})\n";
                    strConditionResult += $"Min Threshold({tPadInspItem.MinThreshold})\n";
                    strConditionResult += $"Max Threshold({tPadInspItem.MaxThreshold})\n";


                    if (tPadInspItem.MinTotalArea <= totalArea && totalArea <= tPadInspItem.MaxTotalArea)
                    {
                        tPadInspItem.InspectionResult = INSPECTION_RESULT.OK;
                        tPadInspItem.Conditions = strConditionResult;
                        tPadInspItem.ResultImageRegion = mat;
                    }
                    else
                    {
                        tPadInspItem.InspectionResult = INSPECTION_RESULT.NG;
                        tPadInspItem.Conditions = strConditionResult;
                        tPadInspItem.ResultImageRegion = mat;
                    }

                    //tPadInspItem.InspectionResult = (tPadInspItem.MinTotalArea <= totalArea && totalArea <= tPadInspItem.MaxTotalArea) ? INSPECTION_RESULT.OK : INSPECTION_RESULT.NG;
                    //tPadInspItem.Conditions = tPadInspItem.InspectionResult == INSPECTION_RESULT.OK
                    //                            ? $"{tPadInspItem.MinTotalArea} <= Total Area ({totalArea}) <= {tPadInspItem.MaxTotalArea}"
                    //                            : $"{tPadInspItem.MinTotalArea} > Total Area ({totalArea}) > {tPadInspItem.MaxTotalArea}";
                }
                else
                {
                    tPadInspItem.InspectionResult = INSPECTION_RESULT.NOT_FOUND;
                }
                return tPadInspItem;
            }
        }

        public static TrialWhitePadInspectionItem ExecuteTrialWhitePadInpection(TrialWhitePadInspectionItem tPadInspItem)
        {
            if (tPadInspItem.ImageRegion.Empty())
            {
                return tPadInspItem;
            }

            if (!tPadInspItem.ImageRegion.IsContinuous())
            {
                tPadInspItem.ImageRegion = tPadInspItem.ImageRegion.Clone();
            }
            Mat foamGray = TrialWhitePadPreProcess(tPadInspItem.ImageRegion, tPadInspItem.MinThreshold, tPadInspItem.MaxThreshold);

            Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(5, 5));
            Cv2.MorphologyEx(foamGray, foamGray, MorphTypes.Close, kernel);
            Mat Mask = new Mat();
            Cv2.Threshold(foamGray, Mask, tPadInspItem.MaxThreshold, 255, ThresholdTypes.BinaryInv);
            // Mask를 foamGray에 적용
            Mat maskedImage = new Mat();
            Cv2.BitwiseAnd(foamGray, foamGray, maskedImage, Mask);

            // 새로운 Threshold 처리
            Mat threshHoled = new Mat();
            Cv2.Threshold(maskedImage, threshHoled, tPadInspItem.MinThreshold, 255, ThresholdTypes.Binary);

            int nonZeroCount = Cv2.CountNonZero(threshHoled);
            tPadInspItem.InspectionResult = (nonZeroCount > tPadInspItem.MaxTotalArea) ? INSPECTION_RESULT.OK : INSPECTION_RESULT.NG;


            string strConditionResult = "";
            strConditionResult += nonZeroCount > tPadInspItem.MinTotalArea ? $"Pixel Area {nonZeroCount} < Min Area {tPadInspItem.MinTotalArea}\n" : $"Pixel Area {nonZeroCount} > Min Area {tPadInspItem.MinTotalArea}\n";
            strConditionResult += $"Min Total Area({tPadInspItem.MinTotalArea})\n";
            strConditionResult += $"Max Total Area({tPadInspItem.MaxTotalArea})\n";
            strConditionResult += $"Min Threshold({tPadInspItem.MinThreshold})\n";
            strConditionResult += $"Max Threshold({tPadInspItem.MaxThreshold})\n";


            if (tPadInspItem.MinTotalArea <= nonZeroCount && nonZeroCount <= tPadInspItem.MaxTotalArea)
            {
                tPadInspItem.InspectionResult = INSPECTION_RESULT.OK;
                tPadInspItem.Conditions = strConditionResult;
                tPadInspItem.ResultImageRegion = threshHoled;
            }
            else
            {
                tPadInspItem.InspectionResult = INSPECTION_RESULT.NG;
                tPadInspItem.Conditions = strConditionResult;
                tPadInspItem.ResultImageRegion = threshHoled;
            }
            return tPadInspItem;
        }

        public static TrialPlugMatchInspectionItem ExecuteTrialPlugMatchInpection(TrialPlugMatchInspectionItem tPlugMatchInspItem)
        {
            const string Direction_Left = "left";
            const string Direction_Right = "right";
            const string Direction_Up = "up";
            const string Direction_Down = "down";

            if (tPlugMatchInspItem.ImageRegion.Empty())
            {
                return tPlugMatchInspItem;
            }

            if (!tPlugMatchInspItem.ImageRegion.IsContinuous())
            {
                tPlugMatchInspItem.ImageRegion = tPlugMatchInspItem.ImageRegion.Clone();
            }

            try
            {
                double Accuracy = tPlugMatchInspItem.Accuracy;
                double MaxLengthX = tPlugMatchInspItem.MaxLengthX;
                double MaxLengthY = tPlugMatchInspItem.MaxLengthY;
                List<double> results1 = new List<double>();
                List<double> results2 = new List<double>();
                string templatePath1 = tPlugMatchInspItem.TemplatePath01;
                string templatePath2 = tPlugMatchInspItem.TemplatePath02;
                Mat templateBgr1 = Cv2.ImRead(templatePath1);
                Mat templateBgr2 = Cv2.ImRead(templatePath2);
                string Direction = tPlugMatchInspItem.Direction;
                bool isOk = false;
                if (templateBgr1.Empty() || templateBgr2.Empty())
                {
                    Machine.logger.Write(eLogType.ERROR, $"Template {templatePath1}, {templatePath2} not found.");
                    tPlugMatchInspItem.Conditions = $"Template {templatePath1}, {templatePath2} not found.";
                    tPlugMatchInspItem.ResultImageRegion = tPlugMatchInspItem.ImageRegion.Clone();
                    tPlugMatchInspItem.InspectionResult = INSPECTION_RESULT.NG;
                    return tPlugMatchInspItem;
                }


                // 이미지 전처리
                Mat processedImageRegion = PreProcessImagePlug(tPlugMatchInspItem.ImageRegion);
                Mat processedTemplate1 = PreProcessImagePlug(templateBgr1);
                Mat processedTemplate2 = PreProcessImagePlug(templateBgr2);

                // 템플릿 매칭 
                Mat result1 = new Mat();
                Mat result2 = new Mat();
                DateTime now = DateTime.Now;
                Cv2.ImWrite(@"D:\plug\Plug_temp" + now.ToString("ss.fff") + ".bmp", tPlugMatchInspItem.ImageRegion);
                Cv2.ImWrite(@"D:\plug\Plug_process" + now.ToString("ss.fff") + ".bmp", processedImageRegion);

                //Cv2.MatchTemplate(tPlugMatchInspItem.ImageRegion, templateBgr1, result1, TemplateMatchModes.CCoeffNormed);
                //Cv2.MatchTemplate(tPlugMatchInspItem.ImageRegion, templateBgr2, result2, TemplateMatchModes.CCoeffNormed);

                Cv2.MatchTemplate(processedImageRegion, processedTemplate1, result1, TemplateMatchModes.CCoeffNormed);
                Cv2.MatchTemplate(processedImageRegion, processedTemplate2, result2, TemplateMatchModes.CCoeffNormed);

                // 가장 높은 매칭 점수를 가진 좌표 찾기 
                double minVal, maxVal;
                Point minLoc, maxLoc;
                Cv2.MinMaxLoc(result1, out minVal, out maxVal, out minLoc, out maxLoc);
                double maxValTempl1 = maxVal;
                // 사각형의 크기와 위치 설정 
                Point topLeft1 = maxLoc;

                Cv2.MinMaxLoc(result2, out minVal, out maxVal, out minLoc, out maxLoc);
                double maxValTempl2 = maxVal;
                // 사각형의 크기와 위치 설정                 
                Point topLeft2 = maxLoc;
                Rect matchRect = new Rect(topLeft2.X, topLeft2.Y, templateBgr2.Width, templateBgr2.Height);
                if (matchRect.Width + matchRect.X > tPlugMatchInspItem.ImageRegion.Width)
                    matchRect.Width -= matchRect.Width + matchRect.X - tPlugMatchInspItem.ImageRegion.Width;
                if (matchRect.Height + matchRect.Y > tPlugMatchInspItem.ImageRegion.Height)
                    matchRect.Height -= matchRect.Height + matchRect.Y - tPlugMatchInspItem.ImageRegion.Height;
                Mat matchedSubImage = new Mat(tPlugMatchInspItem.ImageRegion.Clone(), matchRect);

                // DRAW 2 RECT for 2 TEMPLATE 1) topLeft1 2) topLeft2
                Cv2.Rectangle(tPlugMatchInspItem.ImageRegion, new Rect(topLeft1.X, topLeft1.Y, templateBgr1.Width, templateBgr1.Height), Scalar.YellowGreen, 2);
                Cv2.Rectangle(tPlugMatchInspItem.ImageRegion, new Rect(topLeft2.X, topLeft2.Y, templateBgr2.Width, templateBgr2.Height), Scalar.YellowGreen, 2);

                int DistanceX = int.MinValue;
                int DistanceY = int.MinValue;
                int CrossDistance = int.MinValue;
                DistanceX = topLeft2.X - topLeft1.X;
                DistanceY = topLeft2.Y - topLeft1.Y;

                Cv2.ArrowedLine(tPlugMatchInspItem.ImageRegion, topLeft2, topLeft1, Scalar.Black, 2, LineTypes.Link8, 0, .3);

                double HistResult = CompareHistograms(matchedSubImage, templateBgr2);
                int CalcDistanceX = DistanceX - tPlugMatchInspItem.PlugDistanceX;
                int CalcDistanceY = DistanceY - tPlugMatchInspItem.PlugDistanceY;
                bool UseHistogram = false;
                if (UseHistogram && HistResult * 100 < 20)
                {
                    maxValTempl2 = 0;
                    if (maxValTempl2 < 0)
                        maxValTempl2 = 0;
                    isOk = false;
                }
                else
                {

                    if (CalcDistanceX > MaxLengthX || CalcDistanceY > MaxLengthY || ((int)(maxValTempl1 * 100) < Accuracy || (int)(maxValTempl2 * 100) < Accuracy)) isOk = false;
                    else if (CalcDistanceX < -MaxLengthX || CalcDistanceY < -MaxLengthY || ((int)(maxValTempl1 * 100) < Accuracy || (int)(maxValTempl2 * 100) < Accuracy)) isOk = false;
                    else isOk = true;
                }

                tPlugMatchInspItem.ResultImageRegion = tPlugMatchInspItem.ImageRegion.Clone();
                //Cv2.Circle(tPlugMatchInspItem.ResultImageRegion, topLeft2.X, topLeft2.Y, 10, Scalar.SeaGreen, -1);
                //Cv2.Circle(tPlugMatchInspItem.ResultImageRegion, topLeft1.X, topLeft1.Y, 10, Scalar.Crimson, -1);
                tPlugMatchInspItem.InspectionResult = isOk ? INSPECTION_RESULT.OK : INSPECTION_RESULT.NG;
                string conditionString = CalcDistanceX > MaxLengthX ? $"DistanceX ({CalcDistanceX}) > Max Distance {MaxLengthX}\n" : $"DistanceX ({CalcDistanceX}) < Max Distance {MaxLengthX}\n";
                conditionString += CalcDistanceY > MaxLengthY ? $"DistanceY ({CalcDistanceY}) > Max Distance {MaxLengthY}\n" : $"DistanceY ({CalcDistanceY}) < Max Distance {MaxLengthY}\n";
                conditionString += (int)(Math.Min(maxValTempl1, maxValTempl2) * 100) > Accuracy
                    ? $"Template Match {(Math.Min(maxValTempl1, maxValTempl2) * 100).ToString("00.00")}% > Accuracy {Accuracy}%"
                    : $"Template Match {(Math.Min(maxValTempl1, maxValTempl2) * 100).ToString("00.00")}% < Accuracy {Accuracy}%";

                tPlugMatchInspItem.Conditions = conditionString;
            }
            catch (Exception ex)
            {
                Console.WriteLine("오류 발생: " + ex.ToString());
                Machine.logger.Write(eLogType.ERROR, ex.ToString());
                tPlugMatchInspItem.ResultImageRegion = tPlugMatchInspItem.ImageRegion.Clone();
                tPlugMatchInspItem.InspectionResult = INSPECTION_RESULT.NG;
                tPlugMatchInspItem.Conditions = "Template not found"; //@TODO: TRANSLATION
            }
            return tPlugMatchInspItem;
        }

        private static Mat PreProcessImagePlug(Mat image)
        {
            try
            {
                Mat grayImage = new Mat();
                Mat blurredImage = new Mat();
                // 1. 그레이스케일 변환
                if (image.Channels() != 1)
                    Cv2.CvtColor(image, grayImage, ColorConversionCodes.BGR2GRAY);

                // 3. 가우시안 블러
                Cv2.GaussianBlur(grayImage, blurredImage, new Size(5, 5), 0);

                return blurredImage;
            }
            catch (Exception ex)
            {
                Console.WriteLine("전처리 중 오류 발생: " + ex.ToString());
                Machine.logger.Write(eLogType.ERROR, ex.ToString());
                return image; // 오류 발생 시 원본 이미지를 반환
            }
        }
#if USE_COGNEX
        public static TrialCognexPlugMatchInspectionItem ExecuteTrialCognexPlugMatchInpection(TrialCognexPlugMatchInspectionItem tCognexPlugMatchInspItem)
        {
            if (tCognexPlugMatchInspItem.ImageRegion.Empty())
            {
                return tCognexPlugMatchInspItem;
            }

            if (!tCognexPlugMatchInspItem.ImageRegion.IsContinuous())
            {
                tCognexPlugMatchInspItem.ImageRegion = tCognexPlugMatchInspItem.ImageRegion.Clone();
            }

            try
            {

                double InnerConfidence = (double)tCognexPlugMatchInspItem.InnerConfidence;
                double OuterConfidence = (double)tCognexPlugMatchInspItem.OuterConfidence;
                double MaxLengthX = tCognexPlugMatchInspItem.MaxLengthX;
                double MaxLengthY = tCognexPlugMatchInspItem.MaxLengthY;
                string Direction = tCognexPlugMatchInspItem.Direction;
                bool isOk = false;

                Mat imgMat = tCognexPlugMatchInspItem.ImageRegion;

                Tuple<Rect, double> tmpl1Result = Machine.cognexVisionDetection.FindTemplate(tCognexPlugMatchInspItem.VppPath01, imgMat, new OpenCvSharp.Rect());
                Rect rectTmpl1 = tmpl1Result.Item1;
                double scoreTmpl1 = tmpl1Result.Item2;
                Point topLeft1 = rectTmpl1.TopLeft;

                Tuple<Rect, double> tmpl2Result = Machine.cognexVisionDetection.FindTemplate(tCognexPlugMatchInspItem.VppPath02, imgMat, new OpenCvSharp.Rect());
                Rect rectTmpl2 = tmpl2Result.Item1;
                double scoreTmpl2 = tmpl2Result.Item2;
                Point topLeft2 = rectTmpl2.TopLeft;

                int DistanceX = int.MinValue;
                int DistanceY = int.MinValue;
                int CrossDistance = int.MinValue;

                DistanceX = topLeft2.X - topLeft1.X;
                DistanceY = topLeft2.Y - topLeft1.Y;
                CrossDistance = (int)Math.Abs((topLeft1.Y + rectTmpl1.Height / 2) - (topLeft2.Y + rectTmpl2.Height / 2));

                Rect matchRect = new Rect(topLeft2.X, topLeft2.Y, rectTmpl2.Width, rectTmpl2.Height);
                if (matchRect.Width + matchRect.X > tCognexPlugMatchInspItem.ImageRegion.Width)
                    matchRect.Width -= matchRect.Width + matchRect.X - tCognexPlugMatchInspItem.ImageRegion.Width;
                if (matchRect.Height + matchRect.Y > tCognexPlugMatchInspItem.ImageRegion.Height)
                    matchRect.Height -= matchRect.Height + matchRect.Y - tCognexPlugMatchInspItem.ImageRegion.Height;
                Mat matchedSubImage = new Mat(tCognexPlugMatchInspItem.ImageRegion, matchRect);

                int CalcDistanceX = DistanceX - tCognexPlugMatchInspItem.PlugCogDistanceX;
                int CalcDistanceY = DistanceY - tCognexPlugMatchInspItem.PlugCogDistanceY;

                if (CalcDistanceX > MaxLengthX || CalcDistanceY > MaxLengthY || ((int)(scoreTmpl1 * 100) < OuterConfidence || (int)(scoreTmpl2 * 100) < InnerConfidence)) isOk = false;
                else if (CalcDistanceX < -MaxLengthX || CalcDistanceY < -MaxLengthY || ((int)(scoreTmpl1 * 100) < OuterConfidence || (int)(scoreTmpl2 * 100) < InnerConfidence)) isOk = false;
                else isOk = true;

                tCognexPlugMatchInspItem.InspectionResult = isOk ? INSPECTION_RESULT.OK : INSPECTION_RESULT.NG;


                // DRAW 2 RECT for 2 TEMPLATE 1) topLeft1 2) topLeft2
                tCognexPlugMatchInspItem.ResultImageRegion = tCognexPlugMatchInspItem.ImageRegion.Clone();
                if (scoreTmpl1 * 100 >= OuterConfidence)
                    Cv2.Rectangle(tCognexPlugMatchInspItem.ResultImageRegion, rectTmpl1, Scalar.YellowGreen, 2);
                if (scoreTmpl2 * 100 >= InnerConfidence)
                    Cv2.Rectangle(tCognexPlugMatchInspItem.ResultImageRegion, rectTmpl2, Scalar.YellowGreen, 2);
                if (scoreTmpl1 * 100 >= Accuracy && scoreTmpl2 * 100 >= Accuracy)
                    Cv2.ArrowedLine(tCognexPlugMatchInspItem.ResultImageRegion, topLeft2, topLeft1, Scalar.Black, 2, LineTypes.Link8, 0, .3);

                //tCognexPlugMatchInspItem.ResultImageRegion = tCognexPlugMatchInspItem.ImageRegion.Clone();
                //Cv2.Circle(tPlugMatchInspItem.ResultImageRegion, topLeft2.X, topLeft2.Y, 10, Scalar.SeaGreen, -1);
                //Cv2.Circle(tPlugMatchInspItem.ResultImageRegion, topLeft1.X, topLeft1.Y, 10, Scalar.Crimson, -1);
                tCognexPlugMatchInspItem.InspectionResult = isOk ? INSPECTION_RESULT.OK : INSPECTION_RESULT.NG;
                string conditionString = CalcDistanceX > MaxLengthX ? $"DistanceX ({CalcDistanceX}) > Max Distance {MaxLengthX}\n" : $"DistanceX ({CalcDistanceX}) < Max Distance {MaxLengthX}\n";
                conditionString += CalcDistanceY > MaxLengthY ? $"DistanceY ({CalcDistanceY}) > Max Distance {MaxLengthY}\n" : $"DistanceY ({CalcDistanceY}) < Max Distance {MaxLengthY}\n";
                conditionString += $"Template Match 1 {(scoreTmpl1 * 100).ToString("00.00")}% > OuterConfidence {OuterConfidence}%  \nTopLeft ({topLeft1.X},{topLeft1.Y})\n";
                conditionString += $"Template Match 2 {(scoreTmpl2 * 100).ToString("00.00")}% < InnerConfidence {InnerConfidence}%  \nTopLeft ({topLeft2.X},{topLeft2.Y})";

                tCognexPlugMatchInspItem.Conditions = conditionString;
            }
            catch (Exception ex)
            {
                Console.WriteLine("오류 발생: " + ex.ToString());
                Machine.logger.Write(eLogType.ERROR, ex.ToString());
                tCognexPlugMatchInspItem.ResultImageRegion = tCognexPlugMatchInspItem.ImageRegion.Clone();
                tCognexPlugMatchInspItem.InspectionResult = INSPECTION_RESULT.NG;
                tCognexPlugMatchInspItem.Conditions = "Template not found"; //@TODO: TRANSLATION
            }
            return tCognexPlugMatchInspItem;
        }
#endif
        public static TrialScrewInspectionItem ExecuteTrialScrewInspection(TrialScrewInspectionItem tScrewInspItem)
        {
            if (tScrewInspItem.ImageRegion.Empty())
            {
                return tScrewInspItem;
            }
            if (tScrewInspItem.ImageRegion.Empty())
            {
                return tScrewInspItem;
            }

            if (!tScrewInspItem.ImageRegion.IsContinuous())
            {
                tScrewInspItem.ImageRegion = tScrewInspItem.ImageRegion.Clone();
            }

            try
            {
                tScrewInspItem.ResultImageRegion = TrialScrewPreProcess(tScrewInspItem.ImageRegion, tScrewInspItem.MinThreshold, tScrewInspItem.MaxThreshold, ThresholdTypes.Binary);
                Point[][] contours;
                HierarchyIndex[] hierarchy;

                // 윤곽선 찾기
                Cv2.FindContours(tScrewInspItem.ResultImageRegion, out contours, out hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

                Point[] largestContour = null;
                double largestArea = 0;

                foreach (var contour in contours)
                {
                    double area = Cv2.ContourArea(contour);

                    if (area > largestArea && area > tScrewInspItem.MinContourArea)
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
                    Mat image = tScrewInspItem.ImageRegion.Clone();
                    Mat grayImage = new Mat();
                    Cv2.CvtColor(image, grayImage, ColorConversionCodes.BGR2GRAY); // 그레이스케일 변환
                    Mat binaryImage = new Mat();
                    Cv2.Threshold(grayImage, binaryImage, tScrewInspItem.MinThreshold, 255, ThresholdTypes.Binary); // 이진화

                    // 흰색 픽셀 수 계산
                    int whitePixelCount = Cv2.CountNonZero(binaryImage); // 흰색 픽셀 

                    string StrConditinoResult = "";
                    StrConditinoResult += whitePixelCount >= tScrewInspItem.MinContourArea
                        ? $"Pixel Area ({whitePixelCount}) >= Min Contore {tScrewInspItem.MinContourArea}\n"
                        : $"Pixel Area ({whitePixelCount}) < Min Contore {tScrewInspItem.MinContourArea}\n";
                    StrConditinoResult += $"Min Threshold ({tScrewInspItem.MinThreshold})\n";
                    StrConditinoResult += $"Min Contore ({tScrewInspItem.MinContourArea})\n";

                    if (whitePixelCount >= tScrewInspItem.MinContourArea)
                    {
                        tScrewInspItem.InspectionResult = INSPECTION_RESULT.OK; // 흰색 픽셀이 기준 이상일 경우 OK
                        tScrewInspItem.Conditions = StrConditinoResult;
                    }
                    else
                    {
                        tScrewInspItem.InspectionResult = INSPECTION_RESULT.NG; // 그렇지 않으면 NG
                        tScrewInspItem.Conditions = StrConditinoResult;
                    }
                }
                else
                {
                    tScrewInspItem.InspectionResult = INSPECTION_RESULT.NOT_FOUND; // contour가 없을 경우
                    tScrewInspItem.Conditions = "Pixel Area not enough";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("오류 발생: " + ex.ToString());
                Machine.logger.Write(eLogType.ERROR, ex.ToString());
                tScrewInspItem.InspectionResult = INSPECTION_RESULT.NOT_FOUND;
                tScrewInspItem.Conditions = "Not Found";
            }


            return tScrewInspItem;
        }


        public static TrialColorInspectionItem ExecuteColorInspectionItem(TrialColorInspectionItem tColorInspectionItem)
        {
            if (tColorInspectionItem.ImageRegion.Empty())
            {
                //throw new ArgumentException("The provided image region is empty.", nameof(image_region));
                tColorInspectionItem.Color = System.Windows.Media.Color.FromArgb(255, 0, 0, 0); // RETURN BLACK IN CASE OF EMPTY IMAGE
                return tColorInspectionItem;
            }

            if (!tColorInspectionItem.ImageRegion.IsContinuous())
            {
                tColorInspectionItem.ImageRegion = tColorInspectionItem.ImageRegion.Clone();
            }

            // Reshape the region to a 1D array of pixels (rows * cols x channels)
            var pixels = tColorInspectionItem.ImageRegion.Reshape(1, tColorInspectionItem.ImageRegion.Rows * tColorInspectionItem.ImageRegion.Cols);

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

            tColorInspectionItem.Color = System.Windows.Media.Color.FromArgb(a, r, g, b);
            return tColorInspectionItem;
        }

        public static TrialColorMatchInspectionItem ExecuteCompareColor(TrialColorMatchInspectionItem colInspItem)
        {
            if (colInspItem.ImageRegion.Empty())
            {
                //throw new ArgumentException("The provided image region is empty.", nameof(image_region));
                colInspItem.InspectedColor = System.Windows.Media.Color.FromArgb(255, 0, 0, 0); // RETURN BLACK IN CASE OF EMPTY IMAGE
                return colInspItem;
            }

            if (!colInspItem.ImageRegion.IsContinuous())
            {
                colInspItem.ImageRegion = colInspItem.ImageRegion.Clone();
            }

            // Reshape the region to a 1D array of pixels (rows * cols x channels)
            var pixels = colInspItem.ImageRegion.Reshape(1, colInspItem.ImageRegion.Rows * colInspItem.ImageRegion.Cols);
            int Bound = colInspItem.Bound;

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

            // CHECK COLOR RANGE
            if ((colInspItem.RequiredColor.R + Bound > r && colInspItem.RequiredColor.R - Bound < r)
                && (colInspItem.RequiredColor.G + Bound > g && colInspItem.RequiredColor.G - Bound < g)
                && (colInspItem.RequiredColor.B + Bound > b && colInspItem.RequiredColor.B - Bound < b))
            {
                colInspItem.InspectionResult = INSPECTION_RESULT.OK;
            }
            else
            {
                colInspItem.InspectionResult = INSPECTION_RESULT.NG;
            }

            string strConditionResult = "";
            strConditionResult += $"Inspection R : {colInspItem.InspectedColor.R}, G : {colInspItem.InspectedColor.G}, B : {colInspItem.InspectedColor.B}\n";
            strConditionResult += $"Spec R : {colInspItem.RequiredColor.R}, G : {colInspItem.RequiredColor.G}, B : {colInspItem.RequiredColor.B}\n";

            colInspItem.Conditions = strConditionResult;
            colInspItem.ResultImageRegion = colInspItem.ImageRegion;
            return colInspItem;
        }

        public static TrialSmallPadInspectionItem ExecuteSmallPadInspectionItem(TrialSmallPadInspectionItem tSmallPadInspectionItem)
        {
            int preset_variance = tSmallPadInspectionItem.Variance;
            int useSaltPapperMethod = 1;
            if (useSaltPapperMethod == 0)
            {
                if (tSmallPadInspectionItem.ImageRegion.Empty())
                {
                    return tSmallPadInspectionItem;
                }

                if (!tSmallPadInspectionItem.ImageRegion.IsContinuous())
                {
                    tSmallPadInspectionItem.ImageRegion = tSmallPadInspectionItem.ImageRegion.Clone();
                }

                Mat grayImg = new Mat();
                Cv2.CvtColor(tSmallPadInspectionItem.ImageRegion, grayImg, ColorConversionCodes.BGR2GRAY);

                //Cv2.Normalize(grayImg, grayImg, 0, 255, NormTypes.MinMax);


                Mat thresholded = new Mat();
                // Threshold Adjustment for background color (beige or black) NEEDED? I THINK NO SO FAR I TESTED
                Cv2.Threshold(grayImg, thresholded, tSmallPadInspectionItem.MinThreshold, tSmallPadInspectionItem.MaxThreshold, ThresholdTypes.Binary);

                Mat foamRegion = new Mat();
                tSmallPadInspectionItem.ImageRegion.CopyTo(foamRegion, thresholded);

                Mat foamGray = new Mat();
                Cv2.CvtColor(foamRegion, foamGray, ColorConversionCodes.BGR2GRAY);

                Mat edges = new Mat();
                Cv2.Canny(foamGray, edges, tSmallPadInspectionItem.MinThreshold, tSmallPadInspectionItem.MaxThreshold);

                Mat laplacian = new Mat();
                Cv2.Laplacian(foamGray, laplacian, MatType.CV_64F);
                Scalar mean, stddev;
                Cv2.MeanStdDev(laplacian, out mean, out stddev);
                double variance = stddev.Val0 * stddev.Val0;
                int nonZeroCount = Cv2.CountNonZero(edges);

                string textureType = variance > preset_variance ? "Grainy" : "Plain";

                string strConditionResult = "";
                strConditionResult += variance < preset_variance ? $"Variance {variance.ToString("00.00")} < Preset {preset_variance}\n" : $"Variance {variance.ToString("00.00")} > Preset {preset_variance}\n";
                strConditionResult += $"Texture Type({textureType})\n";
                strConditionResult += $"White Pixel Count({nonZeroCount})\n";
                strConditionResult += $"Min Threshold({tSmallPadInspectionItem.MinThreshold})\n";
                strConditionResult += $"Max Threshold({tSmallPadInspectionItem.MaxThreshold})\n";

                tSmallPadInspectionItem.Conditions = strConditionResult;
                tSmallPadInspectionItem.ResultImageRegion = edges;

                tSmallPadInspectionItem.InspectionResult = (variance > preset_variance && nonZeroCount > tSmallPadInspectionItem.WhitePixelCount) ? INSPECTION_RESULT.OK : INSPECTION_RESULT.NG;
                tSmallPadInspectionItem.ResultImageRegion = edges;


                return tSmallPadInspectionItem;
            }
            else if (useSaltPapperMethod == 1)
            {

                if (tSmallPadInspectionItem.ImageRegion.Empty())
                {
                    return tSmallPadInspectionItem;
                }

                if (!tSmallPadInspectionItem.ImageRegion.IsContinuous())
                {
                    tSmallPadInspectionItem.ImageRegion = tSmallPadInspectionItem.ImageRegion.Clone();
                }

                Mat grayImg = new Mat();
                Cv2.CvtColor(tSmallPadInspectionItem.ImageRegion, grayImg, ColorConversionCodes.BGR2RGB);

                //Cv2.Normalize(grayImg, grayImg, 0, 255, NormTypes.MinMax);
                Cv2.GaussianBlur(grayImg, grayImg, new Size(9, 9), 0);
                //Cv2.MedianBlur(grayImg, grayImg, 7);
                Cv2.Laplacian(grayImg, grayImg, -1, 5, 1, 0, BorderTypes.Default);

                Cv2.CvtColor(grayImg, grayImg, ColorConversionCodes.RGB2GRAY);

                Cv2.GaussianBlur(grayImg, grayImg, new Size(3, 3), 0);
                Cv2.Threshold(grayImg, grayImg, 1, 255, ThresholdTypes.Otsu);
                CircleSegment[] circles = Cv2.HoughCircles(grayImg, HoughModes.Gradient, 1, 1, 200, 10, 10, 50);

                int nonZeroCount = Cv2.CountNonZero(grayImg);

                string strConditionResult = "";
                strConditionResult += $"White Pixel Count({nonZeroCount})\n";
                strConditionResult += circles.Count() > BLOBCount ? $"BLOB Count({circles.Count()} > PRESET {BLOBCount})\n" : $"BLOB Count({circles.Count()} < PRESET {BLOBCount})\n";
                strConditionResult += $"Min Threshold({tSmallPadInspectionItem.MinThreshold})\n";
                strConditionResult += $"Max Threshold({tSmallPadInspectionItem.MaxThreshold})\n";

                tSmallPadInspectionItem.Conditions = strConditionResult;
                tSmallPadInspectionItem.ResultImageRegion = grayImg;

                tSmallPadInspectionItem.InspectionResult = (nonZeroCount > tSmallPadInspectionItem.WhitePixelCount && circles.Count() > BLOBCount) ? INSPECTION_RESULT.OK : INSPECTION_RESULT.NG;

                return tSmallPadInspectionItem;
            }
            else
            {

                tSmallPadInspectionItem.ResultImageRegion = TrialSmallPadPreProcess(tSmallPadInspectionItem.ImageRegion, tSmallPadInspectionItem.MinThreshold, tSmallPadInspectionItem.MaxThreshold, ThresholdTypes.Binary);
                Mat Result = new Mat();
                Cv2.Canny(tSmallPadInspectionItem.ResultImageRegion, Result, tSmallPadInspectionItem.MinThreshold, tSmallPadInspectionItem.MaxThreshold);
                int areaPixels = Cv2.CountNonZero(Result);


                string StrConditionsResult = "";
                StrConditionsResult += areaPixels < tSmallPadInspectionItem.Variance
                    ? $"Pixel Area ({areaPixels}) < MaxLength ({tSmallPadInspectionItem.Variance})\n"
                    : $"Pixel Area ({areaPixels}) > MaxLength ({tSmallPadInspectionItem.Variance})\n";
                StrConditionsResult += $"Min Threshold ({tSmallPadInspectionItem.MinThreshold})\n";
                StrConditionsResult += $"Max Threshold ({tSmallPadInspectionItem.MaxThreshold})\n";

                if (areaPixels < tSmallPadInspectionItem.Variance)
                {
                    tSmallPadInspectionItem.InspectionResult = INSPECTION_RESULT.OK;
                    tSmallPadInspectionItem.Conditions = StrConditionsResult;
                }
                else
                {
                    tSmallPadInspectionItem.InspectionResult = INSPECTION_RESULT.NG;
                    tSmallPadInspectionItem.Conditions = StrConditionsResult;
                }
                tSmallPadInspectionItem.ResultImageRegion = Result.Clone();
                Console.WriteLine("White Pixels = " + areaPixels);
                return tSmallPadInspectionItem;
            }
        }


        public static TrialPlugInspectionItem ExecutePlugInspectionItem(TrialPlugInspectionItem tPlugInspectionItem)
        {
            try
            {
                tPlugInspectionItem.ResultImageRegion = TrialPlugPreProcess(tPlugInspectionItem.ImageRegion, tPlugInspectionItem.MaskMinThreshold, tPlugInspectionItem.MaskMaxThreshold,
                    tPlugInspectionItem.MaskThresholdType, tPlugInspectionItem.MorphMinThreshold, tPlugInspectionItem.MorphMaxThreshold, tPlugInspectionItem.MorphThresholdType);
                // 윤곽선 찾기
                Cv2.FindContours(tPlugInspectionItem.ResultImageRegion, out Point[][] contours, out HierarchyIndex[] hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);
                //Cv2.ImWrite(@"D:\plug\plug_" + plugInspItem.RegionID + ".bmp", plugInspItem.ImageRegion);
                //Cv2.ImWrite(@"D:\plug\plug_" + plugInspItem.RegionID + "_MO.bmp", plugInspItem.MorphedImageRegion);
                Point[] largestContour = null;
                double largestArea = 0;

                foreach (var contour in contours)
                {
                    double area = Cv2.ContourArea(contour);

                    if (area > largestArea && area > tPlugInspectionItem.MinContourArea)
                    {
                        largestArea = area;
                        largestContour = contour;
                    }
                }

                if (largestContour != null)
                {
                    // contour의 경계 사각형을 구함
                    Rect boundingRect = Cv2.BoundingRect(largestContour);

                    // 극단 포인트 계산
                    var leftmostPoint = largestContour[Array.IndexOf(largestContour, largestContour.OrderBy(p => p.X).First())];
                    var rightmostPoint = largestContour[Array.IndexOf(largestContour, largestContour.OrderByDescending(p => p.X).First())];
                    var topmostPoint = largestContour[Array.IndexOf(largestContour, largestContour.OrderBy(p => p.Y).First())];
                    var bottommostPoint = largestContour[Array.IndexOf(largestContour, largestContour.OrderByDescending(p => p.Y).First())];

                    var extremePoints = new[] { leftmostPoint, rightmostPoint, topmostPoint, bottommostPoint };

                    Point leftBottomPoint, rightBottomPoint;
                    if (bottommostPoint.Y - topmostPoint.Y <= rightmostPoint.X - leftmostPoint.X)
                    {
                        leftBottomPoint = bottommostPoint;
                        rightBottomPoint = rightmostPoint;
                    }
                    else
                    {
                        leftBottomPoint = leftmostPoint;
                        rightBottomPoint = bottommostPoint;
                    }

                    Cv2.DrawContours(tPlugInspectionItem.ImageRegion, new[] { largestContour }, -1, new Scalar(0, 0, 255), 2);
                    Rect bBox = Cv2.BoundingRect(largestContour);

                    //Cv2.ImWrite(@"D:\plug\plug_" + plugInspItem.RegionID + "_CON.bmp", plugInspItem.ImageRegion);
                    //Cv2.Rectangle(plugInspItem.ImageRegion, bBox, new Scalar(255, 0, 255), 2, LineTypes.Link8);
                    //Cv2.ImShow("test", plugInspItem.ImageRegion);
                    //Cv2.WaitKey(0);



                    tPlugInspectionItem.DetectedContour = largestContour;
                    tPlugInspectionItem.DetectedRect =
                        new Rect(leftmostPoint.X, leftmostPoint.Y, rightmostPoint.X - leftmostPoint.X, leftBottomPoint.Y - leftmostPoint.Y);

                    // 회전 각도 계산
                    double angle = Math.Atan2(rightBottomPoint.Y - leftBottomPoint.Y, rightBottomPoint.X - leftBottomPoint.X) * 180 / Math.PI;

                    // 회전 중심 계산
                    Point rotationCenter = new Point((leftBottomPoint.X + rightBottomPoint.X) / 2, (leftBottomPoint.Y + rightBottomPoint.Y) / 2);

                    // 회전 행렬 생성
                    Mat rotationMatrix = Cv2.GetRotationMatrix2D(rotationCenter, angle, 1.0);

                    // 회전된 윤곽선의 꼭지점 다시 계산
                    Point[] rotatedContourPoints = new Point[largestContour.Length];

                    // Point 배열을 Mat으로 변환
                    Mat contourMat = new Mat(largestContour.Length, 1, MatType.CV_32FC2);
                    for (int i = 0; i < largestContour.Length; i++)
                    {
                        contourMat.Set<Vec2f>(i, new Vec2f(largestContour[i].X, largestContour[i].Y)); // Point를 Mat에 추가
                    }

                    // Transform 수행
                    Cv2.Transform(contourMat, contourMat, rotationMatrix); // 변환 후 다시 contourMat에 저장

                    for (int i = 0; i < largestContour.Length; i++)
                    {
                        // 변환된 Point로 업데이트
                        var transformedPoint = contourMat.At<Vec2f>(i);
                        rotatedContourPoints[i] = new Point((int)transformedPoint[0], (int)transformedPoint[1]); // int 형으로 변환
                    }

                    // 회전된 극단 포인트 찾기
                    var leftmostRotated = rotatedContourPoints.OrderBy(p => p.X).FirstOrDefault();
                    var rightmostRotated = rotatedContourPoints.OrderByDescending(p => p.X).FirstOrDefault();
                    var topmostRotated = rotatedContourPoints.OrderBy(p => p.Y).FirstOrDefault();
                    var bottommostRotated = rotatedContourPoints.OrderByDescending(p => p.Y).FirstOrDefault();

                    string StrConditionsResult = "";
                    StrConditionsResult +=
                        ((rightmostRotated.X - leftmostRotated.X) < tPlugInspectionItem.MinError || (rightmostRotated.X - leftmostRotated.X) >= tPlugInspectionItem.MaxError)
                        ? $"Pixel Area ({rightmostRotated.X - leftmostRotated.X}) < Min Error {tPlugInspectionItem.MinError}\n"
                        : $"Pixel Area ({rightmostRotated.X - leftmostRotated.X}) > Min Error {tPlugInspectionItem.MinError}\n";
                    StrConditionsResult += $"Min Contore Area ({tPlugInspectionItem.MinContourArea})\n";
                    StrConditionsResult += $"Min Threshold ({tPlugInspectionItem.MaskMinThreshold})\n";
                    StrConditionsResult += $"Min Error ({tPlugInspectionItem.MinError})\n";
                    StrConditionsResult += $"Max Error ({tPlugInspectionItem.MaxError})\n";

                    if ((rightmostRotated.X - leftmostRotated.X) < tPlugInspectionItem.MinError || (rightmostRotated.X - leftmostRotated.X) >= tPlugInspectionItem.MaxError)
                    {
                        tPlugInspectionItem.InspectionResult = INSPECTION_RESULT.NG;
                        tPlugInspectionItem.Conditions = StrConditionsResult;
                    }
                    else
                    {
                        tPlugInspectionItem.InspectionResult = INSPECTION_RESULT.OK;
                        tPlugInspectionItem.Conditions = StrConditionsResult;
                    }
                }
                else
                {
                    tPlugInspectionItem.InspectionResult = INSPECTION_RESULT.NOT_FOUND;
                    tPlugInspectionItem.Conditions = "Pixel Area 0";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("오류 발생: " + ex.ToString());
                Machine.logger.Write(eLogType.ERROR, ex.ToString());
                tPlugInspectionItem.InspectionResult = INSPECTION_RESULT.NOT_FOUND;

            }

            return tPlugInspectionItem;
        }

        public static TrialScrewMacthInspectionItem ExecuteScrewMetchInspectionItem(TrialScrewMacthInspectionItem tScrewMacthInspectionItem)
        {
            try
            {
                double Accuracy = tScrewMacthInspectionItem.Accuracy;
                List<double> results = new List<double>();
                string templatePath = tScrewMacthInspectionItem.TemplatePath;
                Mat templateBgr = Cv2.ImRead(templatePath);
                if (templateBgr.Empty())
                {
                    Machine.logger.Write(eLogType.ERROR, $"Template {templatePath} not found.");
                    return null;
                }

                Mat ImageRegion = tScrewMacthInspectionItem.ImageRegion.Clone();
                Mat templateBgrClone = templateBgr.Clone();


                double highestMaxVal = 0; // 가장 높은 maxVal 저장
                Point bestMatchLoc = default; // 가장 좋은 매칭 좌표 저장
                int count = 0;
                //// 템플릿 회전하며 매칭 수행
                {

                    // 템플릿 매칭
                    Mat result = new Mat();
                    Cv2.MatchTemplate(ImageRegion, templateBgrClone, result, TemplateMatchModes.CCoeffNormed);

                    // 매칭 점수 확인
                    Cv2.MinMaxLoc(result, out double minVal, out double maxVal, out Point minLoc, out Point maxLoc);
                    // 가장 높은 maxVal을 저장
                    if (maxVal > highestMaxVal)
                    {
                        highestMaxVal = maxVal;
                        bestMatchLoc = maxLoc; // 가장 높은 점수의 위치도 저장
                    }

                    count++;
                }
                Rect matchRect = new Rect(bestMatchLoc.X, bestMatchLoc.Y, templateBgr.Width, templateBgr.Height);
                if (matchRect.Width + matchRect.X > tScrewMacthInspectionItem.ImageRegion.Width)
                    matchRect.Width -= matchRect.Width + matchRect.X - tScrewMacthInspectionItem.ImageRegion.Width;
                if (matchRect.Height + matchRect.Y > tScrewMacthInspectionItem.ImageRegion.Height)
                    matchRect.Height -= matchRect.Height + matchRect.Y - tScrewMacthInspectionItem.ImageRegion.Height;
                Mat matchedSubImage = new Mat(tScrewMacthInspectionItem.ImageRegion, matchRect);

                double HistResult = CompareHistograms(matchedSubImage, templateBgrClone);
                double SSIMResult2 = ComputeSSIM2(matchedSubImage, templateBgrClone);
                highestMaxVal = ((highestMaxVal) + SSIMResult2 + HistResult) / 3;

                if (highestMaxVal * 100 > Accuracy)
                {
                    tScrewMacthInspectionItem.InspectionResult = INSPECTION_RESULT.OK;
                    tScrewMacthInspectionItem.Conditions = $"{(highestMaxVal * 100).ToString("00.00")}% Match > Target {Accuracy}% ";
                }
                else
                {
                    tScrewMacthInspectionItem.InspectionResult = INSPECTION_RESULT.NG;
                    tScrewMacthInspectionItem.Conditions = $"{(highestMaxVal * 100).ToString("00.00")}% Match < Target {Accuracy}% ";
                }
                tScrewMacthInspectionItem.ResultImageRegion = ImageRegion.Clone();
                Cv2.Rectangle(tScrewMacthInspectionItem.ResultImageRegion, new Rect(bestMatchLoc.X, bestMatchLoc.Y, templateBgr.Width, templateBgr.Height), Scalar.YellowGreen, 3);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Machine.logger.Write(eLogType.ERROR, ex.ToString());
                tScrewMacthInspectionItem.InspectionResult = INSPECTION_RESULT.NG;
                tScrewMacthInspectionItem.Conditions = $"00.00% Match < Target {Accuracy}% ";
            }

            return tScrewMacthInspectionItem;
        }


        static double CompareHistograms(Mat img11, Mat img22)
        {
            Mat img1 = img11.Clone(), img2 = img22.Clone();
            if (img1.Channels() != 1)
                Cv2.CvtColor(img1, img1, ColorConversionCodes.BGR2GRAY);
            if (img2.Channels() != 1)
                Cv2.CvtColor(img2, img2, ColorConversionCodes.BGR2GRAY);
            Cv2.GaussianBlur(img1, img1, new Size(5, 5), 1);
            Cv2.GaussianBlur(img2, img2, new Size(5, 5), 1);
            // 히스토그램 계산
            Mat hist1 = new Mat();
            Mat hist2 = new Mat();
            int[] histSize = { 256 };
            Rangef[] ranges = { new Rangef(10, 256) };
            Cv2.CalcHist(new Mat[] { img1 }, new int[] { 0 }, null, hist1, 1, histSize, ranges);
            Cv2.CalcHist(new Mat[] { img2 }, new int[] { 0 }, null, hist2, 1, histSize, ranges);
            // 히스토그램 정규화
            Cv2.Normalize(hist1, hist1, 0, 1, NormTypes.MinMax);
            Cv2.Normalize(hist2, hist2, 0, 1, NormTypes.MinMax);

            // 히스토그램 비교
            double similarity = Cv2.CompareHist(hist1, hist2, HistCompMethods.Correl);
            Console.WriteLine($"히스토그램 유사도: {similarity}");
            if (similarity < 0)
                similarity = 0;
            // 임계값 설정 (예: 유사도가 0.9 이상이면 일치)
            return similarity;
        }

        static double ComputeSSIM(Mat img11, Mat img22)
        {
            Mat img1 = img11.Clone(), img2 = img22.Clone();

            if (img1.Channels() != 1)
                Cv2.CvtColor(img1, img1, ColorConversionCodes.BGR2GRAY);
            if (img2.Channels() != 1)
                Cv2.CvtColor(img2, img2, ColorConversionCodes.BGR2GRAY);
            Mat img1Float = new Mat();
            Mat img2Float = new Mat();

            // 이미지 데이터를 float 형식으로 변환
            img1.ConvertTo(img1Float, MatType.CV_32F);
            img2.ConvertTo(img2Float, MatType.CV_32F);

            // 평균값 계산
            Mat mu1 = new Mat(), mu2 = new Mat();
            Cv2.GaussianBlur(img1Float, mu1, new Size(11, 11), 1.5);
            Cv2.GaussianBlur(img2Float, mu2, new Size(11, 11), 1.5);

            // (mu1)^2, (mu2)^2, (mu1 * mu2) 계산
            Mat mu1Sq = new Mat(), mu2Sq = new Mat(), mu1Mu2 = new Mat();
            Cv2.Multiply(mu1, mu1, mu1Sq);
            Cv2.Multiply(mu2, mu2, mu2Sq);
            Cv2.Multiply(mu1, mu2, mu1Mu2);

            // 표준 편차 계산 (sigma1^2, sigma2^2, sigma12)
            Mat sigma1Sq = new Mat(), sigma2Sq = new Mat(), sigma12 = new Mat();
            Cv2.GaussianBlur(img1Float.Mul(img1Float), sigma1Sq, new Size(11, 11), 1.5);
            Cv2.Subtract(sigma1Sq, mu1Sq, sigma1Sq);

            Cv2.GaussianBlur(img2Float.Mul(img2Float), sigma2Sq, new Size(11, 11), 1.5);
            Cv2.Subtract(sigma2Sq, mu2Sq, sigma2Sq);

            Cv2.GaussianBlur(img1Float.Mul(img2Float), sigma12, new Size(11, 11), 1.5);
            Cv2.Subtract(sigma12, mu1Mu2, sigma12);

            // SSIM 공식 적용
            double C1 = 6.5025, C2 = 58.5225;

            Mat C1Mat = new Mat(mu1Mu2.Size(), MatType.CV_32F, new Scalar(C1));
            Mat C2Mat = new Mat(sigma12.Size(), MatType.CV_32F, new Scalar(C2));

            Mat num1 = new Mat(), num2 = new Mat(), den1 = new Mat(), den2 = new Mat();
            Cv2.Add(mu1Mu2, C1Mat, num1);
            Cv2.Multiply(num1, 2, num1);

            Cv2.Add(sigma12, C2Mat, num2);
            Cv2.Multiply(num2, 2, num2);

            Cv2.Add(mu1Sq, mu2Sq, den1);
            Cv2.Add(den1, C1Mat, den1);

            Cv2.Add(sigma1Sq, sigma2Sq, den2);
            Cv2.Add(den2, C2Mat, den2);

            Mat ssimMap = new Mat();
            Cv2.Divide(num1.Mul(num2), den1.Mul(den2), ssimMap);


            // SSIM 점수 계산 (평균값)
            Scalar meanSSIM = Cv2.Mean(ssimMap);
            double ssimScore = meanSSIM.Val0;

            ssimScore = Math.Max(0, Math.Min(ssimScore, 1));

            return ssimScore;
        }


        static double ComputeSSIM2(Mat img1, Mat img2)
        {
            if (img1.Size() != img2.Size())
                Cv2.Resize(img2, img2, img1.Size());

            Mat img1Float = new Mat(), img2Float = new Mat();

            // Python과 동일하게 double 변환
            img1.ConvertTo(img1Float, MatType.CV_64F);
            img2.ConvertTo(img2Float, MatType.CV_64F);

            // Python과 동일한 Gaussian Blur 적용 (sigma = 1.5)
            Mat mu1 = new Mat(), mu2 = new Mat();
            Cv2.GaussianBlur(img1Float, mu1, new Size(11, 11), 1.5);
            Cv2.GaussianBlur(img2Float, mu2, new Size(11, 11), 1.5);

            // (mu1)^2, (mu2)^2, (mu1 * mu2) 계산
            Mat mu1Sq = mu1.Mul(mu1);
            Mat mu2Sq = mu2.Mul(mu2);
            Mat mu1Mu2 = mu1.Mul(mu2);

            // 표준 편차 계산 (Python과 동일)
            Mat sigma1Sq = new Mat(), sigma2Sq = new Mat(), sigma12 = new Mat();
            Cv2.GaussianBlur(img1Float.Mul(img1Float), sigma1Sq, new Size(11, 11), 1.5);
            Cv2.Subtract(sigma1Sq, mu1Sq, sigma1Sq);

            Cv2.GaussianBlur(img2Float.Mul(img2Float), sigma2Sq, new Size(11, 11), 1.5);
            Cv2.Subtract(sigma2Sq, mu2Sq, sigma2Sq);

            Cv2.GaussianBlur(img1Float.Mul(img2Float), sigma12, new Size(11, 11), 1.5);
            Cv2.Subtract(sigma12, mu1Mu2, sigma12);

            // SSIM 공식 적용 (Python 방식)
            double C1 = 6.5025, C2 = 58.5225;

            Mat num1 = new Mat(), num2 = new Mat(), den1 = new Mat(), den2 = new Mat();
            Cv2.AddWeighted(mu1Mu2, 2, new Mat(mu1Mu2.Size(), mu1Mu2.Type(), Scalar.All(C1)), 1, 0, num1);
            Cv2.AddWeighted(sigma12, 2, new Mat(sigma12.Size(), sigma12.Type(), Scalar.All(C2)), 1, 0, num2);

            Cv2.AddWeighted(mu1Sq, 1, mu2Sq, 1, C1, den1);
            Cv2.AddWeighted(sigma1Sq, 1, sigma2Sq, 1, C2, den2);

            Mat ssimMap = new Mat();
            Cv2.Divide(num1.Mul(num2), den1.Mul(den2), ssimMap);

            // SSIM 점수 계산 (평균값)
            Scalar meanSSIM = Cv2.Mean(ssimMap);
            double ssimScore = meanSSIM.Val0;

            // SSIM 값이 0~1을 초과하지 않도록 보정
            ssimScore = Math.Max(0, Math.Min(ssimScore, 1));

            return ssimScore;
        }


        static double GetMeanStdDev(Mat src)
        {
            Scalar mean, stddev;
            Cv2.MeanStdDev(src, out mean, out stddev);
            return stddev.Val0;
        }

        public static TrialSpeakerInspectionItem ExecuteSpeakerInspectionItem(TrialSpeakerInspectionItem trialSpeakerInspectionItem)
        {
            try
            {
                double Accuracy = trialSpeakerInspectionItem.Accuracy;
                List<double> results = new List<double>();
                string templatePath = trialSpeakerInspectionItem.TemplatePath;
                Mat templateBgr = Cv2.ImRead(templatePath);

                Mat result = new Mat();
                Cv2.MatchTemplate(trialSpeakerInspectionItem.ImageRegion, templateBgr, result, TemplateMatchModes.CCoeffNormed);

                // 가장 높은 매칭 점수를 가진 좌표 찾기 
                double minVal, maxVal;
                Point minLoc, maxLoc;
                Cv2.MinMaxLoc(result, out minVal, out maxVal, out minLoc, out maxLoc);

                trialSpeakerInspectionItem.ResultImageRegion = trialSpeakerInspectionItem.ImageRegion.Clone();

                //Cv2.Circle(trialSpeakerInspectionItem.ResultImageRegion, maxLoc.X, maxLoc.Y, 20, Scalar.SeaGreen, -1);
                //Cv2.Circle(trialSpeakerInspectionItem.ResultImageRegion, minLoc.X, minLoc.Y, 20, Scalar.Crimson, -1);
                Cv2.Rectangle(trialSpeakerInspectionItem.ResultImageRegion, new Rect(maxLoc.X, maxLoc.Y, templateBgr.Width, templateBgr.Height), Scalar.YellowGreen, 3);

                bool inSpeaker = false;

                if (maxVal * 100 > Accuracy)
                {
                    inSpeaker = true;
                    trialSpeakerInspectionItem.InspectionResult = INSPECTION_RESULT.OK;
                    trialSpeakerInspectionItem.Conditions = $"{(maxVal * 100).ToString("00.00")}% Match > Target {Accuracy}% ";
                }
                else
                {
                    trialSpeakerInspectionItem.InspectionResult = INSPECTION_RESULT.NG;
                    trialSpeakerInspectionItem.Conditions = $"{(maxVal * 100).ToString("00.00")}% Match < Target {Accuracy}% ";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Machine.logger.Write(eLogType.ERROR, ex.ToString());
                trialSpeakerInspectionItem.InspectionResult = INSPECTION_RESULT.NG;
                trialSpeakerInspectionItem.Conditions = $"0.00% Match < Target {Accuracy}% ";
            }
            return trialSpeakerInspectionItem;
        }


        #region "helper"
        private static Mat TrialPlugPreProcess(Mat ColorImage,
                        int MaskMinThreshold, int MaskMaxThreshold, ThresholdTypes MaskThresholdType,
                        int MorphMinThreshold, int MorphMaxThreshold, ThresholdTypes MorphThresholdType)
        {
            try
            {
                if (ColorImage.Empty())
                {
                    throw new Exception("이미지를 불러오는 데 실패했습니다.");
                }
                //Cv2.ImWrite(@"d:\plug\plug_" + roi_name + ".bmp", ColorImage);
                // 최종 이미지를 이진화 처리
                Mat GrayScaleImage = new Mat();
                Cv2.CvtColor(ColorImage, GrayScaleImage, ColorConversionCodes.BGR2GRAY);

                // 하얀색 이미지를 추출
                Mat Mask = new Mat();
                Cv2.Threshold(GrayScaleImage, Mask, MaskMinThreshold, MaskMaxThreshold, MaskThresholdType);

                Mat kernel = Cv2.GetStructuringElement(MorphShapes.Cross, new Size(5, 5));
                Cv2.MorphologyEx(Mask, Mask, MorphTypes.Open, kernel); // 팽창 후 침식

                // 컬러 이미지와 마스크를 결합
                Mat MaskedImage = new Mat();
                Cv2.BitwiseAnd(GrayScaleImage, Mask, MaskedImage);

                // 가우시안 블러 처리
                Mat BlurredImage = new Mat();
                Cv2.GaussianBlur(MaskedImage, BlurredImage, new Size(1, 5), 0);

                // Morphological operations - 노이즈 제거를 위한 침식
                kernel = Cv2.GetStructuringElement(MorphShapes.Cross, new Size(3, 3));
                Mat MorphedImage = new Mat();
                Cv2.MorphologyEx(BlurredImage, MorphedImage, MorphTypes.Erode, kernel);

                // 이진화 처리 (흰색은 100 그레이는 60)
                Cv2.Threshold(MorphedImage, MorphedImage, MorphMinThreshold, MorphMaxThreshold, MorphThresholdType);
                return MorphedImage;
            }
            catch (Exception ex)
            {
                Console.WriteLine("오류 발생: " + ex.ToString());
                Machine.logger.Write(eLogType.ERROR, ex.ToString());
                return new Mat(); // 빈 이미지 반환
            }
        }

        private static Mat TrialPadPreProcess(Mat ColorImage,
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


        private static Mat TrialWhitePadPreProcess(Mat ColorImage,
                       int MinThreshold, int MaxThreshold)
        {
            try
            {
                int totalArea = 0;

                // #1. Histogram 2024-12-03 LDH
                Mat equalizedImage = new Mat();

                Mat gray = new Mat();
                if (ColorImage.Channels() != 1)
                    Cv2.CvtColor(ColorImage, gray, ColorConversionCodes.BGR2GRAY);
                Cv2.GaussianBlur(gray, gray, new Size(5, 5), 1);

                return gray;
            }

            catch (Exception ex)
            {
                Console.WriteLine("오류 발생: " + ex.ToString());
                Machine.logger.Write(eLogType.ERROR, ex.ToString());
                return new Mat(); // 빈 이미지 반환
            }
        }


        private static Mat TrialScrewPreProcess(Mat ColorImage,
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

        private static Mat TrialSmallPadPreProcess(Mat ColorImage,
                       int MinThreshold, int MaxThreshold, ThresholdTypes PadThresholdType)
        {
            try
            {
                Mat gray = new Mat();
                Cv2.CvtColor(ColorImage, gray, ColorConversionCodes.BGR2GRAY);
                Cv2.GaussianBlur(gray, gray, new Size(3, 3), 1);
                Mat kernel = Cv2.GetStructuringElement(MorphShapes.Cross, new Size(3, 3));
                Cv2.MorphologyEx(gray, gray, MorphTypes.Dilate, kernel);

                return gray;
            }

            catch (Exception ex)
            {
                Console.WriteLine("오류 발생: " + ex.ToString());
                Machine.logger.Write(eLogType.ERROR, ex.ToString());
                return new Mat(); // 빈 이미지 반환
            }
        }



        private Mat TrialScrewMatchPreProcess(Mat ColorImage,
                       int MinThreshold, int MaxThreshold, ThresholdTypes SpeakerThresholdType)
        {
            try
            {
                // #1. Histogram 2024-12-03 LDH
                Mat equalizedImage = new Mat();
                Cv2.EqualizeHist(ColorImage, equalizedImage);

                // #2. Threshold Binary 2024-12-03 LDH
                Mat binaryImage = new Mat();
                Cv2.Threshold(equalizedImage, binaryImage, MinThreshold, MaxThreshold, SpeakerThresholdType);

                return binaryImage;
            }

            catch (Exception ex)
            {
                Console.WriteLine("오류 발생: " + ex.ToString());
                Machine.logger.Write(eLogType.ERROR, ex.ToString());
                return new Mat(); // 빈 이미지 반환
            }
        }

        private Mat TrialSpeakerInspectionPreProcess(Mat ColorImage,
                           int MinThreshold, int MaxThreshold, ThresholdTypes SpeakerThresholdType)
        {
            try
            {
                // #1. Histogram 2024-12-03 LDH
                Mat equalizedImage = new Mat();
                Cv2.EqualizeHist(ColorImage, equalizedImage);

                // #2. Threshold Binary 2024-12-03 LDH
                Mat binaryImage = new Mat();
                Cv2.Threshold(equalizedImage, binaryImage, MinThreshold, MaxThreshold, SpeakerThresholdType);

                return binaryImage;
            }

            catch (Exception ex)
            {
                Console.WriteLine("오류 발생: " + ex.ToString());
                Machine.logger.Write(eLogType.ERROR, ex.ToString());
                return new Mat(); // 빈 이미지 반환
            }
        }

        #endregion
    }
}
