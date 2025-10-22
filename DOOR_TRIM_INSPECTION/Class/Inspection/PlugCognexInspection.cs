using OpenCvSharp;
using System;
using System.Collections.Generic;
using Cognex.VisionPro;
using Cognex.VisionPro.PMAlign;

namespace DOOR_TRIM_INSPECTION.Class
{
#if USE_COGNEX
    public class PlugCognexInspectionItem
    {
        public int RegionID;
        public string DetectionClassName;
        public string ROIName;
        public OpenCvSharp.Rect ROI;
        public Mat ImageRegion;
        public string PlugVppPath01;
        public string PlugVppPath02;
        public double Accuracy;
        public int OuterConfidence;
        public int InnerConfidence;
        public int MaxLengthX;
        public int MaxLengthY;
        public int PlugCogDistanceX = 0;
        public int PlugCogDistanceY = 0;
        public string Direction;
        public Mat ProcessedImageRegion;
        public INSPECTION_RESULT InspectionResult;
        public string ALC_CODE;
        public string ALC_NAME;
        public string group_name;
        public OpenCvSharp.Rect AlternateRoi;

        public PlugCognexInspectionItem(int RegionID, string DetectionClassName, string ROIName, OpenCvSharp.Rect ROI, Mat ImageRegion, Mat ProcessedImageRegion, int OuterConfidence, int InnerConfidence, int MaxLengthX, int MaxLengthY, string Direction,
            string PlugVppPath01, string PlugVppPath02, int PlugCogDistanceX, int PlugCogDistanceY, string ALC_CODE, string ALC_NAME, string group_name, OpenCvSharp.Rect AlternateRoi)
        {
            this.RegionID = RegionID;
            this.DetectionClassName = DetectionClassName;
            this.ROIName = ROIName;
            this.ROI = ROI;
            this.ImageRegion = ImageRegion;
            this.ProcessedImageRegion = ProcessedImageRegion;
            this.OuterConfidence = OuterConfidence;
            this.InnerConfidence = InnerConfidence;
            this.PlugVppPath01 = PlugVppPath01;
            this.PlugVppPath02 = PlugVppPath02;
            this.MaxLengthX = MaxLengthX;
            this.MaxLengthY = MaxLengthY;
            this.Direction = Direction;
            this.ALC_CODE = ALC_CODE;
            this.ALC_NAME = ALC_NAME;
            this.PlugCogDistanceX = PlugCogDistanceX;
            this.PlugCogDistanceY = PlugCogDistanceY;
            this.group_name = group_name;
            this.AlternateRoi = AlternateRoi;
        }
    }
    public class PlugCognexInspection
    {
        public List<PlugCognexInspectionItem> PlugCognexInspectionItems = new List<PlugCognexInspectionItem>();

        public PlugCognexInspection(Mat image, Mat subImage, List<DetectionROIDetails> detROIs)
        {
            double Accuracy = 0;
            string PlugVppPath1 = "";
            string PlugVppPath2 = "";
            int MaxLengthX = 0;
            int MaxLengthY = 0;
            string Direction = "";
            int PlugCogDistanceX = 0;
            int PlugCogDistanceY = 0;
            int OuterConfidence = 0;
            int InnerConfidence = 0;
            OpenCvSharp.Rect AlternateRoi = new OpenCvSharp.Rect(0, 0, 0, 0);
            string aulternateroi = "";
            bool useAlternateRoi = false;

            foreach (DetectionROIDetails detROI in detROIs)
            {
                AlternateRoi = new OpenCvSharp.Rect(0, 0, 0, 0);
                useAlternateRoi = false;
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
                OpenCvSharp.Rect ROI;
                ROI = new OpenCvSharp.Rect(detROI.start_x, detROI.start_y, detROI.end_x - detROI.start_x, detROI.end_y - detROI.start_y);

                List<KeyValuePair<string, string>> keyValuePairs = HanMechDBHelper.ParseParam(detROI.Parameters);

                foreach (KeyValuePair<string, string> keyValue in keyValuePairs)
                {
                    if (ALGORITHM_OPTION.Accuracy.ToString().Equals(keyValue.Key))
                        Accuracy = Convert.ToDouble(keyValue.Value);
                    if (ALGORITHM_OPTION.PlugVppPath1.ToString().Equals(keyValue.Key))
                        PlugVppPath1 = keyValue.Value;
                    if (ALGORITHM_OPTION.PlugVppPath2.ToString().Equals(keyValue.Key))
                        PlugVppPath2 = keyValue.Value;
                    if (ALGORITHM_OPTION.MaxLengthX.ToString().Equals(keyValue.Key))
                        MaxLengthX = Convert.ToInt16(keyValue.Value);
                    if (ALGORITHM_OPTION.MaxLengthY.ToString().Equals(keyValue.Key))
                        MaxLengthY = Convert.ToInt16(keyValue.Value);
                    if (ALGORITHM_OPTION.Direction.ToString().Equals(keyValue.Key))
                        Direction = keyValue.Value;
                    if (ALGORITHM_OPTION.PlugCogDistanceX.ToString().Equals(keyValue.Key))
                        PlugCogDistanceX = Convert.ToInt16(keyValue.Value);
                    if (ALGORITHM_OPTION.PlugCogDistanceY.ToString().Equals(keyValue.Key))
                        PlugCogDistanceY = Convert.ToInt16(keyValue.Value);
                    if (ALGORITHM_OPTION.OuterConfidence.ToString().Equals(keyValue.Key))
                        OuterConfidence = Convert.ToInt16(keyValue.Value);
                    if (ALGORITHM_OPTION.InnerConfidence.ToString().Equals(keyValue.Key))
                        InnerConfidence = Convert.ToInt16(keyValue.Value);
                    if (ALGORITHM_OPTION.AlternateRoi.ToString().ToLower().Equals(keyValue.Key.ToLower()))
                    {
                        string[] valus = keyValue.Value.Split(',');
                        AlternateRoi = new OpenCvSharp.Rect(int.Parse(valus[0]), int.Parse(valus[1]), int.Parse(valus[2]), int.Parse(valus[3]));
                        //aulternateroi = keyValue.Value;
                    }
                }


                if (AlternateRoi.Width != 0 && AlternateRoi.Height != 0)
                {
                    useAlternateRoi = true;
                }

                Mat ImageRegion = null;
                if (useAlternateRoi)
                {

                    try { ImageRegion = new Mat(subImage, AlternateRoi); } catch (Exception ex) { Machine.logger.Write(eLogType.ERROR, $"sub1Image " + subImage.Width + "x" + subImage.Height + ", AlternateRoi " + AlternateRoi.X + "," + AlternateRoi.Y + ", " + AlternateRoi.Width + "x" + AlternateRoi.Height + "\r\n" + ex.ToString()); continue; }
                    Cv2.ImWrite("Plug_Alter_" + detROI.detection_roi_ID + ".bmp", ImageRegion);
                }
                else
                    ImageRegion = new Mat(image, ROI);

                PlugCognexInspectionItems.Add(new PlugCognexInspectionItem(
                   detROI.detection_roi_ID, detROI.DetectionClassName, detROI.roi_name, ROI, ImageRegion, ImageRegion, OuterConfidence, InnerConfidence, MaxLengthX, MaxLengthY, Direction, PlugVppPath1, PlugVppPath2, PlugCogDistanceX, PlugCogDistanceY, detROI.ALC_CODE, detROI.ALC_NAME, detROI.group_name, AlternateRoi));
            }
        }

        public void Execute()
        {
            foreach (PlugCognexInspectionItem PlugCognexInspectionItem in this.PlugCognexInspectionItems)
                FindPlugCognex(PlugCognexInspectionItem);
        }

        private void FindPlugCognex(PlugCognexInspectionItem PlugCognexInspectionItem)
        {
            try
            {
                double OuterConfidence = (double)PlugCognexInspectionItem.OuterConfidence;
                double InnerConfidence = (double)PlugCognexInspectionItem.InnerConfidence;
                double MaxLengthX = PlugCognexInspectionItem.MaxLengthX;
                double MaxLengthY = PlugCognexInspectionItem.MaxLengthY;
                string Direction = PlugCognexInspectionItem.Direction;
                bool isOk = false;

                Mat imgMat = PlugCognexInspectionItem.ImageRegion;

                Tuple<Rect, double> tmpl1Result = Machine.cognexVisionDetection.FindTemplate(PlugCognexInspectionItem.PlugVppPath01, imgMat, new OpenCvSharp.Rect());
                Rect rectTmpl1 = tmpl1Result.Item1;
                double scoreTmpl1 = tmpl1Result.Item2;
                Point topLeft1 = rectTmpl1.TopLeft;

                Tuple<Rect, double> tmpl2Result = Machine.cognexVisionDetection.FindTemplate(PlugCognexInspectionItem.PlugVppPath02, imgMat, new OpenCvSharp.Rect());
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
                if (matchRect.Width + matchRect.X > PlugCognexInspectionItem.ImageRegion.Width)
                    matchRect.Width -= matchRect.Width + matchRect.X - PlugCognexInspectionItem.ImageRegion.Width;
                if (matchRect.Height + matchRect.Y > PlugCognexInspectionItem.ImageRegion.Height)
                    matchRect.Height -= matchRect.Height + matchRect.Y - PlugCognexInspectionItem.ImageRegion.Height;
                Mat matchedSubImage = new Mat(PlugCognexInspectionItem.ImageRegion, matchRect);

                int CalcDistanceX = DistanceX - PlugCognexInspectionItem.PlugCogDistanceX;
                int CalcDistanceY = DistanceY - PlugCognexInspectionItem.PlugCogDistanceY;

                if (CalcDistanceX > MaxLengthX || CalcDistanceY > MaxLengthY || ((int)(scoreTmpl1 * 100) <  OuterConfidence || (int)(scoreTmpl2 * 100) < InnerConfidence)) isOk = false;
                else if (CalcDistanceX < -MaxLengthX || CalcDistanceY < -MaxLengthY || ((int)(scoreTmpl1 * 100) < OuterConfidence || (int)(scoreTmpl2 * 100) < InnerConfidence)) isOk = false;
                else isOk = true;

                PlugCognexInspectionItem.InspectionResult = isOk ? INSPECTION_RESULT.OK : INSPECTION_RESULT.NG;

#if PROFILE_OUTPUT
                Mat outMat = PlugCognexInspectionItem.ImageRegion.Clone();
                Cv2.Rectangle(outMat, rectTmpl1, Scalar.YellowGreen, 2);
                Cv2.Rectangle(outMat, rectTmpl2, Scalar.YellowGreen, 2);
                Cv2.ArrowedLine(outMat, topLeft2, topLeft1, Scalar.Black, 2, LineTypes.Link8, 0, .3);
                Cv2.CopyMakeBorder(outMat, outMat, 5, 5, 5, 5, BorderTypes.Constant, isOk ? Scalar.SeaGreen : Scalar.Crimson);
                PlugCognexInspectionItem.ProcessedImageRegion = outMat.Clone();
#endif

            }
            catch (Exception ex)
            {
                Console.WriteLine("FindPlugCognex: " + ex.ToString());
                Machine.logger.Write(eLogType.ERROR, ex.ToString());
                PlugCognexInspectionItem.InspectionResult = INSPECTION_RESULT.NG;
            }

        }
    }
#endif
}
