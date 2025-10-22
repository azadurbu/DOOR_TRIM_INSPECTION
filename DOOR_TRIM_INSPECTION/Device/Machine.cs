using DOOR_TRIM_INSPECTION.Device.BarcodeScanner;
using DOOR_TRIM_INSPECTION.Device.DIO;
using DOOR_TRIM_INSPECTION.Device.Light;
using DOOR_TRIM_INSPECTION.Device.Camera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DOOR_TRIM_INSPECTION.Class;
using OpenCvSharp.Flann;
using System.Management;
using System.Threading;

namespace DOOR_TRIM_INSPECTION
{
    public class Machine
    {
        public static DuckYangDBHelper dyDBHelper = null;
        public static HanMechDBHelper hmcDBHelper = null;
        public static ALCMIS3PF ALCData = null;

        public static Config config = null;
        public static Zebra_Serial BarcodeScan;
        public static AJIN_DIO_comm DIO_Comm;
        public static LightSerialCom Light_Comm;
        public static CameraManager camManager = null;
        public static Logger logger = null;

        //// 20241114 jsKim Add s
        public static Sequence sequence = null;
        //// 20241114 jsKim Add e

        public static bool StopSecondGrab = false;
        public static bool SetSecondGrab = false;

        public static eProgramMode ProgramMode = new eProgramMode();
        public static OpenCvSharp.Mat[] LoadImagesFront = null;
        public static OpenCvSharp.Mat[] LoadImagesRear = null;

        public static DOOR Test_ProductType = DOOR.FRONT_RIGHT;
        public static bool Test_Start = false;

        public static BarCodeHelper BarcodeData = null;

        public static string[] ALC_KOREAN = new string[] { "어퍼트림", "암레스트", "로워트림", "그릴", "풀핸들", "흡음재", "핸들", "스위치", "IMS", "스피커", "와이어" };
#if USE_DEEP
        public static CognexDeepVision cognexDeepVision = null;
#endif
#if USE_COGNEX
        public static CognexVisionDetection cognexVisionDetection = null;
#endif

        public static INSPECTION_MODE InspectionMode = new INSPECTION_MODE();

        public static void Initialize()
        {
            logger = new Logger();
            logger.Initialize();
            
            dyDBHelper = new DuckYangDBHelper();
            hmcDBHelper = new HanMechDBHelper();

            
            int LogFileCount = Enum.GetValues(typeof(eLogType)).Length;
            for (int i = 0; i < LogFileCount; i++)
                logger.Write((eLogType)i, "===================Program Start===================");

            config = new Config();
            config.LoadConfig();
            //config.SaveConfig();
            config.LoadAlgorithm();
            //config.SaveAlgorithm();
            config.LoadAIConfig();

            // "com1" change option value
            BarcodeScan = new Zebra_Serial(config.setup.BarcodeComport);
            BarcodeScan.PortOpen();

            DIO_Comm = new AJIN_DIO_comm();
#if PLC_TEST
#else
            DIO_Comm.Open();
#endif

            Light_Comm = new LightSerialCom(Machine.config.setup.lightProp.Comport);
            Light_Comm.PortOpen();
            Light_Comm.LightOnOffEN(false, 1, 0);
            camManager = new CameraManager();
            eCameraStatus camStatus = camManager.Initialize(Machine.config.setup.cameraProp, config.setup.maxCamCount);
            if (camStatus == eCameraStatus.CAM_CONNECTION_SUCCESS)
            {
                camManager.SetSaveMode(true);
            }
            else
            {
                Console.WriteLine(camStatus);
                //Retry Camera Connection
                // WHILE !eCameraStatus.CAM_CONNECTION_SUCCESS 
                // RETRY
                // THREAD.SLEEP
                string[] adapterNames = { "CAM1", "CAM2", "CAM3" };
                foreach (string adapterName in adapterNames)
                {
                    ManageNetworkAdapter(adapterName, false); // 네트워크 어댑터 비활성화
                }

                Console.WriteLine("10초간 대기 중...");
                Thread.Sleep(10000); // 15초 대기

                foreach (string adapterName in adapterNames)
                {
                    ManageNetworkAdapter(adapterName, true); // 네트워크 어댑터 활성화
                }

                Console.WriteLine("카메라 연결 10초간 대기 중...");
                Thread.Sleep(10000);
                camStatus = camManager.Initialize(Machine.config.setup.cameraProp, config.setup.maxCamCount);
                Console.WriteLine(camStatus);
                int count = 0;
                while(camStatus!=eCameraStatus.CAM_CONNECTION_SUCCESS)
                {
                    Console.WriteLine(camStatus);
                    Thread.Sleep(5000);
                    camStatus = camManager.Initialize(Machine.config.setup.cameraProp, config.setup.maxCamCount);
                    count++;
                    Console.WriteLine("카메라 연결 재시도 "+count+"/60");
                    if (count > 0)
                        break;
                }
                camManager.SetSaveMode(true);
            }
#if USE_DEEP
            cognexDeepVision = new CognexDeepVision();
#endif
#if USE_COGNEX
            cognexVisionDetection = new CognexVisionDetection();
#endif
        }
        static void ManageNetworkAdapter(string adapterName, bool enable)
        {
            string query = "SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionID = '" + adapterName + "'";
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject adapter in searcher.Get())
                {
                    try
                    {
                        adapter.InvokeMethod(enable ? "Enable" : "Disable", null);
                        Console.WriteLine($"네트워크 어댑터 {adapterName} {(enable ? "활성화" : "비활성화")} 성공");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"네트워크 어댑터 {adapterName} {(enable ? "활성화" : "비활성화")} 실패: {e.Message}");
                    }
                }
            }
        }
    }
}
