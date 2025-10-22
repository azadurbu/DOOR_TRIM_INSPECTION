using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOOR_TRIM_INSPECTION.Class
{

    public class Recipe2
    {
        int RecipeID { get; set; }
        string Name { get; set; }
        string Model { get; set; }
        string Year { get; set; }
        string ImagePathFront { get; set; }
        string ImagePathRear { get; set; }
        string Door { get; set; }
        DateTime CreateDate { get; set; }
        DateTime ModifyDate { get; set; }

        public Recipe2() { }



    }

    public class AlgorithmParam
    {
        public int RuleD { get; set; }
        public string ParamName { get; set; } = "";
        public int DefaultValue { get; set; }

        public List<AlgorithmParamOption> ParamOptions { get; set; } = new List<AlgorithmParamOption>();
        public AlgorithmParam()
        {
            RuleD = 0;
            ParamName = "";
            DefaultValue = 0;
            ParamOptions = new List<AlgorithmParamOption>();
        }
    }

    public class AlgorithmParamOption
    {
        public AlgorithmParamOption()
        {

        }
        public AlgorithmParamOption(string optionName,  string value)
        {
            OptionName = optionName;
            Value = value;
        }

        public string OptionName { get; set; } = "";
        public string Value { get; set; } = "";


    }

    public class DetectionClass
    {
        public int DetectionClassID { get; set; }
        public string DetectionClassName { get; set; }
        //public int DLClassID { get; set; }
        public int RuleID { get; set; }
        public string Parameters { get; set; }
        public DetectionClass() { }

        // EXTENDED PROPERTIES
        //public string DLClassName { get { return Enum.GetName(typeof(DEEP_CLASS), DLClassID); } } // GET IT FROM XML OR DB
        public string RuleName { get; set; } // GET IT FROM XML OR DB
    }
}
