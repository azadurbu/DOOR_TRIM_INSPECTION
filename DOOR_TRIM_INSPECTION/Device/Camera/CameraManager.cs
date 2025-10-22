using MvCamCtrl.NET;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace DOOR_TRIM_INSPECTION.Device.Camera
{
    public enum eCamPosDefine
    {
        CENTER,
        TOP,
        RIGHT,
        LEFT,
        BOTTOM,
        COUNT
    }

    public class CameraManager
    {
        private MyCamera.MV_CC_DEVICE_INFO_LIST _stDevList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
        private MyCamera.MV_CC_DEVICE_INFO _stDevInfo = new MyCamera.MV_CC_DEVICE_INFO();
        public List<CameraHik> _cameraList = new List<CameraHik>();
        private int MaxCamCnt = 0;
        public DelGrabEnd UpdateMergeImg = null;

        private bool _isGrabMode = false;
        private Mat mergeMat;
        Mat[] subImage;

        DelSendErrMsg sendCamManagerErrMsg;

        public List<string> FindCamera()
        {
            List<string> cameraNameList = new List<string>();
            int nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref _stDevList);

            for (Int32 i = 0; i < _stDevList.nDeviceNum; i++)
            {
                string temp = "";
                _stDevInfo = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(_stDevList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));

                if (MyCamera.MV_GIGE_DEVICE == _stDevInfo.nTLayerType)
                {
                    MyCamera.MV_GIGE_DEVICE_INFO stGigEDeviceInfo = (MyCamera.MV_GIGE_DEVICE_INFO)MyCamera.ByteToStruct(_stDevInfo.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                    uint nIp1 = ((stGigEDeviceInfo.nCurrentIp & 0xff000000) >> 24);
                    uint nIp2 = ((stGigEDeviceInfo.nCurrentIp & 0x00ff0000) >> 16);
                    uint nIp3 = ((stGigEDeviceInfo.nCurrentIp & 0x0000ff00) >> 8);
                    uint nIp4 = (stGigEDeviceInfo.nCurrentIp & 0x000000ff);
                    temp = i.ToString() + ": [GigE] User Define Name : " + stGigEDeviceInfo.chUserDefinedName + ", device IP :" + nIp1 + "." + nIp2 + "." + nIp3 + "." + nIp4 + ", " + stGigEDeviceInfo.chSerialNumber;
                }
                else if (MyCamera.MV_USB_DEVICE == _stDevInfo.nTLayerType)
                {
                    MyCamera.MV_USB3_DEVICE_INFO stUsb3DeviceInfo = (MyCamera.MV_USB3_DEVICE_INFO)MyCamera.ByteToStruct(_stDevInfo.SpecialInfo.stUsb3VInfo, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                    temp = i.ToString() + ": [U3V] User Define Name : " + stUsb3DeviceInfo.chUserDefinedName + ", Serial Number : " + stUsb3DeviceInfo.chSerialNumber;
                }

                cameraNameList.Add(temp);
            }

            return cameraNameList;
        }

        public List<CameraProperty> FindCameras()
        {
            List<CameraProperty> cameraNameList = new List<CameraProperty>();
            int nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref _stDevList);

            for (Int32 i = 0; i < _stDevList.nDeviceNum; i++)
            {
                CameraProperty temp = null;
                _stDevInfo = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(_stDevList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));

                if (MyCamera.MV_GIGE_DEVICE == _stDevInfo.nTLayerType)
                {
                    MyCamera.MV_GIGE_DEVICE_INFO stGigEDeviceInfo = (MyCamera.MV_GIGE_DEVICE_INFO)MyCamera.ByteToStruct(_stDevInfo.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                    uint nIp1 = ((stGigEDeviceInfo.nCurrentIp & 0xff000000) >> 24);
                    uint nIp2 = ((stGigEDeviceInfo.nCurrentIp & 0x00ff0000) >> 16);
                    uint nIp3 = ((stGigEDeviceInfo.nCurrentIp & 0x0000ff00) >> 8);
                    uint nIp4 = (stGigEDeviceInfo.nCurrentIp & 0x000000ff);
                    temp = new CameraProperty();
                    temp.CamName = stGigEDeviceInfo.chUserDefinedName;
                    temp.CamAddress = nIp1 + "." + nIp2 + "." + nIp3 + "." + nIp4;
                    temp.SerialNumber = stGigEDeviceInfo.chSerialNumber;
                }
                else if (MyCamera.MV_USB_DEVICE == _stDevInfo.nTLayerType)
                {
                    MyCamera.MV_USB3_DEVICE_INFO stUsb3DeviceInfo = (MyCamera.MV_USB3_DEVICE_INFO)MyCamera.ByteToStruct(_stDevInfo.SpecialInfo.stUsb3VInfo, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                    temp = new CameraProperty();
                    temp.CamName = stUsb3DeviceInfo.chUserDefinedName;
                    //temp.CamAddress = nIp1 + "." + nIp2 + "." + nIp3 + "." + nIp4;
                    temp.SerialNumber = stUsb3DeviceInfo.chSerialNumber;
                }

                cameraNameList.Add(temp);
            }

            return cameraNameList;
        }


        public eCameraStatus Initialize(List<CameraProperty> camProList, int camMaxCount)
        {
            try
            {
                if (_cameraList.Count != 0)
                {
                    foreach (CameraHik camera in _cameraList)
                    {
                        camera.Terminate();
                    }
                    _cameraList.Clear();
                }

                MaxCamCnt = Machine.config.setup.cameraProp.Count;

                int nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref _stDevList);
                if (nRet != MyCamera.MV_OK)
                    return eCameraStatus.CAM_CONNECTION_FAIL_NOT_FOUND_CAM;

                if (_stDevList.nDeviceNum == 0 || camProList.Count == 0)
                    return eCameraStatus.CAM_CONNECTION_FAIL_NOT_FOUND_CAM;

                for (int i = 0; i < camMaxCount; i++)
                {
                    for (int j = 0; j < _stDevList.nDeviceNum; j++)
                    {
                        MyCamera.MV_CC_DEVICE_INFO stDevInfo = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(_stDevList.pDeviceInfo[j], typeof(MyCamera.MV_CC_DEVICE_INFO));
                        Machine.logger.Write(eLogType.INFORMATION, camProList[i].SerialNumber + " " + camProList[i].CamName + " " + camProList[i].CamAddress);
                        Machine.logger.Write(eLogType.INFORMATION, GetSerialNumber(stDevInfo) + " " + SetCamProperty(stDevInfo).CamName + " " + SetCamProperty(stDevInfo).CamAddress);
                        Console.WriteLine(camProList[i].SerialNumber + " " + camProList[i].CamName + " " + camProList[i].CamAddress);
                        Console.WriteLine(GetSerialNumber(stDevInfo) + " " + SetCamProperty(stDevInfo).CamName + " " + SetCamProperty(stDevInfo).CamAddress);
                        if (camProList[i].SerialNumber == GetSerialNumber(stDevInfo))
                        {
                            camProList[i].CamName = SetCamProperty(stDevInfo).CamName;
                            camProList[i].CamAddress = SetCamProperty(stDevInfo).CamAddress;
                            _cameraList.Add(new CameraHik());
                            eCameraStatus eStatus = _cameraList[i].Initialize(stDevInfo);
                            
                            if (eStatus != eCameraStatus.CAM_CONNECTION_SUCCESS)
                            {
                                _cameraList = new List<CameraHik>();
                                Console.WriteLine("HIK Camera Create Device Failed.");
                                Machine.logger.Write(eLogType.ERROR, "HIK Camera Create Device Failed.");
                                return eCameraStatus.CAM_CONNECTION_FAIL_NOT_FOUND_CAM;
                            }
                            _cameraList[i].SetCamNo(i);
                            _cameraList[i].SetProperty(camProList[i]);
                            _cameraList[i].Initialize(stDevInfo);
                            _cameraList[i].ActiveProperty();

                            break;
                        }
                    }
                }

                if(_cameraList.Count != camMaxCount)
                    return eCameraStatus.CAM_CONNECTION_FAIL_NOT_FOUND_CAM;

                subImage = new Mat[_cameraList.Count];

                return eCameraStatus.CAM_CONNECTION_SUCCESS;
            }
            catch (Exception ex)
            {
                //Machine.ShowErrorInfoDlg("Camera Not Found!", false); // MEER 2024.03.08

                return eCameraStatus.CAM_NOT_FOUND;
            }
        }

        public void SetAcquisitionMode(eAcquisitionMode mode, bool useTriggerMode = false)
        {
            MyCamera.MV_CAM_ACQUISITION_MODE HIKMode = MyCamera.MV_CAM_ACQUISITION_MODE.MV_ACQ_MODE_CONTINUOUS;

            switch (mode)
            {
                case eAcquisitionMode.Single:
                    HIKMode = MyCamera.MV_CAM_ACQUISITION_MODE.MV_ACQ_MODE_SINGLE;
                    break;
                case eAcquisitionMode.Multi:
                    HIKMode = MyCamera.MV_CAM_ACQUISITION_MODE.MV_ACQ_MODE_MUTLI;
                    break;
                case eAcquisitionMode.Continuous:
                    HIKMode = MyCamera.MV_CAM_ACQUISITION_MODE.MV_ACQ_MODE_CONTINUOUS;
                    break;
            }

            foreach (CameraHik camera in _cameraList)
            {
                camera.CameraContext.MV_CC_SetAcquisitionMode_NET((uint)HIKMode);

                if (!useTriggerMode)
                    camera.CameraContext.MV_CC_SetEnumValue_NET("TriggerMode", (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF);
                else
                    camera.CameraContext.MV_CC_SetEnumValue_NET("TriggerMode", (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON);
            }
        }

        public void SetGrabEndDelegate(DelGrabEnd delGrabEnd)
        {
            foreach (CameraHik camera in _cameraList)
            {
                camera.dGrabEnd = delGrabEnd;
            }
        }

        public void SetExpose(int camNo, double expose)
        {
            if (expose <= 0.002 || _cameraList.Count == 0)
                expose = 0.002;

            _cameraList[camNo].SetExpose(expose);
        }

        public string GetIpaddress(MyCamera.MV_CC_DEVICE_INFO stDevInfo)
        {
            if (MyCamera.MV_GIGE_DEVICE == stDevInfo.nTLayerType)
            {
                MyCamera.MV_GIGE_DEVICE_INFO stGigeDeviceInfo = (MyCamera.MV_GIGE_DEVICE_INFO)MyCamera.ByteToStruct(stDevInfo.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                return stGigeDeviceInfo.chModelName + "#" + stGigeDeviceInfo.chSerialNumber; ;
            }
            return "";
        }

        public bool SetProperty(int camNum, CameraProperty _property)
        {
            if (_cameraList != null && _cameraList[camNum].IsOpen() == false)
                return false;

            _cameraList[camNum].SetProperty(_property);
            return _cameraList[camNum].ActiveProperty();
        }

        public object GetProperty(int camNum)
        {
            if (_cameraList != null && _cameraList.Count == 0)
                return null;

            return _cameraList[camNum].GetProperty();
        }

        public bool IsGrabCompleted()
        {
            bool ret = true;

            foreach (CameraHik camera in _cameraList)
            {
                if (!camera.IsGrabCompleted())
                {
                    ret = false;
                    break;
                }
                //ret &= camera.IsGrabCompleted();
            }

            return ret;
        }

        public bool StartGrab()
        {
            bool ret = true;
            _isGrabMode = true;
            Console.WriteLine("Start Grab");
            Machine.logger.Write(eLogType.CAMERA, "Start Grab");
            bool useParallel = false;
            if (!useParallel)
            {
                foreach (CameraHik camera in _cameraList)
                {
                    ret |= camera.StartGrab();
                    Thread.Sleep(1);
                }
            }
            else
            {
                Parallel.ForEach(_cameraList, camera =>
                {
                    ret |= camera.StartGrab();
                    Thread.Sleep(1);
                });
            }
            return ret;
        }

        public bool StopGrab()
        {
            bool ret = true;
            foreach (CameraHik camera in _cameraList)
            {
                ret |= camera.StopGrab();
            }
            _isGrabMode = false;
            return ret;
        }

        public bool StartGrab(int camNo)
        {
            bool ret = true;
            _isGrabMode = true;
            Console.WriteLine("Start Grab");
            Machine.logger.Write(eLogType.CAMERA, "Start Grab");

            ret |= _cameraList[camNo].StartGrab();
            return ret;
        }

        public bool StopGrab(int camNo)
        {
            bool ret = true;
            ret |= _cameraList[camNo].StopGrab();
            _isGrabMode = false;

            return ret;
        }

        public bool IsGrabbing()
        {
            bool ret = true;
            foreach (CameraHik camera in _cameraList)
            {
                bool te = camera.IsGrabbing();
                if (te)
                {
                    ret = true;
                    break;
                }
                ret = false;
            }
            return ret;
        }

        public bool IsGrabbing(int camNo)
        {
            return _cameraList[camNo].IsGrabbing();
        }

        public bool IsOpen()
        {
            bool ret = false;
            foreach (CameraHik camera in _cameraList)
            {
                ret |= camera.IsOpen();
            }
            return ret;
        }

        public bool IsOpen(int camNo)
        {
            if (_cameraList == null)
                return false;
            if (_cameraList.Count == 0)
                return false;
            return _cameraList[camNo].IsOpen();
        }

        public void Terminate()
        {
            if (_cameraList == null)
                return;

            foreach (CameraHik camera in this._cameraList)
            {
                camera.Terminate();
            }
        }

        public bool IsCallBackCompleted()
        {
            bool ret = true;
            foreach (CameraHik camera in _cameraList)
            {
                ret &= camera.IsCallBackCompleted();
            }
            return ret;
        }

        //public Mat GetGrabImage(int camNo, int subNo)
        //{
        //    if (_cameraList.Count == 0)
        //        return null;

        //    CameraHik cameraHik = _cameraList[camNo];

        //    if (cameraHik.GetSubImageList().Count <= subNo)
        //        return null;

        //    return cameraHik.GetSubImageList()[subNo];
        //}

        public Mat GetGrabImage(int camNo)
        {
            if (_cameraList.Count == 0)
                return null;

            CameraHik cameraHik = _cameraList[camNo];

            return cameraHik.GetSubImage();
        }
        

        //public Bitmap GetGrabImage(int camNo, int subNo)
        //{
        //    if (_cameraList.Count == 0)
        //        return null;

        //    CameraHik cameraHik = _cameraList[camNo];

        //    if (cameraHik.GetSubImageList().Count <= subNo)
        //        return null;

        //    return cameraHik.GetSubImageList()[subNo];
        //}

        public void SetSaveMode(bool isSaveMode)
        {
            try
            {
                if (_cameraList.Count == 0) return;

                foreach (CameraHik item in _cameraList)
                {
                    item.IsSaveMode = isSaveMode;
                }
            }
            catch (Exception ex) { }
        }

        public List<CameraHik> GetCameraList()
        {
            return _cameraList;
        }

        //public int GetSubImageCnt(int camNum)
        //{
        //    if (_cameraList == null || _cameraList.Count <= camNum) return 0;

        //    return _cameraList[camNum].GetSubImageCount();
        //}

        //public List<Mat> GetGrabImageList(int camNo)
        //{
        //    if (_cameraList.Count == 0) return null;

        //    return _cameraList[camNo].GetSubImageList();
        //}

        


        //public List<Bitmap> GetGrabImageList(int camNo)
        //{
        //    if (_cameraList.Count == 0) return null;

        //    return _cameraList[camNo].GetSubImageList();
        //}

        public int GetConnectedCount()
        {
            if (_cameraList.Count == 0)
                return 0;
            return _cameraList.Count;

        }

        public double GetExpose(int camNo)
        {
            if (_cameraList.Count == 0)
                return 0;

            return _cameraList[camNo].GetExpose();
        }

        public void SetUIDelegate(int camNum)
        {
            if (_cameraList.Count == 0 || _cameraList[camNum].IsOpen() == false)
                return;

            _cameraList[camNum].UIDelegate();
        }

        /*

        public bool MakeCamImage()
        {
            bool ret = true;
                        
            for (int i = 0; i < _cameraList.Count; i++)
            {
                if(Machine.config.mergeProp.SubImageoffsetArray.Count >= Machine._cameraManager.GetSubImageCnt(i))
                {
                    Console.WriteLine("Merge Param Error :  CamNo : " + i.ToString() + "SubImage Count : " + Machine._cameraManager.GetSubImageCnt(i).ToString() + "MergeProp SubImageOffsetArray Cnt : " + Machine.mCfg.mSetup.mergeProp.SubImageoffsetArray.Count.ToString());
                    ret = false;                    
                }

                if (Machine.mCfg.mSetup.mergeProp.IsCamPlusDirection == true)
                    ret = MakePlusDirectionMergeCamImage(i);
                else
                    ret =  MakeMinusDirectionMergeCamImage(i);

            }

            return ret;
        }

        public bool MakeTotalImage()
        {
            if (Machine.mCfg.mSetup.mergeProp.IsCamPlusDirection == true)
                return MakePlusDirectionMergeTotalImage();
            else
                return MakeMinusDirectionMergeTotalImage();
        }
        */
        private int GetWidth(int camIdx)
        {
            int value = 0;

            for (int i = 0; i < camIdx; i++)
            {
                value += subImage[i].Width;
            }

            return value;
        }

        private int GetMaxValue(int[] valueArray)
        {
            int max = 0;

            foreach (int item in valueArray)
            {
                if (max < item)
                    max = item;
            }

            return max;
        }

        //public void ClearSubImageList()
        //{
        //    if (_cameraList == null) return;

        //    foreach (CameraHik camera in _cameraList)
        //    {
        //        camera.ClearSubImageList();
        //    }
        //}
        /*
        public void SetMaxCamCnt(int count)
        {
            MaxCamCnt = count;
        }

        public int GetMaxCamCnt()
        {
            return MaxCamCnt;
        }

        public Mat GetMergeImage()
        {
            return mergeMat;
        }

        public Mat[] GetSubCamImage()
        {
            return subImage;
        }
        */
        private CameraProperty SetCamProperty(MyCamera.MV_CC_DEVICE_INFO stDevInfo)
        {
            MyCamera.MV_GIGE_DEVICE_INFO stGigEDeviceInfo = (MyCamera.MV_GIGE_DEVICE_INFO)MyCamera.ByteToStruct(stDevInfo.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO));

            CameraProperty camPropety = new CameraProperty();
            camPropety.CamName = stGigEDeviceInfo.chModelName + "#" + stGigEDeviceInfo.chSerialNumber;
            uint nIp1 = ((stGigEDeviceInfo.nCurrentIp & 0xff000000) >> 24);
            uint nIp2 = ((stGigEDeviceInfo.nCurrentIp & 0x00ff0000) >> 16);
            uint nIp3 = ((stGigEDeviceInfo.nCurrentIp & 0x0000ff00) >> 8);
            uint nIp4 = (stGigEDeviceInfo.nCurrentIp & 0x000000ff);
            camPropety.CamAddress = nIp1 + "." + nIp2 + "." + nIp3 + "." + nIp4;
            return camPropety;
        }

        private string GetSerialNumber(MyCamera.MV_CC_DEVICE_INFO stDevInfo)
        {
            if (MyCamera.MV_GIGE_DEVICE == stDevInfo.nTLayerType)
            {
                MyCamera.MV_GIGE_DEVICE_INFO stGigeDeviceInfo = (MyCamera.MV_GIGE_DEVICE_INFO)MyCamera.ByteToStruct(stDevInfo.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                return stGigeDeviceInfo.chSerialNumber;
            }
            return "";
        }
        /*
        public void SetSubCamImage()
        {
            if (subImage == null || subImage.Length == 0) return;

            for (int camNo = 0; camNo < subImage.Length; camNo++)
            {
                int subMax = _cameraList[camNo].GetSubImageCount();

                if (subMax == 0) return;

                Bitmap bmp = GetGrabImage(camNo, subMax - 1);
                subImage[camNo] = OpenCvSharp.Extensions.BitmapConverter.ToMat(bmp);
            }
        }

        public bool MakePlusDirectionMergeCamImage(int camNo)
        {
            try
            {
                if (subImage[camNo] != null)
                {
                    subImage[camNo].Dispose();
                    subImage[camNo] = null;
                }

                List<Bitmap> _subImageList = _cameraList[camNo].GetSubImageList();

                int width = _subImageList[camNo].Width;
                int height = _subImageList[camNo].Height;

                if (mergeMat != null) mergeMat.Dispose();

                MergeProperty prop = Machine.mCfg.mSetup.mergeProp;

                if (prop.CamImageOffsetArray.Length == 0)
                    subImage[camNo] = OpenCvSharp.Extensions.BitmapConverter.ToMat(_subImageList[0])[new OpenCvSharp.Rect(0, 0, width, height)];
                else
                    subImage[camNo] = OpenCvSharp.Extensions.BitmapConverter.ToMat(_subImageList[0])[new OpenCvSharp.Rect(0, 0, width, height - prop.SubImageoffsetArray[0])];

                int subNum = 1;

                for (int propNum = 1; propNum < prop.SubImageoffsetArray.Count; propNum++)
                {
                    Mat mat = OpenCvSharp.Extensions.BitmapConverter.ToMat(_subImageList[subNum])[new OpenCvSharp.Rect(0, 0, width, height - prop.SubImageoffsetArray[propNum])];
                    subImage[camNo].PushBack(mat);
                    subNum++;
                }

                for (; subNum < _subImageList.Count; subNum++)
                {
                    subImage[camNo].PushBack(OpenCvSharp.Extensions.BitmapConverter.ToMat(_subImageList[_subImageList.Count - 1]));
                }

                if (UpdateMergeImg != null)
                {
                    UpdateMergeImg(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(subImage[camNo]), camNo);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool MakeMinusDirectionMergeCamImage(int camNo)
        {
            try
            {
                if (subImage[camNo] != null)
                {
                    subImage[camNo].Dispose();
                    subImage[camNo] = null;
                }

                List<Bitmap> _subImageList = _cameraList[camNo].GetSubImageList();

                int width = _subImageList[camNo].Width;
                int height = _subImageList[camNo].Height;

                if (mergeMat != null) mergeMat.Dispose();

                MergeProperty prop = Machine.mCfg.mSetup.mergeProp;

                subImage[camNo] = OpenCvSharp.Extensions.BitmapConverter.ToMat(_subImageList[_subImageList.Count - 1]);

                int subNum = _subImageList.Count - 2;

                for (; subNum > prop.SubImageoffsetArray.Count - 1; subNum--)
                {
                    subImage[camNo].PushBack(OpenCvSharp.Extensions.BitmapConverter.ToMat(_subImageList[subNum]));
                }

                for (int propNum = prop.SubImageoffsetArray.Count - 1; propNum > -1; propNum--)
                {
                    Mat mat = OpenCvSharp.Extensions.BitmapConverter.ToMat(_subImageList[subNum])[new OpenCvSharp.Rect(0, prop.SubImageoffsetArray[propNum], width, height - prop.SubImageoffsetArray[propNum])];
                    subImage[camNo].PushBack(mat);
                    subNum--;
                }

                if (UpdateMergeImg != null)
                {
                    UpdateMergeImg(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(subImage[camNo]), camNo);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool MakePlusDirectionMergeTotalImage()
        {
            try
            {
                if (mergeMat != null)
                {
                    mergeMat.Dispose();
                    mergeMat = null;
                }

                foreach(Mat item in subImage)
                {
                    if (item == null) return false;
                }

                int mergeWidth = 0;
                for (int camNum = 0; camNum < _cameraList.Count; camNum++)
                {
                    mergeWidth += subImage[camNum].Width;
                }

                int maxoffset = GetMaxValue(Machine.mCfg.mSetup.mergeProp.CamImageOffsetArray);

                mergeMat = new Mat(new OpenCvSharp.Size(mergeWidth, subImage[0].Height - maxoffset), MatType.CV_8UC1);


                for (int camNum = 0; camNum < _cameraList.Count; camNum++)
                {
                    mergeMat[new OpenCvSharp.Rect(GetWidth(camNum), 0, subImage[camNum].Width, subImage[camNum].Height - maxoffset)] = subImage[camNum][new OpenCvSharp.Rect(0, Machine.mCfg.mSetup.mergeProp.CamImageOffsetArray[camNum],
                        subImage[camNum].Width, subImage[camNum].Height - maxoffset)];
                }

                if (UpdateMergeImg != null)
                    UpdateMergeImg(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mergeMat), 0);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool MakeMinusDirectionMergeTotalImage()
        {
            try
            {
                if (mergeMat != null)
                {
                    mergeMat.Dispose();
                    mergeMat = null;
                }

                foreach (Mat item in subImage)
                {
                    if (item == null) return false;
                }

                int mergeWidth = 0;
                for (int camNum = 0; camNum < 3; camNum++)
                {
                    mergeWidth += subImage[camNum].Width;
                }

                int maxoffset = GetMaxValue(Machine.mCfg.mSetup.mergeProp.CamImageOffsetArray);

                mergeMat = new Mat(new OpenCvSharp.Size(mergeWidth, subImage[0].Height - maxoffset), MatType.CV_8UC1);


                for (int camNum = 0; camNum < 3; camNum++)
                {
                    mergeMat[new OpenCvSharp.Rect(GetWidth(camNum), 0, subImage[camNum].Width, subImage[camNum].Height - maxoffset)] =
                        subImage[camNum][new OpenCvSharp.Rect(0, maxoffset - Machine.mCfg.mSetup.mergeProp.CamImageOffsetArray[camNum], subImage[camNum].Width, subImage[camNum].Height - maxoffset)];
                }

                if (UpdateMergeImg != null)
                {
                    UpdateMergeImg(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mergeMat), 0);
                    UpdateMergeImg(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mergeMat), 2);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void AddErrMessageDelegate(DelSendErrMsg sendErrMsgDel)
        {
            if(_cameraList != null)
            {
                foreach(CameraHik item in _cameraList)
                {
                    item.sendCamErrMsg += sendErrMsgDel;
                }
            }
        }
        */
    }
}
