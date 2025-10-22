using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DOOR_TRIM_INSPECTION.Class
{
    public class HanMechDBHelper
    {
        private SqlConnection dbConnection = null;
        private string connectionString = string.Empty;
        public DETECT_LIST dETECT_LIST = null;
        public SqlConnection Connection()
        {
            if (dbConnection == null || dbConnection.State != System.Data.ConnectionState.Open)
            {
                dbConnection = new SqlConnection(connectionString);
                dbConnection.Open();
            }
            return dbConnection;
        }

        public HanMechDBHelper()
        {
            // OPEN AND RETAIN DB CONNECTION HERE
            connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["HanmechDBConnection"].ConnectionString;
            SetDetectList();
        }

        public void Terminate()
        {
            if (dbConnection != null && dbConnection.State == System.Data.ConnectionState.Open)
            {
                dbConnection.Close();
                dbConnection = null;
            }
        }

        public void SetDetectList()
        {
            dETECT_LIST = new DETECT_LIST(GetDetectList());
        }

        public DETECT_NAME GetDetectList(int RuleID)
        {
            try
            {
                DETECT_NAME detectList = new DETECT_NAME();
                string query = string.Format("SELECT * " +
                    " FROM [duckyang].[dbo].[detection_name_list] where RuleID=@RuleID");

                using (SqlCommand command = new SqlCommand(query, Connection()))
                {
                    command.Parameters.Add("@RuleID", SqlDbType.Int).Value = RuleID;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            detectList = new DETECT_NAME()
                            {
                                NameEn = reader.GetString(0),
                                NameKr = reader.GetString(1),
                                RuleID = reader.GetInt32(2)
                            };
                        }
                    }
                }
                return detectList;
            }
            catch (Exception ex)
            {
                Machine.logger.Write(eLogType.ERROR, "DB Read Error: " + ex.ToString());
                return null;
            }
        }

        public List<DETECT_NAME> GetDetectList()
        {
            try
            {
                List<DETECT_NAME> detectList = new List<DETECT_NAME>();
                string query = string.Format("SELECT * " +
                    " FROM [detection_name_list]");

                using (SqlCommand command = new SqlCommand(query, Connection()))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DETECT_NAME detect = new DETECT_NAME()
                            {
                                NameEn = reader.GetString(0),
                                NameKr = reader.GetString(1),
                                RuleID = reader.GetInt32(2)
                            };
                            detectList.Add(detect);
                        }
                    }
                }
                return detectList;
            }
            catch (Exception ex)
            {
                Machine.logger.Write(eLogType.ERROR, "DB Read Error: " + ex.ToString());
                return null;
            }
        }

        public List<InspectionResult> GetInspectionResults()
        {
            List<InspectionResult> inspResults = new List<InspectionResult>();
            try
            {

                string query = "SELECT * FROM InspectionResult WHERE InspectionTime BETWEEN DATEADD(day, -30, GETDATE()) AND GETDATE() ORDER BY DoorTrimID";

                using (SqlCommand command = new SqlCommand(query, Connection()))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            InspectionResult inspResult = new InspectionResult()
                            {
                                InspectionResultID = reader.GetInt32(0),
                                InspectionTime = reader.GetDateTime(1),
                                DoorTrimID = reader.GetString(2),
                                RecipeID = reader.GetInt32(3),
                                Result = reader.GetString(4)
                            };
                            inspResults.Add(inspResult);
                        }
                    }
                }
                return inspResults;
            }
            catch (Exception ex)
            {
                // Machine.logger.Write(eLogType.ERROR, "GetDetectionClasses: " + ex.ToString());
                return inspResults;
            }
        }

        public List<RecipeDB> GetRecipes()
        {

            List<RecipeDB> recipes = new List<RecipeDB>();
            try
            {

                string query = "SELECT * FROM Recipe ORDER BY RecipeID";

                using (SqlCommand command = new SqlCommand(query, Connection()))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            RecipeDB recipe = new RecipeDB()
                            {
                                RecipeID = reader.GetInt32(0),
                                RecipeName = reader.GetString(1),
                                Model = reader.GetString(2),
                                Year = reader.GetString(3),
                                FrontImagePath = reader.GetString(4),
                                RearImagePath = reader.GetString(5),
                                DoorType = reader.GetString(6),
                                CreateDate = reader.GetDateTime(7),
                                ModifyDate = reader.GetDateTime(8)                                
                            };

                            FileInfo fi = new FileInfo(recipe.RearImagePath);
                            if (fi.Exists)
                            {
                                List<string> rearSub1ImageFiles = GetImagesByPattern(fi.DirectoryName, @"rear_\d{6}.bmp_\d{6}_sub_1$").ToList();
                                recipe.RearSub1ImagePath = rearSub1ImageFiles[0];
                            }
                            recipes.Add(recipe);
                        }
                    }
                }
                return recipes;
            }
            catch (Exception ex)
            {
                // Machine.logger.Write(eLogType.ERROR, "GetDetectionClasses: " + ex.ToString());
                return recipes;
            }
        }


        private string[] GetImagesByPattern(string folderPath, string pattern)
        {
            Regex regex = new Regex(pattern);
            return System.IO.Directory.GetFiles(folderPath, "*", System.IO.SearchOption.TopDirectoryOnly)
                            .Where(file => regex.IsMatch(System.IO.Path.GetFileNameWithoutExtension(file)))
                            .ToArray();
        }
        public int GetLatestRecipeID()
        {

            int RecipeID = 0;
            try
            {

                string query = $"SELECT MAX(RecipeID) FROM Recipe";

                using (SqlCommand command = new SqlCommand(query, Connection()))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {


                            RecipeID = reader.GetInt32(0);


                        }
                    }
                }
                return RecipeID;
            }
            catch (Exception ex)
            {
                // Machine.logger.Write(eLogType.ERROR, "GetDetectionClasses: " + ex.ToString());
                return RecipeID;
            }
        }
        public RecipeDB GetRecipe(int RecipeID)
        {

            RecipeDB recipe = new RecipeDB();
            try
            {

                string query = $"SELECT * FROM Recipe Where RecipeID = {RecipeID}";

                using (SqlCommand command = new SqlCommand(query, Connection()))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            recipe = new RecipeDB()
                            {
                                RecipeID = reader.GetInt32(0),
                                RecipeName = reader.GetString(1),
                                Model = reader.GetString(2),
                                Year = reader.GetString(3),
                                FrontImagePath = reader.GetString(4),
                                RearImagePath = reader.GetString(5),
                                DoorType = reader.GetString(6),
                                CreateDate = reader.GetDateTime(7),
                                ModifyDate = reader.GetDateTime(8)
                            };

                            FileInfo fi = new FileInfo(recipe.RearImagePath);
                            if (fi.Exists)
                            {
                                List<string> rearSub1ImageFiles = GetImagesByPattern(fi.DirectoryName, @"rear_\d{6}.bmp_\d{6}_sub_1$").ToList();
                                recipe.RearSub1ImagePath = rearSub1ImageFiles[0];
                            }
                        }
                    }
                }
                return recipe;
            }
            catch (Exception ex)
            {
                // Machine.logger.Write(eLogType.ERROR, "GetDetectionClasses: " + ex.ToString());
                return recipe;
            }
        }


        public bool SaveRecipe(RecipeDB recipe)
        {
            try
            {
                string query = "UPDATE Recipe SET RecipeName = @RecipeName, Model = @Model, Year = @Year, DoorType = @DoorType, ModifyDate = GETDATE(), FrontImagePath = @FrontImagePath, RearImagePath = @RearImagePath WHERE RecipeID = @RecipeID" +
                                             " IF @@ROWCOUNT = 0" +
                                             " INSERT INTO Recipe(RecipeName, Model, Year, DoorType, FrontImagePath, RearImagePath)" +
                                             " VALUES(@RecipeName, @Model, @Year, @DoorType, @FrontImagePath, @RearImagePath)";


                using (SqlCommand sqlCommand = new SqlCommand(query, Connection()))
                {
                    sqlCommand.Parameters.Add("@RecipeName", SqlDbType.VarChar).Value = recipe.RecipeName;
                    sqlCommand.Parameters.Add("@Model", SqlDbType.VarChar).Value = recipe.Model;
                    sqlCommand.Parameters.Add("@Year", SqlDbType.VarChar).Value = recipe.Year;
                    sqlCommand.Parameters.Add("@DoorType", SqlDbType.VarChar).Value = recipe.DoorType;
                    sqlCommand.Parameters.Add("@FrontImagePath", SqlDbType.VarChar).Value = string.IsNullOrEmpty(recipe.FrontImagePath) ? string.Empty : recipe.FrontImagePath;
                    sqlCommand.Parameters.Add("@RearImagePath", SqlDbType.VarChar).Value = string.IsNullOrEmpty(recipe.RearImagePath) ? string.Empty : recipe.RearImagePath;
                    sqlCommand.Parameters.Add("@RecipeID", SqlDbType.Int).Value = recipe.RecipeID;

                    sqlCommand.CommandType = CommandType.Text;
                    return sqlCommand.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                //Machine.logger.Write(eLogType.ERROR, "SaveTB_CHECK_HIST: " + ex.ToString());
                return false;
            }
        }

        public bool DeleteRecipe(int recipeID)
        {
            try
            {
                string query = "DELETE FROM Recipe WHERE RecipeID = @recipeID";
                string queryDelROI = "DELETE FROM detection_roi WHERE RecipeID = @recipeID";

                using (SqlCommand sqlCommand = new SqlCommand(query, Connection()))
                {
                    sqlCommand.Parameters.Add("@recipeID", SqlDbType.Int).Value = recipeID;
                    sqlCommand.CommandType = CommandType.Text;
                    if (sqlCommand.ExecuteNonQuery() > 0)
                    {
                        using (SqlCommand sqlCommandROI = new SqlCommand(queryDelROI, Connection()))
                        {
                            sqlCommand.Parameters.Add("@recipeID", SqlDbType.Int).Value = recipeID;
                            sqlCommand.CommandType = CommandType.Text;
                            if (sqlCommand.ExecuteNonQuery() > 0)
                            {
                                return true;
                            }
                        }

                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                //Machine.logger.Write(eLogType.ERROR, "SaveTB_CHECK_HIST: " + ex.ToString());
                return false;
            }
        }

        public DetectionClass GetDetectionClass(int DetectionClassID)
        {

            DetectionClass detClasses = new DetectionClass();
            try
            {

                string query = "SELECT  [DC].*,  [DNL].[NameKr] FROM  [DetectionClass] AS [DC] LEFT JOIN  [detection_name_list] AS [DNL] ON  [DC].[RuleID] = [DNL].[RuleID] where dc.DetectionClassID=@DetectionClassID";

                using (SqlCommand command = new SqlCommand(query, Connection()))
                {
                    command.Parameters.Add("@DetectionClassID", SqlDbType.Int).Value = DetectionClassID;
                    command.CommandType = CommandType.Text;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            detClasses = new DetectionClass()
                            {
                                DetectionClassID = reader.GetInt32(0),
                                DetectionClassName = reader.GetString(1),
                                RuleID = reader.GetInt32(2),
                                Parameters = reader.GetString(3),
                                RuleName = reader.GetString(6)
                            };
                        }
                    }
                }
                return detClasses;
            }
            catch (Exception ex)
            {
                // Machine.logger.Write(eLogType.ERROR, "GetDetectionClasses: " + ex.ToString());
                return detClasses;
            }
        }

        public List<DetectionClass> GetDetectionClasses()
        {

            List<DetectionClass> detClasses = new List<DetectionClass>();
            try
            {

                string query = "SELECT  [DC].*,  [DNL].[NameKr] FROM  [DetectionClass] AS [DC] LEFT JOIN  [detection_name_list] AS [DNL] ON  [DC].[RuleID] = [DNL].[RuleID] ORDER BY  [DC].[DetectionClassID] DESC";

                using (SqlCommand command = new SqlCommand(query, Connection()))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DetectionClass detClass = new DetectionClass()
                            {
                                DetectionClassID = reader.GetInt32(0),
                                DetectionClassName = reader.GetString(1),
                                RuleID = reader.GetInt32(2),
                                Parameters = reader.GetString(3),
                                RuleName = reader.GetString(6)
                            };
                            detClasses.Add(detClass);
                        }
                    }
                }
                return detClasses;
            }
            catch (Exception ex)
            {
                // Machine.logger.Write(eLogType.ERROR, "GetDetectionClasses: " + ex.ToString());
                return detClasses;
            }
        }

        public DetectionClass GetDetectionDefaultClasses(string DetectionClassName)
        {

            DetectionClass detClasses = new DetectionClass();
            try
            {

                string query = "SELECT * FROM DetectionClassDefault where  DetectionClassName=@DetectionClassName";

                using (SqlCommand command = new SqlCommand(query, Connection()))
                {
                    command.Parameters.Add("@DetectionClassName", SqlDbType.VarChar).Value = DetectionClassName;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            detClasses = new DetectionClass()
                            {
                                DetectionClassID = reader.GetInt32(0),
                                DetectionClassName = reader.GetString(1),
                                RuleID = reader.GetInt32(2),
                                Parameters = reader.GetString(3)
                            };
                        }
                    }
                }
                return detClasses;
            }
            catch (Exception ex)
            {
                // Machine.logger.Write(eLogType.ERROR, "GetDetectionClasses: " + ex.ToString());
                return detClasses;
            }
        }
        public bool DeleteDetectionClass(int detectionClassID)
        {
            try
            {
                string query = "DELETE FROM DetectionClass WHERE DetectionClassID = @DetectionClassID";


                using (SqlCommand sqlCommand = new SqlCommand(query, Connection()))
                {
                    sqlCommand.Parameters.Add("@DetectionClassID", SqlDbType.Int).Value = detectionClassID;
                    sqlCommand.CommandType = CommandType.Text;
                    return sqlCommand.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                //Machine.logger.Write(eLogType.ERROR, "SaveTB_CHECK_HIST: " + ex.ToString());
                return false;
            }
        }

        public bool DetectionClassInUse(int detectionClassID)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM detection_roi INNER JOIN DetectionClass" +
                                " ON detection_roi.detection_class_ID = DetectionClass.RuleID" +
                                " WHERE DetectionClass.DetectionClassID = @DetectionClassID";

                using (SqlCommand sqlCommand = new SqlCommand(query, Connection()))
                {
                    sqlCommand.Parameters.Add("@DetectionClassID", SqlDbType.Int).Value = detectionClassID;
                    sqlCommand.CommandType = CommandType.Text;
                    int count = (int)sqlCommand.ExecuteScalar();
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                //Machine.logger.Write(eLogType.ERROR, "SaveTB_CHECK_HIST: " + ex.ToString());
                return false;
            }
        }

        public bool SaveDetectionClass(DetectionClass detClass)
        {
            try
            {
                string query = "UPDATE DetectionClass SET DetectionClassName = @DetectionClassName, RuleID = @RuleID, Parameters = @Parameters, ModifyDate = GETDATE() WHERE DetectionClassID = @DetectionClassID" +
                                             " IF @@ROWCOUNT = 0" +
                                             " INSERT INTO DetectionClass(DetectionClassName, RuleID, Parameters)" +
                                             " VALUES(@DetectionClassName, @RuleID, @Parameters)";


                using (SqlCommand sqlCommand = new SqlCommand(query, Connection()))
                {
                    sqlCommand.Parameters.Add("@DetectionClassName", SqlDbType.VarChar).Value = detClass.DetectionClassName;
                    sqlCommand.Parameters.Add("@RuleID", SqlDbType.Int).Value = detClass.RuleID;
                    sqlCommand.Parameters.Add("@Parameters", SqlDbType.VarChar).Value = detClass.Parameters;
                    sqlCommand.Parameters.Add("@DetectionClassID", SqlDbType.Int).Value = detClass.DetectionClassID;

                    sqlCommand.CommandType = CommandType.Text;
                    return sqlCommand.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                //Machine.logger.Write(eLogType.ERROR, "SaveTB_CHECK_HIST: " + ex.ToString());
                return false;
            }
        }
        public List<DetectionROIDetails> GetDetectionROIs(int RecipeID)
        {
            try
            {
                List<DetectionROIDetails> DetectionROIDetailsList = new List<DetectionROIDetails>();

                string query = "SELECT dr.detection_roi_ID, dr.start_x, dr.start_y, dr.end_x, dr.end_y," +
                                        " dr.roi_name, dr.detection_class_ID ," +
                                        " dr.Parameters, dr.recipe_ID, dr.front_door, dr.ALC_CODE, dr.ALC_NAME, dr.ROIGroup, dr.InUse, dr.DetectionClassName, dr.roi_name_location" +
                                " FROM detection_roi dr" +
                                //" INNER JOIN DetectionClass dc" +
                                //    " ON dr.detection_class_ID = dc.RuleID" +
                                //" INNER JOIN detection_name_list dnl" +
                                //    " ON dnl.RuleID = dc.RuleID" +
                                $" WHERE dr.recipe_ID = {RecipeID}";



                using (SqlCommand command = new SqlCommand(query, Connection()))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DetectionROIDetails detectionROIDetails = new DetectionROIDetails
                            {
                                detection_roi_ID = reader.GetInt32(0),
                                start_x = (int)(reader.GetDouble(1)),
                                start_y = (int)(reader.GetDouble(2)),
                                end_x = (int)(reader.GetDouble(3)),
                                end_y = (int)(reader.GetDouble(4)),
                                roi_name = reader.GetString(5),
                                detection_class_ID = reader.GetInt32(6),
                                Parameters = reader.GetString(7),
                                recipe_ID = reader.GetInt32(8),
                                front_door = reader.GetInt32(9),
                                ALC_CODE = DbHelper.GetValue<string>(reader, "ALC_CODE") != null ? DbHelper.GetValue<string>(reader, "ALC_CODE").Trim() : "",
                                ALC_NAME = DbHelper.GetValue<string>(reader, "ALC_NAME") != null ? DbHelper.GetValue<string>(reader, "ALC_NAME").Trim() : "",
                                group_name = DbHelper.GetValue<string>(reader, "ROIGroup") != null ? DbHelper.GetValue<string>(reader, "ROIGroup").Trim() : "",
                                Use = DbHelper.GetValue<bool>(reader, "InUse") != null ? DbHelper.GetValue<bool>(reader, "InUse") : false,
                                DetectionClassName = reader.GetString(14),
                                roi_name_location = reader.GetInt32(15)
                            };

                            DetectionROIDetailsList.Add(detectionROIDetails);
                        }
                    }
                }

                return DetectionROIDetailsList;
            }
            catch (Exception ex)
            {
                return new List<DetectionROIDetails>();

            }
        }

        public List<DetectionROIDetails> GetDetectionROIs(int RecipeID, bool IsFront)
        {
            try
            {
                int front_door = IsFront ? 1 : 0;
                List<DetectionROIDetails> DetectionROIDetailsList = new List<DetectionROIDetails>();

                string query = "SELECT dr.detection_roi_ID, dr.start_x, dr.start_y, dr.end_x, dr.end_y," +
                                        " dr.roi_name, dr.detection_class_ID , dnl.NameKr," +
                                        " dr.Parameters, dr.recipe_ID, dr.front_door, dr.ALC_CODE, dr.ALC_NAME, dr.ROIGroup, dr.InUse, dr.DetectionClassName, dr.roi_name_location" +
                                " FROM detection_roi dr" +
                                " INNER JOIN DetectionClass dc" +
                                    " ON dr.detection_class_ID = dc.RuleID" +
                                " INNER JOIN detection_name_list dnl" +
                                    " ON dnl.RuleID = dc.RuleID" +
                                $" WHERE dr.recipe_ID = {RecipeID} AND dr.front_door = {front_door}" +
                                $"GROUP BY dr.detection_roi_ID, dr.start_x, dr.start_y, dr.end_x, dr.end_y, dr.roi_name, dr.detection_class_ID, dnl.NameKr, dr.Parameters, dr.recipe_ID, dr.front_door, dr.ALC_CODE, dr.ALC_NAME, dr.ROIGroup, dr.InUse, dr.DetectionClassName, dr.roi_name_location;";



                using (SqlCommand command = new SqlCommand(query, Connection()))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DetectionROIDetails detectionROIDetails = new DetectionROIDetails
                            {
                                detection_roi_ID = reader.GetInt32(0),
                                start_x = (int)(reader.GetDouble(1)),
                                start_y = (int)(reader.GetDouble(2)),
                                end_x = (int)(reader.GetDouble(3)),
                                end_y = (int)(reader.GetDouble(4)),
                                roi_name = reader.GetString(5),
                                detection_class_ID = reader.GetInt32(6),
                                Parameters = reader.GetString(8),
                                recipe_ID = reader.GetInt32(9),
                                front_door = reader.GetInt32(10),
                                ALC_CODE = DbHelper.GetValue<string>(reader, "ALC_CODE"), //!= null ? DbHelper.GetValue<string>(reader, "ALC_CODE") : "",
                                ALC_NAME = DbHelper.GetValue<string>(reader, "ALC_NAME"), //!= null ? DbHelper.GetValue<string>(reader, "ALC_NAME") : "",
                                group_name = DbHelper.GetValue<string>(reader, "ROIGroup"), //!= null ? DbHelper.GetValue<string>(reader, "ROIGroup") : "",
                                Use = DbHelper.GetValue<bool>(reader, "InUse"),// != null ? DbHelper.GetValue<bool>(reader, "InUse") : false,
                                DetectionClassName = DbHelper.GetValue<string>(reader, "DetectionClassName"),  //reader.GetString(15) != null ? reader.GetString(15) : "",
                                roi_name_location = reader.GetInt32(16)
                            };

                            DetectionROIDetailsList.Add(detectionROIDetails);
                        }
                    }
                }

                return DetectionROIDetailsList;
            }
            catch (Exception ex)
            {
                return new List<DetectionROIDetails>();

            }
        }

        public bool DeleteDetectionROI(int detectionROIID)
        {
            try
            {
                string query = "DELETE FROM detection_roi WHERE detection_roi_ID = @detectionROIID";


                using (SqlCommand sqlCommand = new SqlCommand(query, Connection()))
                {
                    sqlCommand.Parameters.Add("@detectionROIID", SqlDbType.Int).Value = detectionROIID;
                    sqlCommand.CommandType = CommandType.Text;
                    return sqlCommand.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                Machine.logger.Write(eLogType.ERROR, "DeleteDetectionROI: " + ex.ToString());
                return false;
            }
        }


        public int SaveDetectionROI(DetectionROIDetailsUI detROI)
        {
            int DetectionROIID = -1;

            try
            {
                if (detROI.detection_roi_ID != 0)
                {
                    string updateQuery = "UPDATE detection_roi SET start_x = @start_x, start_y = @start_y, end_x = @end_x, end_y = @end_y," +
                                                                " roi_name = @roi_name, detection_class_ID = @detection_class_ID, Parameters = @Parameters, recipe_ID = @recipe_ID," +
                                                                " front_door = @front_door, ALC_CODE = @ALC_CODE, ALC_NAME = @ALC_NAME, ROIGroup = @ROIGroup, InUse = @InUse, DetectionClassName = @DetectionClassName, " +
                                                                " roi_name_location = @roi_name_location" +
                                                                " WHERE detection_roi_ID = @detection_roi_ID";

                    using (SqlCommand sqlCommand = new SqlCommand(updateQuery, Connection()))
                    {
                        sqlCommand.Parameters.Add("@start_x", SqlDbType.Float).Value = detROI.start_x;
                        sqlCommand.Parameters.Add("@start_y", SqlDbType.Float).Value = detROI.start_y;
                        sqlCommand.Parameters.Add("@end_x", SqlDbType.Float).Value = detROI.end_x;
                        sqlCommand.Parameters.Add("@end_y", SqlDbType.Float).Value = detROI.end_y;
                        sqlCommand.Parameters.Add("@roi_name", SqlDbType.VarChar).Value = detROI.roi_name;
                        sqlCommand.Parameters.Add("@detection_class_ID", SqlDbType.Int).Value = detROI.detection_class_ID;
                        sqlCommand.Parameters.Add("@Parameters", SqlDbType.VarChar).Value = detROI.Parameters;
                        sqlCommand.Parameters.Add("@recipe_ID", SqlDbType.Int).Value = detROI.recipe_ID;
                        sqlCommand.Parameters.Add("@front_door", SqlDbType.Int).Value = detROI.front_door;
                        sqlCommand.Parameters.Add("@ALC_CODE", SqlDbType.VarChar).Value = detROI.ALC_CODE == null ? "" : detROI.ALC_CODE;
                        sqlCommand.Parameters.Add("@ALC_NAME", SqlDbType.VarChar).Value = detROI.ALC_NAME == null ? "" : detROI.ALC_NAME;
                        sqlCommand.Parameters.Add("@ROIGroup", SqlDbType.VarChar).Value = detROI.group_name == null ? "" : detROI.group_name;
                        sqlCommand.Parameters.Add("@InUse", SqlDbType.Bit).Value = detROI.Use ? 1 : 0;
                        sqlCommand.Parameters.Add("@DetectionClassName", SqlDbType.VarChar).Value = detROI.DetectionClassName;
                        sqlCommand.Parameters.Add("@roi_name_location", SqlDbType.Int).Value = detROI.roi_name_location;
                        sqlCommand.Parameters.Add("@detection_roi_ID", SqlDbType.Int).Value = detROI.detection_roi_ID;
                        sqlCommand.CommandType = CommandType.Text;
                        if (sqlCommand.ExecuteNonQuery() > 0)
                        {
                            DetectionROIID = detROI.detection_roi_ID;
                        }
                        return DetectionROIID;
                    }
                }
                else
                {
                    string insertQuery =   "INSERT INTO detection_roi(start_x, start_y, end_x, end_y, roi_name, detection_class_ID," +
                                             " Parameters, recipe_ID, front_door, ALC_CODE, ALC_NAME, ROIGroup, InUse, DetectionClassName, roi_name_location)" +
                                             " VALUES(@start_x, @start_y, @end_x, @end_y, @roi_name, @detection_class_ID, @Parameters," +
                                             " @recipe_ID, @front_door, @ALC_CODE, @ALC_NAME, @ROIGroup, @InUse, @DetectionClassName, @roi_name_location); SELECT SCOPE_IDENTITY(); ";


                    using (SqlCommand sqlCommand = new SqlCommand(insertQuery, Connection()))
                    {
                        sqlCommand.Parameters.Add("@start_x", SqlDbType.Float).Value = detROI.start_x;
                        sqlCommand.Parameters.Add("@start_y", SqlDbType.Float).Value = detROI.start_y;
                        sqlCommand.Parameters.Add("@end_x", SqlDbType.Float).Value = detROI.end_x;
                        sqlCommand.Parameters.Add("@end_y", SqlDbType.Float).Value = detROI.end_y;
                        sqlCommand.Parameters.Add("@roi_name", SqlDbType.VarChar).Value = detROI.roi_name;
                        sqlCommand.Parameters.Add("@detection_class_ID", SqlDbType.Int).Value = detROI.detection_class_ID;
                        sqlCommand.Parameters.Add("@Parameters", SqlDbType.VarChar).Value = detROI.Parameters;
                        sqlCommand.Parameters.Add("@recipe_ID", SqlDbType.Int).Value = detROI.recipe_ID;
                        sqlCommand.Parameters.Add("@front_door", SqlDbType.Int).Value = detROI.front_door;
                        sqlCommand.Parameters.Add("@ALC_CODE", SqlDbType.VarChar).Value = detROI.ALC_CODE == null ? "" : detROI.ALC_CODE;
                        sqlCommand.Parameters.Add("@ALC_NAME", SqlDbType.VarChar).Value = detROI.ALC_NAME == null ? "" : detROI.ALC_NAME;
                        sqlCommand.Parameters.Add("@ROIGroup", SqlDbType.VarChar).Value = detROI.group_name == null ? "" : detROI.group_name;
                        sqlCommand.Parameters.Add("@InUse", SqlDbType.Bit).Value = detROI.Use ? 1 : 0;
                        sqlCommand.Parameters.Add("@DetectionClassName", SqlDbType.VarChar).Value = detROI.DetectionClassName;
                        sqlCommand.Parameters.Add("@roi_name_location", SqlDbType.Int).Value = detROI.roi_name_location;
                        sqlCommand.CommandType = CommandType.Text;

                        object id = sqlCommand.ExecuteScalar();

                        if (id != null)
                            int.TryParse(id.ToString(), out DetectionROIID);
                        return DetectionROIID;
                    }
                }

                

                //string query = "UPDATE detection_roi SET start_x = @start_x, start_y = @start_y, end_x = @end_x, end_y = @end_y," +
                //                             " roi_name = @roi_name, detection_class_ID = @detection_class_ID, Parameters = @Parameters, recipe_ID = @recipe_ID," +
                //                             " front_door = @front_door, ALC_CODE = @ALC_CODE, ALC_NAME = @ALC_NAME, ROIGroup = @ROIGroup, InUse = @InUse, DetectionClassName = @DetectionClassName, " +
                //                             " roi_name_location = @roi_name_location" +
                //                             " WHERE detection_roi_ID = @detection_roi_ID" +
                //                             " IF @@ROWCOUNT = 0" +
                //                             " INSERT INTO detection_roi(start_x, start_y, end_x, end_y, roi_name, detection_class_ID," +
                //                             " Parameters, recipe_ID, front_door, ALC_CODE, ALC_NAME, ROIGroup, InUse, DetectionClassName, roi_name_location)" +
                //                             " VALUES(@start_x, @start_y, @end_x, @end_y, @roi_name, @detection_class_ID, @Parameters," +
                //                             " @recipe_ID, @front_door, @ALC_CODE, @ALC_NAME, @ROIGroup, @InUse, @DetectionClassName, @roi_name_location );";


                //using (SqlCommand sqlCommand = new SqlCommand(query, Connection()))
                //{
                //    sqlCommand.Parameters.Add("@start_x", SqlDbType.Float).Value = detROI.start_x;
                //    sqlCommand.Parameters.Add("@start_y", SqlDbType.Float).Value = detROI.start_y;
                //    sqlCommand.Parameters.Add("@end_x", SqlDbType.Float).Value = detROI.end_x;
                //    sqlCommand.Parameters.Add("@end_y", SqlDbType.Float).Value = detROI.end_y;
                //    sqlCommand.Parameters.Add("@roi_name", SqlDbType.VarChar).Value = detROI.roi_name;
                //    sqlCommand.Parameters.Add("@detection_class_ID", SqlDbType.Int).Value = detROI.detection_class_ID;
                //    sqlCommand.Parameters.Add("@Parameters", SqlDbType.VarChar).Value = detROI.Parameters;
                //    sqlCommand.Parameters.Add("@recipe_ID", SqlDbType.Int).Value = detROI.recipe_ID;
                //    sqlCommand.Parameters.Add("@front_door", SqlDbType.Int).Value = detROI.front_door;
                //    sqlCommand.Parameters.Add("@ALC_CODE", SqlDbType.VarChar).Value = detROI.ALC_CODE == null ? "" : detROI.ALC_CODE;
                //    sqlCommand.Parameters.Add("@ALC_NAME", SqlDbType.VarChar).Value = detROI.ALC_NAME == null ? "" : detROI.ALC_NAME;
                //    sqlCommand.Parameters.Add("@ROIGroup", SqlDbType.VarChar).Value = detROI.group_name == null ? "" : detROI.group_name;
                //    sqlCommand.Parameters.Add("@InUse", SqlDbType.Bit).Value = detROI.Use ? 1 : 0;
                //    sqlCommand.Parameters.Add("@detection_roi_ID", SqlDbType.Int).Value = detROI.detection_roi_ID;
                //    sqlCommand.Parameters.Add("@DetectionClassName", SqlDbType.VarChar).Value = detROI.DetectionClassName;
                //    sqlCommand.Parameters.Add("@roi_name_location", SqlDbType.Int).Value = detROI.roi_name_location;
                //    sqlCommand.CommandType = CommandType.Text;
                //    return sqlCommand.ExecuteNonQuery() > 0;
                //}
            }
            catch (Exception ex)
            {
                //Machine.logger.Write(eLogType.ERROR, "SaveTB_CHECK_HIST: " + ex.ToString());
                return DetectionROIID;
            }
        }

        public static List<KeyValuePair<string, string>> ParseParam(string param)
        {
            List<KeyValuePair<string, string>> dlClasses = new List<KeyValuePair<string, string>>(); // LOAD FROM XML (Name, ID)
            if (param == null) return dlClasses;
            string[] datas = param.Split('|');
            if (datas == null)
                return null;
            if (datas[0].Length == 0 || datas[0].IndexOf(";") == -1)
                return null;
            foreach (string s in datas)
            {
                string[] keyvalue = s.Split(';');
                dlClasses.Add(new KeyValuePair<string, string>(keyvalue[0].Trim(), keyvalue[1].Trim()));
            }
            return dlClasses;
        }


        // MEER 2024.12.05
        public bool SaveInspectionResult(InspectionResult result, List<InspectionNGResult> inspectionNGResult, List<DoorTrimInsp> ALCData)
        {

            try
            {

                string query = @"INSERT INTO InspectionResult (InspectionTime, DoorTrimID, RecipeID, Result) VALUES (@InspectionTime, @DoorTrimID, @RecipeID, @Result); SELECT SCOPE_IDENTITY(); ";
                int InspectionResultID = 0;

                using (SqlCommand sqlCommand = new SqlCommand(query, Connection()))
                {
                    sqlCommand.Parameters.Add("@InspectionTime", SqlDbType.DateTime).Value = result.InspectionTime;
                    sqlCommand.Parameters.Add("@DoorTrimID", SqlDbType.VarChar).Value = result.DoorTrimID;
                    sqlCommand.Parameters.Add("@RecipeID", SqlDbType.Int).Value = result.RecipeID;
                    sqlCommand.Parameters.Add("@Result", SqlDbType.VarChar).Value = result.Result;

                    sqlCommand.CommandType = CommandType.Text;
                    object id = sqlCommand.ExecuteScalar();

                    if (id != null)
                        int.TryParse(id.ToString(), out InspectionResultID);
                    if (InspectionResultID > 0)
                    {
                        //if (SaveInspectionNGItems(InspectionResultID, inspectionNGResult))
                        //    return SaveALCInspectionResult(InspectionResultID, ALCData);
                        SaveInspectionNGItems(InspectionResultID, inspectionNGResult);
                        SaveALCInspectionResult(InspectionResultID, ALCData);
                    }

                }
                return true;
            }
            catch (Exception ex)
            {
                //Machine.logger.Write(eLogType.ERROR, "SaveTB_CHECK_HIST: " + ex.ToString());
                return false;
            }
        }

        private bool SaveALCInspectionResult(int inspectionResultID, List<DoorTrimInsp> aLCData)
        {
            try
            {
                string query = @"INSERT INTO InspectionALCResult (InspectionResultID, ALCName, ALCCode, Result) VALUES ";

                List<string> Values = new List<string>();
                foreach (DoorTrimInsp alcItem in aLCData)
                    Values.Add($"({inspectionResultID}, '{alcItem.Name}', '{alcItem.Type}', '{alcItem.InspResult}')");

                query += string.Join(",", Values);

                using (SqlCommand sqlCommand = new SqlCommand(query, Connection()))
                {
                    sqlCommand.CommandType = CommandType.Text;
                    return sqlCommand.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                //Machine.logger.Write(eLogType.ERROR, "SaveTB_CHECK_HIST: " + ex.ToString());
                return false;
            }
        }

        private bool SaveInspectionNGItems(int inspectionResultID, List<InspectionNGResult> inspectionNGResult)
        {
            try
            {
                string query = @"INSERT INTO InspectionNGResult (InspectionResultID, DetectionRoiID) VALUES ";

                List<string> Values = new List<string>();
                foreach (InspectionNGResult ngItem in inspectionNGResult)
                    Values.Add($"({inspectionResultID}, {ngItem.DetectionRoiID})");

                query += string.Join(",", Values);

                using (SqlCommand sqlCommand = new SqlCommand(query, Connection()))
                {
                    sqlCommand.CommandType = CommandType.Text;
                    return sqlCommand.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                //Machine.logger.Write(eLogType.ERROR, "SaveTB_CHECK_HIST: " + ex.ToString());
                return false;
            }
        }
        // MEER 2024.12.05

        public List<InspectionResult> GetInspectionResultsByQuery(DateTime dtFrom, DateTime dtTO, string resultType, string doorTrimID)
        {
            try
            {
                List<InspectionResult> inspectionResults = new List<InspectionResult>();

                string query = "SELECT * " +
                                " FROM InspectionResult" +
                                $" WHERE InspectionTime >= '{dtFrom.Date.ToString("yyyy-MM-dd")}' AND InspectionTime < '{dtTO.Date.AddDays(1).Date.ToString("yyyy-MM-dd")}'";


                if (resultType != "")
                    query += $" AND Result = '{resultType}'";

                if (!string.IsNullOrEmpty(doorTrimID))
                    query += $" AND DoorTrimID LIKE '{doorTrimID}%'";

                //List<string> doorTypeOrConditions = new List<string>();

                //if (doorType != "")
                //{
                //    if (doorType.Contains("A"))
                //        doorTypeOrConditions.Add("DoorTrimID LIKE 'A%'");
                //    if (doorType.Contains("B"))
                //        doorTypeOrConditions.Add("DoorTrimID LIKE 'B%'");
                //    if (doorType.Contains("C"))
                //        doorTypeOrConditions.Add("DoorTrimID LIKE 'C%'");
                //    if (doorType.Contains("D"))
                //        doorTypeOrConditions.Add("DoorTrimID LIKE 'D%'");

                //}
                //if (doorTypeOrConditions.Count > 1)
                //{
                //    query += $" AND (" + string.Join(" OR ", doorTypeOrConditions) + ")";
                //}
                //else if (doorTypeOrConditions.Count == 1)
                //{
                //    query += $" AND {doorTypeOrConditions.ElementAt(0)}";
                //}

                query += $" ORDER BY InspectionTime DESC"; 
                using (SqlCommand command = new SqlCommand(query, Connection()))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            InspectionResult inspectionResult = new InspectionResult
                            {
                                InspectionResultID = reader.GetInt32(0),
                                InspectionTime = reader.GetDateTime(1),
                                DoorTrimID = reader.GetString(2),
                                RecipeID = reader.GetInt32(3),
                                Result = reader.GetString(4)
                            };
                            inspectionResults.Add(inspectionResult);
                        }
                    }
                }
                return inspectionResults;
            }
            catch (Exception ex)
            {
                return new List<InspectionResult>();

            }
        }

        public List<DoorTrimInsp> GetNGInspectionResults(int InspectionResultID)
        {

            List<DoorTrimInsp> ngItems = new List<DoorTrimInsp>();

            try
            {

                string query = "SELECT DROI.DetectionClassName, INGR.DetectionRoiID FROM InspectionNGResult INGR INNER JOIN detection_roi DROI ON INGR.DetectionRoiID = DROI.detection_roi_ID WHERE INGR.InspectionResultID = @InspectionResultID";

                using (SqlCommand command = new SqlCommand(query, Connection()))
                {
                    command.Parameters.Add("@InspectionResultID", SqlDbType.Int).Value = InspectionResultID;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ngItems.Add(new DoorTrimInsp()
                            {
                                Name = reader.GetString(0),
                                Type = reader.GetInt32(1).ToString(),
                                InspResult = "NG"
                            });
                        }
                    }
                }
                return ngItems;
            }
            catch (Exception ex)
            {
                // Machine.logger.Write(eLogType.ERROR, "GetDetectionClasses: " + ex.ToString());
                return ngItems;
            }
        }

        public List<DoorTrimInsp> GetALCInspectionResults(int InspectionResultID)
        {
            List<DoorTrimInsp> ngItems = new List<DoorTrimInsp>();

            try
            {

                string query = "SELECT ALCName, ALCCode, Result FROM InspectionALCResult WHERE InspectionResultID = @InspectionResultID";

                using (SqlCommand command = new SqlCommand(query, Connection()))
                {
                    command.Parameters.Add("@InspectionResultID", SqlDbType.Int).Value = InspectionResultID;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ngItems.Add(new DoorTrimInsp()
                            {
                                Name = reader.GetString(0),
                                Type = reader.GetString(1),
                                InspResult = reader.GetString(2)
                            });
                        }
                    }
                }
                return ngItems;
            }
            catch (Exception ex)
            {
                // Machine.logger.Write(eLogType.ERROR, "GetDetectionClasses: " + ex.ToString());
                return ngItems;
            }
        }
        public List<InspectionResult> GetInspectionResultsByDoorTrim(string DoorTrimID)
        {
            List<InspectionResult> inspResults = new List<InspectionResult>();
            try
            {

                string query = $"SELECT * FROM InspectionResult WHERE DoorTrimID LIKE '%{DoorTrimID}%' AND InspectionTime BETWEEN DATEADD(day, -60, GETDATE()) AND GETDATE() ORDER BY DoorTrimID";

                using (SqlCommand command = new SqlCommand(query, Connection()))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            InspectionResult inspResult = new InspectionResult()
                            {
                                InspectionResultID = reader.GetInt32(0),
                                InspectionTime = reader.GetDateTime(1),
                                DoorTrimID = reader.GetString(2),
                                RecipeID = reader.GetInt32(3),
                                Result = reader.GetString(4)
                            };
                            inspResults.Add(inspResult);
                        }
                    }
                }
                return inspResults;
            }
            catch (Exception ex)
            {
                // Machine.logger.Write(eLogType.ERROR, "GetDetectionClasses: " + ex.ToString());
                return inspResults;
            }
        }


        public int GetTextDirectionByRegionID(int RegionID)
        {
            int roi_name_location = 1;
            try
            {
                string query = $"SELECT roi_name_location FROM detection_roi Where RegionID = {RegionID}";
                using (SqlCommand command = new SqlCommand(query, Connection()))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            roi_name_location = reader.GetInt32(0);
                        }
                    }
                }
                return roi_name_location;
            }
            catch (Exception ex)
            {
                // Machine.logger.Write(eLogType.ERROR, "GetDetectionClasses: " + ex.ToString());
                return roi_name_location;
            }
        }

        // MEER 2025.01.30

        public System.Windows.Media.Color GetColorByALC(string ColorName)
        {
            string selectQuery = "SELECT ColorCodeID, ColorName, ColorValue FROM ColorCode WHERE ColorName = @ColorName";
            SqlCommand selectCmd = new SqlCommand(selectQuery, Connection());
            selectCmd.Parameters.AddWithValue("@ColorName", ColorName);

            using (SqlDataReader reader = selectCmd.ExecuteReader())
            {
                if (reader.Read()) // Color already exists
                {
                    int colorCodeID = (int)reader["ColorCodeID"];
                    string colorName = (string)reader["ColorName"];
                    string colorValue = (string)reader["ColorValue"];

                    return ConvertToColor(colorValue);
                }
            }
            return System.Windows.Media.Colors.Transparent;
        }

        public Dictionary<string, System.Windows.Media.Color> GetColorDictionary()
        {
            Dictionary<string, System.Windows.Media.Color> dictColor = new Dictionary<string, System.Windows.Media.Color>();

            string selectQuery = "SELECT ColorCodeID, ColorName, ColorValue FROM ColorCode";
            SqlCommand selectCmd = new SqlCommand(selectQuery, Connection());
            
            using (SqlDataReader reader = selectCmd.ExecuteReader())
            {
                while (reader.Read()) 
                {
                    int colorCodeID = (int)reader["ColorCodeID"];
                    string colorName = (string)reader["ColorName"];
                    string colorValue = (string)reader["ColorValue"];
                    
                    dictColor.Add(colorName, ConvertToColor(colorValue));
                }
            }
            return dictColor;
        }

        private System.Windows.Media.Color ConvertToColor(string colorValue)
        {
            string[] colorVal = colorValue.Split(',');
            return System.Windows.Media.Color.FromArgb(255, byte.Parse(colorVal[0]), byte.Parse(colorVal[1]), byte.Parse(colorVal[2]));

        }

        public bool SaveALCColor(string ALCColorCode, System.Windows.Media.Color ColorValue)
        {
            try
            {
                string query = "INSERT INTO ColorCode (ColorName, ColorValue)" +
                               " VALUES(@ColorName, @Color)";

                string strColor = $"{ColorValue.R},{ColorValue.G},{ColorValue.B}";
                using (SqlCommand sqlCommand = new SqlCommand(query, Connection()))
                {
                    sqlCommand.Parameters.Add("@ColorName", SqlDbType.VarChar).Value = ALCColorCode;
                    sqlCommand.Parameters.Add("@Color", SqlDbType.VarChar).Value = strColor;
                    
                    sqlCommand.CommandType = CommandType.Text;
                    return sqlCommand.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                //Machine.logger.Write(eLogType.ERROR, "SaveTB_CHECK_HIST: " + ex.ToString());
                return false;
            }
        }

        // MEER 2025.01.30


        // AD 2025.05.08
        public bool HistoryInspectionResult(int frontDoor, int InspectionResultID)
        {
            bool result = true;
            try
            {
                //-- IS FRONT DOOR NG == > 1
                //-- IS REAR DOOR NG == > 0
                string query = @" SELECT COUNT(*) NgCount FROM InspectionNGResult INGR "
                                + " INNER JOIN detection_roi DR "
                                + " ON INGR.DetectionRoiID = DR.detection_roi_ID "
                                + " WHERE DR.front_door = @frontDoor "
                                + " AND INGR.InspectionResultID = @InspectionResultID ";

                using (SqlCommand command = new SqlCommand(query, Connection()))
                {
                    command.Parameters.Add("@frontDoor", SqlDbType.Int).Value = frontDoor;
                    command.Parameters.Add("@InspectionResultID", SqlDbType.Int).Value = InspectionResultID;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if(reader.GetInt32(0) != 0)
                            {
                                result = false;
                                break;
                            }
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                Machine.logger.Write(eLogType.ERROR, "HistoryInspectionResult: " + ex.ToString());
                return result;
            }
        }

        // AD 2025.05.08
    }


    public static class DbHelper
    {
        public static T GetValue<T>(this SqlDataReader sqlDataReader, string columnName)
        {
            var value = sqlDataReader[columnName];

            return value == DBNull.Value ? default(T) : (T)value;
        }
    }
}
