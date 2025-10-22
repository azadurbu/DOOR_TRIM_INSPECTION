using DOOR_TRIM_INSPECTION.Class;
using DOOR_TRIM_INSPECTION.Device.Camera;
using DOOR_TRIM_INSPECTION.Device.Light;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace DOOR_TRIM_INSPECTION
{
    [Serializable]
    public class Setup
    {
        //Setting 폼에 있는 변수 
        //Camera
//#if USE_EXTRA_CAM
//        public int maxCamCount = 4; //사용 가능한 카메라 최대 개수
//#else
        public int maxCamCount = 3; //사용 가능한 카메라 최대 개수
//#endif
        public int TimeoutDataClear = 500; // millisecond
        public int TimeoutGrab = 2000; // millisecond
        public int TimeoutInsp = 5000; // millisecond
        public int RecipeNum    = 0;    // 241116 임시
        public int RecipeNumFrontLeft = 0;
        public int RecipeNumFrontRight = 0;
        public int RecipeNumRearLeft = 0;
        public int RecipeNumRearRight = 0;
        public string BarcodeComport = "com1";
        public string ImagePath = "D:\\Images";
        public int ImageStoragePeriod = 180;
        public int CheckLightLevel = 10;
        public int ResetLightTime = 2000;

        public string MasterModeTestTime = "12:45"; //20250910-AD

        public int AnalysisMethod = 0;
        public string WorkspacePath = "DeokYang.vrws";

        public string GONGCD = "20";
        public string LINECD = "80";
        public List<CameraProperty> cameraProp = new List<CameraProperty>();
        //public CameraProperty CameraProperty_0 = new CameraProperty();
        //public CameraProperty CameraProperty_1 = new CameraProperty();
        //public CameraProperty CameraProperty_2 = new CameraProperty();
        public StitcherParam stitcherParam = new StitcherParam();
        public LightProperty lightProp = new LightProperty();


        public string FL_OK_DOOR_TRIM_ID = "";
        public string FL_OK_F_IMG = "";
        public string FL_OK_R_IMG = "";

        public string FL_NG_DOOR_TRIM_ID = "";
        public string FL_NG_F_IMG = "";
        public string FL_NG_R_IMG = "";

        public string FR_OK_DOOR_TRIM_ID = "";
        public string FR_OK_F_IMG = "";
        public string FR_OK_R_IMG = "";

        public string FR_NG_DOOR_TRIM_ID = "";
        public string FR_NG_F_IMG = "";
        public string FR_NG_R_IMG = "";

        public string RL_OK_DOOR_TRIM_ID = "";
        public string RL_OK_F_IMG = "";
        public string RL_OK_R_IMG = "";

        public string RL_NG_DOOR_TRIM_ID = "";
        public string RL_NG_F_IMG = "";
        public string RL_NG_R_IMG = "";

        public string RR_OK_DOOR_TRIM_ID = "";
        public string RR_OK_F_IMG = "";
        public string RR_OK_R_IMG = "";

        public string RR_NG_DOOR_TRIM_ID = "";
        public string RR_NG_F_IMG = "";
        public string RR_NG_R_IMG = "";


        public bool USE_MASTER_IMAGES = false;
        public string MASTER_TEST_SEQUENCE = "00000000";

        public string VPP_PATH = "D:\\COGNEX_VPP";

        public bool USE_COGNEX_RESULT = false;

        public bool SHOW_ALGO_NAME = true;
    }
    [Serializable]
    public class Algorithm
    {
        public List<AlgorithmParam> parameters = new List<AlgorithmParam>(); // LOAD FROM XML (Name, ID, Default Value)        
    }

    [Serializable]
    public class AlgorithmList
    {
        public List<AlgorithmParamOption> paramOptions = new List<AlgorithmParamOption>();
    }

    [Serializable]
    public class AIConfig
    {
        public bool UseInCalculation = false;
        public List<AIClass> AIClasses = new List<AIClass>(); // LOAD FROM XML (Name, ID, Default Value)        
    }

    public class Config
    {
        public Setup setup = new Setup();
        public Algorithm Alsetup = new Algorithm();
        public AIConfig aiConfig = new AIConfig();

        public void LoadAIConfig()
        {
            Load<AIConfig>(ref aiConfig, "AIConfig");
        }
        public bool SaveAIConfig()
        {
            return Save<AIConfig>(aiConfig, true, "AIConfig");
        }

        public void LoadAlgorithm()
        {
            Load<Algorithm>(ref Alsetup, "Algorithm");
            foreach (AlgorithmParam property in Alsetup.parameters)
                Alsetup.parameters.Add(property);
        }
        public bool SaveAlgorithm()
        {
            return Save<Algorithm>(Alsetup, true, "Algorithm");
        }

        public void LoadConfig()
        {
            try
            {
                Load<Setup>(ref setup, "Setup");
                //setup.cameraProp.Add(setup.CameraProperty_0);
                //setup.cameraProp.Add(setup.CameraProperty_1);
                //setup.cameraProp.Add(setup.CameraProperty_2);
            }
            catch
            {
                Machine.logger.Write(eLogType.ERROR, "Config Data 로드 실패");
            }
        }

        public bool SaveConfig()
        {
            return Save<Setup>(setup, true,"Setup");
        }

        public static bool Load<T>(ref T obj, string fileName)
        {
            //string strDir = System.Environment.CurrentDirectory + "\\Config";

            //string strDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + 
            string strDir = Application.StartupPath + "\\Config";
            string strPath = strDir + "\\"+ fileName + ".cfg";

            try
            {
                XmlSerializer xml = new XmlSerializer(typeof(T));

                if (!Directory.Exists(strDir))
                    Directory.CreateDirectory(strDir);

                if (!File.Exists(strPath))
                {
                    using (TextWriter w = new StreamWriter(strPath))
                    {
                        xml.Serialize(w, obj);
                    }
                }

                using (TextReader r = new StreamReader(strPath))
                {
                    obj = (T)xml.Deserialize(r);
                }
                return true;
            }
            catch(Exception ex)
            {
                Machine.logger.Write(eLogType.ERROR, "Config Data Xml 로드 실패 : "+ fileName + "\r\n"+ex.ToString());
                return false;
            }
        }

        public static bool Save<T>(T obj, bool bBackup, string fileName)
        {
            try
            {
                string strDir = System.Windows.Forms.Application.StartupPath + "\\Config";
                string strPath = strDir + "\\"+fileName+".cfg";

                if (bBackup)
                {
                    string strBackupPath = strPath + ".bak";
                    if (File.Exists(strBackupPath))
                    {
                        File.Delete(strBackupPath);
                    }
                    File.Move(strPath, strBackupPath);
                }

                XmlSerializer xml = new XmlSerializer(typeof(T));
                using (TextWriter w = new StreamWriter(strPath))
                {
                    xml.Serialize(w, obj);
                }

                Machine.logger.Write(eLogType.INFORMATION, "========== Save "+ fileName+" Parameter ==========");
            }
            catch(Exception ex)
            {
                Machine.logger.Write(eLogType.ERROR, "Config Data Xml 세이브 실패 : "+ fileName+"\r\n"+ex.ToString());
                return false;
            }

            return true;
        }
    }
}


