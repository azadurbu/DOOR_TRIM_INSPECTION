using DOOR_TRIM_INSPECTION.Class;
using DOOR_TRIM_INSPECTION.Controls;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DOOR_TRIM_INSPECTION
{
    public class Sequence
    {
        //// 20241114 jsKim Add s
        private eSeqStep _seqStep = new eSeqStep();
        private bool _isStop = false;
        //// 20241114 jsKim Add e

        private Thread _seqThread = null;
        private DateTime _start;
        private DateTime _end;
        private TimeSpan _duration;
        //private int _resultClearTime = 500;
        private DOOR ProductType = DOOR.FRONT_RIGHT;
        private INSPECTION_RESULT _inspResultFirst = INSPECTION_RESULT.NOT_FOUND;
        private INSPECTION_RESULT _inspResultSecond = INSPECTION_RESULT.NOT_FOUND;
        private string _barcode = "";

        private DateTime _barcodeReadTime;   // hhMMss

        public DateTime BarcodeReadTime { get { return _barcodeReadTime; } set { _barcodeReadTime = value; } }

        private Mat MergeImageFirst;
        private Mat MergeImageSecond;
        private Mat[] SubImageFirst;
        private Mat[] subImageSecond;

        private Inspection inspection;
        private Recipe CurrentRecipe;

        string RearImagePath = "";
        string FrontImagePath = "";

        public eSeqStep SeqStep { get => _seqStep; set => _seqStep = value; }
        //// 20241114 jsKim Add e

        public void SetSeqStep(eSeqStep seqStep)
        {
            string msg = _seqStep.ToString() + "->" + seqStep.ToString();
            _seqStep = seqStep;
            Machine.logger.Write(eLogType.SEQ, msg);
        }
        public void StartSequence()
        {
            //// 20241114 jsKim Add s
            _isStop = false;
            //Thread.Sleep(1000);
            if (_seqThread == null)
            {
                _seqThread = new Thread(ThreadFunc);
                _seqThread.Start();
            }
            Machine.ProgramMode = eProgramMode.Inspection;
            //// 20241114 jsKim Add e
        }

        public void StopSequencs()
        {
            _isStop = true;
            //Thread.Sleep(1000);
            if (_seqThread != null)
            {
                _seqThread.Join();
                _seqThread = null;
            }
            Machine.ProgramMode = eProgramMode.Stop;
        }

        private void ThreadFunc()
        {
            //// 20241114 jsKim Add s
            _seqStep = eSeqStep.SEQ_START;
            while (!_isStop)
            {
                if (Machine.ProgramMode == eProgramMode.Inspection|| Machine.ProgramMode == eProgramMode.Test)
                {
                    try { SeqSteps(); }
                    catch (Exception e)
                    {
                        Machine.logger.Write(eLogType.ERROR, e.ToString());
                        Machine.logger.Write(eLogType.SEQ, "Error Sequence. End ThreadFunc.");
                        //FormMain.Instance().LogDisplayControl.AddLog("Error! Sequence.");
                    }
                }
                else
                {
                    _isStop = true;
                }
                Thread.Sleep(5);
            }
            //_subSequence = null;
            Machine.logger.Write(eLogType.SEQ, "End ThreadFunc.");
            //// 20241114 jsKim Add e
        }

        private void CancelOperation()
        {
            if (Machine.InspectionMode != INSPECTION_MODE.MASTER)
            {
                Task.Run(() =>
                  {
                      mainWindow.Dispatcher.Invoke(() =>
                      {
                          mainWindow.ShowWrongBarcode();
                      });

                  });
            }
        }
        
        private void SeqSteps()
        {
            switch (_seqStep)
            {
                case eSeqStep.SEQ_STOP:
                    Machine.logger.Write(eLogType.SEQ, "Sequence Stop.");
                    _isStop = true;

                    break;
                case eSeqStep.SEQ_START:
                    if (!Start())
                    {
                        Machine.logger.Write(eLogType.SEQ, "Barcode fail.");
                    }
                    SendDataClear();
                    Machine.Light_Comm.LightOnOffEN(false, 1, 0);
                    //Machine.Light_Comm.LightOnOffEN(true, 1, 1600);
                    SetSeqStep(eSeqStep.SEQ_READ_BARCODE);
                    break;
                case eSeqStep.SEQ_READ_BARCODE:
                    
                    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                    _barcode = Machine.BarcodeScan.Barcode;
                    if (_barcode == null)
                        break;
                    if (_barcode.Length == 0)
                        break;
                    try
                    {
                        if (_barcode.Length != 17)
                        {
                            CancelOperation();
                            break;
                        }
                        Machine.BarcodeData = new BarCodeHelper(_barcode);
                        //Machine.DIO_Comm.StartCycleTime();   //
                    }
                    catch (Exception ex)
                    {
                        CancelOperation();
                        break;
                    }
                    SendDataClear();

                    Machine.DIO_Comm.SendSetStartMove();        // MOVE STAGE
                    _start = DateTime.Now;
                    _barcodeReadTime = DateTime.Now;
                   
                    SetRecipe();
                    FrontImagePath = "";
                    RearImagePath = "";
                    Machine.Light_Comm.LightOnOffEN(true, 1, Machine.config.setup.lightProp.Level);
                    if (Machine.InspectionMode != INSPECTION_MODE.MASTER)
                    { 
                        Task.Run(() =>
                        {
                            mainWindow.Dispatcher.Invoke(() =>
                            {
                                mainWindow.ClearCanvasSetBarcode(_barcode);
                            });
                        });
                    }
                    SetSeqStep(eSeqStep.SEQ_WAIT_SGINAL_FIRST);
                    break;
                #region First Insp
                case eSeqStep.SEQ_WAIT_SGINAL_FIRST:
                    _end = DateTime.Now;
                    _duration = _end - _start;
                    if (_duration.TotalMilliseconds > 500)
                        Machine.DIO_Comm.SendResetStartMove();
                    if (_duration.TotalMilliseconds > 15000)
                    {
                        Machine.logger.Write(eLogType.ERROR, "SEQ_WAIT_SGINAL_FIRST : 5sec Timeout");
                        SetSeqStep(eSeqStep.SEQ_START);
                        break;
                    }

                    if (Machine.ProgramMode == eProgramMode.Test)
                        SetSeqStep(eSeqStep.SEQ_GRAB_FIRST);
                    if (Machine.SetSecondGrab)
                    {
                        Machine.SetSecondGrab = false;
                        SetSeqStep(eSeqStep.SEQ_WAIT_SGINAL_SECOND);
                        break;
                    }
                    if (GetCaptureStart())
                        SetSeqStep(eSeqStep.SEQ_GRAB_FIRST);
                    break;
                case eSeqStep.SEQ_GRAB_FIRST:
                    //_barcode = GetBarcode();
                    ////LightOnOff(true);
                    //Thread.Sleep(500);
                    // Start Grab
                    Machine.DIO_Comm.SendResetStartMove();
                    StartGrab();

                    // Start Grab Timer
                    _start = DateTime.Now;
                    SetSeqStep(eSeqStep.SEQ_WAIT_GRAB_END_FIRST);
                    break;
                case eSeqStep.SEQ_WAIT_GRAB_END_FIRST:
                    // Check Timeout
                    _end = DateTime.Now;
                    _duration = _end - _start;
                    if (_duration.TotalMilliseconds > Machine.config.setup.TimeoutGrab)
                    {
                        _inspResultFirst = INSPECTION_RESULT.NG;
                        SetSeqStep(eSeqStep.SEQ_SEND_RESULT_FIRST);
                        break;
                    }

                    if(Machine.ProgramMode == eProgramMode.Test)
                        SetSeqStep(eSeqStep.SEQ_STOP_GRAB_FIRST);
                    // Wait Grab End
                    if (Machine.camManager.IsGrabbing() == false)
                        SetSeqStep(eSeqStep.SEQ_STOP_GRAB_FIRST);
                    break;
                case eSeqStep.SEQ_STOP_GRAB_FIRST:
                    SetRecipe();
                    StopGrab();
                    Thread.Sleep(500);
                    SetSeqStep(eSeqStep.SEQ_INSP_FIRST);
                    break;
                case eSeqStep.SEQ_INSP_FIRST:
                    //Merge Image
//#if USE_EXTRA_CAM
//                    Mat[] Images = new Mat[4];
//#else
                    Mat[] Images = new Mat[3];
//#endif
                    //Images[0] = Machine.camManager.GetGrabImage(0);
                    //Images[1] = Machine.camManager.GetGrabImage(1);
                    //Images[2] = Machine.camManager.GetGrabImage(2);


                    //Cv2.Transpose(Images[0], Images[0]);
                    //Cv2.Flip(Images[0], Images[0], FlipMode.Y);
                    ////Cv2.Transpose(Images[1], Images[1]);
                    ////Cv2.Flip(Images[1], Images[1], FlipMode.Y);
                    //Cv2.Transpose(Images[2], Images[2]);
                    //Cv2.Flip(Images[2], Images[2], FlipMode.Y);
                    Images = GetCameraImg(false);

//#if USE_EXTRA_CAM
//                    SubImageFirst = new Mat[4];
//#else
                    SubImageFirst = new Mat[3];
//#endif                    

                    SubImageFirst[0] = Images[0].Clone();
                    SubImageFirst[1] = Images[1].Clone();
                    SubImageFirst[2] = Images[2].Clone();
//#if USE_EXTRA_CAM
//                    SubImageFirst[3] = Images[3].Clone();
//#endif
                    if (!CheckLightLevel(SubImageFirst[1]))
                    {
                        SetSeqStep(eSeqStep.SEQ_RESET_LIGHT_FIRST);
                        break;
                    }

                    //MergeImageFirst = MergeImagesStitcher(Images);
                    MergeImageFirst = MergeImagesMat(Images, true);
                    inspection = new Inspection(CurrentRecipe);
                    // Start Inspetion
//#if USE_EXTRA_CAM
                    InspectionFirstStart(MergeImageFirst, SubImageFirst[1], SubImageFirst[2]);
//#else
//                    InspectionFirstStart(MergeImageFirst, SubImageFirst[1]);
//#endif
                    // Start Insp Timer
                    _start = DateTime.Now;
                    SetSeqStep(eSeqStep.SEQ_WAIT_INSP_END_FIRST);
                    break;
                case eSeqStep.SEQ_RESET_LIGHT_FIRST:
                    Machine.DIO_Comm.SendSetLightRest(true);
                    Thread.Sleep(Machine.config.setup.ResetLightTime);
                    Machine.DIO_Comm.SendSetLightRest(false);

                    Machine.Light_Comm.LightOnOffEN(true, 1, Machine.config.setup.lightProp.Level);
                    SetSeqStep(eSeqStep.SEQ_GRAB_FIRST);                    
                    break;
                case eSeqStep.SEQ_WAIT_INSP_END_FIRST:
                    GetFristInspectionResult();
                    SetSeqStep(eSeqStep.SEQ_SEND_RESULT_FIRST);
                    break;
                case eSeqStep.SEQ_SEND_RESULT_FIRST:
                    SendResult(_inspResultFirst);
                    SetSeqStep(eSeqStep.SEQ_SAVE_IMAGE_FIRST);
                    break;
                case eSeqStep.SEQ_SAVE_IMAGE_FIRST:
                    Thread.Sleep(Machine.config.setup.TimeoutDataClear);
                    Machine.DIO_Comm.SendResetResult();
                    SaveImage(true);
                    Machine.StopSecondGrab = false;
                    SetSeqStep(eSeqStep.SEQ_WAIT_OFF_CAPTURE_SIGNAL);
                    break;
#endregion

                case eSeqStep.SEQ_WAIT_OFF_CAPTURE_SIGNAL:
                    _end = DateTime.Now;
                    _duration = _end - _start;
                    if (_duration.TotalMilliseconds > Machine.config.setup.TimeoutGrab)
                        SetSeqStep(eSeqStep.SEQ_WAIT_SGINAL_SECOND);
                    if (GetCaptureStart() == false)
                        SetSeqStep(eSeqStep.SEQ_WAIT_SGINAL_SECOND);
                    break;

#region Second Insp
                case eSeqStep.SEQ_WAIT_SGINAL_SECOND:
                    if (Machine.StopSecondGrab)
                    {
                        Machine.StopSecondGrab = false;
                        SetSeqStep(eSeqStep.SEQ_START);
                        break;
                    }
                    if (Machine.ProgramMode == eProgramMode.Test)
                        SetSeqStep(eSeqStep.SEQ_GRAB_SECOND);
                    if (GetCaptureStart())
                        SetSeqStep(eSeqStep.SEQ_GRAB_SECOND);
                    break;
                case eSeqStep.SEQ_GRAB_SECOND:
                    _barcode = GetBarcode();
                    //LightOnOff(false);
                    Thread.Sleep(500);
                    // Start Grab
                    StartGrab();
                    // Start Grab Timer
                    _start = DateTime.Now;
                    SetSeqStep(eSeqStep.SEQ_WAIT_GRAB_END_SECOND);
                    break;
                case eSeqStep.SEQ_WAIT_GRAB_END_SECOND:
                    // Check Timeout
                    _end = DateTime.Now;
                    _duration = _end - _start;
                    if (_duration.TotalMilliseconds > Machine.config.setup.TimeoutGrab)
                    {
                        _inspResultSecond = INSPECTION_RESULT.NG;
                        SetSeqStep(eSeqStep.SEQ_SEND_RESULT_SECOND);
                        break;
                    }
                    // Wait Grab End
                    if (Machine.camManager.IsGrabbing() == false)
                        SetSeqStep(eSeqStep.SEQ_STOP_GRAB_SECOND);
                    break;
                case eSeqStep.SEQ_STOP_GRAB_SECOND:
                    SetRecipe();
                    StopGrab();
                    Thread.Sleep(500);
                    SetSeqStep(eSeqStep.SEQ_INSP_SECOND);
                    break;
                case eSeqStep.SEQ_INSP_SECOND:
                    //Merge Image
//#if USE_EXTRA_CAM
//                    Images = new Mat[4];
//#else
                    Images = new Mat[3];
//#endif
                    //Images[0] = Machine.camManager.GetGrabImage(0);
                    //Images[1] = Machine.camManager.GetGrabImage(1);
                    //Images[2] = Machine.camManager.GetGrabImage(2);


                    //Cv2.Transpose(Images[0], Images[0]);
                    //Cv2.Flip(Images[0], Images[0], FlipMode.Y);
                    ////Cv2.Transpose(Images[1], Images[1]);
                    ////Cv2.Flip(Images[1], Images[1], FlipMode.Y);
                    //Cv2.Transpose(Images[2], Images[2]);
                    //Cv2.Flip(Images[2], Images[2], FlipMode.Y);
                    Images = GetCameraImg(true);

                    subImageSecond = new Mat[3];

                    subImageSecond[0] = Images[0].Clone();
                    subImageSecond[1] = Images[1].Clone();
                    subImageSecond[2] = Images[2].Clone();

                    if (!CheckLightLevel(SubImageFirst[1]))
                    {
                        SetSeqStep(eSeqStep.SEQ_RESET_LIGHT_SECOND);
                        break;
                    }

                    //MergeImageSecond = MergeImagesStitcher(Images);
                    MergeImageSecond = MergeImagesMat(Images);

                    // Start Inspetion
                    InspectionSecondStart(MergeImageSecond);

                    // Start Insp Timer
                    _start = DateTime.Now;
                    SetSeqStep(eSeqStep.SEQ_WAIT_INSP_END_SECOND);
                    break;
                case eSeqStep.SEQ_RESET_LIGHT_SECOND:
                    Machine.DIO_Comm.SendSetLightRest(true);
                    Thread.Sleep(Machine.config.setup.ResetLightTime);
                    Machine.DIO_Comm.SendSetLightRest(false);

                    Machine.Light_Comm.LightOnOffEN(true, 1, Machine.config.setup.lightProp.Level);
                    SetSeqStep(eSeqStep.SEQ_GRAB_SECOND);
                    break;
                case eSeqStep.SEQ_WAIT_INSP_END_SECOND:
                        GetSecondInspectionResult();
                        SetSeqStep(eSeqStep.SEQ_SEND_RESULT_SECOND);
                    break;
                case eSeqStep.SEQ_SEND_RESULT_SECOND:
                    SendResult(_inspResultSecond);
                    SetSeqStep(eSeqStep.SEQ_SAVE_IMAGE_SECOND);
                    //LightOnOff(false);
                    break;
                case eSeqStep.SEQ_SAVE_IMAGE_SECOND:
                    Thread.Sleep(Machine.config.setup.TimeoutDataClear);
                    SendDataClear();
                    SaveImage(false);
                    SetSeqStep(eSeqStep.SEQ_END);
                    break;
#endregion

                case eSeqStep.SEQ_END:
                    if (Machine.InspectionMode == INSPECTION_MODE.MASTER)
                    {
                        Task.Run(() =>
                        {
                            masterModeInspectionWindow.Dispatcher.Invoke(() =>
                            {
                                masterModeInspectionWindow.UpdateUI(inspection, _barcode, BarcodeReadTime, RearImagePath, FrontImagePath);
                            });
                        });
                    }
                    else
                    {
                        Task.Run(() =>
                        {
                            mainWindow.Dispatcher.Invoke(() =>
                            {
                                mainWindow.UpdateUI(inspection, _barcode, BarcodeReadTime, RearImagePath, FrontImagePath);
                            });
                        });
                    }
                    UpdateCycleTime(); //Process complete time 250909
                    Machine.ProgramMode = eProgramMode.Inspection;
                    SetSeqStep(eSeqStep.SEQ_START);
                    break;
                case eSeqStep.SEQ_ERROR:
                    Machine.ProgramMode = eProgramMode.Inspection;
                    break;
            }
        }

        private void UpdateCycleTime()
        {
            DateTime now = Machine.DIO_Comm.GetSampleOutTime();
            double elapsed = (now - BarcodeReadTime).TotalSeconds;
            string OriginalBarcode = Machine.BarcodeData.Barcode;
            if (Machine.InspectionMode == INSPECTION_MODE.MASTER)
            {
                string BarCodeWithDateTime = $"{OriginalBarcode}_{BarcodeReadTime.ToString("yyMMddHHmmss")}";
                Machine.BarcodeData.Barcode = BarCodeWithDateTime;
            }
            string elapsedTime = $"{elapsed:F1}";
            Machine.dyDBHelper.UpdateCycleTime(Machine.BarcodeData, elapsedTime);
            if (Machine.InspectionMode == INSPECTION_MODE.MASTER)
                Machine.BarcodeData.Barcode = OriginalBarcode;
        }

        public bool CheckLightLevel(Mat sub1image)
        {
            // 이미지를 그레이스케일로 변환
            Mat grayImage = new Mat();
            if (sub1image.Channels() != 1)
                Cv2.CvtColor(sub1image, grayImage, ColorConversionCodes.BGR2GRAY);
            else
                grayImage = sub1image.Clone();

            // 그레이스케일 이미지의 평균값 계산
            Scalar meanScalar = Cv2.Mean(grayImage);
            if(meanScalar.Val0 < Machine.config.setup.CheckLightLevel)
            {
                Machine.logger.Write(eLogType.ERROR, "CheckLightLevel : " + meanScalar.Val0 + " < " + Machine.config.setup.CheckLightLevel);
                Machine.logger.Write(eLogType.SEQ, "CheckLightLevel : " + meanScalar.Val0 + " < " + Machine.config.setup.CheckLightLevel);
                // 밝기가 낮은 경우
                return false;
            }
            else
            {
                // 밝기가 적절한 경우
                return true;
            }
        }

        public Mat[] GetCameraImg(bool isFront=false)
        {
//#if USE_EXTRA_CAM
//            Mat[] images = new Mat[4];
//#else
            Mat[] images = new Mat[3];
//#endif
            if (Machine.ProgramMode == eProgramMode.Test)
            {
                if(isFront)
                {
                    images[0] = Machine.LoadImagesFront[0].Clone();
                    images[1] = Machine.LoadImagesFront[1].Clone();
                    images[2] = Machine.LoadImagesFront[2].Clone();
                }
                else
                {
                    images[0] = Machine.LoadImagesRear[0].Clone();
                    images[1] = Machine.LoadImagesRear[1].Clone();
                    images[2] = Machine.LoadImagesRear[2].Clone();
                }
            }
            else
            {
                images[0] = Machine.camManager.GetGrabImage(0);
                images[1] = Machine.camManager.GetGrabImage(1);
                images[2] = Machine.camManager.GetGrabImage(2);
//#if USE_EXTRA_CAM
//                images[3] = Machine.camManager.GetGrabImage(3);
//#endif
            }

            Cv2.Transpose(images[0], images[0]);
            Cv2.Flip(images[0], images[0], FlipMode.Y);
            Cv2.Transpose(images[1], images[1]);
            Cv2.Flip(images[1], images[1], FlipMode.Y);

            return images;
        }

        private bool Start()
        {
            bool r = true;

            try
            {
                // Clear Bacode data
                //if (Machine.BarcodeScan == null)
                //{
                //    Machine.logger.Write(eLogType.ERROR, "BarcodeScan is NULL.");
                //    r = false;
                //}
                //else
                {
                    Machine.BarcodeScan.ClearBarcodeData();

                    _inspResultFirst = INSPECTION_RESULT.NOT_FOUND;
                    _inspResultSecond = INSPECTION_RESULT.NOT_FOUND;
                }
            }
            catch (Exception ex)
            {
                r = false;
            }

            return r;
        }


        private string GetBarcode()
        {
            string data = null;
            data = Machine.BarcodeScan.Barcode;
            DateTime now = DateTime.Now;

            string empty = "No_" + now.ToString("yyMMddHHmmss");
            if (data == null) return empty;
            if (data.Length == 0) return empty;
            return data;
        }

        private bool GetCaptureStart()
        {
            bool result = false;
#if PLC_TEST
            result = Machine.Test_Start;
            if(result)
                Machine.Test_Start=false;
#else
            result = Machine.DIO_Comm.GetCaptureStart();
#endif
            return result;
        }

        private bool LightOnOff(bool bOn)
        {
#if LIGHT_TEST
            bool result = true;
#else
            try
            {
                int channel = Machine.config.setup.lightProp.Channel;
                int value = Machine.config.setup.lightProp.Level;
                Machine.Light_Comm.LightOnOffEN(bOn, channel, value);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
#endif
        }

        private bool StartGrab()
        {
            return Machine.camManager.StartGrab();
        }


        private bool StopGrab()
        {
            return Machine.camManager.StopGrab();
        }
        private bool SaveImage(bool isFirst)
        {
            bool r = true;

            try
            {
                int MaxCamCount = Machine.config.setup.maxCamCount;
                string preName = Machine.config.setup.ImagePath + "\\";
                bool isFornt = !isFirst;
                if (isFirst)
                    SaveMergedImage(_barcode, MergeImageFirst, SubImageFirst, isFornt);
                else
                    SaveMergedImage(_barcode, MergeImageSecond, subImageSecond, isFornt);

                for (int i = 0; i < MaxCamCount; i++)
                {
                    Mat images = Machine.camManager.GetGrabImage(i);

                    images.ImWrite(preName + i + ".bmp");
                    images.Dispose();
                    images = null;
                }
            }
            catch (Exception e)
            {
                string err = e.ToString();
                Machine.logger.Write(eLogType.ERROR, err);
                Machine.logger.Write(eLogType.SEQ, "SaveImage Error");
            }

            return r;
        }


        //        private void GetProductType()
        //        {
        //#if PLC_TEST
        //            ProductType = Machine.Test_ProductType;
        //#else
        //            ProductType = Machine.DIO_Comm.GetProductType();
        //#endif
        //        }

        // ========================================================
        private MainWindow mainWindow;
        private Form.MasterModeInspectionWindow masterModeInspectionWindow;
        public Sequence(MainWindow _mainWindow)
        {
            mainWindow = _mainWindow;
        }

        public Sequence(Form.MasterModeInspectionWindow _masterModeInspectionWindow)
        {
            masterModeInspectionWindow = _masterModeInspectionWindow;
        }

        //public void Test()
        //{

        //    //images
        //    string tempImagesFolderPath = @"./images";
        //    SetRecipe();
        //    Inspection inspection = new Inspection(CurrentRecipe);
        //    // MOVEIN
        //    // GRAB FINISHED
        //    // Get all rear images 
        //    DirectoryInfo di = new DirectoryInfo(tempImagesFolderPath);
        //    string[] imagePaths_r = Directory.GetFiles(tempImagesFolderPath, "back*.bmp");
        //    string RearImagePath = MergeImages("Temp", imagePaths_r, 2076, false);
        //    inspection.SetRearInspectionImage(RearImagePath);
        //    inspection.ExecuteRearInspection();
        //    //ROTATE
        //    // GRAB FINISHED
        //    string[] imagePaths_f = Directory.GetFiles(tempImagesFolderPath, "front*.bmp");
        //    string FrontImagePath = MergeImages("Temp", imagePaths_f, 50, true);
        //    inspection.SetFrontInspectionImage(FrontImagePath);
        //    inspection.ExecuteFrontInspection();

        //    // show main window
        //    Task.Run(() =>
        //    {
        //        mainWindow.Dispatcher.Invoke(() => {
        //            mainWindow.UpdateUI(inspection, BarcodeReadTime, RearImagePath, FrontImagePath);
        //        });
        //    });
        //    //return inspection;
        //}


        private string MergeImages(string doorTrimID, string[] imagePaths, int overlap, bool IsFront)
        {
            try
            {
                Mat[] images = new Mat[3];
                images[0] = Cv2.ImRead(imagePaths[0]);
                images[1] = Cv2.ImRead(imagePaths[1]);
                Mat result = MergeImagesMat(images);
                Bitmap mergedImage = BitmapConverter.ToBitmap(result);
                //// save the merged image
                return SaveMergedImage(doorTrimID, mergedImage, IsFront);

            }
            catch (Exception ex)
            {
                return "";
            }
        }
        private string SaveMergedImage(string doorTrimID, Mat mergedImage, Mat[] subimage, bool IsFront)
        {
            string imageSaveFolder = Machine.config.setup.ImagePath;

            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            string Month = _barcodeReadTime.ToString("MM");
            string Date = _barcodeReadTime.ToString("dd");

            imageSaveFolder = System.IO.Path.Combine(imageSaveFolder, Month);
            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            imageSaveFolder = System.IO.Path.Combine(imageSaveFolder, Date);
            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            imageSaveFolder = System.IO.Path.Combine(imageSaveFolder, doorTrimID + "_" + _barcodeReadTime.ToString("HHmmss"));
            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            string filePath = System.IO.Path.Combine(imageSaveFolder, IsFront ? "front" : "rear");
            filePath += "_" + _barcodeReadTime.ToString("HHmmss") + ".bmp";
            string subfilePath = filePath + "_" + _barcodeReadTime.ToString("HHmmss") + "_sub";
            if (IsFront)
                FrontImagePath = filePath;
            else
                RearImagePath = filePath;
            Cv2.ImWrite(filePath, mergedImage);
            for (int i = 0; i < subimage.Length; i++)
            {
                Cv2.ImWrite(subfilePath + "_" + i + ".bmp", subimage[i]);
            }
            //mergedImage.Save(filePath, System.Drawing.Imaging.ImageFormat.Bmp);

            return filePath;
        }


        private string SaveMergedImage(string doorTrimID, Bitmap mergedImage, bool IsFront)
        {
            string imageSaveFolder = Machine.config.setup.ImagePath;

            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            DateTime Today = DateTime.Now;
            string Month = Today.ToString("MM");
            string Date = Today.ToString("dd");

            imageSaveFolder = System.IO.Path.Combine(imageSaveFolder, Month);
            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            imageSaveFolder = System.IO.Path.Combine(imageSaveFolder, Date);
            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            imageSaveFolder = System.IO.Path.Combine(imageSaveFolder, doorTrimID);
            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            string filePath = System.IO.Path.Combine(imageSaveFolder, IsFront ? "front.bmp" : "rear.bmp");
            mergedImage.Save(filePath, System.Drawing.Imaging.ImageFormat.Bmp);

            return filePath;
        }
        // ========================================================
        private void SetRecipe()
        {
            // 임시 barcode test start
            int recipenum = 0;
            DOOR dOOR = new DOOR();
            try
            {
                //Machine.BarcodeData = new BarCodeHelper(_barcode);
                if (Machine.BarcodeData.DOOR_TYPE % 5 == 1)
                {
                    dOOR = DOOR.FRONT_LEFT;
                    CurrentRecipe = new Recipe(Machine.config.setup.RecipeNumFrontLeft);
                }
                else if (Machine.BarcodeData.DOOR_TYPE % 5 == 2)
                {
                    dOOR = DOOR.FRONT_RIGHT;
                    CurrentRecipe = new Recipe(Machine.config.setup.RecipeNumFrontRight);
                }
                else if (Machine.BarcodeData.DOOR_TYPE % 5 == 3)
                {
                    dOOR = DOOR.REAR_LEFT;
                    CurrentRecipe = new Recipe(Machine.config.setup.RecipeNumRearLeft);
                }
                else if (Machine.BarcodeData.DOOR_TYPE % 5 == 4)
                {
                    dOOR = DOOR.REAR_RIGHT;
                    CurrentRecipe = new Recipe(Machine.config.setup.RecipeNumRearRight);
                }
                else
                    CurrentRecipe = new Recipe(Machine.config.setup.RecipeNum);
            }
            catch { CurrentRecipe = new Recipe(Machine.config.setup.RecipeNum); }

            Machine.ALCData = Machine.dyDBHelper.GetALCMIS3PF(Machine.BarcodeData);
            Machine.DIO_Comm.SendProductType(dOOR);
        }

        private void SendResult(INSPECTION_RESULT result)
        {
#if PLC_TEST
#else
            //test
            //Machine.DIO_Comm.SendResult(true);
            //return;
            if (result == INSPECTION_RESULT.OK)
                Machine.DIO_Comm.SendResult(true);
            else
                Machine.DIO_Comm.SendResult(false);
#endif
        }

        private void SendDataClear()
        {
#if PLC_TEST
#else
            Machine.DIO_Comm.ClearData();
#endif
        }

        private Mat MergeImagesMat(Mat[] imgs, bool IsFirst = false)
        {
            //int yoffset = 98;
            //int xoffset01 = 548;
            //int xoffset02 = 234;
            int yoffset = 127;
            int xoffset01 = 469;
            int xoffset02 = 467;
            Mat left = imgs[0].Clone();
            Mat right = imgs[1].Clone();
            Mat leftCrop = new Mat(left, new OpenCvSharp.Rect(0, 0, left.Width - xoffset02, left.Height - yoffset));
            Mat rightCrop = new Mat(right, new OpenCvSharp.Rect(xoffset01, yoffset, left.Width - xoffset01, left.Height - yoffset));

            Mat result = new Mat();

            //if (IsFirst) // MEER 2025.01.23 SAVE ORIGINAL IMAGES FOR DEEP LEARNING
            //{
            //    string tempFileName = "infer.bmp";
            //    Mat resultOri = new Mat();
            //    Cv2.HConcat(new Mat[] { leftCrop, rightCrop }, resultOri);
            //    Cv2.ImWrite(tempFileName, resultOri);
            //}

            leftCrop = LevelOps.EqualizeHistColor(leftCrop);
            rightCrop = LevelOps.EqualizeHistColor(rightCrop);

            Cv2.HConcat(new Mat[] { leftCrop, rightCrop }, result);

            if (IsFirst) // MEER 2025.04.28 SAVE WHITE BALANCED IMAGES FOR DEEP LEARNING
            {
                string tempFileName = "infer.bmp";
                Cv2.ImWrite(tempFileName, result);
            }

            return result;
        }

        private Mat MergeImagesStitcher(Mat[] imgs)
        {
            Mat result = ImageHelper.MergeImagesStitcher(imgs);

            return result;
        }

        private void InspectionFirstStart(Mat MergeImg,Mat Sub1Img,Mat Sub2Img)
        {
            var start = DateTime.Now;
            //inspection = new Inspection(CurrentRecipe);
            inspection.SetRearInspectionImage(MergeImg);
            inspection.SetRearSub1InspectionImage(Sub1Img);
//#if USE_EXTRA_CAM
            inspection.SetRearSub2InspectionImage(Sub2Img);
//#endif
            inspection.ExecuteRearInspection();
            var total = DateTime.Now - start;
            Machine.logger.Write(eLogType.INFORMATION, "Front Inspection Time: " + total.TotalSeconds.ToString("F3") + " Second");
        }

        private void InspectionFirstStart(Mat MergeImg,Mat Sub1Img)
        {
            //inspection = new Inspection(CurrentRecipe);
            inspection.SetRearInspectionImage(MergeImg);
            inspection.SetRearSub1InspectionImage(Sub1Img);


            inspection.ExecuteRearInspection();
        }


        private void InspectionSecondStart(Mat MergeImg)
        {
            var start = DateTime.Now;
            //inspection = new Inspection(CurrentRecipe);
            inspection.SetFrontInspectionImage(MergeImg);
            inspection.ExecuteFrontInspection();
            var total = DateTime.Now - start;
            Machine.logger.Write(eLogType.INFORMATION, "Rear Inspection Time : " + total.TotalSeconds.ToString("F3") + " Second");
        }

        private void GetFristInspectionResult()
        {
            //// Check PlugInspectionResult
            //foreach (PlugInspectionItem result in inspection.RearInspectionResult.PlugInspectionResult)
            //{
            //    if (result.InspectionResult == INSPECTION_RESULT.NG)
            //    {
            //        _inspResultFirst = INSPECTION_RESULT.NG;
            //        return;
            //    }
            //}

            //// Check ScrewInspectionResult
            //foreach (ScrewInspectionItem result in inspection.RearInspectionResult.ScrewInspectionResult)
            //{
            //    if (result.InspectionResult == INSPECTION_RESULT.NG)
            //    {
            //        _inspResultFirst = INSPECTION_RESULT.NG;
            //        return;
            //    }
            //}

            //// Check BoltInspectionResult
            //foreach (BoltInspectionItem result in inspection.RearInspectionResult.BoltInspectionResult)
            //{
            //    if (result.InspectionResult == INSPECTION_RESULT.NG)
            //    {
            //        _inspResultFirst = INSPECTION_RESULT.NG;
            //        return;
            //    }
            //}

            //// Check ColorInspectionResult
            //foreach (ColorInspectionItem result in inspection.RearInspectionResult.ColorInspectionResult)
            //{
            //    if (result.InspectionResult == INSPECTION_RESULT.NG)
            //    {
            //        _inspResultFirst = INSPECTION_RESULT.NG;
            //        return;
            //    }
            //}

            //// Check PadInspectionResult
            //foreach (PadInspectionItem result in inspection.RearInspectionResult.PadInspectionResult)
            //{
            //    if (result.InspectionResult == INSPECTION_RESULT.NG)
            //    {
            //        _inspResultFirst = INSPECTION_RESULT.NG;
            //        return;
            //    }
            //}

            //// Check SpeakerInspectionResult
            //foreach (SpeakerInspectionItem result in inspection.RearInspectionResult.SpeakerInspectionResult)
            //{
            //    if (result.InspectionResult == INSPECTION_RESULT.NG)
            //    {
            //        _inspResultFirst = INSPECTION_RESULT.NG;
            //        return;
            //    }
            //}

            //// Check SmallPadInspectionResult
            //foreach (SmallPadInspectionItem result in inspection.RearInspectionResult.SmallPadInspectionResult)
            //{
            //    if (result.InspectionResult == INSPECTION_RESULT.NG)
            //    {
            //        _inspResultFirst = INSPECTION_RESULT.NG;
            //        return;
            //    }
            //}

            //// Check ScrewMacthInspectionResult
            //foreach (ScrewMacthInspectionItem result in inspection.RearInspectionResult.ScrewMacthInspectionResult)
            //{
            //    if (result.InspectionResult == INSPECTION_RESULT.NG)
            //    {
            //        _inspResultFirst = INSPECTION_RESULT.NG;
            //        return;
            //    }
            //}

            //// Check PlugMatchInspectionResult
            //foreach (PlugMatchInspectionItem result in inspection.RearInspectionResult.PlugMatchInspectionResult)
            //{
            //    if (result.InspectionResult == INSPECTION_RESULT.NG)
            //    {
            //        _inspResultFirst = INSPECTION_RESULT.NG;
            //        return;
            //    }
            //}

            //// Check DeepScrewInspectionResult
            //if ((eAnalysisMethod)Machine.config.setup.AnalysisMethod == eAnalysisMethod.DEEP)
            //{
            //    foreach (DeepInspectionItem result in inspection.RearInspectionResult.DeepScrewInspectionResult)
            //    {
            //        if (result.InspectionResult == INSPECTION_RESULT.NG)
            //        {
            //            _inspResultFirst = INSPECTION_RESULT.NG;
            //            return;
            //        }
            //    }
            //}

            //// Check DeepFusionInspectionResult
            //if ((eAnalysisMethod)Machine.config.setup.AnalysisMethod == eAnalysisMethod.DEEP)
            //{
            //    foreach (DeepInspectionItem result in inspection.RearInspectionResult.DeepFusionInspectionResult)
            //    {
            //        if (result.InspectionResult == INSPECTION_RESULT.NG)
            //        {
            //            _inspResultFirst = INSPECTION_RESULT.NG;
            //            return;
            //        }
            //    }
            //}

            //// Check WhitePadInspectionResult
            //foreach (WhitePadInspectionItem result in inspection.RearInspectionResult.WhitePadInspectionResult)
            //{
            //    if (result.InspectionResult == INSPECTION_RESULT.NG)
            //    {
            //        _inspResultFirst = INSPECTION_RESULT.NG;
            //        return;
            //    }
            //}

            //// If all results are OK, set _inspResultFirst to OK
            //_inspResultFirst = INSPECTION_RESULT.OK;

            _inspResultFirst = inspection.RearInspectionResultCode; // MEER 2025.04.18 1) REMOVE RECDUNDANCY CODE, USE THE CLASS LEVEL PROPERTY, 2) REMOVE DEEP RESULTS FROM OK/NG CALCULATION 

        }

        private void GetSecondInspectionResult()
        {
            //foreach (ColorMatchInspectionItem result in inspection.FrontInspectionResult.ColorMatchInspectionResult)
            //{
            //    if (result.InspectionResult == INSPECTION_RESULT.NG)
            //    {
            //        _inspResultFirst = INSPECTION_RESULT.NG;
            //        return;
            //    }
            //}
            //_inspResultSecond = INSPECTION_RESULT.OK;

            _inspResultSecond = inspection.FrontInspectionResultCode; // MEER 2025.04.18 1) REMOVE RECDUNDANCY CODE, USE THE CLASS LEVEL PROPERTY
        }
    }
}
