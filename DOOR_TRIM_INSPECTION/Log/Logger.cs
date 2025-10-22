using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DOOR_TRIM_INSPECTION
{
    public enum eLogType
    {
        INSPECTION,
        CAMERA,
        LIGHT,
        DIO,
        SEQ,
        INFORMATION,
        ERROR,
        RESULT
    }

    public class Logger
    {
        public static string logDir = "";
        public static string resultDir = "";
        public static string iamgeDir = "";

        private static object _objLock = new object();

        public  void Initialize(string filePath = "")
        {
            if (filePath == "")
                logDir = Path.Combine(System.Windows.Forms.Application.StartupPath, "log");
            else
                logDir = filePath;

            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);
        }

        private void WriteFirstLine(eLogType logType)
        {


        }

        public void Write(eLogType logType, string logMessage)
        {

            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            string msg = "===================Program Version " + asm.GetName().Version.ToString() + " ===================";
            string[] lines = ResultGetData(logType);

            string logpath = getLogPath(logType);
            string strDir = logpath.Substring(0, logpath.LastIndexOf('\\'));
            string time = GetTimeString();
            string message = "[" + time + "] " + logMessage;
            message = message.Replace("\r\n", "");

            if (!Directory.Exists(strDir))
                Directory.CreateDirectory(strDir);

            lock (_objLock)
            {
                StreamWriter log = new StreamWriter(logpath, true);
                using (log)
                {
                    if (lines == null)
                        log.WriteLine(msg);
                    else if (lines.Length==0)
                        log.WriteLine(msg);
                    log.WriteLine(message);
                }
            }

            //FormMain.Instance().ctrlLog.AddLog(message);
        }

        public string[] ResultGetData(eLogType logType)
        {
            string logpath = getLogPath(logType);
            if (File.Exists(logpath))
            {
                string[] value = File.ReadAllLines(logpath);

                return value;
            }
            else
                return null;
        }

        public void ResultUpdate(eLogType logType, int total,int pallet, int NGcount)
        {
            string logpath = getLogPath(logType);
            string strDir = logpath.Substring(0, logpath.LastIndexOf('\\'));
            string time = GetTimeString();
            //string message = "[" + time + "] " + logMessage;
            //message = message.Replace("\r\n", "");

            if (!Directory.Exists(strDir))
                Directory.CreateDirectory(strDir);

            lock (_objLock)
            {
                StreamWriter log = new StreamWriter(logpath, false);
                using (log)
                {
                    log.WriteLine(total.ToString());
                    log.WriteLine(pallet.ToString());
                    log.WriteLine(NGcount.ToString());
                }
            }
        }

        private string getLogPath(eLogType logType)
        {
            string logPath = string.Format(@"{0}\{1:00}\{2:00}{3:00}\log_{4:0000}{5:00}{6:00}_" + logType.ToString() + ".log", logDir, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            return logPath;
        }

        public void WriteException(eLogType logType, Exception exception)
        {
            string logpath = getLogPath(logType);
            string strDir = logpath.Substring(0, logpath.LastIndexOf('\\'));
            string time = GetTimeString();
            string logMessage = "[" + time + "] " + exception.StackTrace + " : " + exception.Message;
            logMessage = logMessage.Replace("\r\n", "");

            if (!Directory.Exists(strDir))
                Directory.CreateDirectory(strDir);

            lock (_objLock)
            {
                StreamWriter log = new StreamWriter(logpath, true);
                using (log)
                {
                    log.WriteLine(logMessage);
                }
            }
        }
        public static void DeleteFilesInDir(string dir, string searchPattern, int day)
        {
            DirectoryInfo di = new DirectoryInfo(dir);    // 인자값으로 들어온 절대 주소를 객체로 정의합니다.
            Dirs(di, searchPattern, day);                 // 삭제를 시작합니다.
        }

        private static void Dirs(DirectoryInfo dirinfo, string searchPattern, int day)
        {
            DirectoryInfo[] di = dirinfo.GetDirectories(); // 받은 주소의 하위 폴더 주소들을 반환합니다.

            if (di.Length < 1) // 반환받은 주소가 없을 경우 빠져나갑니다.
            {
                return;
            }

            for (int i = 0; i < di.Length; i++) // 반환받은 주소의 수 만큼 반복문을 실행시킵니다.
            {
                if (di[i].GetFiles().Count<FileInfo>() < 1 && di[i].GetDirectories().Count<DirectoryInfo>() < 1) // 하위 폴더가 빈 폴더면 삭제
                {
                    di[i].Delete();
                }
                else
                {
                    DelFiles(di[i], searchPattern, day); // 하위 폴더의 파일을 지움
                    Dirs(di[i], searchPattern, day); //  하위 폴더로 재귀호출
                }
            }
        }


        private static void DelFiles(DirectoryInfo diinfo, string searchPattern, int day)
        {
            try
            {
                DateTime dayAgoTime = DateTime.Now.AddSeconds(-(day * 3600 * 24)); // 인자로 받은 날을 객체로 정의합니다.
                DateTime agoTime = DateTime.Now.AddSeconds(-(1 * 3600 * 24)); // bmp 삭제를 위해
                foreach (FileInfo fileName in diinfo.GetFiles()) // 해당 폴더에 파일 갯수 만큼 반복합니다.
                {
                    if (searchPattern.Equals(".*")) //확장명이 .*일 경우 모든 파일을 제거합니다.
                    {
                        DateTime dt = fileName.CreationTime; // 파일을 만들었던 시간을 객체로 정의합니다.

                            if (dayAgoTime > dt) // 사용자가 설정한 날보다 더 이전에 만들었을 경우
                            {
                                fileName.Delete(); // 파일을 제거합니다.
                            }
                    }
                    else if (fileName.Extension.Equals(searchPattern)) // 인자값의 확장명이 반복문의 확장명과 같을 경우 제거합니다.
                    {
                        DateTime dt = fileName.CreationTime; // 파일을 만들었던 시간을 객체로 정의합니다.
                        if (dayAgoTime > dt) // 사용자가 설정한 날보다 더 이전에 만들었을 경우
                        {
                            fileName.Delete(); // 파일을 제거합니다.
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Machine.logger.WriteException(eLogType.ERROR, ex);
            }
        }

        public string GetTimeString()
        {
            // 시간은 plc의 시간. ms는 없으므로 pc의 ms
            DateTime now = DateTime.Now;
            string strTime = string.Format(@"{0:00}{1:00} {2:00}:{3:00}:{4:00}.{5:000}ms", now.Month, now.Day,
                now.Hour, now.Minute, now.Second, now.Millisecond);
            return strTime;
        }
    }
}
