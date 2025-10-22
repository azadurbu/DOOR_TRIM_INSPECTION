using Cognex.VisionPro;
using Cognex.VisionPro.PMAlign;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOOR_TRIM_INSPECTION.Class
{
    public class PMAlignMultiToolList
    {
        private Dictionary<string, CogPMAlignMultiTool> CogTools = new Dictionary<string, CogPMAlignMultiTool>();

        public void AddTool(string Path)
        {
            if (CogTools.ContainsKey(Path))
                CogTools.Remove(Path);
            
            CogPMAlignMultiTool cogPMAlignMultiTool = (CogPMAlignMultiTool)CogSerializer.LoadObjectFromFile(Path);
            CogTools.Add(Path, cogPMAlignMultiTool);
            
        }

        public void PreloadTools(string VPPRepoPath)
        {
            // READ THE FOLDER AND LOAD ALL TOOLS    
            string[] vppFiles = Directory.GetFiles(VPPRepoPath, "*.vpp");

            foreach (string vppFile in vppFiles)
            {
                DateTime startLoad = DateTime.Now;
                AddTool(vppFile);
                DateTime endLoad = DateTime.Now;
                TimeSpan duration_total = endLoad - startLoad;
                Console.WriteLine($"Load {vppFile} Time taken: {duration_total.TotalMilliseconds} ms");
            }
        }

        public CogPMAlignMultiTool GetTool(string VPPToolPath)
        {
            if (!File.Exists(VPPToolPath))
                return null;
            if (!CogTools.ContainsKey(VPPToolPath))
                AddTool(VPPToolPath);
            return CogTools[VPPToolPath];
        }
    }

    public class CognexVisionDetection
    {
        public PMAlignMultiToolList cogToolList = new PMAlignMultiToolList();

        public CognexVisionDetection()
        {
            //cogToolList.PreloadTools(Machine.config.setup.VPP_PATH);
            cogToolList.PreloadTools(Machine.config.setup.VPP_PATH);
        }

        public void AddOrUpdateTool(string vppPath)
        {
            cogToolList.AddTool(vppPath);
        }

        public Tuple<Rect, double> FindTemplate(string vppPath, Mat ImageRegion, Rect ROI)
        {
            DateTime startFindTemplate = DateTime.Now;

            CogPMAlignMultiTool pmAlignMultiTool = cogToolList.GetTool(vppPath);
            if (pmAlignMultiTool == null)
                return new Tuple<Rect, double>(new Rect(), 0);

            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(BitmapConverter.ToBitmap(ImageRegion)); // CONVERT IMAGEREGUION MAT TO BMP
            pmAlignMultiTool.InputImage = new CogImage8Grey(bmp);

            // MAKE FULL IMAGE ROI
            CogRectangle pmAlignROI = new CogRectangle();
            if (ROI.Height == 0 && ROI.Width == 0)
            {
                pmAlignROI.X = 0;
                pmAlignROI.Y = 0;
                pmAlignROI.Height = ImageRegion.Height;
                pmAlignROI.Width = ImageRegion.Width;
            }
            else
            {
                pmAlignROI.X = ROI.X;
                pmAlignROI.Y = ROI.Y;
                pmAlignROI.Height = ROI.Height;
                pmAlignROI.Width = ROI.Width;
            }
            pmAlignMultiTool.SearchRegion = pmAlignROI;

            pmAlignMultiTool.Run();

            var result = (CogPMAlignMultiResults)pmAlignMultiTool.Results;

            if (result == null)
            {
                Rect nullBBox = new Rect(0,0,0,0);

                return new Tuple<Rect, double>(nullBBox, 0.0);
            }


            double tmplX = 0.0f, tmplY = 0.0f, findScore = 0.0f;
            CogRectangle tmplRect = new CogRectangle();


            for (int i = 0; i < result.PMAlignResults.Count; i++)
            {
                var alignResult = result.PMAlignResults[i];
                var pose = alignResult.GetPose();
                findScore = alignResult.Score;

                //string tbAccuracy = score.ToString("F2");

                var graphics = alignResult.CreateResultGraphics(CogPMAlignResultGraphicConstants.MatchRegion);

                

                foreach (ICogGraphic graphic in graphics.Shapes)
                {
                    if (graphic is CogRectangle cogRectangle)
                    {
                        //tmplX = cogRectangle.X;
                        //tmplY = cogRectangle.Y;

                        tmplRect = cogRectangle;
                        //string text = "(" + X.ToString() + ", " + Y.ToString() + ")";
                    }
                    else if (graphic is CogPolygon cogPolygon)
                    {
                        //tmplX = cogPolygon.GetVertexX(cogPolygon.NearestVertex(0, 0));
                        //tmplY = cogPolygon.GetVertexY(cogPolygon.NearestVertex(0, 0));
                        tmplRect = cogPolygon.EnclosingRectangle(CogCopyShapeConstants.GeometryOnly);
                        //string text = "(" + x.ToString() + ", " + y.ToString() + ")";
                    }else if (graphic is CogRectangleAffine cogRectangleA)
                    {
                        tmplRect = cogRectangleA.EnclosingRectangle(CogCopyShapeConstants.GeometryOnly);
                    }
                }
            }
            DateTime endFindTemplate = DateTime.Now;
            TimeSpan duration_total = endFindTemplate - startFindTemplate;
            Console.WriteLine($"FindTemplate Time taken: {duration_total.TotalMilliseconds} ms");
            //Console.WriteLine($"FindTemplate : {tmplX}, {tmplY}, {findScore}");

            Rect rectBBox = new Rect((int)tmplRect.X, (int)tmplRect.Y, (int)tmplRect.Width, (int)tmplRect.Height);

            return new Tuple<Rect, double>(rectBBox, findScore);

        }
    }
}

