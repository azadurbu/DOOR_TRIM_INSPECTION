using MvCamCtrl.NET;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace DOOR_TRIM_INSPECTION.Device.Camera
{
    public delegate void DelGrabEnd(int camNo, int subImgNo);
    public delegate void DelSendErrMsg(string msg);

    public class CameraHik
    {
        private CameraProperty _property = new CameraProperty();

        public System.Drawing.Bitmap newImage = null;
        public Mat _newImage = null;

        public static bool IsLive = false;

        private bool _isConnectedError = false;
        private bool _isGrabbing = false;
        private bool _isConnected = false;
        private bool _isopen = false;
        public bool Accessible = false; // 끊김 체크

        private int _camNo = 0;

        private MyCamera.cbOutputExdelegate ImageCallback;
        private bool _isCallBackCompleted = true;
        private bool _IsGrabCompleted = true;

        public Action<int> DeleDisConnected;
        public DelGrabEnd dGrabEnd = null;
        public static object ObjLock = new object();

        //private List<Bitmap> _subImageList = new List<Bitmap>();
        //private List<Mat> _matSubImageList = new List<Mat>();

        private MyCamera _cameraContext = null;

        public bool IsSaveMode = false;

        public DelSendErrMsg sendCamErrMsg;

        public MyCamera CameraContext { get => _cameraContext; set => _cameraContext = value; }

        int cropX = 0;
        int cropWidth = 0;
        int cropHeight = 0;

        public eCameraStatus Initialize(MyCamera.MV_CC_DEVICE_INFO stDevInfo)
        {
            try
            {
                if (_cameraContext == null)
                {
                    _cameraContext = new MyCamera();
                    _cameraContext.MV_CC_CreateDevice_NET(ref stDevInfo);

                    if (OpenDevice())
                        _isConnected = true;
                    else
                        _isConnected = false;

                    _isopen = true;

                    if (!IsAccessible())
                        return eCameraStatus.CAM_CONNECTION_FAIL_CAM_NULL;

                    SetProperty(this._property);

                    ImageCallback = new MyCamera.cbOutputExdelegate(OnImageGrabbed);
                    int nRet = _cameraContext.MV_CC_RegisterImageCallBackEx_NET(ImageCallback, IntPtr.Zero);


                    if (nRet != MyCamera.MV_OK)
                    {
                        string msg = "Register Image CallBack Failed";
                        Machine.logger.Write(eLogType.ERROR, "Register Image CallBack Failed");
                        Console.WriteLine(msg);

                        if(sendCamErrMsg != null)
                            sendCamErrMsg(msg);
                        return eCameraStatus.CAM_CONNECTION_FAIL_INTERNAL_ERROR;
                    }

                    return eCameraStatus.CAM_CONNECTION_SUCCESS;
                }
                else
                {
                    //Logger.Write(eLogType.ERROR, "Camera is not connected", DateTime.Now);
                    return eCameraStatus.CAM_CONNECTION_FAIL_CAM_NULL;
                }


            }
            catch (Exception err)
            {
                // Machine.ShowErrorInfoDlg("Camera Initialize Error. \r\n" + err.Message, false); // MEER 2024.03.08 
                return eCameraStatus.CAM_CONNECTION_ERR;
            }
        }

        public void Terminate()
        {
            try
            {
                int nRet;

                nRet = _cameraContext.MV_CC_StopGrabbing_NET();
                _isGrabbing = false;

                nRet = _cameraContext.MV_CC_CloseDevice_NET();
                _isopen = false;

                nRet = _cameraContext.MV_CC_DestroyDevice_NET();
                if (nRet == MyCamera.MV_OK)
                {
                    string msg = string.Format("Destory Camera : Camnum {0}", _camNo);
                    if (sendCamErrMsg != null)
                        sendCamErrMsg(msg);
                    Console.WriteLine(msg);
                    Machine.logger.Write(eLogType.ERROR, string.Format("Destory Camera : Camnum {0}", _camNo));
                }
                _cameraContext = null;
                //SubImageListClear();
            }
            catch (Exception err)
            {
                Machine.logger.Write(eLogType.ERROR, err.ToString());
                Console.WriteLine(err.Message);
            }
        }

        public void SetProperty(CameraProperty param)
        {
            this._property = (CameraProperty)param;
        }

        public CameraProperty GetProperty()
        {
            return _property.Copy();
        }

        public void SetCamNo(int camNo)
        {
            this._camNo = camNo;
        }

        public int GetCamNo()
        {
            return _camNo;
        }

        public void SetExpose(double expose)
        {
            int nRet = _cameraContext.MV_CC_SetFloatValue_NET("ExposureTime", (float)expose);
            if (nRet != MyCamera.MV_OK)
            {
                string msg = string.Format("Set ExposureTime Failed {0}", this._camNo);

                Machine.logger.Write(eLogType.ERROR, string.Format("Set ExposureTime Failed {0}", this._camNo));
                Console.WriteLine(msg);
                if (sendCamErrMsg != null)
                    sendCamErrMsg(msg);
            }
        }
        public double GetGain()
        {
            if (CameraContext.MV_CC_IsDeviceConnected_NET() == false)
                return -1;

            MyCamera.MVCC_FLOATVALUE stFloatVal = new MyCamera.MVCC_FLOATVALUE();
            _cameraContext.MV_CC_GetFloatValue_NET("ExposureTime", ref stFloatVal);

            return stFloatVal.fCurValue;
        }


        public double GetExpose()
        {
            if (CameraContext.MV_CC_IsDeviceConnected_NET() == false)
                return -1;

            MyCamera.MVCC_FLOATVALUE stFloatVal = new MyCamera.MVCC_FLOATVALUE();
            _cameraContext.MV_CC_GetFloatValue_NET("ExposureTime", ref stFloatVal);

            return stFloatVal.fCurValue;
        }

        public bool IsCallBackCompleted()
        {
            return _isCallBackCompleted;
        }

        public bool IsGrabCompleted()
        {
            return _IsGrabCompleted;
        }

        public bool StartGrab()
        {
            try
            {
                //if (!IsAccessible())
                //    return false;

                if (!IsOpen())
                {
                    string message = "camera context is null. cam index : " + _camNo.ToString();
                    if (sendCamErrMsg != null)
                        sendCamErrMsg(message);

                    // Machine.ShowErrorInfoDlg(message, false); // MEER 2024.03.08
                    //Logger.Write(eLogType.ERROR, message, DateTime.Now);
                    return false;
                }

                if (!IsGrabbing())
                {
                    Console.WriteLine("CamNo : " + _camNo.ToString() + " Start Grab");
                    Machine.logger.Write(eLogType.CAMERA, "CamNo : " + _camNo.ToString() + " Start Grab");
                    Machine.logger.Write(eLogType.SEQ, "CamNo : " + _camNo.ToString() + " Start Grab");
                    //SetCropValue();
                    _newImage = null;
                    _cameraContext.MV_CC_StartGrabbing_NET();
                    _IsGrabCompleted = false;
                    _isGrabbing = true;
                }
                return true;
            }
            catch (Exception err)
            {
                //Logger.WriteException(eLogType.ERROR, err);
                // Machine.ShowErrorInfoDlg("Camera Grab Error. Cam Num:" + _camNo.ToString() + "\r\n" + err.Message, false); // MEER 2024.03.08
                return false;
            }
        }

        public bool StopGrab()
        {
            try
            {
                //if (!IsAccessible())
                //    return false;
                //if (_isGrabbing)
                _cameraContext.MV_CC_StopGrabbing_NET();

                _isGrabbing = false;
                string msg = "CamNo : " + _camNo.ToString() + " Stop Grab";
                Machine.logger.Write(eLogType.CAMERA, msg);
                Console.WriteLine(msg);
                if(sendCamErrMsg != null)
                    sendCamErrMsg(msg);
                return true;
            }
            catch (Exception ex)
            {
                string str = ex.ToString();

                Machine.logger.Write(eLogType.ERROR, str);
                Console.WriteLine(str);
                if (sendCamErrMsg != null)
                    sendCamErrMsg(str);

                return false;

            }
        }

        public bool ActiveProperty()
        {
            _cameraContext.MV_CC_SetAcquisitionMode_NET((uint)MyCamera.MV_CAM_ACQUISITION_MODE.MV_ACQ_MODE_SINGLE);

            int nRet;
            bool result = true;

            nRet = _cameraContext.MV_CC_SetFloatValue_NET("ExposureTime", (float)this._property.Exposure);
           

            if (nRet != MyCamera.MV_OK)
            {
                result = false;
                string msg = string.Format("Set ExposureTime Failed {0}", this._camNo);
                if(sendCamErrMsg != null)
                    sendCamErrMsg(msg);

                Console.WriteLine(msg);
                Machine.logger.Write(eLogType.ERROR, msg);
            }

            // 240730 LDH
            nRet = _cameraContext.MV_CC_SetGain_NET((float)this._property.gain);


            if (nRet != MyCamera.MV_OK)
            {
                result = false;
                string msg = string.Format("Set Gain Failed {0}", this._camNo);
                if (sendCamErrMsg != null)
                    sendCamErrMsg(msg);

                Console.WriteLine(msg);
                Machine.logger.Write(eLogType.ERROR, msg);
            }

            return result;
        }

        public Mat GetSubImage()
        {
            if (_newImage == null)
                return null;
            //Mat temp = _newImage.Clone();
            //_newImage.Dispose();
            //_newImage=null;
            return _newImage.Clone();            
        }

        //public List<Mat> GetSubImageList()
        //{
        //    if (_matSubImageList == null)
        //        return null;

        //    return _matSubImageList;

        //    //if (_subImageList == null)
        //    //    return null;

        //    //return _subImageList;
        //}

        //public int GetSubImageCount()
        //{
        //    return _matSubImageList.Count;
        //    //return _subImageList.Count;
        //}

        private bool OpenDevice()
        {
            int nRet;
            int nPacketSize;
            bool ret = false;
            nRet = _cameraContext.MV_CC_OpenDevice_NET();
            if (nRet != MyCamera.MV_OK)
            {
                string msg = string.Format("Failed Open Device {0}", _camNo);
                if (sendCamErrMsg != null)
                    sendCamErrMsg(msg);
                Machine.logger.Write(eLogType.ERROR, msg);
                Console.WriteLine(msg);
                ret = false;
                return false;
            }
            else
                ret = true;

            nPacketSize = _cameraContext.MV_CC_GetOptimalPacketSize_NET();
            if (nPacketSize > 0)
            {
                nRet = _cameraContext.MV_CC_SetIntValue_NET("GevSCPSPacketSize", (uint)nPacketSize);
                if (nRet != MyCamera.MV_OK)
                {
                    Console.WriteLine("Failed Set Packet Size");
                    Machine.logger.Write(eLogType.ERROR, "Failed Set Packet Size");
                    ret = false;
                }
                else
                    ret = true;
            }

            //nRet = HikCamera.CameraContextList[_camNo].MV_CC_SetEnumValue_NET("TriggerMode", (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF);
            //if (nRet != MyCamera.MV_OK)
            //{
            //    Console.WriteLine("Failed Set Trigger Mode");
            //    ret = false;
            //}
            //else
            //    ret = true;
            return ret;
        }

        public bool IsOpen()
        {
            if (_cameraContext == null)
            {
                this._isConnectedError = true;
                string message = "CamNo : " + _camNo.ToString() + " CameraContext = null ";
                //Logger.Write(eLogType.ERROR, message, DateTime.Now);
                return false;
            }
            if (_isConnected == false)
                return false;
            _isopen = true;
            try
            {

                bool ret = false;
                
                try { ret = _cameraContext.MV_CC_IsDeviceConnected_NET(); } catch { }

                if (ret == false)
                {
                    string message = "CamNo : " + _camNo.ToString() + " IsOpen Error_Accessible : " + Accessible.ToString() + " IsConnected : "
                        + this._isConnected.ToString() + " IsOpen : " + this.IsOpen().ToString();

                    //Logger.Write(eLogType.ERROR, message, DateTime.Now);

                    this._isConnectedError = true;
                }
                else
                    this._isConnectedError = false;

                if (!ret)
                {
                    if (DeleDisConnected != null)
                        DeleDisConnected(_camNo);
                }

                return ret;
            }
            catch
            {
                return false;
            }
        }

        public bool IsGrabbing()
        {
            return this._isGrabbing;
        }

        //public void ClearSubImageList()
        //{
        //    if (_matSubImageList == null) return;

        //    _matSubImageList.Clear();
        //    //if (_subImageList == null) return;

        //    //_subImageList.Clear();
        //}

        private bool IsAccessible()
        {
            try
            {
                if (_cameraContext == null)
                {
                    //Logger.Write(eLogType.ERROR, "Camera is not connected", DateTime.Now);
                    _isConnected = false;
                }

                return _cameraContext.MV_CC_IsDeviceConnected_NET();
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                Machine.logger.Write(eLogType.ERROR, "CAM ERROR: " + err.Message);

                return false;
            }
        }

        private void OnImageGrabbed(IntPtr pData, ref MyCamera.MV_FRAME_OUT_INFO_EX FrameInfo, IntPtr pUser)
        {
            try
            {
                _isCallBackCompleted = false;
                if (pData == null)
                    return;

                int size = FrameInfo.nWidth * FrameInfo.nHeight*3;
                byte[] buffer = new byte[size];

                bool isMono = FrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8 ||
                    FrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10 ||
                    FrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12;

                if (isMono)
                {

                    size = FrameInfo.nWidth * FrameInfo.nHeight;
                    buffer = new byte[size];

                }

                Marshal.Copy(pData, buffer, 0, size);

                _newImage = isMono ?
                    ConvertToGray8BitBitmap(buffer, FrameInfo.nWidth, FrameInfo.nHeight) :
                    ConvertToGray24BitBitmap(buffer, FrameInfo.nWidth, FrameInfo.nHeight);

                //if (IsSaveMode == true)
                //    _matSubImageList.Add(_newImage);

                //화면에 뿌려줄 이미지
                if (dGrabEnd != null)
                {
                    int subIndex =0;// _matSubImageList.Count - 1;
                    dGrabEnd(_camNo, subIndex);
                }

                _isCallBackCompleted = true;
                _IsGrabCompleted = true;
                _isGrabbing = false;
                //Machine.logger.Write(eLogType.SEQ, "CAM "+_camNo+" END");

            }
            catch (Exception ex)
            {
                Machine.logger.Write(eLogType.ERROR, "CAM OnImageGrabbed ERROR: " + ex.ToString());
                Console.WriteLine(ex.ToString());
            }
        }

        public void UIDelegate()
        {
            //if (_subImageList.Count <= 0) return;

            //int subIndex = _subImageList.Count - 1;

            //if(dGrabEnd != null)
            //    dGrabEnd((Bitmap)_subImageList[subIndex].Clone(new System.Drawing.Rectangle(cropX, 0, cropWidth, cropHeight), _subImageList[subIndex].PixelFormat), _camNo, subIndex);
        }

        //private Bitmap ConvertToGray8BitBitmap(byte[] buffer, int width, int height)
        //{
        //    Bitmap bmp = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
        //    ColorPalette palette = bmp.Palette;
        //    Color[] _entries = palette.Entries;

        //    for (int i = 0; i < 256; i++)
        //    {
        //        Color b = new Color();
        //        b = Color.FromArgb((byte)i, (byte)i, (byte)i);
        //        _entries[i] = b;
        //    }

        //    bmp.Palette = palette;
        //    BitmapData bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);
        //    Marshal.Copy(buffer, 0, bmpData.Scan0, buffer.Length);
        //    bmp.UnlockBits(bmpData);

        //    return bmp;
        //}

        private Mat ConvertToGray8BitBitmap(byte[] buffer, int width, int height)
        {
            Mat image = new Mat(height, width, MatType.CV_8UC1);
            int length = height * width * 1; // or image.Height * image.Step;
            Marshal.Copy(buffer, 0, image.Data, length);

            return image;
        }

        private Mat ConvertToGray24BitBitmap(byte[] buffer, int width, int height)
        {
            Mat image = new Mat(height, width, MatType.CV_8UC3);
            int length = buffer.Length;
            Marshal.Copy(buffer, 0, image.Data, length);

            return image;
        }
        //private void SetCropValue()
        //{
        //    cropX = Machine.config.setup.cameraProp[_camNo].OffsetX;
        //    cropWidth = Machine.config.setup.cameraProp[_camNo].Width;
        //    cropHeight = Machine.config.setup.cameraProp[_camNo].Height;
        //}
    }
}
