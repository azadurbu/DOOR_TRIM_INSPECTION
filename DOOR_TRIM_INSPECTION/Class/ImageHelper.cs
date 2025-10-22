using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DOOR_TRIM_INSPECTION.Class
{
    public class ImageHelper
    {
        public static BitmapSource ConvertMatToBitmapSource(Mat mat)
        {
            // Convert the Mat to BitmapImage (WPF compatible format)
            if (mat.Empty())
            {
                throw new ArgumentException("Mat is empty");
            }

            // Convert Mat to byte array
            byte[] byteArray = mat.ToBytes();

            // Create a MemoryStream from the byte array
            using (var stream = new System.IO.MemoryStream(byteArray))
            {
                // Create a BitmapImage from the MemoryStream
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        public static ImageBrush ConvertMatToImageBrush(Mat mat)
        {
            // Convert Mat to BitmapSource
            BitmapSource bitmapSource = ConvertMatToBitmapSource(mat);

            // Create and return the ImageBrush
            return new ImageBrush(bitmapSource);
        }

        public static Mat MergeImagesStitcher(Mat[] imgs)
        {
            Stitcher stitcher = Stitcher.Create();

            Mat result = new Mat();
            stitcher.Stitch(imgs, result);

            return result;
        }

    }

    public struct ColorBgra
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        // 인덱서를 추가하여 R, G, B를 배열처럼 접근 가능

        public byte this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return R;
                    case 1:
                        return G;
                    case 2:
                        return B;
                    case 3:
                        return A;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(index), "Index must be between 0 and 3.");
                }
            }
        }

        public static ColorBgra Black => new ColorBgra { R = 0, G = 0, B = 0, A = 255 };
        public static ColorBgra White => new ColorBgra { R = 255, G = 255, B = 255, A = 255 };
    }

    public static class Extensions
    {
        public static double Clamp(this double value, double min, double max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }

    public class LevelAdjustment
    {
        private ColorBgra colorInLow;
        private ColorBgra colorInHigh;
        private ColorBgra colorOutLow;
        private ColorBgra colorOutHigh;
        public float[] Gamma { get; set; }

        public LevelAdjustment()
        {
            Gamma = new float[3] { 1f, 1f, 1f }; // 기본 감마 값
        }
        public ColorBgra ColorInLow
        {
            get => this.colorInLow;
            set
            {
                if (value.R == byte.MaxValue) value.R = 254;
                if (value.G == byte.MaxValue) value.G = 254;
                if (value.B == byte.MaxValue) value.B = 254;

                if (this.colorInHigh.R < value.R + 1) this.colorInHigh.R = (byte)(value.R + 1);
                if (this.colorInHigh.G < value.G + 1) this.colorInHigh.G = (byte)(value.G + 1);
                if (this.colorInHigh.B < value.B + 1) this.colorInHigh.B = (byte)(value.B + 1);

                this.colorInLow = value;
            }
        }

        public ColorBgra ColorInHigh
        {
            get => this.colorInHigh;
            set
            {
                if (value.R == 0) value.R = 1;
                if (value.G == 0) value.G = 1;
                if (value.B == 0) value.B = 1;

                if (this.colorInLow.R > value.R - 1) this.colorInLow.R = (byte)(value.R - 1);
                if (this.colorInLow.G > value.G - 1) this.colorInLow.G = (byte)(value.G - 1);
                if (this.colorInLow.B > value.B - 1) this.colorInLow.B = (byte)(value.B - 1);

                this.colorInHigh = value;
            }
        }

        public ColorBgra ColorOutLow
        {
            get => this.colorOutLow;
            set
            {
                if (value.R == byte.MaxValue) value.R = 254;
                if (value.G == byte.MaxValue) value.G = 254;
                if (value.B == byte.MaxValue) value.B = 254;

                if (this.colorOutHigh.R < value.R + 1) this.colorOutHigh.R = (byte)(value.R + 1);
                if (this.colorOutHigh.G < value.G + 1) this.colorOutHigh.G = (byte)(value.G + 1);
                if (this.colorOutHigh.B < value.B + 1) this.colorOutHigh.B = (byte)(value.B + 1);

                this.colorOutLow = value;
            }
        }

        public ColorBgra ColorOutHigh
        {
            get => this.colorOutHigh;
            set
            {
                if (value.R == 0) value.R = 1;
                if (value.G == 0) value.G = 1;
                if (value.B == 0) value.B = 1;

                if (this.colorOutLow.R > value.R - 1) this.colorOutLow.R = (byte)(value.R - 1);
                if (this.colorOutLow.G > value.G - 1) this.colorOutLow.G = (byte)(value.G - 1);
                if (this.colorOutLow.B > value.B - 1) this.colorOutLow.B = (byte)(value.B - 1);

                this.colorOutHigh = value;
            }
        }
    }

    public static class LevelOps
    {
        private static LevelAdjustment AutoAdjust(ColorBgra lo, ColorBgra mid, ColorBgra hi)
        {
            var levelAdjustment = new LevelAdjustment
            {
                ColorInLow = lo,
                ColorInHigh = hi,
                ColorOutLow = ColorBgra.Black,
                ColorOutHigh = ColorBgra.White
            };

            float[] gamma = new float[3];
            for (int channel = 0; channel < 3; ++channel)
            {
                if ((int)lo[channel] >= (int)mid[channel] || (int)mid[channel] >= (int)hi[channel])
                {
                    gamma[channel] = 1f;
                }
                else
                {
                    double ratio = (double)((int)mid[channel] - (int)lo[channel]) / (double)((int)hi[channel] - (int)lo[channel]);
                    gamma[channel] = (float)Math.Log(0.5, ratio.Clamp(0.1, 10.0));
                }
            }

            levelAdjustment.Gamma = gamma;
            return levelAdjustment;
        }

        public static Mat EqualizeHistColor(Mat image, byte loVealue = 100, byte midValue = 110, byte hiValue = 255)
        {
            ColorBgra lo = new ColorBgra { R = loVealue, G = loVealue, B = loVealue, A = 255 };
            ColorBgra mid = new ColorBgra { R = midValue, G = midValue, B = midValue, A = 255 };
            ColorBgra hi = new ColorBgra { R = hiValue, G = hiValue, B = hiValue, A = 255 };
            // 감마 값 계산
            LevelAdjustment levelOp = LevelOps.AutoAdjust(lo, mid, hi);
            float[] gamma = levelOp.Gamma;
            // 채널 분리
            Mat[] channels = Cv2.Split(image);

            // 각 채널에 대해 감마 조정
            for (int i = 0; i < 3; i++) // R, G, B 채널만 처리
            {
                channels[i].ConvertTo(channels[i], MatType.CV_32F, 1.0 / 255.0); // 0~1로 정규화
                Cv2.Pow(channels[i], gamma[i], channels[i]); // 감마 연산
                channels[i] *= 255.0; // 다시 0~255로 스케일링
                channels[i].ConvertTo(channels[i], MatType.CV_8U); // 타입 변환
            }

            // 채널 병합
            Mat result = new Mat();
            Cv2.Merge(channels, result);

            return result;
        }
    }

    public class StitcherParam
    {
        public double RegistrationResol = 0.8;
        public double SeamEstimationResol = 0.4;
        public double CompositingResol = -1;
        public double PanoConfidenceThresh = 0.3;

        public void SetParam(StitcherParam stitcherParam)
        {
            RegistrationResol = stitcherParam.RegistrationResol;
            SeamEstimationResol = stitcherParam.SeamEstimationResol;
            CompositingResol = stitcherParam.CompositingResol;
            PanoConfidenceThresh = stitcherParam.PanoConfidenceThresh;
        }

        public StitcherParam Copy()
        {
            StitcherParam stitcherParam = new StitcherParam();
            stitcherParam.RegistrationResol = this.RegistrationResol;
            stitcherParam.SeamEstimationResol = this.SeamEstimationResol;
            stitcherParam.CompositingResol = this.CompositingResol;
            stitcherParam.PanoConfidenceThresh = this.PanoConfidenceThresh;
            return stitcherParam;
        }
    }

}
