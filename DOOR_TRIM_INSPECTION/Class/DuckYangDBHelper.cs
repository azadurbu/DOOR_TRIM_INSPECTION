using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOOR_TRIM_INSPECTION.Class
{
    public class ALCList
    {
        public const string SPA01 = "SPA01";
        public const string SPA02 = "SPA02";
        public const string SPA03 = "SPA03";
        public const string SPA04 = "SPA04";
        public const string SPA05 = "SPA05";
        public const string SPA06 = "SPA06";
        public const string SPA07 = "SPA07";
        public const string SPA08 = "SPA08";
        public const string SPA09 = "SPA09";
        public const string SPA10 = "SPA10";        
        public const string SPA11 = "SPA11";
    }

    public class ALCMIS3PF
    {
        public string CARCD { get; set; }
        public string JEPUMCD { get; set; }
        public string ALCCD { get; set; }
        /// <summary>
        /// Upper Trim
        /// </summary>
        public string SPA01 { get; set; }
        /// <summary>
        /// Arm
        /// </summary>
        public string SPA02 { get; set; }
        /// <summary>
        /// LowerTrim
        /// </summary>
        public string SPA03 { get; set; }
        /// <summary>
        /// Grill
        /// </summary>
        public string SPA04 { get; set; }
        /// <summary>
        /// Full Handle
        /// </summary>
        public string SPA05 { get; set; }
        /// <summary>
        /// 흡음제
        /// </summary>
        public string SPA06 { get; set; }
        /// <summary>
        /// Handle
        /// </summary>
        public string SPA07 { get; set; }
        /// <summary>
        /// Swicth
        /// </summary>
        public string SPA08 { get; set; }
        /// <summary>
        /// IMS
        /// </summary>
        public string SPA09 { get; set; }
        /// <summary>
        /// Speaker
        /// </summary>
        public string SPA10 { get; set; }
        /// <summary>
        /// Wire
        /// </summary>
        public string SPA11 { get; set; }
        
        public ALCMIS3PF() { }

        public string GetSPA(string spa)
        {
            if (ALCList.SPA01 == spa)
                return SPA01;
            else if (ALCList.SPA02 == spa)
                return SPA02;
            else if (ALCList.SPA03 == spa)
                return SPA03;
            else if (ALCList.SPA04 == spa)
                return SPA04;
            else if (ALCList.SPA05 == spa)
                return SPA05;
            else if (ALCList.SPA06 == spa)
                return SPA06;
            else if (ALCList.SPA07 == spa)
                return SPA07;
            else if (ALCList.SPA08 == spa)
                return SPA08;
            else if (ALCList.SPA09 == spa)
                return SPA09;
            else if (ALCList.SPA10 == spa)
                return SPA10;
            else if (ALCList.SPA11 == spa)
                return SPA11;
            else
                return "";
        }

        public string ChageXtoBar(string str)
        {
            string temp = str.Trim().ToLower();
            if (temp == "x")
                return "-";
            return temp.ToUpper();
        }
    }

    public class DuckYangDBHelper
    {
        
        private SqlConnection dbConnection = null;
        private string connectionString = string.Empty;
        public SqlConnection Connection()
        {
            if (dbConnection == null || dbConnection.State != System.Data.ConnectionState.Open)
            {
                dbConnection = new SqlConnection(connectionString);
                dbConnection.Open();
            }
            return dbConnection;
        }

        public DuckYangDBHelper()
        {
            // OPEN AND RETAIN DB CONNECTION HERE
            connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DuckYangDBConnection"].ConnectionString;
        }

        //~DuckYangDBHelper()
        //{
        //    CloseDBConnection();
        //}

        public void Terminate()
        {
            if (dbConnection != null && dbConnection.State == System.Data.ConnectionState.Open)
            {
                dbConnection.Close();
                dbConnection = null;
            }
        }

        public byte[] BitmapToByteArray(Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }

        public bool SaveCheckHistory(BarCodeHelper BarcodeData, bool OK_NG, DateTime BarcodeReadTime)
        {
            try
            {
                //CARCD,JEPUMCD,ALCCD,SEQ,BARCODE,INS_DATETIME,GONGCD,OK_NG
                //string query = "UPDATE [dyc_mes2].[dbo].[TB_CHECK_HIST] SET OK_NG = '@OK_NG', INS_DATETIME = GETDATE() WHERE BARCODE = '@BARCODE' AND GONGCD = '@GONGCD'" +
                //                             " IF @@ROWCOUNT = 0" +
                //                             "   INSERT INTO[dbo].[TB_CHECK_HIST](CARCD, JEPUMCD, ALCCD, SEQ, BARCODE, INS_DATETIME, GONGCD, OK_NG)" +
                //                             "  VALUES('@CARCD', '@JEPUMCD', '@ALCCD', '@SEQ', '@BARCODE', GETDATE(), '@GONGCD', '@OK_NG')";

                string query = "UPDATE [dyc_mes2].[dbo].[TB_CHECK_HIST] SET OK_NG = @OK_NG, INS_DATETIME = @BARCODEREADTIME WHERE BARCODE = @BARCODE AND GONGCD = @GONGCD" +
                                             " IF @@ROWCOUNT = 0" +
                                             "   INSERT INTO[dbo].[TB_CHECK_HIST](CARCD, JEPUMCD, ALCCD, SEQ, BARCODE, INS_DATETIME, GONGCD, OK_NG)" +
                                             "  VALUES(@CARCD, @JEPUMCD, @ALCCD, @SEQ, @BARCODE, @BARCODEREADTIME, @GONGCD, @OK_NG)";


                using (SqlCommand sqlCommand = new SqlCommand(query, Connection()))
                {
                    sqlCommand.Parameters.Add("@OK_NG", SqlDbType.VarChar).Value = OK_NG == true ? "OK" : "NG";
                    sqlCommand.Parameters.Add("@BARCODE", SqlDbType.VarChar).Value = BarcodeData.Barcode;
                    sqlCommand.Parameters.Add("@GONGCD", SqlDbType.VarChar).Value = Machine.config.setup.GONGCD;
                    sqlCommand.Parameters.Add("@CARCD", SqlDbType.VarChar).Value = BarcodeData.CARCD;
                    sqlCommand.Parameters.Add("@JEPUMCD", SqlDbType.VarChar).Value = BarcodeData.JEPUMCD;
                    sqlCommand.Parameters.Add("@ALCCD", SqlDbType.VarChar).Value = BarcodeData.ALCCD;
                    sqlCommand.Parameters.Add("@SEQ", SqlDbType.VarChar).Value = BarcodeData.SEQ;
                    sqlCommand.Parameters.Add("@BARCODEREADTIME", SqlDbType.VarChar).Value = BarcodeReadTime.ToString("yyyy-MM-dd HH:mm:ss.fff");


                    sqlCommand.CommandType = CommandType.Text;
                    return sqlCommand.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                Machine.logger.Write(eLogType.ERROR, "SaveTB_CHECK_HIST: " + ex.ToString());
                return false;
            }
        }
        public byte[] GetImageTEST(string barcode)
        {
            string query = "SELECT [VS_IMAGE]  FROM[dyc_mes2].[dbo].[TB_VISION_RESULT]  where BARCODE = '" + barcode + "'";
            byte[] imgData = null;
            using (SqlCommand command = new SqlCommand(query, Connection()))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        imgData = (byte[])reader["VS_IMAGE"];
                    }
                }
            }
            return imgData;
        }


        public Bitmap GetBitmapFromDatabaseTest(string barcode)
        {
            byte[] imageBytes = GetImageTEST(barcode);
            if (imageBytes != null)
            {
                return ByteArrayToBitmap(imageBytes);
            }
            return null;
        }

        public Bitmap ByteArrayToBitmap(byte[] imageBytes)
        {
            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                Bitmap bmp = new Bitmap(ms);
                bmp.Save("GetBitmapFromDatabaseTest.bmp");

                return bmp;
            }
        }

        public bool SaveVisionResult(BarCodeHelper BarcodeData, string imagePath, bool OK_NG, DateTime BarcodeReadTime)
        {
            try
            {
                //CARCD,JEPUMCD,ALCCD,SEQ,BARCODE,INS_DATETIME,GONGCD,OK_NG
                string query = "UPDATE [dyc_mes2].[dbo].[TB_VISION_RESULT] SET OK_NG = @OK_NG, VS_IMAGE = @VS_IMAGE, CRTDATE = @BARCODEREADTIME WHERE BARCODE = @BARCODE AND LINECD = @LINECD" +
                                             " IF @@ROWCOUNT = 0" +
                                             "   INSERT INTO[dbo].[TB_VISION_RESULT](LINECD, BARCODE, CARCD, JEPUMCD, VS_IMAGE, OK_NG, CRTDATE)" +
                                             "  VALUES(@LINECD, @BARCODE, @CARCD, @JEPUMCD, @VS_IMAGE, @OK_NG, @BARCODEREADTIME)";// GETDATE()
                Bitmap img = new Bitmap(imagePath);
                byte[] VS_IMAGE = BitmapToByteArray(new Bitmap(img));
                if (img != null)
                {
                    img.Dispose();
                    img = null;
                }

                using (SqlCommand sqlCommand = new SqlCommand(query, Connection()))
                {
                    sqlCommand.Parameters.Add("@OK_NG", SqlDbType.VarChar).Value = OK_NG == true ? "OK" : "NG";
                    sqlCommand.Parameters.Add("@VS_IMAGE", SqlDbType.Image).Value = VS_IMAGE;
                    sqlCommand.Parameters.Add("@BARCODE", SqlDbType.VarChar).Value = BarcodeData.Barcode;
                    sqlCommand.Parameters.Add("@LINECD", SqlDbType.VarChar).Value = Machine.config.setup.LINECD;
                    sqlCommand.Parameters.Add("@CARCD", SqlDbType.VarChar).Value = BarcodeData.CARCD;
                    sqlCommand.Parameters.Add("@JEPUMCD", SqlDbType.VarChar).Value = BarcodeData.JEPUMCD;
                    sqlCommand.Parameters.Add("@BARCODEREADTIME", SqlDbType.VarChar).Value = BarcodeReadTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                 
                    sqlCommand.CommandType = CommandType.Text;

                    return sqlCommand.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                Machine.logger.Write(eLogType.ERROR, "SaveTB_CHECK_HIST: " + ex.ToString());
                return false;
            }
        }
        
        public ALCMIS3PF GetALCMIS3PF(BarCodeHelper data)
        {
            return GetALCMIS3PF(data.CARCD, data.JEPUMCD, data.ALCCD);
        }

        public ALCMIS3PF GetALCMIS3PF(string CARCD, string JEPUMCD, string ALCCD)
        {
            try
            {
                ALCMIS3PF aLCMIS3PF = null;
                string query = string.Format("SELECT TOP 1 " +
                    "CARCD, JEPUMCD, ALCCD, SPA01, SPA02, SPA03, SPA04, SPA05, SPA06, SPA07, SPA08, SPA09, SPA10, SPA11" +
                    " FROM [dyc_mes2].[dbo].[ALCMIS3PF] where CARCD = '{0}' and JEPUMCD = '{1}' and ALCCD = '{2}'", CARCD, JEPUMCD, ALCCD);

                
                using(SqlCommand command = new SqlCommand(query, Connection()))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            aLCMIS3PF = new ALCMIS3PF() {
                                CARCD = reader.GetString(0),
                                JEPUMCD = reader.GetString(1),
                                ALCCD = reader.GetString(2),
                                SPA01 = reader.GetString(3),
                                SPA02 = reader.GetString(4),
                                SPA03 = reader.GetString(5),
                                SPA04 = reader.GetString(6),
                                SPA05 = reader.GetString(7),
                                SPA06 = reader.GetString(8),
                                SPA07 = reader.GetString(9),
                                SPA08 = reader.GetString(10),
                                SPA09 = reader.GetString(11),
                                SPA10 = reader.GetString(12),
                                SPA11 = reader.GetString(13)
                            };
                        }
                    }
                }
                return aLCMIS3PF;
            }
            catch (Exception ex)
            {
                Machine.logger.Write(eLogType.ERROR, "DB Read Error: " + ex.ToString());
                return null;
            }
        }

        public bool UpdateCycleTime(BarCodeHelper BarcodeData, string WorkTime)
        {
            try
            {
                string query = "UPDATE [dyc_mes2].[dbo].[TB_VISION_RESULT] SET WORKTIME = @WorkTime WHERE BARCODE = @BARCODE AND LINECD = @LINECD";


                using (SqlCommand sqlCommand = new SqlCommand(query, Connection()))
                {
                    sqlCommand.Parameters.Add("@WorkTime", SqlDbType.VarChar).Value = WorkTime;
                    sqlCommand.Parameters.Add("@BARCODE", SqlDbType.VarChar).Value = BarcodeData.Barcode;
                    sqlCommand.Parameters.Add("@LINECD", SqlDbType.VarChar).Value = Machine.config.setup.LINECD;

                    sqlCommand.CommandType = CommandType.Text;
                    return sqlCommand.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                Machine.logger.Write(eLogType.ERROR, "UpdateCycleTime: " + ex.ToString());
                return false;
            }
        }
    }
}
