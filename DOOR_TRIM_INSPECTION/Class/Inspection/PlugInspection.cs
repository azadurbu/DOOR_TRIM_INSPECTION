using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOOR_TRIM_INSPECTION.Class
{
    public class PlugInspectionItem
    {
        public int RegionID;
        public string DetectionClassName;
        public string ROIName;
        public Rect ROI;
        public Mat ImageRegion;
        public int MaskMinThreshold;
        public int MaskMaxThreshold;
        ThresholdTypes MaskThresholdType;
        public int MorphMinThreshold;
        public int MorphMaxThreshold;
        ThresholdTypes MorphThresholdType;
        public int MinError;
        public int MaxError;
        public int MinContourArea;

        public Mat MorphedImageRegion;

        public Point[] DetectedContour;
        public Rect DetectedRect;
        public INSPECTION_RESULT InspectionResult;

        public string ALC_CODE;
        public string ALC_NAME;
        public string group_name; 

        public PlugInspectionItem(int RegionID,string DetectionClassName, string ROIName, Rect ROI, Mat ImageRegion, Mat MorphedImageRegion, 
            int MaskMinThreshold, int MaskMaxThreshold, ThresholdTypes MaskThresholdType, int MorphMinThreshold, int MorphMaxThreshold, ThresholdTypes MorphThresholdType, 
            int MinError, int MaxError, int MinContourArea, string ALC_CODE,string ALC_NAME, string group_name)
        {
            this.RegionID = RegionID;
            this.DetectionClassName = DetectionClassName;
            this.ROIName = ROIName;
            this.ROI = ROI;
            this.ImageRegion = ImageRegion;
            this.MorphedImageRegion = MorphedImageRegion;
            this.MaskMinThreshold = MaskMinThreshold;
            this.MaskMaxThreshold = MaskMaxThreshold;
            this.MaskThresholdType = MaskThresholdType;
            this.MorphMinThreshold = MorphMinThreshold;
            this.MorphMaxThreshold = MorphMaxThreshold;
            this.MorphThresholdType = MorphThresholdType;
            this.MinError = MinError;
            this.MaxError = MaxError;
            this.MinContourArea = MinContourArea;
            this.ALC_CODE = ALC_CODE;
            this.ALC_NAME = ALC_NAME;
            this.group_name = group_name;
        }
    }

    public class PlugInspection
    {
        public List<PlugInspectionItem> PlugInspectionItems = new List<PlugInspectionItem>();

        private Mat PreProcess(Mat ColorImage,
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

        public PlugInspection(Mat image, List<DetectionROIDetails> detROIs/*, bool IsTrialInspection*/)
        {
            int MaskMinThreshold = 200;
            int MaskMaxThreshold = 2550;
            ThresholdTypes MaskThresholdType = ThresholdTypes.BinaryInv;
            int MorphMinThreshold = 0;
            int MorphMaxThreshold = 255;
            ThresholdTypes MorphThresholdType = ThresholdTypes.Binary;
            int MinError = 0;
            int MaxError = 0;
            int MinContourArea = 0;
            
            foreach (DetectionROIDetails detROI in detROIs)
            {
                string ALCSPA = detROI.ALC_CODE;

                if (ALCSPA != ""/* && !IsTrialInspection*/)
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
                MorphMinThreshold = 0;
                MinError = 0;
                MaxError = 0;
                MinContourArea = 0;

                List<KeyValuePair<string, string>> keyValuePairs= HanMechDBHelper.ParseParam(detROI.Parameters);

                foreach (KeyValuePair<string,string> keyValue in keyValuePairs)
                {
                    if (ALGORITHM_OPTION.MorphMinThreshold.ToString().Equals(keyValue.Key))
                        MorphMinThreshold = Convert.ToInt32(keyValue.Value);
                    if (ALGORITHM_OPTION.MinError.ToString().Equals(keyValue.Key))
                        MinError = Convert.ToInt32(keyValue.Value);
                    if (ALGORITHM_OPTION.MaxError.ToString().Equals(keyValue.Key))
                        MaxError = Convert.ToInt32(keyValue.Value);
                    if (ALGORITHM_OPTION.MinContourArea.ToString().Equals(keyValue.Key))
                        MinContourArea = Convert.ToInt32(keyValue.Value);
                }

                Rect ROI = new Rect(detROI.start_x, detROI.start_y, detROI.end_x - detROI.start_x, detROI.end_y - detROI.start_y);

                Mat ImageRegion = new Mat(image, ROI);
                Mat MorphedImageRegion = PreProcess(ImageRegion, 
                        MaskMinThreshold, MaskMaxThreshold, MaskThresholdType,
                        MorphMinThreshold, MorphMaxThreshold, MorphThresholdType);

                PlugInspectionItems.Add(new PlugInspectionItem(
                    detROI.detection_roi_ID, detROI.DetectionClassName, detROI.roi_name, ROI, ImageRegion, MorphedImageRegion, 
                    MaskMinThreshold, MaskMaxThreshold, MaskThresholdType,
                    MorphMinThreshold, MorphMaxThreshold, MorphThresholdType, 
                    MinError, MaxError, 
                    MinContourArea, detROI.ALC_CODE, detROI.ALC_NAME, detROI.group_name
                ));
            }
        }

        private void FindPlug(PlugInspectionItem plugInspItem)
        {
            try
            {
                // 윤곽선 찾기
                Cv2.FindContours(plugInspItem.MorphedImageRegion, out Point[][] contours, out HierarchyIndex[] hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);
                //Cv2.ImWrite(@"D:\plug\plug_" + plugInspItem.RegionID + ".bmp", plugInspItem.ImageRegion);
                //Cv2.ImWrite(@"D:\plug\plug_" + plugInspItem.RegionID + "_MO.bmp", plugInspItem.MorphedImageRegion);
                Point[] largestContour = null;
                double largestArea = 0;

                foreach (var contour in contours)
                {
                    double area = Cv2.ContourArea(contour);

                    if (area > largestArea && area > plugInspItem.MinContourArea)
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

                    Cv2.DrawContours(plugInspItem.ImageRegion, new[] { largestContour }, -1, new Scalar(0, 0, 255), 2);
                    Rect bBox = Cv2.BoundingRect(largestContour);

                    //Cv2.ImWrite(@"D:\plug\plug_" + plugInspItem.RegionID + "_CON.bmp", plugInspItem.ImageRegion);
                    //Cv2.Rectangle(plugInspItem.ImageRegion, bBox, new Scalar(255, 0, 255), 2, LineTypes.Link8);
                    //Cv2.ImShow("test", plugInspItem.ImageRegion);
                    //Cv2.WaitKey(0);



                    plugInspItem.DetectedContour = TranslateToOrigin(plugInspItem.ROI.X, plugInspItem.ROI.Y, largestContour);
                    plugInspItem.DetectedRect = TranslateToOrigin(plugInspItem.ROI.X, plugInspItem.ROI.Y,
                        new Rect(leftmostPoint.X, leftmostPoint.Y, rightmostPoint.X - leftmostPoint.X, leftBottomPoint.Y - leftmostPoint.Y));

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

                    if ((rightmostRotated.X - leftmostRotated.X) < plugInspItem.MinError || (rightmostRotated.X - leftmostRotated.X) >= plugInspItem.MaxError)
                    {
                        plugInspItem.InspectionResult = INSPECTION_RESULT.NG;
                    }
                    else
                    {
                        plugInspItem.InspectionResult = INSPECTION_RESULT.OK;
                    }
                }
                else
                {
                    plugInspItem.InspectionResult = INSPECTION_RESULT.NOT_FOUND;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("오류 발생: " + ex.ToString());
                Machine.logger.Write(eLogType.ERROR, ex.ToString());
                plugInspItem.InspectionResult = INSPECTION_RESULT.NOT_FOUND;

            }
        }

        private Point[] TranslateToOrigin(int OffsetX, int OffsetY, Point[] largestContour)
        {
            for (int i = 0; i < largestContour.Length; i++)
            {
                largestContour[i] = new Point(OffsetX + largestContour[i].X, OffsetY + largestContour[i].Y);
            }
            return largestContour;
        }

        private Rect TranslateToOrigin(int OffsetX, int OffsetY, Rect boundingBox)
        {
            return new Rect(boundingBox.X + OffsetX, boundingBox.Y + OffsetY, boundingBox.Width, boundingBox.Height);
        }

        public void Execute()
        {
            foreach (PlugInspectionItem plugInspItem in this.PlugInspectionItems)
                FindPlug(plugInspItem);
        }

       
    }
}
