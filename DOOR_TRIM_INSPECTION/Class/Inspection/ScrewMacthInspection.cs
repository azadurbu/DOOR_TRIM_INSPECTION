using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DOOR_TRIM_INSPECTION.Class
{
    public class ScrewMacthInspectionItem
    {
        public int RegionID;
        public string DetectionClassName;
        public string ROIName;
        public Rect ROI;
        public Mat ImageRegion;
        public string TemplatePath;
        public double Accuracy;
        public Mat ProcessedImageRegion;
        public INSPECTION_RESULT InspectionResult;
        public double DetectionAccuracy;
        public string ALC_CODE;
        public string ALC_NAME;
        public string group_name;

        public ScrewMacthInspectionItem(int RegionID, string DetectionClassName, string ROIName, Rect ROI, Mat ImageRegion, Mat ProcessedImageRegion, double Accuracy, string templatePath, string ALC_CODE, string ALC_NAME, string group_name)
        {
            this.RegionID = RegionID;
            this.DetectionClassName = DetectionClassName;
            this.ROIName = ROIName;
            this.ROI = ROI;
            this.ImageRegion = ImageRegion;
            this.ProcessedImageRegion = ProcessedImageRegion;
            this.Accuracy = Accuracy;
            TemplatePath = templatePath;
            this.ALC_CODE = ALC_CODE;
            this.ALC_NAME = ALC_NAME;
            this.group_name = group_name;
        }
    }
    public class ScrewMacthInspection
    {
        public List<ScrewMacthInspectionItem> screwMacthInspectionItems = new List<ScrewMacthInspectionItem>();

        public ScrewMacthInspection(Mat image, List<DetectionROIDetails> detROIs)
        {
            double Accuracy = 0;
            string TemplatePath = "";

            foreach (DetectionROIDetails detROI in detROIs)
            {
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
                Rect ROI = new Rect(detROI.start_x, detROI.start_y, detROI.end_x - detROI.start_x, detROI.end_y - detROI.start_y);
                // MEER EXPANDING ROI FOR CHECKING 2025.02.27
                int newWidth = (int)(ROI.Width * 1.3);
                int newHeight = (int)(ROI.Height * 1.3);
                ROI = new Rect(ROI.X - ((newWidth - ROI.Width) / 2), ROI.Y - ((newHeight - ROI.Height) / 2), newWidth, newHeight);
                // MEER EXPANDING ROI FOR CHECKING 2025.02.27
                List<KeyValuePair<string, string>> keyValuePairs = HanMechDBHelper.ParseParam(detROI.Parameters);

                foreach (KeyValuePair<string, string> keyValue in keyValuePairs)
                {
                    if (ALGORITHM_OPTION.Accuracy.ToString().Equals(keyValue.Key))
                        Accuracy = Convert.ToDouble(keyValue.Value);
                    if (ALGORITHM_OPTION.TemplatePath.ToString().Equals(keyValue.Key))
                        TemplatePath = keyValue.Value;
                }


                Mat ImageRegion = new Mat(image, ROI);
                //Mat MorphedImageRegion = PreProcess(ImageRegion, MinThreshold, MaxThreshold, SpeakerThresholdType);

                screwMacthInspectionItems.Add(new ScrewMacthInspectionItem(
                   detROI.detection_roi_ID, detROI.DetectionClassName, detROI.roi_name, ROI, ImageRegion, ImageRegion, Accuracy, TemplatePath, detROI.ALC_CODE, detROI.ALC_NAME, detROI.group_name));
            }
        }

        public void Execute()
        {
            foreach (ScrewMacthInspectionItem screwMacthInspectionItem in this.screwMacthInspectionItems)
                FindScrewMetch(screwMacthInspectionItem);

            //Console.WriteLine($"SCREW MATCH");
            //Console.WriteLine($"RegionID\tClass\tDetectionAccuracy");
            //foreach (var item in this.screwMacthInspectionItems.OrderBy(item => item.DetectionClassName).ThenBy(item => item.RegionID).ToList())
            //{
            //    Console.WriteLine($"{item.RegionID}\t{item.DetectionClassName}\t{item.DetectionAccuracy}");
            //}
        }

        private void FindScrewMetch(ScrewMacthInspectionItem screwMacthInspectionItem)
        {
            try
            {
                double Accuracy = screwMacthInspectionItem.Accuracy;
                List<double> results = new List<double>();
                string templatePath = screwMacthInspectionItem.TemplatePath;
                Mat templateBgr = Cv2.ImRead(templatePath);
                if (templateBgr.Empty())
                {
                    Machine.logger.Write(eLogType.ERROR, $"Template {templatePath} not found.");
                    return;
                }

                Mat ImageRegion = screwMacthInspectionItem.ImageRegion.Clone();
                Mat templateBgrClone = templateBgr.Clone();

                double highestMaxVal = 0; // 가장 높은 maxVal 저장
                Point bestMatchLoc = default; // 가장 좋은 매칭 좌표 저장

                //Mat result = new Mat();
                //Cv2.MatchTemplate(screwMacthInspectionItem.ImageRegion, templateBgr, result, TemplateMatchModes.CCoeffNormed);

                //// 가장 높은 매칭 점수를 가진 좌표 찾기 
                //double minVal, maxVal;
                //Point minLoc, maxLoc;
                //Cv2.MinMaxLoc(result, out minVal, out maxVal, out minLoc, out maxLoc);
                // 원래 방향 매칭 추가
                {
                    Mat result = new Mat();
                    Cv2.MatchTemplate(screwMacthInspectionItem.ImageRegion, templateBgr, result, TemplateMatchModes.CCoeffNormed);

                    Cv2.MinMaxLoc(result, out double minVal, out double maxVal, out Point minLoc, out Point maxLoc);

                    if (maxVal > highestMaxVal)
                    {
                        highestMaxVal = maxVal;
                        bestMatchLoc = maxLoc;
                    }
                }

                Rect matchRect = new Rect(bestMatchLoc.X, bestMatchLoc.Y, templateBgr.Width, templateBgr.Height);
                if (matchRect.Width + matchRect.X > screwMacthInspectionItem.ImageRegion.Width)
                    matchRect.Width -= matchRect.Width + matchRect.X - screwMacthInspectionItem.ImageRegion.Width;
                if (matchRect.Height + matchRect.Y > screwMacthInspectionItem.ImageRegion.Height)
                    matchRect.Height -= matchRect.Height + matchRect.Y - screwMacthInspectionItem.ImageRegion.Height;
                Mat matchedSubImage = new Mat(screwMacthInspectionItem.ImageRegion, matchRect);

                double HistResult = CompareHistograms(matchedSubImage, templateBgrClone);
                double SSIMResult2 = ComputeSSIM2(matchedSubImage, templateBgrClone);

                highestMaxVal = ((highestMaxVal) + SSIMResult2 + HistResult) / 3;

                screwMacthInspectionItem.InspectionResult = (highestMaxVal * 100 > Accuracy)
                    ? INSPECTION_RESULT.OK
                    : INSPECTION_RESULT.NG;
                screwMacthInspectionItem.DetectionAccuracy = highestMaxVal;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Machine.logger.Write(eLogType.ERROR, ex.ToString());
                screwMacthInspectionItem.InspectionResult = INSPECTION_RESULT.NG;
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
            Rangef[] ranges = { new Rangef(0, 256) };
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
    }
}
