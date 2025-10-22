using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DOOR_TRIM_INSPECTION.Class.SpeakerInspectionItem;

namespace DOOR_TRIM_INSPECTION.Class
{
    public class FrontInspectionResult
    {
        public List<ColorInspectionItem> ColorInspectionResult;
        public List<ColorMatchInspectionItem> ColorMatchInspectionResult;
        // MEER 2025.05.14
        public List<PlugInspectionItem> PlugInspectionResult;
        public List<ScrewInspectionItem> ScrewInspectionResult;
        public List<BoltInspectionItem> BoltInspectionResult;
        public List<PadInspectionItem> PadInspectionResult;
        public List<SpeakerInspectionItem> SpeakerInspectionResult;
        public List<SmallPadInspectionItem> SmallPadInspectionResult;
        public List<ScrewMacthInspectionItem> ScrewMacthInspectionResult;
        public List<WhitePadInspectionItem> WhitePadInspectionResult;
        // MEER 2025.05.14
        public FrontInspectionResult()
        {
            ColorInspectionResult = new List<ColorInspectionItem>();
            ColorMatchInspectionResult = new List<ColorMatchInspectionItem>();
            // MEER 2025.05.14
            PlugInspectionResult = new List<PlugInspectionItem>();
            ScrewInspectionResult = new List<ScrewInspectionItem>();
            BoltInspectionResult = new List<BoltInspectionItem>();
            PadInspectionResult = new List<PadInspectionItem>();
            SpeakerInspectionResult = new List<SpeakerInspectionItem>();
            SmallPadInspectionResult = new List<SmallPadInspectionItem>();
            ScrewMacthInspectionResult = new List<ScrewMacthInspectionItem>();
            WhitePadInspectionResult = new List<WhitePadInspectionItem>();
        }
    }
    public class RearInspectionResult
    {
        public List<PlugInspectionItem> PlugInspectionResult;
        public List<ScrewInspectionItem> ScrewInspectionResult;
        public List<BoltInspectionItem> BoltInspectionResult;
        public List<ColorInspectionItem> ColorInspectionResult;
        public List<PadInspectionItem> PadInspectionResult;
        public List<SpeakerInspectionItem> SpeakerInspectionResult;
        public List<SmallPadInspectionItem> SmallPadInspectionResult;
        public List<ScrewMacthInspectionItem> ScrewMacthInspectionResult;
        public List<PlugMatchInspectionItem> PlugMatchInspectionResult;
        public List<DeepInspectionItem> DeepScrewInspectionResult;
        public List<DeepInspectionItem> DeepFusionInspectionResult;
        public List<WhitePadInspectionItem> WhitePadInspectionResult;
#if USE_COGNEX
        public List<PlugCognexInspectionItem> PlugCognexInspectionResult;
#endif
        public RearInspectionResult()
        {

            PlugInspectionResult = new List<PlugInspectionItem>();
            ScrewInspectionResult = new List<ScrewInspectionItem>();
            BoltInspectionResult = new List<BoltInspectionItem>();
            ColorInspectionResult = new List<ColorInspectionItem>();
            PadInspectionResult = new List<PadInspectionItem>();
            SpeakerInspectionResult = new List<SpeakerInspectionItem>();
            SmallPadInspectionResult = new List<SmallPadInspectionItem>();
            ScrewMacthInspectionResult = new List<ScrewMacthInspectionItem>();
            PlugMatchInspectionResult = new List<PlugMatchInspectionItem>();
            DeepFusionInspectionResult = new List<DeepInspectionItem>();
            WhitePadInspectionResult = new List<WhitePadInspectionItem>();
#if USE_COGNEX
            PlugCognexInspectionResult = new List<PlugCognexInspectionItem>();
#endif
        }
    }


    public class Inspection
    {
        private Recipe currentRecipe;
        private string FrontImagePath;
        private string RearImagePath;
        private string RearSub1ImagePath;
//#if USE_EXTRA_CAM
        private string RearSub2ImagePath;
//#endif
        private Mat FrontImage;
        private Mat RearImage;
        private Mat RearSub1Image;
//#if USE_EXTRA_CAM
        private Mat RearSub2Image;
//#endif
        //public bool IsTrialInspection = false;

        private InspectionSummary inspectionSummary = null;

        public bool USE_COGNEX_RESULT { get { return Machine.config.setup.USE_COGNEX_RESULT; } }
        private List<int> OldLeadwireIDs = new List<int>() { 1584, 1903, 1910, 1919, 1911, 1915 };//1581, 1857, 1864, 1886, 3124, 3106, 3133, 3105
       

        private bool PlugMatchResultAnyOK
        {
            get {
                if (USE_COGNEX_RESULT)
                {
                    foreach (PlugMatchInspectionItem plugMatchInpsItem in RearInspectionResult.PlugMatchInspectionResult)
                    {
                        if (OldLeadwireIDs.Contains(plugMatchInpsItem.RegionID)) continue;
                        if (plugMatchInpsItem.InspectionResult == INSPECTION_RESULT.OK)
                            return true;
                    }
#if USE_COGNEX
                    return (RearInspectionResult.PlugCognexInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.OK));
#endif
                }
                else
                {
                    return RearInspectionResult.PlugMatchInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.OK);
                }
            }
        }

        private bool PlugMatchResultAnyNG
        {
            get
            {
                if (USE_COGNEX_RESULT)
                {
                    foreach (PlugMatchInspectionItem plugMatchInpsItem in RearInspectionResult.PlugMatchInspectionResult)
                    {
                        if (OldLeadwireIDs.Contains(plugMatchInpsItem.RegionID)) continue;
                        if (plugMatchInpsItem.InspectionResult == INSPECTION_RESULT.NG)
                            return true;
                    }
#if USE_COGNEX
                    return (RearInspectionResult.PlugCognexInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG));
#endif
                }
                else
                {
                    return RearInspectionResult.PlugMatchInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG);
                }
            }
        }



        public bool AnyOK
        {
            get
            {
                bool result = false;
                if (
                    (RearInspectionResult.BoltInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.OK))
                    || (RearInspectionResult.PlugInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.OK))
                    || (RearInspectionResult.ScrewInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.OK))
                    || (RearInspectionResult.PadInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.OK))
                    || (RearInspectionResult.SmallPadInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.OK))
                    || (RearInspectionResult.WhitePadInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.OK))
                    || (RearInspectionResult.SpeakerInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.OK))
                    || (RearInspectionResult.ScrewMacthInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.OK))
//#if USE_COGNEX
//                    || (RearInspectionResult.PlugCognexInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.OK))
//#else
//                    || (RearInspectionResult.PlugMatchInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.OK))
//#endif
                    || (FrontInspectionResult.ColorInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.OK)) // MEER 2025.01.31
                    || (FrontInspectionResult.PlugInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.OK)) // MEER 2025.05.14
                    || (FrontInspectionResult.ScrewInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.OK))
                    || (FrontInspectionResult.BoltInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.OK))
                    || (FrontInspectionResult.PadInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.OK))
                    || (FrontInspectionResult.SpeakerInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.OK))
                    || (FrontInspectionResult.SmallPadInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.OK))
                    || (FrontInspectionResult.ScrewMacthInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.OK))
                    || (FrontInspectionResult.WhitePadInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.OK)) // MEER 2025.05.14
                    || PlugMatchResultAnyOK // 2024.08.18
                    )
                    result = true;
                return result;
            }
        }

        public bool AnyNG
        {
            get
            {
                bool result = false;
                if (
                    (RearInspectionResult.BoltInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG))
                    || (RearInspectionResult.PlugInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG))
                    || (RearInspectionResult.ScrewInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG))
                    || (RearInspectionResult.PadInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG))
                    || (RearInspectionResult.SmallPadInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG))
                    || (RearInspectionResult.WhitePadInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG))
                    || (RearInspectionResult.SpeakerInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG))
                    || (RearInspectionResult.ScrewMacthInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG))
//#if USE_COGNEX
//                  || (RearInspectionResult.PlugCognexInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG))
//#else
//                    || (RearInspectionResult.PlugMatchInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG))
//#endif
                    || (FrontInspectionResult.ColorInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG)) // MEER 2025.01.31
                    || (FrontInspectionResult.PlugInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG)) // MEER 2025.05.14
                    || (FrontInspectionResult.ScrewInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG))
                    || (FrontInspectionResult.BoltInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG))
                    || (FrontInspectionResult.PadInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG))
                    || (FrontInspectionResult.SpeakerInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG))
                    || (FrontInspectionResult.SmallPadInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG))
                    || (FrontInspectionResult.ScrewMacthInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG))
                    || (FrontInspectionResult.WhitePadInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG)) // MEER 2025.05.14
                    || PlugMatchResultAnyNG // 2024.08.18
                    )
                    result = true;
                return result;
            }
        }

        
        public INSPECTION_RESULT RearInspectionResultCode
        {
            get
            {
                INSPECTION_RESULT result = INSPECTION_RESULT.OK;
                if (
                    (RearInspectionResult.BoltInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG || x.InspectionResult == INSPECTION_RESULT.NOT_FOUND))
                    || (RearInspectionResult.PlugInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG || x.InspectionResult == INSPECTION_RESULT.NOT_FOUND))
                    || (RearInspectionResult.ScrewInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG || x.InspectionResult == INSPECTION_RESULT.NOT_FOUND))
                    || (RearInspectionResult.PadInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG || x.InspectionResult == INSPECTION_RESULT.NOT_FOUND))
                    || (RearInspectionResult.SmallPadInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG || x.InspectionResult == INSPECTION_RESULT.NOT_FOUND))
                    || (RearInspectionResult.WhitePadInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG || x.InspectionResult == INSPECTION_RESULT.NOT_FOUND))
                    || (RearInspectionResult.SpeakerInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG || x.InspectionResult == INSPECTION_RESULT.NOT_FOUND))
                    || (RearInspectionResult.ScrewMacthInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG || x.InspectionResult == INSPECTION_RESULT.NOT_FOUND))
//#if USE_COGNEX
//                    || (RearInspectionResult.PlugCognexInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG || x.InspectionResult == INSPECTION_RESULT.NOT_FOUND))
//#else
//                    || (RearInspectionResult.PlugMatchInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG || x.InspectionResult == INSPECTION_RESULT.NOT_FOUND))
//#endif


                    || (RearInspectionResult.ColorInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG || x.InspectionResult == INSPECTION_RESULT.NOT_FOUND)) // MEER 2025.04.18
                    )
                    result = INSPECTION_RESULT.NG;

                if (Machine.config.aiConfig.UseInCalculation
                    && RearInspectionResult.DeepFusionInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG || x.InspectionResult == INSPECTION_RESULT.NOT_FOUND)
                    )
                {
                    result = INSPECTION_RESULT.NG;
                }

                if (USE_COGNEX_RESULT)
                {
                    foreach (PlugMatchInspectionItem plugMatchInpsItem in RearInspectionResult.PlugMatchInspectionResult)
                    {
                        if (OldLeadwireIDs.Contains(plugMatchInpsItem.RegionID)) continue;
                        if (plugMatchInpsItem.InspectionResult == INSPECTION_RESULT.NG)
                        {
                            result = INSPECTION_RESULT.NG;
                            break;
                        }
                    }

                    if (RearInspectionResult.PlugCognexInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG || x.InspectionResult == INSPECTION_RESULT.NOT_FOUND))
                        result = INSPECTION_RESULT.NG;
                }
                else
                {
                    if (RearInspectionResult.PlugMatchInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG || x.InspectionResult == INSPECTION_RESULT.NOT_FOUND))
                        result = INSPECTION_RESULT.NG;
                }
                return result;
            }
        }

        public INSPECTION_RESULT FrontInspectionResultCode
        {
            get
            {
                INSPECTION_RESULT result = INSPECTION_RESULT.OK;
                if (
                    //FrontInspectionResult.ColorInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG || x.InspectionResult == INSPECTION_RESULT.NOT_FOUND) || // MEER 2025.01.31
                    FrontInspectionResult.ColorMatchInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG || x.InspectionResult == INSPECTION_RESULT.NOT_FOUND)
                    || (FrontInspectionResult.PlugInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG || x.InspectionResult == INSPECTION_RESULT.NOT_FOUND)) // MEER 2025.05.14
                    || (FrontInspectionResult.ScrewInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG || x.InspectionResult == INSPECTION_RESULT.NOT_FOUND))
                    || (FrontInspectionResult.BoltInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG || x.InspectionResult == INSPECTION_RESULT.NOT_FOUND))
                    || (FrontInspectionResult.PadInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG || x.InspectionResult == INSPECTION_RESULT.NOT_FOUND))
                    || (FrontInspectionResult.SpeakerInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG || x.InspectionResult == INSPECTION_RESULT.NOT_FOUND))
                    || (FrontInspectionResult.SmallPadInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG || x.InspectionResult == INSPECTION_RESULT.NOT_FOUND))
                    || (FrontInspectionResult.ScrewMacthInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG || x.InspectionResult == INSPECTION_RESULT.NOT_FOUND))
                    || (FrontInspectionResult.WhitePadInspectionResult.Any(x => x.InspectionResult == INSPECTION_RESULT.NG || x.InspectionResult == INSPECTION_RESULT.NOT_FOUND)) // MEER 2025.05.14
                    )
                    result = INSPECTION_RESULT.NG;
                return result;
            }

        }
        public Mat Getimage(bool isFront)
        {
            if (isFront)
                return FrontImage.Clone();
            else
                return RearImage.Clone();
        }

        public FrontInspectionResult FrontInspectionResult = new FrontInspectionResult();
        public RearInspectionResult RearInspectionResult = new RearInspectionResult();

        public Inspection(Recipe Recipe)
        {
            this.currentRecipe = Recipe;
        }

        public int GetRecipeNo()
        {
            return currentRecipe.RecipeID;
        }

        public void SetFrontInspectionImage(string FrontImagePath, bool isNeedEQ = true)
        {
            if (FrontImagePath == null)
                return;
            if (FrontImagePath.Length == 0)
                return;
            this.FrontImagePath = FrontImagePath;
            this.FrontImage = Cv2.ImRead(FrontImagePath, ImreadModes.Color);
            if (isNeedEQ)
                FrontImage = LevelOps.EqualizeHistColor(FrontImage);
        }

        public void SetRearInspectionImage(string RearImagePath, bool isNeedEQ = true)
        {
            if (RearImagePath == null)
                return;
            if (RearImagePath.Length == 0)
                return;

            this.RearImagePath = RearImagePath;
            this.RearImage = Cv2.ImRead(RearImagePath, ImreadModes.Color);
            if (isNeedEQ)
                RearImage = LevelOps.EqualizeHistColor(RearImage);
        }

        public void SetRearSub1InspectionImage(string RearSub1ImagePath)
        {
            if (RearSub1ImagePath == null)
                return;
            if (RearSub1ImagePath.Length == 0)
                return;

            this.RearSub1ImagePath = RearSub1ImagePath;
            this.RearSub1Image = Cv2.ImRead(RearSub1ImagePath, ImreadModes.Color);
            RearSub1Image = LevelOps.EqualizeHistColor(RearSub1Image);
        }

        public void SetFrontInspectionImage(Mat FrontImage)
        {
            this.FrontImage = FrontImage.Clone();
        }

        public void SetRearInspectionImage(Mat RearImage)
        {
            this.RearImage = RearImage.Clone();
        }

        public void SetRearSub1InspectionImage(Mat RearImage)
        {
            this.RearSub1Image = RearImage.Clone();
            RearSub1Image = LevelOps.EqualizeHistColor(RearSub1Image);
        }
//#if USE_EXTRA_CAM
        public void SetRearSub2InspectionImage(Mat RearImage2)
        {
            this.RearSub2Image = RearImage2.Clone();
            //try
            //{
            //    this.RearSub3Image = LevelOps.EqualizeHistColor(RearSub3Image);
            //}
            //catch(Exception ex)
            //{
            //    Console.WriteLine(ex.ToString());
            //}

        }

        public void SetRearSub2InspectionImage(string RearSub2ImagePath)
        {
            if (RearSub2ImagePath == null)
                return;
            if (RearSub2ImagePath.Length == 0)
                return;

            this.RearSub2ImagePath = RearSub2ImagePath;
            this.RearSub2Image = Cv2.ImRead(RearSub2ImagePath, ImreadModes.Grayscale);
            //RearSub1Image = LevelOps.EqualizeHistColor(RearSub1Image);
        }
//#endif
        public void ExecuteFrontInspection()
        {
            ColorInspection colorInspection = new ColorInspection(FrontImage, currentRecipe.FrontColorInspectionROIs);
            colorInspection.Execute();
            FrontInspectionResult.ColorInspectionResult = colorInspection.ColorInspectionItems;

            ColorMatchInspection colorMatchInspection = new ColorMatchInspection(FrontImage, currentRecipe.FrontColorMatchInspectionROIs);
            colorMatchInspection.Execute();
            FrontInspectionResult.ColorMatchInspectionResult = colorMatchInspection.ColorInspectionItems;

            PlugInspection plugInspection = new PlugInspection(FrontImage, currentRecipe.FrontPlugInspectionROIs);
            plugInspection.Execute();
            FrontInspectionResult.PlugInspectionResult = plugInspection.PlugInspectionItems;

            ScrewInspection screwInspection = new ScrewInspection(FrontImage, currentRecipe.FrontScrewInspectionROIs);
            screwInspection.Execute();
            FrontInspectionResult.ScrewInspectionResult = screwInspection.ScrewInspectionItems;

            BoltInspection boltInspection = new BoltInspection(FrontImage, currentRecipe.FrontBoltInspectionROIs);
            boltInspection.Execute();
            FrontInspectionResult.BoltInspectionResult = boltInspection.BoltInspectionItems;

            PadInspection padInspection = new PadInspection(FrontImage, currentRecipe.FrontPadInspectionROIs);
            padInspection.Execute();
            FrontInspectionResult.PadInspectionResult = padInspection.PadInspectionItems;

            SpeakerInspection speakerInspection = new SpeakerInspection(FrontImage, currentRecipe.FrontSpeakerInspectionROIs);
            speakerInspection.Execute();
            FrontInspectionResult.SpeakerInspectionResult = speakerInspection.SpeakerInspectionItems;

            SmallPadInspection smallPadInspection = new SmallPadInspection(FrontImage, currentRecipe.FrontSmallPadInspectionROIs);
            smallPadInspection.Execute();
            FrontInspectionResult.SmallPadInspectionResult = smallPadInspection.PadInspectionItems;

            ScrewMacthInspection screwMatchInspection = new ScrewMacthInspection(FrontImage, currentRecipe.FrontScrewMatchInspectionROIs);
            screwMatchInspection.Execute();
            FrontInspectionResult.ScrewMacthInspectionResult = screwMatchInspection.screwMacthInspectionItems;

            WhitePadInspection whitePadInspection = new WhitePadInspection(FrontImage, currentRecipe.FrontWhitePadInspectionROIs);
            whitePadInspection.Execute();
            FrontInspectionResult.WhitePadInspectionResult = whitePadInspection.PadInspectionItems;
        }

        public void ExecuteRearInspection()
        {
            PlugInspection plugInspection = new PlugInspection(RearImage, currentRecipe.RearPlugInspectionROIs);
            plugInspection.Execute();
            RearInspectionResult.PlugInspectionResult = plugInspection.PlugInspectionItems;

            ScrewInspection screwInspection = new ScrewInspection(RearImage, currentRecipe.RearScrewInspectionROIs);  // ONE TIME PARAMETER SETUP FOR ALL SCREWS
                                                                                                                                         //MinThreshold, MaxThreshold, ThresholdType, MinContourArea); // ONE TIME PARAMETER SETUP FOR ALL SCREWS
            screwInspection.Execute();
            RearInspectionResult.ScrewInspectionResult = screwInspection.ScrewInspectionItems;

            BoltInspection boltInspection = new BoltInspection(RearImage, currentRecipe.RearBoltInspectionROIs); // ONE TIME PARAMETER SETUP FOR ALL SCREWS
            boltInspection.Execute();
            RearInspectionResult.BoltInspectionResult = boltInspection.BoltInspectionItems;

            ColorInspection colorInspection = new ColorInspection(RearImage, currentRecipe.RearColorInspectionROIs);
            colorInspection.Execute();
            RearInspectionResult.ColorInspectionResult = colorInspection.ColorInspectionItems;

            PadInspection padInspection = new PadInspection(RearImage, currentRecipe.RearPadInspectionROIs);
            padInspection.Execute();
            RearInspectionResult.PadInspectionResult = padInspection.PadInspectionItems;

            SpeakerInspection speakerInspection = new SpeakerInspection(RearImage, currentRecipe.RearSpeakerInspectionROIs);
            speakerInspection.Execute();
            RearInspectionResult.SpeakerInspectionResult = speakerInspection.SpeakerInspectionItems;

            SmallPadInspection smallPadInspection = new SmallPadInspection(RearImage, currentRecipe.RearSmallPadInspectionROIs);
            smallPadInspection.Execute();
            RearInspectionResult.SmallPadInspectionResult = smallPadInspection.PadInspectionItems;

            ScrewMacthInspection screwMacthInspection = new ScrewMacthInspection(RearImage, currentRecipe.RearScrewMatchInspectionROIs);
            screwMacthInspection.Execute();
            RearInspectionResult.ScrewMacthInspectionResult = screwMacthInspection.screwMacthInspectionItems;

            PlugMatchInspection plugMatchInspection = new PlugMatchInspection(RearImage, RearSub1Image, currentRecipe.RearPlugMatchInspectionROIs);
            plugMatchInspection.Execute();
            RearInspectionResult.PlugMatchInspectionResult = plugMatchInspection.PlugMatchInspectionItems;

            WhitePadInspection whitePadInspection = new WhitePadInspection(RearImage, currentRecipe.RearWhitePadInspectionROIs);
            whitePadInspection.Execute();
            RearInspectionResult.WhitePadInspectionResult = whitePadInspection.PadInspectionItems;

#if USE_DEEP
                try
                {
                    DeepInspection deepInspection = new DeepInspection(currentRecipe.RearDeepInspectionROIs);
                    deepInspection.Execute();
                    RearInspectionResult.DeepFusionInspectionResult = deepInspection.DeepFusionInspectionItems;
                    //RearInspectionResult.DeepScrewInspectionResult = deepInspection.DeepScrewInspectionItems;

                    //RevisitAggrigatedScrewInspection();
                }
                catch (Exception ex)
                {
                    Machine.logger.Write(eLogType.INSPECTION, ex.ToString());
                }
#endif


#if USE_COGNEX
            PlugCognexInspection plugCognexInspection = new PlugCognexInspection(RearImage, RearSub2Image, currentRecipe.PlugCognexInspectionROIs);
            plugCognexInspection.Execute();
            RearInspectionResult.PlugCognexInspectionResult = plugCognexInspection.PlugCognexInspectionItems;
//#else
//            PlugMatchInspection plugMatchInspection = new PlugMatchInspection(RearImage, RearSub1Image, currentRecipe.RearPlugMatchInspectionROIs);
//            plugMatchInspection.Execute();
//            RearInspectionResult.PlugMatchInspectionResult = plugMatchInspection.PlugMatchInspectionItems;
#endif

            PrepareInspectionSummary();
        }

        private void RevisitAggrigatedScrewInspection()
        {
            foreach (ScrewInspectionItem screwInspectionItem in RearInspectionResult.ScrewInspectionResult)
            {
                DeepInspectionItem deepScrewItem = RearInspectionResult.DeepScrewInspectionResult.Find(X => X.RegionID == screwInspectionItem.RegionID);
                if (deepScrewItem != null)
                {
                    if (deepScrewItem.Confidence > 0.9)
                    {
                        screwInspectionItem.InspectionResult = INSPECTION_RESULT.OK;
                        screwInspectionItem.DetectionClassName = "스크류(D)";
                    }
                }
            }

            foreach (ScrewMacthInspectionItem screwMatchInspectionItem in RearInspectionResult.ScrewMacthInspectionResult)
            {
                DeepInspectionItem deepScrewItem = RearInspectionResult.DeepScrewInspectionResult.Find(X => X.RegionID == screwMatchInspectionItem.RegionID);
                if (deepScrewItem != null)
                {
                    if (deepScrewItem.Confidence > 0.9)
                    {
                        screwMatchInspectionItem.InspectionResult = INSPECTION_RESULT.OK;
                        screwMatchInspectionItem.DetectionClassName = "스크류(D)";
                    }
                }
            }
        }

        public InspectionSummary GetInspectionSummary()
        {
            return inspectionSummary;
        }

        private void PrepareInspectionSummary()
        {
            inspectionSummary = new InspectionSummary();

            int screwOK = RearInspectionResult.ScrewInspectionResult.Where(x => x.InspectionResult == INSPECTION_RESULT.OK).Count() +
                            RearInspectionResult.ScrewMacthInspectionResult.Where(x => x.InspectionResult == INSPECTION_RESULT.OK).Count();
            int screwTotal = RearInspectionResult.ScrewInspectionResult.Count() +
                            RearInspectionResult.ScrewMacthInspectionResult.Count();

            inspectionSummary.SetScrewInfo(screwOK, screwTotal);

            int padOK = RearInspectionResult.PadInspectionResult.Where(x => x.InspectionResult == INSPECTION_RESULT.OK).Count() +
                            RearInspectionResult.SmallPadInspectionResult.Where(x => x.InspectionResult == INSPECTION_RESULT.OK).Count() +
                             RearInspectionResult.WhitePadInspectionResult.Where(x => x.InspectionResult == INSPECTION_RESULT.OK).Count();
            int padTotal = RearInspectionResult.PadInspectionResult.Count() +
                            RearInspectionResult.SmallPadInspectionResult.Count() +
                            RearInspectionResult.WhitePadInspectionResult.Count();

            inspectionSummary.SetPadInfo(padOK, padTotal);

            int speakerOK = RearInspectionResult.SpeakerInspectionResult.Where(x => x.InspectionResult == INSPECTION_RESULT.OK).Count(); 
            int speakerTotal = RearInspectionResult.SpeakerInspectionResult.Count();

            inspectionSummary.SetSpeakerInfo(speakerOK, speakerTotal);

            
            int leadWireOK = 0;
            int leadWireTotal = 0;

            if (USE_COGNEX_RESULT)
            {
                foreach (PlugMatchInspectionItem inspItem in RearInspectionResult.PlugMatchInspectionResult)
                {
                    if (OldLeadwireIDs.Contains(inspItem.RegionID)) continue;
                    if (inspItem.InspectionResult == INSPECTION_RESULT.OK)
                    {
                        leadWireOK += 1;
                    }
                    leadWireTotal += 1;
                }
#if USE_CONGNEX
                leadWireOK += RearInspectionResult.PlugCognexInspectionResult.Where(x => x.InspectionResult == INSPECTION_RESULT.OK).Count();
                leadWireTotal += RearInspectionResult.PlugCognexInspectionResult.Count();
#endif
            }
            else
            {
                leadWireOK = RearInspectionResult.PlugMatchInspectionResult.Where(x => x.InspectionResult == INSPECTION_RESULT.OK).Count();
                leadWireTotal = RearInspectionResult.PlugMatchInspectionResult.Count();
            }

            inspectionSummary.SetLeadwireInfo(leadWireOK, leadWireTotal);

            int fastenerOK = RearInspectionResult.BoltInspectionResult.Where(x => x.InspectionResult == INSPECTION_RESULT.OK).Count();
            int fastenerTotal = RearInspectionResult.BoltInspectionResult.Count();

            inspectionSummary.SetFastenerInfo(fastenerOK, fastenerTotal);

            int fusionOK = RearInspectionResult.DeepFusionInspectionResult.Where(x => x.InspectionResult == INSPECTION_RESULT.OK).Count(); // MEER DL 2025.01.24
            int fusionTotal = RearInspectionResult.DeepFusionInspectionResult.Count(); // MEER DL 2025.01.24

            inspectionSummary.SetFusionInfo(fusionOK, fusionTotal);
        }

        // MEER 2024.12.05
        public List<DoorTrimInsp> SaveInspectionResult(string DoorTrimID, DateTime BarCodeReadTime, List<DoorTrimInsp> ALCData) // MEER CHANGED TO BARCODE READ TIME 2025.01.22
        {

            InspectionResult inspResult = new InspectionResult();
            inspResult.RecipeID = this.currentRecipe.RecipeID;
            inspResult.DoorTrimID = DoorTrimID;
            inspResult.InspectionTime = BarCodeReadTime;
            if (RearInspectionResultCode == INSPECTION_RESULT.OK && FrontInspectionResultCode == INSPECTION_RESULT.OK)
                inspResult.Result = "OK";
            else
                inspResult.Result = "NG";

            List<InspectionNGResult> inspNG = new List<InspectionNGResult>();
            // PREPARE LIST OF ALL THE INSPECTION ITEMS
#if USE_COGNEX
            if (USE_COGNEX_RESULT)
            {
                foreach (PlugCognexInspectionItem result in RearInspectionResult.PlugCognexInspectionResult)
                {
                    if (result.InspectionResult == INSPECTION_RESULT.NG)
                    {
                        inspNG.Add(new InspectionNGResult() { DetectionRoiID = result.RegionID });
                        string ALCItem = Machine.ALCData.GetSPA(result.ALC_CODE).Trim();

                        if (ALCData.Where(x => x.Type == ALCItem).Count() > 0)
                        {
                            ALCData.Where(x => x.Type == ALCItem).First().InspResult = "NG";
                        }
                    }
                }
            }
#endif
            foreach (PlugMatchInspectionItem result in RearInspectionResult.PlugMatchInspectionResult)
            {
                if (USE_COGNEX_RESULT && OldLeadwireIDs.Contains(result.RegionID))
                    continue;

                if (result.InspectionResult == INSPECTION_RESULT.NG)
                {
                    inspNG.Add(new InspectionNGResult() { DetectionRoiID = result.RegionID });
                    string ALCItem = Machine.ALCData.GetSPA(result.ALC_CODE).Trim();

                    if (ALCData.Where(x => x.Type == ALCItem).Count() > 0)
                    {
                        ALCData.Where(x => x.Type == ALCItem).First().InspResult = "NG";
                    }
                }
            }
 
            foreach (BoltInspectionItem result in RearInspectionResult.BoltInspectionResult)
            {
                if (result.InspectionResult == INSPECTION_RESULT.NG)
                {
                    inspNG.Add(new InspectionNGResult() { DetectionRoiID = result.RegionID });
                    string ALCGroup = currentRecipe.GetALCGroup(result.RegionID);
                    if (ALCData.Where(x => x.Type == ALCGroup).Count() > 0)
                    {
                        ALCData.Where(x => x.Type == ALCGroup).First().InspResult = "NG";
                    }
                }
            }
            foreach (PlugInspectionItem result in RearInspectionResult.PlugInspectionResult)
            {
                if (result.InspectionResult == INSPECTION_RESULT.NG)
                {
                    inspNG.Add(new InspectionNGResult() { DetectionRoiID = result.RegionID });
                    string ALCGroup = currentRecipe.GetALCGroup(result.RegionID);
                    if (ALCData.Where(x => x.Type == ALCGroup).Count() > 0)
                    {
                        ALCData.Where(x => x.Type == ALCGroup).First().InspResult = "NG";
                    }
                }
            }
            foreach (ScrewInspectionItem result in RearInspectionResult.ScrewInspectionResult)
            {
                if (result.InspectionResult == INSPECTION_RESULT.NG)
                {
                    inspNG.Add(new InspectionNGResult() { DetectionRoiID = result.RegionID });
                    string ALCGroup = currentRecipe.GetALCGroup(result.RegionID);
                    if (ALCData.Where(x => x.Type == ALCGroup).Count() > 0)
                    {
                        ALCData.Where(x => x.Type == ALCGroup).First().InspResult = "NG";
                    }
                }
            }
            foreach (PadInspectionItem result in RearInspectionResult.PadInspectionResult)
            {
                if (result.InspectionResult == INSPECTION_RESULT.NG)
                {
                    inspNG.Add(new InspectionNGResult() { DetectionRoiID = result.RegionID });
                    string ALCGroup = currentRecipe.GetALCGroup(result.RegionID);
                    if (ALCData.Where(x => x.Type == ALCGroup).Count() > 0)
                    {
                        ALCData.Where(x => x.Type == ALCGroup).First().InspResult = "NG";
                    }
                }
            }
            foreach (SpeakerInspectionItem result in RearInspectionResult.SpeakerInspectionResult)
            {
                if (result.InspectionResult == INSPECTION_RESULT.NG)
                {
                    inspNG.Add(new InspectionNGResult() { DetectionRoiID = result.RegionID });
                    string ALCGroup = currentRecipe.GetALCGroup(result.RegionID);
                    if (ALCData.Where(x => x.Type == ALCGroup).Count() > 0)
                    {
                        ALCData.Where(x => x.Type == ALCGroup).First().InspResult = "NG";
                    }
                }
            }

            foreach (SmallPadInspectionItem result in RearInspectionResult.SmallPadInspectionResult)
            {
                if (result.InspectionResult == INSPECTION_RESULT.NG)
                {
                    inspNG.Add(new InspectionNGResult() { DetectionRoiID = result.RegionID });
                    string ALCGroup = currentRecipe.GetALCGroup(result.RegionID);
                    if (ALCData.Where(x => x.Type == ALCGroup).Count() > 0)
                    {
                        ALCData.Where(x => x.Type == ALCGroup).First().InspResult = "NG";
                    }
                }
            }
            //////
            ///         @TODO: ADD ALL OTHER INSPECTION ITEMS AS NECESSARY
            //////

            // MEER 2025.01.31 FRONT
            foreach (ColorInspectionItem result in FrontInspectionResult.ColorInspectionResult)
            {
                if (result.InspectionResult == INSPECTION_RESULT.NG)
                {
                    inspNG.Add(new InspectionNGResult() { DetectionRoiID = result.RegionID });
                    string ALCItem = Machine.ALCData.GetSPA(result.ALC_CODE).Trim();

                    if (ALCData.Where(x => x.Type == ALCItem).Count() > 0)
                    {
                        ALCData.Where(x => x.Type == ALCItem).First().InspResult = "NG";
                    }
                }
            }

            foreach (ColorMatchInspectionItem result in FrontInspectionResult.ColorMatchInspectionResult)
            {
                if (result.InspectionResult == INSPECTION_RESULT.NG)
                {
                    inspNG.Add(new InspectionNGResult() { DetectionRoiID = result.RegionID });
                    string ALCItem = Machine.ALCData.GetSPA(result.ALC_CODE).Trim();

                    if (ALCData.Where(x => x.Type == ALCItem).Count() > 0)
                    {
                        ALCData.Where(x => x.Type == ALCItem).First().InspResult = "NG";
                    }
                }
            }

            //////
            ///         @TODO: ALC RELATED INSPECTION LOGIC
            ///         // FIND ITEMS WITH ALC CODE -> ROI IDs
            ///         // MATCH IF ANY OF THEM ARE NG
            //////

            Machine.hmcDBHelper.SaveInspectionResult(inspResult, inspNG, ALCData);

            return ALCData;
        }

        // MEER 2024.12.05

    }
}
