using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
namespace DOOR_TRIM_INSPECTION.Class
{
    public class PlugMatchInspectionItem
    {
        public int RegionID;
        public string DetectionClassName;
        public string ROIName;
        public Rect ROI;
        public Mat ImageRegion;
        public string TemplatePath01;
        public string TemplatePath02;
        public double Accuracy;
        public int MaxLengthX;
        public int MaxLengthY;
        public int PlugDistanceX = 0;
        public int PlugDistanceY = 0;
        public string Direction;
        public Mat ProcessedImageRegion;
        public INSPECTION_RESULT InspectionResult;
        public string ALC_CODE;
        public string ALC_NAME;
        public string group_name;

        public PlugMatchInspectionItem(int RegionID, string DetectionClassName, string ROIName, Rect ROI, Mat ImageRegion, Mat ProcessedImageRegion, double Accuracy, int MaxLengthX, int MaxLengthY, string Direction, string templatePath01, string templatePath02, int PlugDistanceX, int PlugDistanceY, string ALC_CODE, string ALC_NAME, string group_name)
        {
            this.RegionID = RegionID;
            this.DetectionClassName = DetectionClassName;
            this.ROIName = ROIName;
            this.ROI = ROI;
            this.ImageRegion = ImageRegion;
            this.ProcessedImageRegion = ProcessedImageRegion;
            this.Accuracy = Accuracy;
            this.TemplatePath01 = templatePath01;
            this.TemplatePath02 = templatePath02;
            this.MaxLengthX = MaxLengthX;
            this.MaxLengthY = MaxLengthY;
            this.Direction = Direction;
            this.ALC_CODE = ALC_CODE;
            this.ALC_NAME = ALC_NAME;
            this.PlugDistanceX = PlugDistanceX;
            this.PlugDistanceY = PlugDistanceY;
            this.group_name = group_name;
        }
    }
    public class PlugMatchInspection
    {
        public const string Direction_Left = "left";
        public const string Direction_Right = "right";
        public const string Direction_Up = "up";
        public const string Direction_Down = "down";
        public List<PlugMatchInspectionItem> PlugMatchInspectionItems = new List<PlugMatchInspectionItem>();

        public PlugMatchInspection(Mat image, Mat sub1Image, List<DetectionROIDetails> detROIs)
        {
            double Accuracy = 0;
            string TemplatePath1 = "";
            string TemplatePath2 = "";
            int MaxLengthX = 0;
            int MaxLengthY = 0;
            string Direction = "";
            int PlugDistanceX = 0;
            int PlugDistanceY = 0;
            Rect AlternateRoi = new Rect(0, 0, 0, 0);

            bool useAlternateRoi = false;

            foreach (DetectionROIDetails detROI in detROIs)
            {
                AlternateRoi = new Rect(0, 0, 0, 0);
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
                Rect ROI;
                ROI = new Rect(detROI.start_x, detROI.start_y, detROI.end_x - detROI.start_x, detROI.end_y - detROI.start_y);

                List<KeyValuePair<string, string>> keyValuePairs = HanMechDBHelper.ParseParam(detROI.Parameters);

                foreach (KeyValuePair<string, string> keyValue in keyValuePairs)
                {
                    if (ALGORITHM_OPTION.Accuracy.ToString().Equals(keyValue.Key))
                        Accuracy = Convert.ToDouble(keyValue.Value);
                    if (ALGORITHM_OPTION.TemplatePath1.ToString().Equals(keyValue.Key))
                        TemplatePath1 = keyValue.Value;
                    if (ALGORITHM_OPTION.TemplatePath2.ToString().Equals(keyValue.Key))
                        TemplatePath2 = keyValue.Value;
                    if (ALGORITHM_OPTION.MaxLengthX.ToString().Equals(keyValue.Key))
                        MaxLengthX = Convert.ToInt16(keyValue.Value);
                    if (ALGORITHM_OPTION.MaxLengthY.ToString().Equals(keyValue.Key))
                        MaxLengthY = Convert.ToInt16(keyValue.Value);                    
                    if (ALGORITHM_OPTION.Direction.ToString().Equals(keyValue.Key))
                        Direction = keyValue.Value;
                    if (ALGORITHM_OPTION.PlugDistanceX.ToString().Equals(keyValue.Key))
                        PlugDistanceX = Convert.ToInt16(keyValue.Value);
                    if (ALGORITHM_OPTION.PlugDistanceY.ToString().Equals(keyValue.Key))
                        PlugDistanceY = Convert.ToInt16(keyValue.Value);
                    if (ALGORITHM_OPTION.AlternateRoi.ToString().ToLower().Equals(keyValue.Key.ToLower()))
                    {
                        string[] valus = keyValue.Value.Split(',');
                        AlternateRoi = new Rect(int.Parse(valus[0]), int.Parse(valus[1]), int.Parse(valus[2]), int.Parse(valus[3]));
                    }
                }

                //int newWidth = (int)(ROI.Width * 1.3);
                //int newHeight = (int)(ROI.Height * 1.3);
                //ROI = new Rect(ROI.X - ((newWidth - ROI.Width) / 2), ROI.Y - ((newHeight - ROI.Height) / 2), newWidth, newHeight);

                if (AlternateRoi.Width != 0 && AlternateRoi.Height != 0)
                {
                    useAlternateRoi = true;
                    //newWidth = (int)(AlternateRoi.Width * 1.3);
                    //newHeight = (int)(AlternateRoi.Height * 1.3);
                    //AlternateRoi = new Rect(AlternateRoi.X - ((newWidth - AlternateRoi.Width) / 2), AlternateRoi.Y - ((newHeight - AlternateRoi.Height) / 2), newWidth, newHeight);
                }

                Mat ImageRegion = null;
                if (useAlternateRoi)
                {

                    try { ImageRegion = new Mat(sub1Image, AlternateRoi); } catch(Exception ex) { Machine.logger.Write(eLogType.ERROR, $"sub1Image " + sub1Image.Width+"x"+ sub1Image.Height+ ", AlternateRoi " + AlternateRoi.X+","+ AlternateRoi.Y+", "+ AlternateRoi.Width+"x"+ AlternateRoi.Height+"\r\n"+ex.ToString());  continue; }
                    Cv2.ImWrite("Plug_Alter_" + detROI.detection_roi_ID + ".bmp", ImageRegion);
                }
                else
                    ImageRegion = new Mat(image, ROI);
                //Mat MorphedImageRegion = PreProcess(ImageRegion, MinThreshold, MaxThreshold, SpeakerThresholdType);

                PlugMatchInspectionItems.Add(new PlugMatchInspectionItem(
                   detROI.detection_roi_ID, detROI.DetectionClassName, detROI.roi_name, ROI, ImageRegion, ImageRegion, Accuracy, MaxLengthX, MaxLengthY, Direction, TemplatePath1, TemplatePath2, PlugDistanceX, PlugDistanceY, detROI.ALC_CODE, detROI.ALC_NAME, detROI.group_name));
            }
        }

        public void Execute()
        {
            foreach (PlugMatchInspectionItem PlugMatchInspectionItem in this.PlugMatchInspectionItems)
                FindPlugMatch(PlugMatchInspectionItem);
        }

        private void FindPlugMatch(PlugMatchInspectionItem PlugMatchInspectionItem)
        {
            try
            {
                double Accuracy = PlugMatchInspectionItem.Accuracy;
                double MaxLengthX = PlugMatchInspectionItem.MaxLengthX;
                double MaxLengthY = PlugMatchInspectionItem.MaxLengthY;
                List<double> results1 = new List<double>();
                List<double> results2 = new List<double>();
                string templatePath1 = PlugMatchInspectionItem.TemplatePath01;
                string templatePath2 = PlugMatchInspectionItem.TemplatePath02;
                Mat templateBgr1 = Cv2.ImRead(templatePath1);
                Mat templateBgr2 = Cv2.ImRead(templatePath2);
                string Direction = PlugMatchInspectionItem.Direction;
                bool isOk = false;
                if (templateBgr1.Empty() || templateBgr2.Empty())
                {
                    Machine.logger.Write(eLogType.ERROR, $"Template {templatePath1}, {templatePath2} not found.");
                    PlugMatchInspectionItem.InspectionResult = INSPECTION_RESULT.NG;
                    return;
                }


                Cv2.ImWrite(@"D:\plug\Plug_" + PlugMatchInspectionItem.RegionID + ".bmp", PlugMatchInspectionItem.ImageRegion);

                // 이미지 전처리
                Mat processedImageRegion = PreProcessImage(PlugMatchInspectionItem.ImageRegion);
                Mat processedTemplate1 = PreProcessImage(templateBgr1);
                Mat processedTemplate2 = PreProcessImage(templateBgr2);

                // 템플릿 매칭 
                Mat result1 = new Mat();
                Mat result2 = new Mat();

                //Cv2.MatchTemplate(PlugMatchInspectionItem.ImageRegion, templateBgr1, result1, TemplateMatchModes.CCoeffNormed);
                //Cv2.MatchTemplate(PlugMatchInspectionItem.ImageRegion, templateBgr2, result2, TemplateMatchModes.CCoeffNormed);
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

                int DistanceX = int.MinValue;
                int DistanceY = int.MinValue;
                int CrossDistance = int.MinValue;

                DistanceX = topLeft2.X - topLeft1.X;
                DistanceY = topLeft2.Y - topLeft1.Y;
                CrossDistance = (int)Math.Abs((topLeft1.Y + templateBgr1.Height / 2) - (topLeft2.Y + templateBgr2.Height / 2));

                Rect matchRect = new Rect(topLeft2.X, topLeft2.Y, templateBgr2.Width, templateBgr2.Height);
                if (matchRect.Width + matchRect.X > PlugMatchInspectionItem.ImageRegion.Width)
                    matchRect.Width -= matchRect.Width + matchRect.X - PlugMatchInspectionItem.ImageRegion.Width;
                if (matchRect.Height + matchRect.Y > PlugMatchInspectionItem.ImageRegion.Height)
                    matchRect.Height -= matchRect.Height + matchRect.Y - PlugMatchInspectionItem.ImageRegion.Height;
                Mat matchedSubImage = new Mat(PlugMatchInspectionItem.ImageRegion, matchRect);
                
                double HistResult = CompareHistograms(matchedSubImage, templateBgr2);

                int CalcDistanceX = DistanceX - PlugMatchInspectionItem.PlugDistanceX;
                int CalcDistanceY = DistanceY - PlugMatchInspectionItem.PlugDistanceY;
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
#if PROFILE_OUTPUT
                Mat outMat = PlugMatchInspectionItem.ImageRegion.Clone();
                // DRAW 2 RECT for 2 TEMPLATE 1) topLeft1 2) topLeft2
                Cv2.Rectangle(outMat, new Rect(topLeft1.X, topLeft1.Y, templateBgr1.Width, templateBgr1.Height), Scalar.YellowGreen, 2);
                Cv2.Rectangle(outMat, new Rect(topLeft2.X, topLeft2.Y, templateBgr2.Width, templateBgr2.Height), Scalar.YellowGreen, 2);
                Cv2.ArrowedLine(outMat, topLeft2, topLeft1, Scalar.Black, 2, LineTypes.Link8, 0, .3);
                Cv2.CopyMakeBorder(outMat, outMat, 5, 5, 5, 5, BorderTypes.Constant, isOk ? Scalar.SeaGreen : Scalar.Crimson);
                PlugMatchInspectionItem.ProcessedImageRegion = outMat.Clone();
#endif
                PlugMatchInspectionItem.InspectionResult = isOk ? INSPECTION_RESULT.OK : INSPECTION_RESULT.NG;

                
            }
            catch (Exception ex)
            {
                Console.WriteLine("오류 발생: " + ex.ToString());
                Machine.logger.Write(eLogType.ERROR, ex.ToString());
                PlugMatchInspectionItem.InspectionResult = INSPECTION_RESULT.NG;
            }

        }

        private Mat PreProcessImage(Mat image)
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


        static double CalculateSSIM(Mat img1, Mat img2)
        {
            int count1 = Cv2.CountNonZero(img1);
            int count2 = Cv2.CountNonZero(img2);
            if (count1 < 50 || count2 < 50)
                return 0;
            // **정밀도를 높이기 위해 CV_32F 형식으로 변환**
            img1.ConvertTo(img1, MatType.CV_32F);
            img2.ConvertTo(img2, MatType.CV_32F);

            // 가우시안 블러 적용 (Python과 유사한 방식)
            Mat mu1 = new Mat(), mu2 = new Mat();
            Cv2.GaussianBlur(img1, mu1, new Size(11, 11), 1.5);
            Cv2.GaussianBlur(img2, mu2, new Size(11, 11), 1.5);
            // mu1, mu2 제곱
            Mat mu1Sq = mu1.Mul(mu1);
            Mat mu2Sq = mu2.Mul(mu2);
            Mat mu1Mu2 = mu1.Mul(mu2);

            // **표준 편차 (Variance) 계산**
            Mat sigma1Sq = new Mat(), sigma2Sq = new Mat(), sigma12 = new Mat();
            Cv2.GaussianBlur(img1.Mul(img1), sigma1Sq, new Size(11, 11), 1.5);
            Cv2.GaussianBlur(img2.Mul(img2), sigma2Sq, new Size(11, 11), 1.5);
            Cv2.GaussianBlur(img1.Mul(img2), sigma12, new Size(11, 11), 1.5);

            // **분산 = E(X^2) - (E(X))^2**
            sigma1Sq -= mu1Sq;
            sigma2Sq -= mu2Sq;
            sigma12 -= mu1Mu2;

            // SSIM 공식 적용 (분자 및 분모)
            Scalar C1 = new Scalar(6.5025);
            Scalar C2 = new Scalar(58.5225);
            Mat numerator = (mu1Mu2 * 2 + C1).Mul(sigma12 * 2 + C2);
            Mat denominator = (mu1Sq + mu2Sq + C1).Mul(sigma1Sq + sigma2Sq + C2);

            Mat ssimMap = new Mat();
            Cv2.Divide(numerator, denominator, ssimMap);

            Scalar meanSSIM = Cv2.Mean(ssimMap); // SSIM 평균값 계산

            // **디버깅용 로그 (확인 후 제거 가능)**
            Console.WriteLine($"평균 밝기 (mu1): {Cv2.Mean(mu1).Val0:F4}, (mu2): {Cv2.Mean(mu2).Val0:F4}");
            Console.WriteLine($"표준 편차 (sigma1^2): {Cv2.Mean(sigma1Sq).Val0:F4}, (sigma2^2): {Cv2.Mean(sigma2Sq).Val0:F4}");
            Console.WriteLine($"공분산 (sigma12): {Cv2.Mean(sigma12).Val0:F4}");
            Console.WriteLine($"SSIM 분자 평균값: {Cv2.Mean(numerator).Val0:F4}");
            Console.WriteLine($"SSIM 분모 평균값: {Cv2.Mean(denominator).Val0:F4}");
            Console.WriteLine($"최종 SSIM 값: {meanSSIM.Val0:F4}");

            return meanSSIM.Val0;
        }

        private Mat PreProcess(Mat ColorImage,
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
    }
}
