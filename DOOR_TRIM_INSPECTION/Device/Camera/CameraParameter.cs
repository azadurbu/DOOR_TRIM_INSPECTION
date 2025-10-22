using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOOR_TRIM_INSPECTION.Device.Camera
{
    public enum eAcquisitionMode
    {
        Single,
        Multi,
        Continuous
    }

    public enum eCameraStatus
    {
        CAM_CONNECTION_SUCCESS,
        CAM_CONNECTION_ERR,
        CAM_NOT_FOUND,
        CAM_CONNECTION_FAIL_ALREADY_USED_CAM,
        CAM_CONNECTION_FAIL_SERIAL_NOT_EXIST,
        CAM_CONNECTION_FAIL_NOT_FOUND_CAM,
        CAM_CONNECTION_FAIL_CAM_NULL,
        CAM_CONNECTION_FAIL_INTERNAL_ERROR,
    };

    public class MergeProperty
    {
        public List<int> SubImageoffsetArray = new List<int>();
        public int[] CamImageOffsetArray = new int[3];
        public bool IsCamPlusDirection = false;
    }

    public class CameraProperty
    {
        public string CamName;
        public string CamAddress;
        public string SerialNumber;
        public int Exposure = 1000;
        public int OffsetX = 0;
        public int OffsetY = 0;
        public int Width = 5472;
        public int Height = 3648;
        public int frameRate = 10;
        public int gain = 0;

        public void SetProperty(CameraProperty property)
        {
            this.CamName = property.CamName;
            this.CamAddress = property.CamAddress;
            this.SerialNumber = property.SerialNumber;
            this.Exposure = property.Exposure;
            this.OffsetX = property.OffsetX;
            this.OffsetY = property.OffsetY;
            this.Width = property.Width;
            this.Height = property.Height;
            this.frameRate = property.frameRate;
            this.gain = property.gain;
    }

        public CameraProperty Copy()
        {
            CameraProperty property = new CameraProperty();
            property.CamName = this.CamName;
            property.CamAddress = this.CamAddress;
            property.SerialNumber = this.SerialNumber;
            property.Exposure = this.Exposure;
            property.OffsetX = this.OffsetX;
            property.OffsetY = this.OffsetY;
            property.Width = this.Width;
            property.Height = this.Height;
            property.frameRate = this.frameRate;
            property.gain = this.gain;

            return property;
        }
    }
}
